using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.StockProfile.GetStockProfile;

public sealed class MassiveTickerOverviewResponse
{
    [property: JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
    [property: JsonPropertyName("count")]
    public int Count { get; set; }
    [property: JsonPropertyName("results")]
    public TickerInfo? TickerData { get; set; }
    [property: JsonPropertyName("status")]
    public string? Status { get; set; }

    public sealed class TickerInfo
    {
        public bool Active { get; set; }
        public string? Cik { get; set; }
        public string? CompositeFigi { get; set; }
        public string? CurrencyName { get; set; }
        public string? Description { get; set; }
        public string? HomepageUrl { get; set; }
        public string? ListDate { get; set; }
        public string? Locale { get; set; }
        public string? Market { get; set; }
        public long? MarketCap { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PrimaryExchange { get; set; }
        public decimal? RoundLot { get; set; }
        public string? ShareClassFigi { get; set; }
        public long? ShareClassSharesOutstanding { get; set; }
        public string? SicCode { get; set; }
        public string? SicDescription { get; set; }
        public string? Ticker { get; set; }
        public string? TickerRoot { get; set; }
        public string? TickerSuffix { get; set; }
        public int? TotalEmployees { get; set; }
        public string? Type { get; set; }
        public long? WeightedSharesOutstanding { get; set; }
        public Address? Address { get; set; }
        public Branding? Branding { get; set; }
        public string? DelistedUtc { get; set; }
    }

    public sealed class Address
    {
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }

    public sealed class Branding
    {
        public string? IconUrl { get; set; }
        public string? LogoUrl { get; set; }
    }
}
