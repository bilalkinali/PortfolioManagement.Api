using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Features.Auth.Me;

public static class MeEndpoint
{
    public static void MapMeEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/me", async (
            MeHandler handler,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var result = await handler.Handle(userId);

                return result is null
                    ? Results.Unauthorized()
                    : Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }

        }).RequireAuthorization();
    }
    
}

public record MeResponse(string Email, string FirstName, string LastName);

public class MeHandler
{
    private readonly UserManager<AppUser> _userManager;

    public MeHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<MeResponse?> Handle(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        return new MeResponse(
            user.Email!,
            user.FirstName,
            user.LastName
        );
    }
}