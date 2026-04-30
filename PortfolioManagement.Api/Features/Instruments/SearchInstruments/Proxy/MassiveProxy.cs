using System.Text.Json;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments.Proxy;

public class MassiveProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<MassiveProxy> _logger;
    private readonly HttpClient _httpClient;

    public MassiveProxy(IHttpClientFactory httpClientFactory, ILogger<MassiveProxy> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Massive");
    }

    public async Task<MassiveTickerResponse?> SearchAsync(string query, int limit, SearchInstrumentType type)
    {

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

        _logger.LogInformation("Calling Massive API: {Url}", url);

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<MassiveTickerResponse>(
            content,
            JsonSerializerOptions);
    }
}