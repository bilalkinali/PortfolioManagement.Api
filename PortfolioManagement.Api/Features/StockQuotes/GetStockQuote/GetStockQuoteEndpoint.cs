using FluentValidation;

namespace PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;

public static class GetStockQuoteEndpoint
{
    public static void MapGetStockQuoteEndpoint(this WebApplication app)
    {
        app.MapGet("/api/instruments/{ticker}/quote", async (
            [AsParameters] GetStockQuoteRequest request,
            IValidator<GetStockQuoteRequest> validator,
            GetStockQuoteHandler getStockQuoteHandler,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var result = await getStockQuoteHandler.Handle(request, cancellationToken);

                return result is null
                    ? Results.NotFound($"No quote found for {request.Ticker}.")
                    : Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        });
    }
}
