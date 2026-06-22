using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Finnhub;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
using PortfolioManagement.Api.Features.Instruments.SearchInstruments.Proxy;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed class SearchInstrumentsHandler
{
    private readonly PortfolioDbContext _db;
    private readonly FinnhubSearchProxy _finnhubSearchProxy;
    private readonly ILogger<SearchInstrumentsHandler> _logger;
    private readonly MassiveSearchProxy _massiveSearchProxy;
    private readonly MarketDataProviderRouter _providerRouter;
    private readonly YahooMarketDataProxy _yahooMarketDataProxy;

    public SearchInstrumentsHandler(
        PortfolioDbContext db,
        MassiveSearchProxy massiveSearchProxy,
        FinnhubSearchProxy finnhubSearchProxy,
        YahooMarketDataProxy yahooMarketDataProxy,
        MarketDataProviderRouter providerRouter,
        ILogger<SearchInstrumentsHandler> logger)
    {
        _db = db;
        _logger = logger;
        _massiveSearchProxy = massiveSearchProxy;
        _finnhubSearchProxy = finnhubSearchProxy;
        _yahooMarketDataProxy = yahooMarketDataProxy;
        _providerRouter = providerRouter;
    }

    public async Task<SearchInstrumentsResponse?> Handle(SearchInstrumentsRequest request, CancellationToken cancellationToken)
    {
        var query = request.Query.Trim();
        var limit = request.Limit ?? 10;
        var type = request.Type ?? SearchInstrumentType.CS;

        var localResults = await SearchLocalDatabase(query, limit, cancellationToken);

        if (localResults.Count >= limit)
        {
            return new SearchInstrumentsResponse(localResults);
        }

        var remainingLimit = limit - localResults.Count;
        var provider = _providerRouter.ResolveSearchProvider(query);

        var providerResults = provider == MarketDataProvider.Yahoo
            ? await _yahooMarketDataProxy.LookupAsync(query, cancellationToken)
            : await _finnhubSearchProxy.SearchAsync(query, remainingLimit, cancellationToken);

        if ((providerResults is null || providerResults.Count == 0) &&
            provider == MarketDataProvider.Finnhub)
        {
            providerResults = await SearchMassiveFallback(query, remainingLimit, type, cancellationToken);
        }

        if (providerResults is null)
        {
            return localResults.Count > 0
                ? new SearchInstrumentsResponse(localResults)
                : null;
        }

        if (providerResults.Count == 0)
        {
            return new SearchInstrumentsResponse(localResults);
        }

        var idBySymbol = await SaveMissingInstruments(providerResults, cancellationToken);
        var localSymbolSet = localResults.Select(x => x.Symbol).ToHashSet(StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation(
            "Returning saved provider data without live quote enrichment for tickers: {Tickers}.",
            string.Join(", ", providerResults.Select(result => result.Symbol)));

        var remoteResults = providerResults
                .Where(ticker =>
                    !localSymbolSet.Contains(ticker.Symbol) &&
                    !string.IsNullOrWhiteSpace(ticker.Symbol) &&
                    !string.IsNullOrWhiteSpace(ticker.Name))
                .Select(ticker =>
                {
                    var symbol = ticker.Symbol.Trim().ToUpperInvariant();

                    return new SearchInstrumentResult(
                        idBySymbol[symbol],
                        symbol,
                        ticker.Name,
                        ticker.Cik,
                        ticker.Market,
                        ticker.ExchangeCode,
                        ticker.Currency,
                        ticker.Type,
                        null,
                        null);
                })
                .ToList();

        return new SearchInstrumentsResponse(localResults.Concat(remoteResults).Take(limit).ToList());
    }

    private async Task<List<SearchInstrumentResult>> SearchLocalDatabase(string query, int limit, CancellationToken cancellationToken)
    {
        var pattern = $"%{query}%";
        var startsWithPattern = $"{query}%";

        var instruments = await _db.Instruments
            .AsNoTracking()
            .Where(i =>
                EF.Functions.ILike(i.Symbol, pattern) ||
                EF.Functions.ILike(i.Name, pattern))
            .OrderBy(i =>
                EF.Functions.ILike(i.Symbol, startsWithPattern) ? 0 :
                EF.Functions.ILike(i.Name, startsWithPattern) ? 1 :
                2)
            .ThenBy(i => i.Symbol)
            .Take(limit)
            .Select(i => new
            {
                i.Id,
                i.Symbol,
                i.Name,
                i.Cik,
                i.Market,
                Exchange = i.ExchangeCode,
                i.Currency,
                i.Type,
                LatestBar = _db.MarketDataBars
                    .Where(b => b.InstrumentId == i.Id &&
                                b.Period == MarketDataPeriod.Daily)
                    .OrderByDescending(b => b.Date)
                    .Select(b => new
                    {
                        b.Date,
                        b.Close
                    })
                    .FirstOrDefault()
            })
            .Select(x => new SearchInstrumentResult(
                x.Id,
                x.Symbol,
                x.Name,
                x.Cik,
                x.Market,
                x.Exchange,
                x.Currency,
                x.Type,
                x.LatestBar != null ? x.LatestBar.Close : null,
                x.LatestBar != null ? x.LatestBar.Date : null))
            .ToListAsync(cancellationToken);

        return instruments;
    }


    private async Task<IReadOnlyList<MarketDataInstrumentLookupResult>?> SearchMassiveFallback(
        string query,
        int limit,
        SearchInstrumentType type,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Calling Massive API as a fallback for query '{Query}'.",
            query);

        var massiveResponse = await _massiveSearchProxy.SearchAsync(query, limit, type, cancellationToken);

        return massiveResponse?.Results
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Ticker) &&
                !string.IsNullOrWhiteSpace(x.Name))
            .Select(x =>
            {
                var symbol = x.Ticker!.Trim().ToUpperInvariant();

                return new MarketDataInstrumentLookupResult(
                    Symbol: symbol,
                    Name: x.Name!,
                    ProviderSymbol: symbol,
                    Cik: null,
                    Market: x.Market,
                    ExchangeCode: x.PrimaryExchange,
                    Currency: x.CurrencyName,
                    Type: x.Type);
            })
            .ToList();
    }

    private async Task<Dictionary<string, int>> SaveMissingInstruments(
        IReadOnlyList<MarketDataInstrumentLookupResult> providerResults,
        CancellationToken cancellationToken)
    {
        var validResults = providerResults
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Symbol) &&
                !string.IsNullOrWhiteSpace(x.Name))
            .ToList();

        var symbols = validResults
            .Select(x => x.Symbol.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var existingSymbols = await _db.Instruments
            .Where(i => symbols.Contains(i.Symbol))
            .Select(i => i.Symbol)
            .ToListAsync(cancellationToken);

        var existingSymbolSet = existingSymbols.ToHashSet();

        foreach (var result in validResults)
        {
            var symbol = result.Symbol.Trim().ToUpperInvariant();

            if (existingSymbolSet.Contains(symbol))
            {
                continue;
            }

            var instrument = Instrument.Create(
                symbol: symbol,
                name: result.Name,
                providerSymbol: result.ProviderSymbol,
                cik: result.Cik,
                market: result.Market,
                exchangeCode: result.ExchangeCode,
                currency: result.Currency,
                type: result.Type);


            _logger.LogInformation(
                "Added instrument from provider. Symbol: {Symbol}, Name: {Name}, Market: {Market}, ExchangeCode: {ExchangeCode}, Currency: {Currency}, Type: {Type}",
                symbol,
                result.Name,
                result.Market,
                result.ExchangeCode,
                result.Currency,
                result.Type);

            _db.Instruments.Add(instrument);
            existingSymbolSet.Add(symbol);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await _db.Instruments
            .Where(i => symbols.Contains(i.Symbol))
            .Select(i => new { i.Symbol, i.Id })
            .ToDictionaryAsync(x => x.Symbol, x => x.Id, cancellationToken);
    }
}
