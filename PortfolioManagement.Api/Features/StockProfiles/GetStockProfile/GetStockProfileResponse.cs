namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public sealed record GetStockProfileResponse(
    bool Active,
    string? Cik,
    string? CompositeFigi,
    string? CurrencyName,
    string? Description,
    string? HomepageUrl,
    string? ListDate,
    string? Locale,
    string? Market,
    decimal? MarketCap,
    string? Name,
    string? PhoneNumber,
    string? PrimaryExchange,
    long? RoundLot,
    string? ShareClassFigi,
    long? ShareClassSharesOutstanding,
    string? SicCode,
    string? SicDescription,
    string? Ticker,
    string? TickerRoot,
    string? TickerSuffix,
    int? TotalEmployees,
    string? Type,
    long? WeightedSharesOutstanding,
    AddressResponse? Address,
    BrandingResponse? Branding,
    string? DelistedUtc,
    DateOnly LastSyncedDate
);

public sealed record AddressResponse(
    string? Address1,
    string? City,
    string? State,
    string? PostalCode
);

public sealed record BrandingResponse(
    string? IconUrl,
    string? LogoUrl
);