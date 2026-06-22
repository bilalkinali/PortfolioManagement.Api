using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.Trades.AddTrade;
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

                if (userId is null)
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

public sealed class GetPortfolioQuery(PortfolioDbContext db)
{
    public async Task<GetPortfolioResponse> GetPortfolioAsync(int portfolioId, string userId)
    {
        var portfolio = await db.Portfolios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Instrument)
            .Include(p => p.Positions)
                .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == portfolioId);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }

        var instrumentIds = portfolio.Positions
            .Select(p => p.InstrumentId)
            .Distinct()
            .ToList();

        var latestBars = await db.MarketDataBars
            .AsNoTracking()
            .Where(b => instrumentIds.Contains(b.InstrumentId))
            .GroupBy(b => b.InstrumentId)
            .Select(g => g
                .OrderByDescending(b => b.Date)
                .Select(b => new
                {
                    b.InstrumentId,
                    LatestPrice = b.Close,
                    LatestPriceDate = b.Date
                })
                .First())
            .ToDictionaryAsync(b => b.InstrumentId);

        var positionValues = portfolio.Positions
            .Select(position =>
            {
                latestBars.TryGetValue(position.InstrumentId, out var latestBar);

                var hasLatestPrice = latestBar is not null;
                var latestPrice = latestBar?.LatestPrice;

                var costBasis = Math.Abs(position.Quantity) * position.AverageCostBasis;

                decimal? marketValue = hasLatestPrice
                    ? position.Quantity * latestPrice!.Value
                    : null;

                decimal? unrealizedPnL = hasLatestPrice
                    ? CalculateUnrealizedPnL(
                        position.Quantity,
                        position.AverageCostBasis,
                        latestPrice!.Value)
                    : null;

                decimal? unrealizedPnLPercentage = costBasis > 0 && unrealizedPnL is not null
                    ? unrealizedPnL.Value / costBasis * 100
                    : null;

                var realizedGainByTradeId = CalculateTradeRealizedGains(position.Trades);

                var trades = position.Trades
                    .OrderByDescending(t => t.ExecutedDate)
                    .Select(t => ToTradeResponse(t, realizedGainByTradeId))
                    .ToList();

                return new PortfolioPositionValue(
                    position.Id,
                    position.InstrumentId,
                    position.Instrument.Symbol,
                    position.Instrument.Name,
                    position.Instrument.Currency,
                    position.Quantity,
                    position.AverageCostBasis,
                    position.RealizedPnL,
                    latestPrice,
                    latestBar?.LatestPriceDate,
                    costBasis,
                    marketValue,
                    unrealizedPnL,
                    unrealizedPnLPercentage,
                    position.Status,
                    trades);
            })
            .ToList();

        var totalCostBasis = positionValues.Sum(p => p.CostBasis);
        var totalMarketValue = positionValues.Sum(p => p.MarketValue ?? 0);
        var totalUnrealizedPnL = positionValues.Sum(p => p.UnrealizedPnL ?? 0);
        var totalRealizedPnL = positionValues.Sum(p => p.RealizedPnL);
        var totalPnL = totalRealizedPnL + totalUnrealizedPnL;

        var totalPnLPercentage = totalCostBasis > 0
            ? totalPnL / totalCostBasis * 100
            : 0;

        var positions = positionValues
            .Select(position => new GetPortfolioPositionResponse(
                position.Id,
                position.InstrumentId,
                position.Symbol,
                position.Name,
                position.Currency,
                position.Quantity,
                position.AverageCostBasis,
                position.RealizedPnL,
                position.LatestPrice,
                position.LatestPriceDate,
                position.CostBasis,
                position.MarketValue,
                position.UnrealizedPnL,
                position.UnrealizedPnLPercentage,
                CalculateAllocationPercentage(position.Quantity, position.MarketValue, totalMarketValue),
                position.Status,
                position.Trades))
            .ToList();

        return new GetPortfolioResponse(
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
            positions);
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

    private static GetPortfolioPositionTradeResponse ToTradeResponse(
        Trade trade,
        IReadOnlyDictionary<int, TradeRealizedGain> realizedGainByTradeId)
    {
        realizedGainByTradeId.TryGetValue(trade.Id, out var realizedGain);

        return new GetPortfolioPositionTradeResponse(
            trade.Id,
            trade.IsBuy,
            trade.Quantity,
            trade.Price,
            trade.ExecutedDate,
            TradeType.FromQuantity(trade.Quantity),
            Math.Abs(trade.Quantity),
            trade.TradeValue,
            realizedGain?.Amount,
            realizedGain?.Percentage);
    }

    private static Dictionary<int, TradeRealizedGain> CalculateTradeRealizedGains(IEnumerable<Trade> trades)
    {
        var realizedGainByTradeId = new Dictionary<int, TradeRealizedGain>();
        var quantity = 0;
        var averageCostBasis = 0m;

        foreach (var trade in trades.OrderBy(t => t.ExecutedDate).ThenBy(t => t.Id))
        {
            if (quantity == 0)
            {
                quantity = trade.Quantity;
                averageCostBasis = trade.Price;
                continue;
            }

            var sameDirection =
                quantity > 0 && trade.Quantity > 0 ||
                quantity < 0 && trade.Quantity < 0;

            if (sameDirection)
            {
                var currentAbsQuantity = Math.Abs(quantity);
                var tradeAbsQuantity = Math.Abs(trade.Quantity);

                averageCostBasis =
                    ((currentAbsQuantity * averageCostBasis) + (tradeAbsQuantity * trade.Price))
                    / (currentAbsQuantity + tradeAbsQuantity);

                quantity += trade.Quantity;
                continue;
            }

            var closingQuantity = Math.Min(Math.Abs(quantity), Math.Abs(trade.Quantity));
            var amount = quantity > 0
                ? closingQuantity * (trade.Price - averageCostBasis)
                : closingQuantity * (averageCostBasis - trade.Price);
            var closedCostBasis = closingQuantity * averageCostBasis;
            var percentage = closedCostBasis > 0
                ? amount / closedCostBasis * 100
                : 0;

            realizedGainByTradeId[trade.Id] = new TradeRealizedGain(amount, percentage);

            var previousQuantity = quantity;
            quantity += trade.Quantity;

            if (quantity == 0)
            {
                averageCostBasis = 0m;
            }
            else if (Math.Abs(trade.Quantity) > Math.Abs(previousQuantity))
            {
                averageCostBasis = trade.Price;
            }
        }

        return realizedGainByTradeId;
    }

    private sealed record TradeRealizedGain(decimal Amount, decimal Percentage);

    private sealed record PortfolioPositionValue(
        int Id,
        int InstrumentId,
        string Symbol,
        string Name,
        string? Currency,
        int Quantity,
        decimal AverageCostBasis,
        decimal RealizedPnL,
        decimal? LatestPrice,
        DateOnly? LatestPriceDate,
        decimal CostBasis,
        decimal? MarketValue,
        decimal? UnrealizedPnL,
        decimal? UnrealizedPnLPercentage,
        string Status,
        IReadOnlyCollection<GetPortfolioPositionTradeResponse> Trades);
}

