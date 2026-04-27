namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed record SearchInstrumentsRequest
{
    public string Query { get; init; } = null!;
    public int? Limit { get; init; }
    public SearchInstrumentType? Type { get; init; }
}

public enum SearchInstrumentType
{
    All,
    CS,
    ADRC,
    ADRP,
    ADRR,
    UNIT,
    RIGHT,
    PFD,
    FUND,
    SP,
    WARRANT,
    INDEX,
    ETF,
    ETN,
    OS,
    GDR,
    OTHER,
    NYRS,
    AGEN,
    EQLK,
    BOND,
    ADRW,
    BASKET,
    LT
}
