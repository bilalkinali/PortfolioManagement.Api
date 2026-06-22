namespace PortfolioManagement.Api.Features.MarketData;

public sealed class MarketDataProviderRouter
{
    private static readonly string[] YahooSuffixes =
    [
        ".DE",
        ".PA",
        ".AS",
        ".L",
        ".ST",
        ".CO",
        ".MI",
        ".SW",
        ".TO",
        ".V",
        ".HK",
        ".AX"
    ];

    internal MarketDataProvider ResolveSearchProvider(string query)
        => IsYahooStyleSymbol(query) ? MarketDataProvider.Yahoo : MarketDataProvider.Finnhub;

    internal MarketDataProvider ResolveQuoteProvider(string symbol, string? exchangeCode = null)
        => IsYahooStyleSymbol(symbol) || IsKnownGlobalExchange(exchangeCode)
            ? MarketDataProvider.Yahoo
            : MarketDataProvider.Finnhub;

    internal MarketDataProvider ResolveHistoryProvider(string symbol, bool massiveConfigured, string? exchangeCode = null)
    {
        if (IsYahooStyleSymbol(symbol) || IsKnownGlobalExchange(exchangeCode))
        {
            return MarketDataProvider.Yahoo;
        }

        return massiveConfigured ? MarketDataProvider.Massive : MarketDataProvider.Finnhub;
    }

    internal bool IsYahooStyleSymbol(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return YahooSuffixes.Any(normalized.EndsWith);
    }

    private static bool IsKnownGlobalExchange(string? exchangeCode)
    {
        if (string.IsNullOrWhiteSpace(exchangeCode))
        {
            return false;
        }

        var normalized = exchangeCode.Trim().ToUpperInvariant();

        return normalized is not ("XNYS" or "XNAS" or "ARCX" or "BATS" or "NYSE" or "NASDAQ");
    }
}
