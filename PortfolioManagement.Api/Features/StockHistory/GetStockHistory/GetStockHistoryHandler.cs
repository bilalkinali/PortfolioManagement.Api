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
        var timespan = string.IsNullOrWhiteSpace(request.Timespan)
            ? "day"
            : request.Timespan.Trim().ToLowerInvariant();

        var period = MapToMarketDataPeriod(timespan);
        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(x => x.Symbol == ticker, cancellationToken);

        if (instrument is not null && period is not null)
        {
            var localBars = await GetLocalBarsAsync(
                instrument.Id,
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

    private static MarketDataPeriod? MapToMarketDataPeriod(string timespan)
    {
        return timespan switch
        {
            "day" => MarketDataPeriod.Daily,
            "week" => MarketDataPeriod.Weekly,
            "month" => MarketDataPeriod.Monthly,
            _ => null
        };
    }
}
