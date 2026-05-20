using System.Text.Json;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory.Proxy;

public sealed class MassiveStockHistoryProxy
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<MassiveStockHistoryProxy> _logger;

    public MassiveStockHistoryProxy(
        IHttpClientFactory httpClientFactory,
        ILogger<MassiveStockHistoryProxy> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Massive");
        _logger = logger;
    }

    public async Task<GetStockHistoryResponse?> GetHistoryAsync(
        string ticker,
        DateOnly from,
        DateOnly to,
        string timespan,
        CancellationToken cancellationToken)
    {
        var url = $"/v2/aggs/ticker/{ticker}/range/1/{timespan}/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}";

        _logger.LogInformation(
            "Calling Massive API for stock history. Ticker: {Ticker}, From: {From}, To: {To}, Timespan: {Timespan}",
            ticker,
            from,
            to,
            timespan);

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<GetStockHistoryResponse>(
            content,
            JsonSerializerOptions);
    }
}
