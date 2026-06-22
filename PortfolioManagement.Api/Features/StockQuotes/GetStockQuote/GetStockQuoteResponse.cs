namespace PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;

public sealed record GetStockQuoteResponse(
    string Symbol,
    decimal CurrentPrice,
    decimal? PreviousClose,
    decimal? Open,
    decimal? High,
    decimal? Low,
    long? Volume,
    DateTimeOffset? TimestampUtc,
    DateOnly? PriceDate,
    string? Currency,
    string Source,
    DateTimeOffset CachedAtUtc);
