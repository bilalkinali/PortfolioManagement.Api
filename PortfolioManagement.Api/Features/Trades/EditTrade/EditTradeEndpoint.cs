using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Trades.EditTrade;

public static class EditTradeEndpoint
{
    public static void MapEditTradeEndpoint(this WebApplication app)
    {
        app.MapPut("/api/portfolios/{portfolioId}/positions/{positionId}/trades/{tradeId}", async (
            EditTradeHandler editTradeHandler,
            EditTradeRequest request,
            ClaimsPrincipal user,
            int portfolioId, 
            int positionId, 
            int tradeId) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                await editTradeHandler.HandleAsync(request, portfolioId, positionId, tradeId, userId);
                return Results.NoContent();
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
public sealed record EditTradeRequest(int Quantity, decimal Price, DateOnly ExecutedDate);

public class EditTradeHandler(PortfolioDbContext db)
{
    public async Task HandleAsync(
        EditTradeRequest request, 
        int portfolioId, 
        int positionId, 
        int tradeId, 
        string userId)
    {
        var portfolio = await db.Portfolios
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(
                p => p.Id == portfolioId &&
                p.UserId == userId);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found");
        }

        portfolio.EditTrade(positionId, tradeId, request.Quantity, request.Price, request.ExecutedDate, userId);
        
        await db.SaveChangesAsync();
    }
}