namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public sealed record GetStockProfileRequest
{
    public string Ticker { get; init; } = null!;
    public string? Date { get; init; }
}
