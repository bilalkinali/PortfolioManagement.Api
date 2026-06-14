using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.MarketData.Finnhub;

public sealed class FinnhubSearchProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinnhubSearchProxy> _logger;

    public FinnhubSearchProxy(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<FinnhubSearchProxy> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("Finnhub");
        _logger = logger;
    }

    internal async Task<IReadOnlyList<MarketDataInstrumentLookupResult>?> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Finnhub:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Finnhub API key is not configured. Skipping Finnhub search.");
            return null;
        }

        _logger.LogInformation("Calling Finnhub symbol search for query {Query}.", query);

        var url = $"/api/v1/search?q={Uri.EscapeDataString(query)}&token={Uri.EscapeDataString(apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var searchResponse = JsonSerializer.Deserialize<FinnhubSearchResponse>(content, JsonSerializerOptions);

        return searchResponse?.Results
            .Where(x => !string.IsNullOrWhiteSpace(x.Symbol))
            .Take(limit)
            .Select(x =>
            {
                var symbol = x.Symbol!.Trim().ToUpperInvariant();
                var name = string.IsNullOrWhiteSpace(x.Description)
                    ? symbol
                    : x.Description.Trim();

                return new MarketDataInstrumentLookupResult(
                    Symbol: symbol,
                    Name: name,
                    ProviderSymbol: symbol,
                    Cik: null,
                    Market: "stocks",
                    ExchangeCode: null,
                    Currency: null,
                    Type: x.Type);
            })
            .ToList();
    }

    private sealed record FinnhubSearchResponse(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("result")] IReadOnlyList<FinnhubSearchResult> Results);

    private sealed record FinnhubSearchResult(
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("displaySymbol")] string? DisplaySymbol,
        [property: JsonPropertyName("symbol")] string? Symbol,
        [property: JsonPropertyName("type")] string? Type);
}
