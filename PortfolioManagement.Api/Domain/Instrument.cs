namespace PortfolioManagement.Api.Domain;

public class Instrument
{
    protected Instrument() { }

    private Instrument(
        string symbol, string name, string? providerSymbol, int? cik,
        string? market, string? exchange, string? currency, string? type)
    {
        Symbol = symbol.Trim().ToUpperInvariant();
        Name = name.Trim();
        ProviderSymbol = providerSymbol?.Trim().ToUpperInvariant();
        Cik = cik;

        Market = market?.Trim();
        Exchange = exchange?.Trim();
        Currency = currency?.Trim().ToUpperInvariant();
        Type = type?.Trim();
    }

    private readonly List<MarketDataBar> _marketDataBars = [];

    public int Id { get; protected set; }
    public string Symbol { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;
    public string? ProviderSymbol { get; protected set; }
    public int? Cik { get; protected set; }

    public string? Market { get; protected set; }
    public string? Exchange { get; protected set; }
    public string? Currency { get; protected set; }
    public string? Type { get; protected set; }
    public IReadOnlyCollection<MarketDataBar> MarketDataBars => _marketDataBars.AsReadOnly();


    /**************************************************************************************/


    public static Instrument Create(
        string symbol, string name, string? providerSymbol = null, int? cik = null,
        string? market = null, string? exchange = null, string? currency = null, string? type = null)
    {
        return new Instrument(symbol, name, providerSymbol, cik, market, exchange, currency, type);
    }

    public void UpdateMetadata(
        string name, string? providerSymbol = null, int? cik = null, string? market = null,
        string? exchange = null, string? currency = null, string? type = null)
    {
        Name = name.Trim();
        ProviderSymbol = providerSymbol?.Trim().ToUpperInvariant();
        Cik = cik;

        Market = market?.Trim();
        Exchange = exchange?.Trim();
        Currency = currency?.Trim().ToUpperInvariant();
        Type = type?.Trim();
    }

    //public void UpdateDetails(
    //    string? name,
    //    string? exchange,
    //    string? currency,
    //    string? type)
    //{
    //    Name = name;
    //    Exchange = exchange;
    //    Currency = currency;
    //    Type = type;
    //}

    public MarketDataBar AddMarketDataBar(
        DateOnly date, MarketDataPeriod period, decimal open, 
        decimal high, decimal low, decimal close, long volume)
    {
        if (Id == 0)
            throw new InvalidOperationException("Instrument must be persisted before adding market data bars");

        var marketDataBar = MarketDataBar.Create(Id, date, period, open, high, low, close, volume);

        _marketDataBars.Add(marketDataBar);
        return marketDataBar;
    }
}
