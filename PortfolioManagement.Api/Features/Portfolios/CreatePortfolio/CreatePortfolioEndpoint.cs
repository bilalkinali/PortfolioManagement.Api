namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public static class CreatePortfolioEndpoint
{
    public static void MapCreatePortfolioEndpoints(this WebApplication app)
    {
        app.MapPost("/createportfolio", async (CreatePortfolioRequest request) =>
        {
            var response = await CreatePortfolioHandler.Handle(request);
            return Results.Ok(response); // Results.Created("location", response)?
        });
    }
}