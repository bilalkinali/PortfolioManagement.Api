using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory.Proxy;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed class GetStockHistoryHandler
{
    private readonly PortfolioDbContext _db;
    private readonly MassiveStockHistoryProxy _proxy;

    public GetStockHistoryHandler(
        PortfolioDbContext db,
        MassiveStockHistoryProxy proxy)
    {
        _db = db;
        _proxy = proxy;
    }

    public async Task<GetStockHistoryResponse?> Handle(
        GetStockHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();
        var from = DateOnly.Parse(request.From);
        var to = DateOnly.Parse(request.To);
        var timespan = StockHistoryRangeRules.ResolveTimespan(request.Range, request.Timespan);
        var period = StockHistoryRangeRules.ResolvePeriod(request.Range, request.Timespan);
        var isRangeRequest = !string.IsNullOrWhiteSpace(request.Range);

        var instrumentId = await _db.Instruments
            .Where(x => x.Symbol == ticker)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (instrumentId is not null && period is not null)
        {
            var localBars = isRangeRequest
                ? await GetAggregatedLocalBarsAsync(
                    instrumentId.Value,
                    period.Value,
                    from,
                    to,
                    cancellationToken)
                : await GetLocalBarsAsync(
                    instrumentId.Value,
                    period.Value,
                    from,
                    to,
                    cancellationToken);

            // For now: local cache wins if any bars exist.
            // Later: check full date coverage and fetch missing ranges.
            if (localBars.Count > 0)
            {
                return MapToResponse(ticker, localBars);
            }
        }

        var result = await _proxy.GetHistoryAsync(
            ticker,
            from,
            to,
            timespan,
            cancellationToken);

        if (result?.Results is null)
        {
            return result;
        }

        return result;
    }

    private async Task<List<StockBar>> GetAggregatedLocalBarsAsync(
        int instrumentId,
        MarketDataPeriod period,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var dailyBars = await _db.MarketDataBars
            .AsNoTracking()
            .Where(x =>
                x.InstrumentId == instrumentId &&
                x.Period == MarketDataPeriod.Daily &&
                x.Date >= from &&
                x.Date <= to)
            .OrderBy(x => x.Date)
            .Select(x => new StockHistoryDailyBar(
                x.Date,
                x.Open,
                x.High,
                x.Low,
                x.Close,
                x.Volume))
            .ToListAsync(cancellationToken);

        return StockHistoryBarAggregator.Aggregate(dailyBars, period);
    }

    private async Task<List<StockBar>> GetLocalBarsAsync(
        int instrumentId,
        MarketDataPeriod period,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        return await _db.MarketDataBars
            .AsNoTracking()
            .Where(x =>
                x.InstrumentId == instrumentId &&
                x.Period == period &&
                x.Date >= from &&
                x.Date <= to)
            .OrderBy(x => x.Date)
            .Select(x => new StockBar(
                x.Close,
                x.High,
                x.Low,
                null,
                x.Open,
                new DateTimeOffset(x.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds(),
                x.Volume,
                null))
            .ToListAsync(cancellationToken);
    }

    private static GetStockHistoryResponse MapToResponse(string ticker, IReadOnlyList<StockBar> bars)
    {
        return new GetStockHistoryResponse(
            Adjusted: true,
            NextUrl: null,
            QueryCount: bars.Count,
            RequestId: null,
            ResultsCount: bars.Count,
            Status: "OK",
            Ticker: ticker,
            Results: bars.ToList());
    }

}
