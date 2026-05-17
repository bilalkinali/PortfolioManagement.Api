using FluentValidation;

namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public static class GetStockProfileEndpoint
{
    public static void MapGetStockProfileEndpoint(this WebApplication app)
    {
        app.MapGet("/api/instruments/{ticker}/profile", async (
            [AsParameters] GetStockProfileRequest request,
            IValidator<GetStockProfileRequest> validator,
            GetStockProfileHandler getStockProfileHandler,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var result = await getStockProfileHandler.Handle(request, cancellationToken);

                return result is null
                    ? Results.NotFound($"No profile found for {request.Ticker}.")
                    : Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        });
    }
}
