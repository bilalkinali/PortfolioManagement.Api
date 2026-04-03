namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public static class CreatePortfolioEndpoint
{
    public static void MapCreatePortfolioEndpoints(this WebApplication app)
    {
        app.MapPost("/api/portfolios", async (CreatePortfolioRequest request) =>
        {
            // Get userId from the authenticated user context.
            // For simplicity, hardcoded userId for now.
            var userId = new Random().Next(1, 1000).ToString(); // Replace with actual user ID retrieval logic.
            var response = await CreatePortfolioHandler.Handle(request, userId);
            return Results.Created("/api/portfolios/{response.Id}", response);
        });
    }
}