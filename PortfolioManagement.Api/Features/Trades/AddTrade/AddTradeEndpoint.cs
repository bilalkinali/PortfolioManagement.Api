using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Features.Trades.EditTrade;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Api.Shared.Events;

namespace PortfolioManagement.Api.Features.Trades.AddTrade;

public static class AddTradeEndpoint
{
    public static void MapAddTradeEndpoint(this WebApplication app)
    {
        app.MapPost("/api/portfolios/{portfolioId:int}/trades", async (
            AddTradeHandler addTradeHandler,
            AddTradeRequest request,
            IValidator<AddTradeRequest> validator,
            ClaimsPrincipal user,
            int portfolioId) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

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

public sealed record AddTradeRequest(
    int InstrumentId,
    string? Type,
    int? Shares,
    decimal Price,
    DateOnly ExecutedDate,
    int? Quantity = null)
{
    public int ToSignedQuantity()
    {
        if (TradeType.IsValid(Type) && Shares is > 0)
        {
            return TradeType.ToSignedQuantity(Type!, Shares.Value);
        }

        return Quantity!.Value;
    }
}

public class AddTradeValidator : AbstractValidator<AddTradeRequest>
{
    public AddTradeValidator()
    {
        RuleFor(x => x.InstrumentId)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(TradeType.HasValidTradeSize)
            .WithMessage("Provide Buy or Sell with positive shares.");

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.ExecutedDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

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

        portfolio.AddTrade(instrument.Id, request.ToSignedQuantity(), request.Price, request.ExecutedDate);

        await _dbContext.SaveChangesAsync();
    }

}

public static class TradeType
{
    public const string Buy = "Buy";
    public const string Sell = "Sell";

    public static bool IsValid(string? type)
    {
        return string.Equals(type, Buy, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(type, Sell, StringComparison.OrdinalIgnoreCase);
    }

    public static string FromQuantity(int quantity)
    {
        return quantity > 0 ? Buy : Sell;
    }

    public static int ToSignedQuantity(string type, int shares)
    {
        return string.Equals(type, Sell, StringComparison.OrdinalIgnoreCase)
            ? -shares
            : shares;
    }

    public static bool HasValidTradeSize(AddTradeRequest request)
    {
        if (IsValid(request.Type) && request.Shares is > 0)
        {
            return true;
        }

        return request.Quantity is not null and not 0;
    }

    public static bool HasValidTradeSize(EditTradeRequest request)
    {
        if (IsValid(request.Type) && request.Shares is > 0)
        {
            return true;
        }

        return request.Quantity is not null and not 0;
    }
}
