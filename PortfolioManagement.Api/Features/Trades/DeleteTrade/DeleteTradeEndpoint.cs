using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Trades.DeleteTrade;

public static class DeleteTradeEndpoint
{
    public static void MapDeleteTradeEndpoint(this WebApplication app)
    {
        app.MapDelete("/api/portfolios/{portfolioId:int}/positions/{positionId:int}/trades/{tradeId:int}", async (
            DeleteTradeHandler deleteTradeHandler,
            CancellationToken cancellationToken,
            ClaimsPrincipal user,
            int portfolioId, int positionId, int tradeId) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                await deleteTradeHandler.Handle(portfolioId, positionId, tradeId, userId, cancellationToken);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        }).RequireAuthorization();
    }
}

public class DeleteTradeHandler(PortfolioDbContext db)
{
    public async Task Handle(
        int portfolioId,
        int positionId,
        int tradeId,
        string userId,
        CancellationToken cancellationToken)
    {
        var portfolio = await db.Portfolios
            .Include(p => p.Positions)
            .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(
                p => p.Id == portfolioId &&
                     p.UserId == userId,
                cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }

        portfolio.DeleteTrade(positionId, tradeId);

        await db.SaveChangesAsync(cancellationToken);
    }
}
