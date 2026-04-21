using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed record GetStockHistoryResponse(
    [property: JsonPropertyName("adjusted")] bool Adjusted,
    [property: JsonPropertyName("next_url")] string? NextUrl,
    [property: JsonPropertyName("queryCount")] int QueryCount,
    [property: JsonPropertyName("request_id")] string? RequestId,
    [property: JsonPropertyName("resultsCount")] int ResultsCount,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("results")] IReadOnlyList<StockBar>? Results);

public sealed record StockBar(
    [property: JsonPropertyName("c")] decimal Close,
    [property: JsonPropertyName("h")] decimal High,
    [property: JsonPropertyName("l")] decimal Low,
    [property: JsonPropertyName("n")] int? NumberOfTrades,
    [property: JsonPropertyName("o")] decimal Open,
    [property: JsonPropertyName("t")] long Timestamp,
    [property: JsonPropertyName("v")] decimal Volume,
    [property: JsonPropertyName("vw")] decimal? Vwap);

//{
//"adjusted": true,
//"next_url": "https://api.massive.com/v2/aggs/ticker/AAPL/range/1/day/1578114000000/2020-01-10?cursor=bGltaXQ9MiZzb3J0PWFzYw",
//"queryCount": 2,
//"request_id": "6a7e466379af0a71039d60cc78e72282",
//"results": [
//{
//    "c": 75.0875,
//    "h": 75.15,
//    "l": 73.7975,
//    "n": 1,
//    "o": 74.06,
//    "t": 1577941200000,
//    "v": 135647456,
//    "vw": 74.6099
//},
//{
//"c": 74.3575,
//"h": 75.145,
//"l": 74.125,
//"n": 1,
//"o": 74.2875,
//"t": 1578027600000,
//"v": 146535512,
//"vw": 74.7026
//}
//],
//"resultsCount": 2,
//"status": "OK",
//"ticker": "AAPL"
//}