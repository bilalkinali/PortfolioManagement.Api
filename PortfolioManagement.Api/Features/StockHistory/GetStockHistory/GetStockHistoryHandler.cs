using System.Text.Json;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed class GetStockHistoryHandler
{
    private readonly HttpClient _httpClient;

    public GetStockHistoryHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Massive");
    }

    public async Task<GetStockHistoryResponse?> Handle(GetStockHistoryRequest request)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();
        var from = DateOnly.Parse(request.From);
        var to = DateOnly.Parse(request.To);
        var timespan = string.IsNullOrWhiteSpace(request.Timespan)
            ? "day"
            : request.Timespan.Trim().ToLowerInvariant();

        // 1. Check your database here first.
        // If you already have the data for ticker/from/to/timespan, return it.
        //
        // Example:
        // var existing = await _dbContext.StockHistory...
        // if (existing is not null) return existing;

        var url = $"/v2/aggs/ticker/{ticker}/range/1/{timespan}/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}";

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<GetStockHistoryResponse>(content);

        if (result is null)
        {
            return null;
        }

        // 2. Save to your database here before returning.
        //
        // Example:
        // _dbContext.StockHistory.Add(...)
        // await _dbContext.SaveChangesAsync();

        return result;
    }
}