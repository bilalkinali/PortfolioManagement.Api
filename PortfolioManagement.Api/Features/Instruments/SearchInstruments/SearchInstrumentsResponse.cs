namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed record SearchInstrumentsResponse(
    IReadOnlyCollection<SearchInstrumentResult> Results);

public sealed record SearchInstrumentResult(
    string Symbol,
    string Name,
    string? Exchange,
    string? Currency,
    string? Market,
    string? Type,
    bool Active);
