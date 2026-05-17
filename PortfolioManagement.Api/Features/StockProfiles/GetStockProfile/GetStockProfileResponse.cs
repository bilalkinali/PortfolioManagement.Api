using System.Text.Json.Serialization;

namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public sealed record GetStockProfileApiResponse(
    [property: JsonPropertyName("request_id")] string? RequestId,
    [property: JsonPropertyName("results")] GetStockProfileResponse? Results,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("count")] int? Count);

public sealed record GetStockProfileResponse(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("cik")] string? Cik,
    [property: JsonPropertyName("composite_figi")] string? CompositeFigi,
    [property: JsonPropertyName("currency_name")] string? CurrencyName,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("homepage_url")] string? HomepageUrl,
    [property: JsonPropertyName("list_date")] string? ListDate,
    [property: JsonPropertyName("locale")] string? Locale,
    [property: JsonPropertyName("market")] string? Market,
    [property: JsonPropertyName("market_cap")] decimal? MarketCap,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber,
    [property: JsonPropertyName("primary_exchange")] string? PrimaryExchange,
    [property: JsonPropertyName("round_lot")] long? RoundLot,
    [property: JsonPropertyName("share_class_figi")] string? ShareClassFigi,
    [property: JsonPropertyName("share_class_shares_outstanding")] long? ShareClassSharesOutstanding,
    [property: JsonPropertyName("sic_code")] string? SicCode,
    [property: JsonPropertyName("sic_description")] string? SicDescription,
    [property: JsonPropertyName("ticker")] string? Ticker,
    [property: JsonPropertyName("ticker_root")] string? TickerRoot,
    [property: JsonPropertyName("ticker_suffix")] string? TickerSuffix,
    [property: JsonPropertyName("total_employees")] int? TotalEmployees,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("weighted_shares_outstanding")] long? WeightedSharesOutstanding,
    [property: JsonPropertyName("address")] Address? Address,
    [property: JsonPropertyName("branding")] Branding? Branding,
    [property: JsonPropertyName("delisted_utc")] string? DelistedUtc);

public sealed record Address(
    [property: JsonPropertyName("address1")] string? Address1,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("postal_code")] string? PostalCode);

public sealed record Branding(
    [property: JsonPropertyName("icon_url")] string? IconUrl,
    [property: JsonPropertyName("logo_url")] string? LogoUrl);
