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
    private static readonly Dictionary<string, string> KnownAliasSymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        ["novo"] = "NOVO-B.CO",
        ["novo nordisk"] = "NOVO-B.CO"
    };

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

        var aliasSymbol = ResolveKnownAlias(query);
        var hasExactLocalSymbol = localResults.Any(x => IsExactSymbolMatch(query, x.Symbol));

        if (hasExactLocalSymbol && aliasSymbol is null)
        {
            return new SearchInstrumentsResponse(
                RankResults(query, localResults, aliasSymbol, localResults.Select(x => x.Symbol)).Take(limit).ToList());
        }

        var remainingLimit = Math.Max(limit - localResults.Count, 1);
        var provider = _providerRouter.ResolveSearchProvider(query);
        IReadOnlyList<MarketDataInstrumentLookupResult>? aliasResults = null;
        IReadOnlyList<MarketDataInstrumentLookupResult>? providerResults = null;

        if (aliasSymbol is not null)
        {
            aliasResults = await _yahooMarketDataProxy.LookupAsync(aliasSymbol, cancellationToken);
        }

        var shouldSearchProvider = localResults.Count < limit || !HasStrongLocalResult(query, localResults);

        if (shouldSearchProvider)
        {
            providerResults = provider == MarketDataProvider.Yahoo
                ? await _yahooMarketDataProxy.LookupAsync(query, cancellationToken)
                : await _finnhubSearchProxy.SearchAsync(query, remainingLimit, cancellationToken);

            if ((providerResults is null || providerResults.Count == 0) &&
                provider == MarketDataProvider.Finnhub)
            {
                providerResults = await SearchMassiveFallback(query, remainingLimit, type, cancellationToken);
            }
        }

        var combinedProviderResults = CombineProviderResults(aliasResults, providerResults);

        if (combinedProviderResults.Count == 0)
        {
            return localResults.Count > 0
                ? new SearchInstrumentsResponse(
                    RankResults(query, localResults, aliasSymbol, localResults.Select(x => x.Symbol)).Take(limit).ToList())
                : null;
        }

        var idBySymbol = await SaveMissingInstruments(combinedProviderResults, cancellationToken);
        var localSymbolSet = localResults.Select(x => x.Symbol).ToHashSet(StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation(
            "Returning saved provider data without live quote enrichment for tickers: {Tickers}.",
            string.Join(", ", combinedProviderResults.Select(result => result.Symbol)));

        var remoteResults = combinedProviderResults
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

        return new SearchInstrumentsResponse(
            RankResults(query, localResults.Concat(remoteResults), aliasSymbol, localSymbolSet).Take(limit).ToList());
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

    private static string? ResolveKnownAlias(string query)
    {
        var normalized = NormalizeSearchText(query);

        return KnownAliasSymbols.TryGetValue(normalized, out var symbol)
            ? symbol
            : null;
    }

    private static IReadOnlyList<MarketDataInstrumentLookupResult> CombineProviderResults(
        IReadOnlyList<MarketDataInstrumentLookupResult>? aliasResults,
        IReadOnlyList<MarketDataInstrumentLookupResult>? providerResults)
    {
        return (aliasResults ?? [])
            .Concat(providerResults ?? [])
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Symbol) &&
                !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Symbol.Trim().ToUpperInvariant())
            .Select(x => x.First())
            .ToList();
    }

    private static List<SearchInstrumentResult> RankResults(
        string query,
        IEnumerable<SearchInstrumentResult> results,
        string? aliasSymbol,
        IEnumerable<string> localSymbols)
    {
        var localSymbolSet = localSymbols.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return results
            .GroupBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => GetResultScore(query, x, aliasSymbol, localSymbolSet.Contains(x.Symbol)))
            .ThenBy(x => x.Symbol)
            .ToList();
    }

    private static int GetResultScore(
        string query,
        SearchInstrumentResult result,
        string? aliasSymbol,
        bool isLocalResult)
    {
        var score = GetMatchScore(query, result);

        if (isLocalResult)
        {
            score -= 5;
        }

        if (aliasSymbol is not null && IsExactSymbolMatch(aliasSymbol, result.Symbol))
        {
            score -= 45;
        }

        score += GetExchangeScore(result);

        return score;
    }

    private static int GetMatchScore(string query, SearchInstrumentResult result)
    {
        var normalizedQuery = NormalizeSearchText(query);
        var normalizedSymbol = NormalizeSearchText(result.Symbol);
        var normalizedName = NormalizeSearchText(result.Name);

        if (normalizedSymbol == normalizedQuery)
        {
            return 0;
        }

        if (normalizedName == normalizedQuery)
        {
            return 10;
        }

        if (normalizedName.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 30;
        }

        if (normalizedSymbol.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 40;
        }

        if (normalizedSymbol.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 60;
        }

        if (normalizedName.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 70;
        }

        return 100;
    }

    private static int GetExchangeScore(SearchInstrumentResult result)
    {
        var market = NormalizeSearchText(result.Market ?? string.Empty);
        var exchange = NormalizeSearchText(result.ExchangeCode ?? string.Empty);
        var symbol = result.Symbol.Trim().ToUpperInvariant();
        var type = NormalizeSearchText(result.Type ?? string.Empty);
        var combined = $"{market} {exchange} {type}";

        var score = 0;

        if (combined.Contains("OTC", StringComparison.Ordinal) ||
            symbol.EndsWith('F') ||
            symbol.EndsWith('Y'))
        {
            score += 45;
        }

        if (combined.Contains("ADR", StringComparison.Ordinal))
        {
            score += 25;
        }

        return score;
    }

    private static bool HasStrongLocalResult(string query, IEnumerable<SearchInstrumentResult> localResults)
        => localResults.Any(result => GetMatchScore(query, result) <= 30);

    private static bool IsExactSymbolMatch(string query, string symbol)
        => string.Equals(
            query.Trim(),
            symbol.Trim(),
            StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSearchText(string value)
        => value.Trim().ToUpperInvariant();
}
