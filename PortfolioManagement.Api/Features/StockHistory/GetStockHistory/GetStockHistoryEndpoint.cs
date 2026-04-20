using Microsoft.AspNetCore.Mvc;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public static class GetStockHistoryEndpoint
{
    public static void MapGetStockHistoryEndpoint(this WebApplication app)
    {
        app.MapGet("/stocks/{ticker}/history", async (
            string ticker,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string? timespan,
            GetStockHistoryHandler getStockHistoryHandler) =>
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                return Results.BadRequest("Query parameters 'from' and 'to' are required.");
            }

            var request = new GetStockHistoryRequest(
                ticker.ToUpper(),
                from,
                to,
                timespan ?? "day"
            );

            var result = await getStockHistoryHandler.Handle(request);

            return result is null
                ? Results.NotFound($"No data found for {ticker}")
                : Results.Ok(result);

        });
    }
}

public sealed record GetStockHistoryRequest(
    string Ticker,
    string From,
    string To,
    string? Timespan); // "minute" | "hour" | "day" | "week" | "month"

public sealed record GetStockHistoryResponse(
    string Ticker,
    string From,
    string To,
    string Timespan,
    List<StockBar> Bars);

public sealed record StockBar(
    long Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);

public class GetStockHistoryHandler
{
    private readonly HttpClient _httpClient;

    public GetStockHistoryHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Massive");
    }

    public async Task<string?> Handle(GetStockHistoryRequest request)
    {
        var url = $"/v2/aggs/ticker/{request.Ticker}/range/1/{request.Timespan}/{request.From}/{request.To}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return content;

        //return System.Text.Json.JsonSerializer.Deserialize<GetStockHistoryResponse>(
        //    content, new System.Text.Json.JsonSerializerOptions
        //{
        //    PropertyNameCaseInsensitive = true
        //});
    }
}