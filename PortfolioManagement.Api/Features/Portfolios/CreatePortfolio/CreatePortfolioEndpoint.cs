using System.Security.Claims;
using FluentValidation;

namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public static class CreatePortfolioEndpoint
{
    public static void MapCreatePortfolioEndpoint(this WebApplication app)
    {
        app.MapPost("/api/portfolios", async (
            CreatePortfolioHandler createPortfolioHandler,
            CreatePortfolioRequest request,
            IValidator<CreatePortfolioRequest> validator,
            ClaimsPrincipal user) =>
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

                var response = await createPortfolioHandler.Handle(request, userId);

                return Results.Created($"/api/portfolios/{response.Id}", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        }).RequireAuthorization();
    }
}
