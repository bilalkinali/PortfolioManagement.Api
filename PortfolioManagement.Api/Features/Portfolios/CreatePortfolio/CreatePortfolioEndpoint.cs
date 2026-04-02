namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public static class CreatePortfolioEndpoint
{
    public static void MapCreatePortfolioEndpoints(this WebApplication app)
    {
        app.MapPost("/api/portfolios", async (CreatePortfolioRequest request) =>
        {
            var response = await CreatePortfolioHandler.Handle(request);
            return Results.Created("/api/portfolios/{response.Id}", response);
        });
    }
}