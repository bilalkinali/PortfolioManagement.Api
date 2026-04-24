using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfolios;

public static class GetPortfoliosEndpoint
{
    public static void MapGetPortfoliosEndpoint(this WebApplication app)
    {
        app.MapGet("/api/portfolios/", async (
            GetPortfoliosQuery query,
            ClaimsPrincipal user) =>
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Results.Unauthorized();
                }

                var portfolios = await query.GetPortfoliosAsync(userId);

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

public class GetPortfoliosQuery(PortfolioDbContext db)
{
    public async Task<IReadOnlyCollection<GetPortfoliosResponse>> GetPortfoliosAsync(string userId)
    {
        return await db.Portfolios
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new GetPortfoliosResponse(
                p.Id,
                p.Name,
                p.Description,
                p.UserId,
                p.CreatedAt
            ))
            .ToListAsync();
    }
}

public sealed record GetPortfoliosResponse(
    int Id, 
    string Name, 
    string? Description, 
    string UserId, 
    DateTimeOffset CreatedAt);