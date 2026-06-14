namespace PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;

public sealed class GetStockQuoteRequest
{
    public string Ticker { get; init; } = null!;
}
