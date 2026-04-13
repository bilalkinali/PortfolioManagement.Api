using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Trades.AddTrade;

public static class AddTradeEndpoint
{
    public static void MapAddTradeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/portfolios/{portfolioId}/trades", async (
            AddTradeHandler addTradeHandler, 
            AddTradeRequest request,
            ClaimsPrincipal user,
            int portfolioId) =>
        {
            try
            {
                var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                await addTradeHandler.Handle(request, portfolioId, userId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.Problem("Something went wrong");
            }
        }).RequireAuthorization();
    }
}

public record AddTradeRequest(string Symbol, int Quantity, decimal Price, DateOnly ExecutedDate);


public class AddTradeHandler
{
    private readonly PortfolioDbContext _dbContext;

    public AddTradeHandler(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddTradeRequest request, int portfolioId, string userId)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(port => port.Positions)
            .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(port => port.Id == portfolioId);

        if (portfolio == null || portfolio.UserId != userId)
        {
            throw new Exception("Portfolio not found or user does not have access");
        }

        var trade = portfolio.AddTrade(request.Symbol, request.Quantity, request.Price, request.ExecutedDate);

        foreach (var prop in trade.GetType().GetProperties())
        {
            Console.WriteLine($"{prop.Name}: {prop.GetValue(trade)}");
        }

        await _dbContext.SaveChangesAsync();
    }

}