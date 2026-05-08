using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Trades.EditTrade;

public static class EditTradeEndpoint
{
    public static void MapEditTradeEndpoint(this WebApplication app)
    {
        app.MapPut("/api/portfolios/{portfolioId:int}/positions/{positionId:int}/trades/{tradeId:int}", async (
            EditTradeHandler editTradeHandler,
            EditTradeRequest request,
            IValidator<EditTradeRequest> validator,
            ClaimsPrincipal user,
            int portfolioId,
            int positionId,
            int tradeId) =>
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

public class EditTradeValidator : AbstractValidator<EditTradeRequest>
{
    public EditTradeValidator()
    {
        RuleFor(x => x.Quantity)
            .NotEqual(0);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.ExecutedDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

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
