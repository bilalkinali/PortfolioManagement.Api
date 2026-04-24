using FluentValidation;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public static class GetStockHistoryEndpoint
{
    public static void MapGetStockHistoryEndpoint(this WebApplication app)
    {
        app.MapGet("/stocks/{ticker}/history", async (
            [AsParameters] GetStockHistoryRequest request,
            IValidator<GetStockHistoryRequest> validator,
            GetStockHistoryHandler getStockHistoryHandler) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var result = await getStockHistoryHandler.Handle(request);

                return result is null
                    ? Results.NotFound($"No data found for {request.Ticker}.")
                    : Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        });
    }
}