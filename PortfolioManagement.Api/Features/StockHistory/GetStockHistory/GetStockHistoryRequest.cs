namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed class GetStockHistoryRequest
{
    public string Ticker { get; init; } = null!;
    public string From { get; init; } = null!;
    public string To { get; init; } = null!;
    public string? Timespan { get; init; }
}