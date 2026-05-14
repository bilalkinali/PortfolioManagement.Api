namespace PortfolioManagement.Api.Features.StockProfile.GetStockProfile;

public sealed class GetStockProfileRequest
{
    public string Ticker { get; init; } = null!;
    public string? Date { get; init; }
}
