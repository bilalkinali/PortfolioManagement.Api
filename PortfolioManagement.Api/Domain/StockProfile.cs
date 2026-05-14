namespace PortfolioManagement.Api.Domain;

public class StockProfile
{
    protected StockProfile() { }

    private StockProfile(
        int instrumentId, string ticker, bool active, string? cik, string? compositeFigi,
        string? currencyName, string? description, string? homepageUrl, string? listDate,
        string? locale, string? market, decimal? marketCap, string? name, string? phoneNumber,
        string? primaryExchange, long? roundLot, string? shareClassFigi,
        long? shareClassSharesOutstanding, string? sicCode, string? sicDescription,
        string? tickerRoot, string? tickerSuffix, int? totalEmployees, string? type,
        long? weightedSharesOutstanding, string? addressLine1, string? city,
        string? state, string? postalCode, string? iconUrl, string? logoUrl,
        string? delistedUtc)
    {
        InstrumentId = instrumentId;
        Ticker = ticker.Trim().ToUpperInvariant();
        Active = active;
        Cik = cik;
        CompositeFigi = compositeFigi;
        CurrencyName = currencyName;
        Description = description;
        HomepageUrl = homepageUrl;
        ListDate = listDate;
        Locale = locale;
        Market = market;
        MarketCap = marketCap;
        Name = name;
        PhoneNumber = phoneNumber;
        PrimaryExchange = primaryExchange;
        RoundLot = roundLot;
        ShareClassFigi = shareClassFigi;
        ShareClassSharesOutstanding = shareClassSharesOutstanding;
        SicCode = sicCode;
        SicDescription = sicDescription;
        TickerRoot = tickerRoot;
        TickerSuffix = tickerSuffix;
        TotalEmployees = totalEmployees;
        Type = type;
        WeightedSharesOutstanding = weightedSharesOutstanding;
        AddressLine1 = addressLine1;
        City = city;
        State = state;
        PostalCode = postalCode;
        IconUrl = iconUrl;
        LogoUrl = logoUrl;
        DelistedUtc = delistedUtc;
    }

    public int Id { get; private set; }
    public int InstrumentId { get; private set; }
    public Instrument Instrument { get; private set; } = null!;
    public string Ticker { get; private set; } = null!;
    public bool Active { get; private set; }
    public string? Cik { get; private set; }
    public string? CompositeFigi { get; private set; }
    public string? CurrencyName { get; private set; }
    public string? Description { get; private set; }
    public string? HomepageUrl { get; private set; }
    public string? ListDate { get; private set; }
    public string? Locale { get; private set; }
    public string? Market { get; private set; }
    public decimal? MarketCap { get; private set; }
    public string? Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PrimaryExchange { get; private set; }
    public long? RoundLot { get; private set; }
    public string? ShareClassFigi { get; private set; }
    public long? ShareClassSharesOutstanding { get; private set; }
    public string? SicCode { get; private set; }
    public string? SicDescription { get; private set; }
    public string? TickerRoot { get; private set; }
    public string? TickerSuffix { get; private set; }
    public int? TotalEmployees { get; private set; }
    public string? Type { get; private set; }
    public long? WeightedSharesOutstanding { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string? IconUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? DelistedUtc { get; private set; }

    public static StockProfile Create(
        int instrumentId,
        string ticker, bool active, string? cik, string? compositeFigi,
        string? currencyName, string? description, string? homepageUrl, string? listDate,
        string? locale, string? market, decimal? marketCap, string? name, string? phoneNumber,
        string? primaryExchange, long? roundLot, string? shareClassFigi,
        long? shareClassSharesOutstanding, string? sicCode, string? sicDescription,
        string? tickerRoot, string? tickerSuffix, int? totalEmployees, string? type,
        long? weightedSharesOutstanding, string? addressLine1, string? city,
        string? state, string? postalCode, string? iconUrl, string? logoUrl,
        string? delistedUtc)
    {
        return new StockProfile(
            instrumentId, ticker, active, cik, compositeFigi, currencyName, description,
            homepageUrl, listDate, locale, market, marketCap, name, phoneNumber,
            primaryExchange, roundLot, shareClassFigi, shareClassSharesOutstanding,
            sicCode, sicDescription, tickerRoot, tickerSuffix, totalEmployees, type,
            weightedSharesOutstanding, addressLine1, city, state, postalCode,
            iconUrl, logoUrl, delistedUtc);
    }
}