public sealed record GetPortfolioResponse(
    int Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    int PositionCount,
    int OpenPositionCount,
    decimal TotalCostBasis,
    decimal TotalMarketValue,
    decimal TotalUnrealizedPnL,
    decimal TotalRealizedPnL,
    decimal TotalPnL,
    decimal TotalPnLPercentage,
    IReadOnlyCollection<GetPortfolioPositionResponse> Positions
);

public sealed record GetPortfolioPositionResponse(
    int Id,
    int InstrumentId,
    string Symbol,
    string Name,
    string? Currency,
    int Quantity,
    decimal AverageCostBasis,
    decimal RealizedPnL,
    decimal? LatestPrice,
    DateOnly? LatestPriceDate,
    decimal CostBasis,
    decimal? MarketValue,
    decimal? UnrealizedPnL,
    decimal? UnrealizedPnLPercentage,
    decimal? AllocationPercentage,
    string Status,
    IReadOnlyCollection<GetPortfolioPositionTradeResponse> Trades
);

public sealed record GetPortfolioPositionTradeResponse(
    int Id,
    bool IsBuy,
    int Quantity,
    decimal Price,
    DateOnly ExecutedDate,
    string Type,
    int Shares,
    decimal TotalCost,
    decimal? RealizedGain,
    decimal? RealizedGainPercentage
);
