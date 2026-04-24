using System.Security.Claims;

namespace PortfolioManagement.Api.Features.Portfolios.DeletePortfolio;

public static class DeletePortfolioEndpoint
{
    public static void MapDeletePortfolioEndpoint(this WebApplication app)
        {
        app.MapDelete("/api/portfolios/{portfolioId:int}", async (
            int portfolioId,
            DeletePortfolioHandler deletePortfolioHandler,
            ClaimsPrincipal user) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                await deletePortfolioHandler.Handle(portfolioId, userId);

                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        }).RequireAuthorization();
    }
}
