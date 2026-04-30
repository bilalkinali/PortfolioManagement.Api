using System.Text.Json.Serialization;

namespace PortfolioManagement.Tools.ImportData.ImportInstruments;

public sealed record SecCompany
{
    [JsonPropertyName("cik_str")]
    public int Cik { get; set; }
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
}