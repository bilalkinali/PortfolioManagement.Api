using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed class SearchInstrumentsHandler
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public SearchInstrumentsHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Massive");
    }

    public async Task<SearchInstrumentsResponse?> Handle(SearchInstrumentsRequest request)
    {
        var query = request.Query.Trim();
        var limit = request.Limit ?? 10;
        var type = request.Type ?? SearchInstrumentType.CS;

        var queryParameters = new List<string>
        {
            $"search={Uri.EscapeDataString(query)}",
            "market=stocks",
            "active=true",
            $"limit={limit}",
            "sort=ticker",
            "order=asc"
        };

        if (type != SearchInstrumentType.All)
        {
            queryParameters.Add($"type={type}");
        }

        var url = $"/v3/reference/tickers?{string.Join("&", queryParameters)}";

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var massiveResponse = JsonSerializer.Deserialize<MassiveTickerResponse>(content, JsonSerializerOptions);

        if (massiveResponse is null)
        {
            return null;
        }

        var results = massiveResponse.Results
            .Where(ticker => !string.IsNullOrWhiteSpace(ticker.Ticker) && !string.IsNullOrWhiteSpace(ticker.Name))
            .Select(ticker => new SearchInstrumentResult(
                ticker.Ticker!,
                ticker.Name!,
                ticker.PrimaryExchange,
                ticker.CurrencyName,
                ticker.Market,
                ticker.Type,
                ticker.Active))
            .ToList();

        return new SearchInstrumentsResponse(results);
    }

    private sealed record MassiveTickerResponse
    {
        [JsonPropertyName("results")]
        public List<MassiveTickerResult> Results { get; init; } = [];
    }

    private sealed record MassiveTickerResult
    {
        [JsonPropertyName("active")]
        public bool Active { get; init; }

        [JsonPropertyName("currency_name")]
        public string? CurrencyName { get; init; }

        [JsonPropertyName("market")]
        public string? Market { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("primary_exchange")]
        public string? PrimaryExchange { get; init; }

        [JsonPropertyName("ticker")]
        public string? Ticker { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
