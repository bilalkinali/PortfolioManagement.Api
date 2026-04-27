using FluentValidation;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public static class SearchInstrumentsEndpoint
{
    public static void MapSearchInstrumentsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/instruments/search", async (
            [AsParameters] SearchInstrumentsRequest request,
            IValidator<SearchInstrumentsRequest> validator,
            SearchInstrumentsHandler searchInstrumentsHandler) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var result = await searchInstrumentsHandler.Handle(request);

                return result is null
                    ? Results.InternalServerError("Server is unreachable at the moment.")
                    : Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        }); // .RequireAuthorization();
    }
}
