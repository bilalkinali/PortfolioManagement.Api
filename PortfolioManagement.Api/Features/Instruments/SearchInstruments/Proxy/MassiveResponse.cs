using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments.Proxy;

public sealed record MassiveTickerResponse
{
    [JsonPropertyName("results")]
    public List<MassiveTickerResult> Results { get; init; } = [];
}

public sealed record MassiveTickerResult
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