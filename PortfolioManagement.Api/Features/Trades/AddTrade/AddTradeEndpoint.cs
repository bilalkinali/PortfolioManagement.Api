using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Api.Shared.Events;

namespace PortfolioManagement.Api.Features.Trades.AddTrade;

public static class AddTradeEndpoint
{
    public static void MapAddTradeEndpoint(this WebApplication app)
    {
        app.MapPost("/api/portfolios/{portfolioId}/trades", async (
            AddTradeHandler addTradeHandler, 
            AddTradeRequest request,
            ClaimsPrincipal user,
            int portfolioId) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                await addTradeHandler.HandleAsync(request, portfolioId, userId);
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

public record AddTradeRequest(int InstrumentId, int Quantity, decimal Price, DateOnly ExecutedDate);


public class AddTradeHandler
{
    private readonly PortfolioDbContext _dbContext;
    private readonly DomainEventDispatcher _domainEventDispatcher;

    public AddTradeHandler(PortfolioDbContext dbContext, DomainEventDispatcher domainEventDispatcher)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task HandleAsync(AddTradeRequest request, int portfolioId, string userId)
    {
        var instrument = await _dbContext.Instruments
            .FirstOrDefaultAsync(i => i.Id == request.InstrumentId);

        if (instrument is null)
        {
            throw new Exception("Instrument not found");
        }

        var portfolio = await _dbContext.Portfolios
            .Include(port => port.Positions)
            .ThenInclude(pos => pos.Trades)
            .FirstOrDefaultAsync(
                port => port.Id == portfolioId &&
                port.UserId == userId);

        if (portfolio is null)
        {
            throw new InvalidOperationException("Portfolio not found");
        }

        var trade = portfolio.AddTrade(instrument.Id, request.Quantity, request.Price, request.ExecutedDate);

        await _dbContext.SaveChangesAsync();
    }

}