using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
                x.Symbol,
                x.ProviderSymbol,
                x.Currency,
                x.ExchangeCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        var provider = _providerRouter.ResolveQuoteProvider(ticker, instrument?.ExchangeCode);
        var providerSymbol = string.IsNullOrWhiteSpace(instrument?.ProviderSymbol)
            ? ticker
            : instrument.ProviderSymbol;
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
            return null;
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
            Currency: quote.Currency ?? instrument?.Currency,
            CachedAtUtc: DateTimeOffset.UtcNow);

        _memoryCache.Set(
            cacheKey,
            response,
            GetQuoteCacheDuration(provider));

        return response;
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
