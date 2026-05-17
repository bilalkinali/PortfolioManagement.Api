using System.Text.Json;

namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile.Proxy;

public class MassiveProfileProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<MassiveProfileProxy> _logger;

    public MassiveProfileProxy(IHttpClientFactory httpClientFactory, ILogger<MassiveProfileProxy> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Massive");
        _logger = logger;
    }
    public async Task<MassiveTickerOverviewResponse.TickerInfo?> GetProfileFromProxyAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var url = $"/v3/reference/tickers/{ticker}";

        _logger.LogInformation("Calling Massive API for ticker profile: {Ticker}", ticker);

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResult = JsonSerializer.Deserialize<MassiveTickerOverviewResponse>(
            content,
            JsonSerializerOptions);

        return apiResult?.TickerData;
    }
}