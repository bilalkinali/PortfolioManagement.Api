using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.MarketData.Finnhub;

public sealed class FinnhubQuoteProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinnhubQuoteProxy> _logger;

    public FinnhubQuoteProxy(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<FinnhubQuoteProxy> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("Finnhub");
        _logger = logger;
    }

    internal async Task<MarketDataQuote?> GetQuoteAsync(
        string symbol,
        string? currency,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Finnhub:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Finnhub API key is not configured. Skipping Finnhub quote.");
            return null;
        }

        _logger.LogInformation("Calling Finnhub quote for symbol {Symbol}.", symbol);

        var url = $"/api/v1/quote?symbol={Uri.EscapeDataString(symbol)}&token={Uri.EscapeDataString(apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var quote = JsonSerializer.Deserialize<FinnhubQuoteResponse>(content, JsonSerializerOptions);

        if (quote is null || quote.CurrentPrice <= 0)
        {
            return null;
        }

        return new MarketDataQuote(
            Symbol: symbol,
            ProviderSymbol: symbol,
            CurrentPrice: quote.CurrentPrice,
            PreviousClose: quote.PreviousClose > 0 ? quote.PreviousClose : null,
            Open: quote.Open > 0 ? quote.Open : null,
            High: quote.High > 0 ? quote.High : null,
            Low: quote.Low > 0 ? quote.Low : null,
            Volume: null,
            TimestampUtc: quote.Timestamp > 0
                ? DateTimeOffset.FromUnixTimeSeconds(quote.Timestamp)
                : null,
            Currency: currency);
    }

    private sealed record FinnhubQuoteResponse(
        [property: JsonPropertyName("c")] decimal CurrentPrice,
        [property: JsonPropertyName("h")] decimal High,
        [property: JsonPropertyName("l")] decimal Low,
        [property: JsonPropertyName("o")] decimal Open,
        [property: JsonPropertyName("pc")] decimal PreviousClose,
        [property: JsonPropertyName("t")] long Timestamp);
}
