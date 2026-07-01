using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Finnhub;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;

public sealed class GetStockQuoteHandler
{
    private readonly IConfiguration _configuration;
    private readonly PortfolioDbContext _db;
    private readonly FinnhubQuoteProxy _finnhubQuoteProxy;
    private readonly IMemoryCache _memoryCache;
    private readonly MarketDataProviderRouter _providerRouter;
    private readonly YahooMarketDataProxy _yahooMarketDataProxy;

    public GetStockQuoteHandler(
        PortfolioDbContext db,
        IMemoryCache memoryCache,
        MarketDataProviderRouter providerRouter,
        FinnhubQuoteProxy finnhubQuoteProxy,
        YahooMarketDataProxy yahooMarketDataProxy,
        IConfiguration configuration)
    {
        _db = db;
        _memoryCache = memoryCache;
        _providerRouter = providerRouter;
        _finnhubQuoteProxy = finnhubQuoteProxy;
        _yahooMarketDataProxy = yahooMarketDataProxy;
        _configuration = configuration;
    }

    public async Task<GetStockQuoteResponse?> Handle(
        GetStockQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();

        var instrument = await _db.Instruments
            .AsNoTracking()
            .Where(x => x.Symbol == ticker)
            .Select(x => new
            {
                x.Id,
                x.Symbol,
                x.ProviderSymbol,
                x.Currency,
                x.ExchangeCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        var provider = _providerRouter.ResolveQuoteProvider(ticker, instrument?.ExchangeCode);
        var providerSymbol = _providerRouter.ResolveProviderSymbol(provider, ticker, instrument?.ProviderSymbol);
        var cacheKey = $"quote:{provider}:{providerSymbol}";

        if (_memoryCache.TryGetValue(cacheKey, out GetStockQuoteResponse? cachedQuote))
        {
            return cachedQuote;
        }

        var quote = provider == MarketDataProvider.Yahoo
            ? await _yahooMarketDataProxy.GetLatestQuoteAsync(providerSymbol, cancellationToken)
            : await _finnhubQuoteProxy.GetQuoteAsync(providerSymbol, instrument?.Currency, cancellationToken);

        if (quote is null)
        {
            return await GetLatestLoadedPriceResponse(ticker, instrument?.Id, instrument?.Currency, cancellationToken);
        }

        var response = new GetStockQuoteResponse(
            Symbol: ticker,
            CurrentPrice: quote.CurrentPrice,
            PreviousClose: quote.PreviousClose,
            Open: quote.Open,
            High: quote.High,
            Low: quote.Low,
            Volume: quote.Volume,
            TimestampUtc: quote.TimestampUtc,
            PriceDate: null,
            Currency: quote.Currency ?? instrument?.Currency,
            Source: "Live",
            CachedAtUtc: DateTimeOffset.UtcNow);

        _memoryCache.Set(
            cacheKey,
            response,
            GetQuoteCacheDuration(provider));

        return response;
    }

    private async Task<GetStockQuoteResponse?> GetLatestLoadedPriceResponse(
        string ticker,
        int? instrumentId,
        string? currency,
        CancellationToken cancellationToken)
    {
        if (instrumentId is null)
        {
            return null;
        }

        var latestBar = await _db.MarketDataBars
            .AsNoTracking()
            .Where(x =>
                x.InstrumentId == instrumentId &&
                x.Period == MarketDataPeriod.Daily)
            .OrderByDescending(x => x.Date)
            .Select(x => new
            {
                x.Date,
                x.Open,
                x.High,
                x.Low,
                x.Close,
                x.Volume
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (latestBar is null)
        {
            return null;
        }

        return new GetStockQuoteResponse(
            Symbol: ticker,
            CurrentPrice: latestBar.Close,
            PreviousClose: null,
            Open: latestBar.Open,
            High: latestBar.High,
            Low: latestBar.Low,
            Volume: latestBar.Volume,
            TimestampUtc: null,
            PriceDate: latestBar.Date,
            Currency: currency,
            Source: "LatestLoaded",
            CachedAtUtc: DateTimeOffset.UtcNow);
    }

    private TimeSpan GetQuoteCacheDuration(MarketDataProvider provider)
    {
        var key = provider == MarketDataProvider.Yahoo
            ? "MarketDataCache:GlobalQuoteTtlMinutes"
            : "MarketDataCache:UsQuoteTtlMinutes";

        var defaultMinutes = provider == MarketDataProvider.Yahoo ? 5 : 2;
        var minutes = _configuration.GetValue(key, defaultMinutes);

        return TimeSpan.FromMinutes(minutes);
    }
}
