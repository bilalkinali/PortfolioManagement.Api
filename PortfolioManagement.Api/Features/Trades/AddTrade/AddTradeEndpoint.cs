using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Auth;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Trades.AddTrade;

public static class AddTradeEndpoint
{
    public static void MapAddTradeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/portfolios/{portfolioId}/trades", async (
            AddTradeHandler addTradeHandler, 
            AddTradeRequest request,
            UserManager<AppUser> userManager, // temporary
            int portfolioId) =>
        {
            try
            {
                var user = await userManager.FindByEmailAsync("test@test.com");

                if (user == null)
                {
                    return Results.Unauthorized();
                }

                await addTradeHandler.Handle(request, portfolioId, user.Id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.Problem("Something went wrong");
            }
        });
    }
}

public record AddTradeRequest
{
}

public record AddTradeResponse
{
}

public class AddTradeHandler(PortfolioDbContext db)
{
    public async Task Handle(AddTradeRequest request, int portfolioId, string userId)
    {
        var portfolio = await db.Portfolios
            .Include(port => port.Positions)
            .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(port => port.Id == portfolioId);

        if (portfolio == null || portfolio.UserId != userId)
        {
            throw new Exception("Portfolio not found or user does not have access");
        }

        portfolio.AddTrade("AAPL", 10, 150.00m, DateOnly.FromDateTime(DateTime.UtcNow));

        await db.SaveChangesAsync();
    }

}