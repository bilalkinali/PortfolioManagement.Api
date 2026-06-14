namespace PortfolioManagement.Api.Features.MarketData;

internal sealed record MarketDataInstrumentLookupResult(
    string Symbol,
    string Name,
    string? ProviderSymbol,
    int? Cik,
    string? Market,
    string? ExchangeCode,
    string? Currency,
    string? Type);

internal sealed record MarketDataQuote(
    string Symbol,
    string? ProviderSymbol,
    decimal CurrentPrice,
    decimal? PreviousClose,
    decimal? Open,
    decimal? High,
    decimal? Low,
    long? Volume,
    DateTimeOffset? TimestampUtc,
    string? Currency);

internal sealed record MarketDataHistoricalCandle(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);

internal sealed record MarketDataStockProfileSummary(
    string Ticker,
    bool Active,
    string? Cik,
    string? CurrencyName,
    string? Description,
    string? HomepageUrl,
    string? ListDate,
    string? Locale,
    string? Market,
    decimal? MarketCap,
    string? Name,
    string? PhoneNumber,
    string? PrimaryExchange,
    string? Type,
    long? WeightedSharesOutstanding,
    string? IconUrl,
    string? LogoUrl);
