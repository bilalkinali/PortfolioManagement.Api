using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.MarketData.Finnhub;

public sealed class FinnhubProfileProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinnhubProfileProxy> _logger;

    public FinnhubProfileProxy(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<FinnhubProfileProxy> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("Finnhub");
        _logger = logger;
    }

    internal async Task<MarketDataStockProfileSummary?> GetProfileAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Finnhub:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Finnhub API key is not configured. Skipping Finnhub profile.");
            return null;
        }

        _logger.LogInformation("Calling Finnhub profile for symbol {Symbol}.", symbol);

        var url = $"/api/v1/stock/profile2?symbol={Uri.EscapeDataString(symbol)}&token={Uri.EscapeDataString(apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var profile = JsonSerializer.Deserialize<FinnhubProfileResponse>(content, JsonSerializerOptions);

        if (profile is null || string.IsNullOrWhiteSpace(profile.Ticker))
        {
            return null;
        }

        return new MarketDataStockProfileSummary(
            Ticker: profile.Ticker.Trim().ToUpperInvariant(),
            Active: true,
            Cik: null,
            CurrencyName: profile.Currency,
            Description: null,
            HomepageUrl: profile.WebUrl,
            ListDate: profile.Ipo,
            Locale: profile.Country,
            Market: "stocks",
            MarketCap: profile.MarketCapitalization,
            Name: profile.Name,
            PhoneNumber: profile.Phone,
            PrimaryExchange: profile.Exchange,
            Type: null,
            WeightedSharesOutstanding: ToWholeShares(profile.ShareOutstanding),
            IconUrl: profile.Logo,
            LogoUrl: profile.Logo);
    }

    private static long? ToWholeShares(decimal? sharesInMillions)
        => sharesInMillions is null ? null : decimal.ToInt64(sharesInMillions.Value * 1_000_000);

    private sealed record FinnhubProfileResponse(
        [property: JsonPropertyName("country")] string? Country,
        [property: JsonPropertyName("currency")] string? Currency,
        [property: JsonPropertyName("exchange")] string? Exchange,
        [property: JsonPropertyName("ipo")] string? Ipo,
        [property: JsonPropertyName("logo")] string? Logo,
        [property: JsonPropertyName("marketCapitalization")] decimal? MarketCapitalization,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("phone")] string? Phone,
        [property: JsonPropertyName("shareOutstanding")] decimal? ShareOutstanding,
        [property: JsonPropertyName("ticker")] string? Ticker,
        [property: JsonPropertyName("weburl")] string? WebUrl);
}
