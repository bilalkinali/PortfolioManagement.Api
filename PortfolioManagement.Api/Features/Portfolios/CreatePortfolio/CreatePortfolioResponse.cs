namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public record CreatePortfolioResponse(int Id, string Name, string? Description, DateTimeOffset CreatedAt);