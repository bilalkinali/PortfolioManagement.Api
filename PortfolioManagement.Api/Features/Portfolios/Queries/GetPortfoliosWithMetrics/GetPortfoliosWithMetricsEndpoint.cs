using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfoliosWithMetrics;

public static class GetPortfoliosWithMetricsEndpoint
{
    public static void MapGetPortfoliosWithMetricsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/portfolios/metrics", async (
            GetPortfoliosWithMetricsQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var portfolios = await query.GetPortfoliosWithMetricsAsync(userId, cancellationToken);

                return Results.Ok(portfolios);
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

public class GetPortfoliosWithMetricsQuery(PortfolioDbContext db)
{
    public async Task<IReadOnlyCollection<GetPortfoliosWithMetricsResponse>> GetPortfoliosWithMetricsAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        return await db.Portfolios
            .AsNoTracking()
            //.AsSplitQuery()
            .Where(p => p.UserId == userId)
            .Select(p => new GetPortfoliosWithMetricsResponse(
                p.Id,
                p.Name,
                p.Description,
                p.UserId,
                p.CreatedAt,
                p.Positions.Select(pos => new GetPositionsWithMetricsResponse(
                    pos.Id,
                    pos.Quantity,
                    pos.AverageCostBasis,
                    pos.RealizedPnL,
                    pos.Status,
                    pos.OpenDate,
                    pos.CloseDate,
                    pos.PortfolioId,
                    pos.InstrumentId,
                    pos.Trades.Select(t => new GetTradesWithMetricsResponse(
                        t.Id,
                        t.IsBuy,
                        t.Quantity,
                        t.Price,
                        t.ExecutedDate,
                        t.PositionId)
                    ).ToList()
                )).ToList()
            )).ToListAsync(cancellationToken);
    }
}

public sealed record GetPortfoliosWithMetricsResponse(
    int Id, 
    string Name, 
    string? Description, 
    string UserId, 
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<GetPositionsWithMetricsResponse> Positions);

public sealed record GetPositionsWithMetricsResponse(
    int Id,
    int Quantity,
    decimal AverageCostBasis,
    decimal RealizedPnL,
    string Status,
    DateOnly OpenDate,
    DateOnly? CloseDate,
    int PortfolioId,
    int InstrumentId,
    IReadOnlyCollection<GetTradesWithMetricsResponse> Trades);

public sealed record GetTradesWithMetricsResponse(
    int Id,
    bool IsBuy,
    int Quantity,
    decimal Price,
    DateOnly ExecutedDate,
    int PositionId);
