namespace PortfolioManagement.Tools.ImportData.ImportHistoricData;

public sealed record StooqHistoricData
{
    public string Ticker { get; init; } = null!;
    public string Period { get; init; } = null!;
    public DateOnly Date { get; init; }
    public TimeOnly Time { get; init; }

    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }

    public long Volume { get; init; }
    public int OpenInterest { get; init; }
}