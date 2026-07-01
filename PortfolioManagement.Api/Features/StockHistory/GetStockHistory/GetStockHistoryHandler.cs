using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory.Proxy;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed class GetStockHistoryHandler
{
    private readonly IConfiguration _configuration;
    private readonly PortfolioDbContext _db;
    private readonly MassiveStockHistoryProxy _massiveStockHistoryProxy;
    private readonly MarketDataProviderRouter _providerRouter;
    private readonly YahooMarketDataProxy _yahooMarketDataProxy;

    public GetStockHistoryHandler(
        PortfolioDbContext db,
        MassiveStockHistoryProxy massiveStockHistoryProxy,
        MarketDataProviderRouter providerRouter,
        YahooMarketDataProxy yahooMarketDataProxy,
        IConfiguration configuration)
    {
        _db = db;
        _massiveStockHistoryProxy = massiveStockHistoryProxy;
        _providerRouter = providerRouter;
        _yahooMarketDataProxy = yahooMarketDataProxy;
        _configuration = configuration;
    }

    public async Task<GetStockHistoryResponse?> Handle(
        GetStockHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();
        var from = DateOnly.Parse(request.From);
        var to = DateOnly.Parse(request.To);
        var period = StockHistoryRangeRules.ResolvePeriod(request.Range, request.Timespan);
        var isRangeRequest = !string.IsNullOrWhiteSpace(request.Range);

        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(x => x.Symbol == ticker, cancellationToken);
        List<StockBar> dailyLocalBars = [];

        if (instrument is not null && period is not null)
        {
            dailyLocalBars = await GetDailyLocalBarsAsync(
                instrument.Id,
                from,
                to,
                cancellationToken);

            if (HasRequestedRangeCoverage(dailyLocalBars, from, to))
            {
                return MapToResponse(ticker, MapResponseBars(dailyLocalBars, period.Value, isRangeRequest));
            }
        }

        var provider = _providerRouter.ResolveHistoryProvider(
            ticker,
            IsMassiveConfigured(),
            instrument?.ExchangeCode);
        var providerSymbol = _providerRouter.ResolveProviderSymbol(provider, ticker, instrument?.ProviderSymbol);
        var missingRanges = DetermineMissingHistoryRanges(dailyLocalBars, from, to);

        var fetchedDailyBars = new List<MarketDataHistoricalCandle>();
        foreach (var missingRange in missingRanges)
        {
            var fetchedBars = await FetchDailyBarsAsync(
                provider,
                providerSymbol,
                missingRange.From,
                missingRange.To,
                cancellationToken);

            if (fetchedBars is not null)
            {
                fetchedDailyBars.AddRange(fetchedBars);
            }
        }

        if (fetchedDailyBars.Count == 0 && dailyLocalBars.Count == 0)
        {
            return null;
        }

        if (fetchedDailyBars.Count > 0)
        {
            instrument ??= await CreatePlaceholderInstrumentAsync(ticker, cancellationToken);
            await SaveDailyBarsAsync(instrument, fetchedDailyBars, cancellationToken);
        }

        var localDailyBars = instrument is null
            ? dailyLocalBars
            : await GetDailyLocalBarsAsync(
                instrument.Id,
                from,
                to,
                cancellationToken);

        return MapToResponse(
            ticker,
            MapResponseBars(localDailyBars, period ?? MarketDataPeriod.Daily, isRangeRequest));
    }

    private async Task<List<StockBar>> GetDailyLocalBarsAsync(
        int instrumentId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        return await _db.MarketDataBars
            .AsNoTracking()
            .Where(x =>
                x.InstrumentId == instrumentId &&
                x.Period == MarketDataPeriod.Daily &&
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

    private async Task<IReadOnlyList<MarketDataHistoricalCandle>?> FetchDailyBarsAsync(
        MarketDataProvider provider,
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        if (provider == MarketDataProvider.Yahoo)
        {
            return await _yahooMarketDataProxy.GetDailyHistoryAsync(symbol, from, to, cancellationToken);
        }

        if (provider == MarketDataProvider.Massive)
        {
            var massiveResponse = await _massiveStockHistoryProxy.GetHistoryAsync(
                symbol,
                from,
                to,
                "day",
                cancellationToken);

            return massiveResponse?.Results?
                .Select(MapToCandle)
                .OrderBy(x => x.Date)
                .ToList();
        }

        return null;
    }

    private async Task SaveDailyBarsAsync(
        Instrument instrument,
        IReadOnlyList<MarketDataHistoricalCandle> candles,
        CancellationToken cancellationToken)
    {
        var dates = candles.Select(x => x.Date).Distinct().ToList();

        var existingDates = await _db.MarketDataBars
            .Where(x =>
                x.InstrumentId == instrument.Id &&
                x.Period == MarketDataPeriod.Daily &&
                dates.Contains(x.Date))
            .Select(x => x.Date)
            .ToListAsync(cancellationToken);

        var existingDateSet = existingDates.ToHashSet();

        foreach (var candle in candles)
        {
            if (existingDateSet.Contains(candle.Date))
            {
                continue;
            }

            instrument.AddMarketDataBar(
                candle.Date,
                MarketDataPeriod.Daily,
                candle.Open,
                candle.High,
                candle.Low,
                candle.Close,
                candle.Volume);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Instrument> CreatePlaceholderInstrumentAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var instrument = Instrument.Create(
            symbol: ticker,
            name: ticker,
            providerSymbol: ticker,
            market: "stocks",
            type: "CS");

        _db.Instruments.Add(instrument);
        await _db.SaveChangesAsync(cancellationToken);

        return instrument;
    }

    private bool IsMassiveConfigured()
        => !string.IsNullOrWhiteSpace(_configuration["Massive:ApiKey"]);

    private static List<StockHistoryDailyBar> MapToDailyBars(IReadOnlyList<StockBar> bars)
    {
        return bars
            .Select(x => new StockHistoryDailyBar(
                DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).UtcDateTime),
                x.Open,
                x.High,
                x.Low,
                x.Close,
                Convert.ToInt64(x.Volume)))
            .ToList();
    }

    private static IReadOnlyList<StockBar> MapResponseBars(
        IReadOnlyList<StockBar> dailyLocalBars,
        MarketDataPeriod period,
        bool isRangeRequest)
    {
        return period == MarketDataPeriod.Daily && !isRangeRequest
            ? dailyLocalBars
            : StockHistoryBarAggregator.Aggregate(MapToDailyBars(dailyLocalBars), period);
    }

    private static bool HasRequestedRangeCoverage(
        IReadOnlyList<StockBar> dailyLocalBars,
        DateOnly from,
        DateOnly to)
    {
        if (dailyLocalBars.Count == 0)
        {
            return false;
        }

        var firstLocalDate = ToDateOnly(dailyLocalBars[0]);
        var lastLocalDate = ToDateOnly(dailyLocalBars[^1]);

        return firstLocalDate <= from.AddDays(3) && lastLocalDate >= to.AddDays(-3);
    }

    private static IReadOnlyList<MissingHistoryRange> DetermineMissingHistoryRanges(
        IReadOnlyList<StockBar> dailyLocalBars,
        DateOnly from,
        DateOnly to)
    {
        if (dailyLocalBars.Count == 0)
        {
            return [new MissingHistoryRange(from, to)];
        }

        var firstLocalDate = ToDateOnly(dailyLocalBars[0]);
        var lastLocalDate = ToDateOnly(dailyLocalBars[^1]);
        var ranges = new List<MissingHistoryRange>();

        if (firstLocalDate > from.AddDays(3))
        {
            AddRangeIfValid(ranges, from, firstLocalDate.AddDays(-1));
        }

        if (lastLocalDate < to.AddDays(-3))
        {
            AddRangeIfValid(ranges, lastLocalDate.AddDays(1), to);
        }

        return ranges;
    }

    private static void AddRangeIfValid(
        List<MissingHistoryRange> ranges,
        DateOnly from,
        DateOnly to)
    {
        if (from <= to)
        {
            ranges.Add(new MissingHistoryRange(from, to));
        }
    }

    private static DateOnly ToDateOnly(StockBar bar)
        => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(bar.Timestamp).UtcDateTime);

    private static MarketDataHistoricalCandle MapToCandle(StockBar bar)
    {
        return new MarketDataHistoricalCandle(
            Date: DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(bar.Timestamp).UtcDateTime),
            Open: bar.Open,
            High: bar.High,
            Low: bar.Low,
            Close: bar.Close,
            Volume: Convert.ToInt64(bar.Volume));
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

internal sealed record MissingHistoryRange(DateOnly From, DateOnly To);
