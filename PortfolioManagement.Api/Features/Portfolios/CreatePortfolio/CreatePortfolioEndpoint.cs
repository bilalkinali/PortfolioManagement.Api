using Microsoft.AspNetCore.Identity;
using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public static class CreatePortfolioEndpoint
{
    public static void MapCreatePortfolioEndpoints(this WebApplication app)
    {
        app.MapPost("/api/portfolios", async (
            CreatePortfolioHandler createPortfolioHandler, 
            CreatePortfolioRequest request,
            UserManager<AppUser> userManager) => // temporary
        {
            // Get userId from the authenticated user context.
            // For simplicity, hardcoded userId for now.
            //var userId = "100"; // Replace with actual user ID retrieval logic.
            //var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

            try
            {
                var user = await userManager.FindByEmailAsync("test@test.com");

                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var response = await createPortfolioHandler.Handle(request, user.Id);
                
                return Results.Created($"/api/portfolios/{response.Id}", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.Problem("Something went wrong");
            }
        });
    }
}