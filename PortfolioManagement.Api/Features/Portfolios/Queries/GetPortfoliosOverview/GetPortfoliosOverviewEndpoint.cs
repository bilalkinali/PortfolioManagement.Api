using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;
using System.Security.Claims;

namespace PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfoliosOverview;

public static class GetPortfoliosOverviewEndpoint
{
    public static void MapGetPortfoliosWithMetricsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/portfolios/overview", async (
            GetPortfoliosOverviewQuery query,
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

                var portfolios = await query.GetPortfoliosOverviewAsync(userId, cancellationToken);

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

public class GetPortfoliosOverviewQuery(PortfolioDbContext db)
{
    public async Task<IReadOnlyCollection<GetPortfoliosOverviewResponse>> GetPortfoliosOverviewAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var portfolios = await db.Portfolios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Trades)
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Instrument)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var instrumentIds = portfolios
            .SelectMany(p => p.Positions)
            .Select(p => p.InstrumentId)
            .Distinct()
            .ToList();

        var latestPrices = await db.MarketDataBars
            .AsNoTracking()
            .Where(m => instrumentIds.Contains(m.InstrumentId))
            .GroupBy(m => m.InstrumentId)
            .Select(g => g
                .OrderByDescending(m => m.Date)
                .Select(m => new
                {
                    m.InstrumentId,
                    LatestPrice = m.Close
                })
                .First())
            .ToDictionaryAsync(x => x.InstrumentId, x => x.LatestPrice, cancellationToken);

        return portfolios
            .Select(portfolio =>
            {
                var positionValues = portfolio.Positions
                    .Select(position =>
                    {
                        var hasLatestPrice = latestPrices.TryGetValue(position.InstrumentId, out var latestPrice);

                        var costBasis = Math.Abs(position.Quantity) * position.AverageCostBasis;

                        decimal? marketValue = hasLatestPrice
                            ? position.Quantity * latestPrice
                            : null;

                        decimal? unrealizedPnL = hasLatestPrice
                            ? CalculateUnrealizedPnL(position.Quantity, position.AverageCostBasis, latestPrice)
                            : null;

                        decimal? unrealizedPnLPercentage = costBasis > 0 && unrealizedPnL is not null
                            ? unrealizedPnL.Value / costBasis * 100
                            : null;

                        return new PortfolioPositionSummaryValue(
                            position.Id,
                            position.InstrumentId,
                            position.Instrument.Symbol,
                            position.Instrument.Name,
                            position.Instrument.Currency,
                            position.Quantity,
                            position.AverageCostBasis,
                            position.RealizedPnL,
                            hasLatestPrice ? latestPrice : null,
                            costBasis,
                            marketValue,
                            unrealizedPnL,
                            unrealizedPnLPercentage,
                            position.Status);
                    })
                    .ToList();

                var totalCostBasis = positionValues.Sum(p => p.CostBasis);
                var totalMarketValue = positionValues.Sum(p => p.MarketValue ?? 0);
                var totalUnrealizedPnL = positionValues.Sum(p => p.UnrealizedPnL ?? 0);
                var totalRealizedPnL = positionValues.Sum(p => p.RealizedPnL);
                var totalPnL = totalRealizedPnL + totalUnrealizedPnL;
                var missingPricePositionCount = positionValues.Count(p => p.Quantity != 0 && p.LatestPrice is null);

                var totalPnLPercentage = totalCostBasis > 0
                    ? totalPnL / totalCostBasis * 100
                    : 0;

                var positions = positionValues
                    .Select(position => new PortfolioPositionSummaryResponse(
                        position.PositionId,
                        position.InstrumentId,
                        position.Symbol,
                        position.Name,
                        position.Currency,
                        position.Quantity,
                        position.AverageCostBasis,
                        position.RealizedPnL,
                        position.LatestPrice,
                        position.CostBasis,
                        position.MarketValue,
                        position.UnrealizedPnL,
                        position.UnrealizedPnLPercentage,
                        CalculateAllocationPercentage(position.Quantity, position.MarketValue, totalMarketValue),
                        position.Status))
                    .ToList();

                return new GetPortfoliosOverviewResponse(
                    portfolio.Id,
                    portfolio.Name,
                    portfolio.Description,
                    portfolio.CreatedAt,
                    portfolio.Positions.Count,
                    portfolio.Positions.Count(p => p.Quantity != 0),
                    totalCostBasis,
                    totalMarketValue,
                    totalUnrealizedPnL,
                    totalRealizedPnL,
                    totalPnL,
                    totalPnLPercentage,
                    missingPricePositionCount,
                    positions);
            })
            .ToList();
    }

    private static decimal CalculateUnrealizedPnL(
        int quantity,
        decimal averageCostBasis,
        decimal latestPrice)
    {
        if (quantity > 0)
        {
            return quantity * (latestPrice - averageCostBasis);
        }

        if (quantity < 0)
        {
            return Math.Abs(quantity) * (averageCostBasis - latestPrice);
        }

        return 0;
    }

    private static decimal? CalculateAllocationPercentage(int quantity, decimal? marketValue, decimal totalMarketValue)
    {
        if (quantity == 0 || marketValue is null || totalMarketValue == 0)
        {
            return null;
        }

        return marketValue.Value / totalMarketValue * 100;
    }

    private sealed record PortfolioPositionSummaryValue(
        int PositionId,
        int InstrumentId,
        string Symbol,
        string Name,
        string? Currency,
        int Quantity,
        decimal AverageCostBasis,
        decimal RealizedPnL,
        decimal? LatestPrice,
        decimal CostBasis,
        decimal? MarketValue,
        decimal? UnrealizedPnL,
        decimal? UnrealizedPnLPercentage,
        string Status
    );
}

public sealed record GetPortfoliosOverviewResponse(
    int Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    int PositionCount,
    int OpenPositionCount,
    decimal TotalCostBasis,
    decimal TotalMarketValue,//
    decimal TotalUnrealizedPnL,//
    decimal TotalRealizedPnL,
    decimal TotalPnL,//
    decimal TotalPnLPercentage,//
    int MissingPricePositionCount,
    IReadOnlyCollection<PortfolioPositionSummaryResponse> Positions
);

public sealed record PortfolioPositionSummaryResponse(
    int PositionId,
    int InstrumentId,
    string Symbol,
    string Name,
    string? Currency,
    int Quantity,
    decimal AverageCostBasis,
    decimal RealizedPnL,
    decimal? LatestPrice,//
    decimal CostBasis,
    decimal? MarketValue,//
    decimal? UnrealizedPnL,//
    decimal? UnrealizedPnLPercentage,//
    decimal? AllocationPercentage,
    string Status
);
