using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfolio;

public static class GetPortfolioEndpoint
{
    public static void MapGetPortfolioEndpoint(this WebApplication app)
    {
        app.MapGet("/api/portfolios/{portfolioId:int}", async (
            int portfolioId,
            GetPortfolioQuery query,
            ClaimsPrincipal user) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var portfolio = await query.GetPortfolioAsync(portfolioId, userId);

                return Results.Ok(portfolio);
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

public class GetPortfolioQuery(PortfolioDbContext db)
{
    public async Task<GetPortfolioResponse> GetPortfolioAsync(int portfolioId, string userId)
    {
        var portfolio = await db.Portfolios
            .AsNoTracking()
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == portfolioId);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }

        var getPortfolioResponse = new GetPortfolioResponse
        (
            portfolio.Id,
            portfolio.Name,
            portfolio.Description,
            portfolio.CreatedAt,
            portfolio.Positions.Select(p => new GetPortfolioPositionResponse(
                p.Id,
                p.Symbol,
                p.Quantity,
                p.AvgCost,
                p.RealizedPnL,
                p.Status,
                p.OpenDate,
                p.CloseDate,
                p.InstrumentId,
                p.Trades.Select(pos => new GetPortfolioTradeResponse(
                    pos.Id,
                    pos.IsBuy,
                    pos.Quantity,
                    pos.Price,
                    pos.ExecutedDate)).ToList()
                ))
                .ToList()
        );

        return getPortfolioResponse;
    }
}

public record GetPortfolioResponse
(
    int Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<GetPortfolioPositionResponse> Positions
);

public record GetPortfolioPositionResponse
(
    int Id,
    string Symbol,
    int Quantity,
    decimal AvgCost,
    decimal RealizedPnL,
    string Status,
    DateOnly OpenDate,
    DateOnly? CloseDate,
    int? InstrumentId,
    IReadOnlyCollection<GetPortfolioTradeResponse> Trades
);

public record GetPortfolioTradeResponse
(
    int Id, 
    bool IsBuy, 
    int Quantity, 
    decimal Price, 
    DateOnly ExecutedDate
);
