namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed record SearchInstrumentsResponse(
    IReadOnlyCollection<SearchInstrumentResult> Results);

public sealed record SearchInstrumentResult(
    int Id,
    string Symbol,
    string Name,
    int? Cik,
    string? Market,
    string? ExchangeCode,
    string? Currency,
    string? Type,
    decimal? LatestPrice,
    DateOnly? LatestPriceDate);
