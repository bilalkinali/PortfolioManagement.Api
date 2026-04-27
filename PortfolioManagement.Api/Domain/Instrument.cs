namespace PortfolioManagement.Api.Domain;

public class Instrument
{
    protected Instrument() { }

    private Instrument(
        string symbol,
        string? name,
        string? exchange,
        string? currency,
        string? assetType)
    {
        Symbol = symbol.Trim().ToUpperInvariant();
        Name = name;
        Exchange = exchange;
        Currency = currency;
        AssetType = assetType;
    }

    private readonly List<MarketDataBar> _marketDataBars = [];

    public int Id { get; protected set; }
    public string Symbol { get; protected set; } = null!;
    public string? Name { get; protected set; }
    public string? Exchange { get; protected set; }
    public string? Currency { get; protected set; }
    public string? AssetType { get; protected set; }
    public IReadOnlyCollection<MarketDataBar> MarketDataBars => _marketDataBars.AsReadOnly();


    /**************************************************************************************/


    public static Instrument Create(
        string symbol,
        string? name,
        string? exchange,
        string? currency,
        string? assetType)
    {
        return new Instrument(
            symbol,
            name,
            exchange,
            currency,
            assetType);
    }

    public void UpdateDetails(
        string? name,
        string? exchange,
        string? currency,
        string? assetType)
    {
        Name = name;
        Exchange = exchange;
        Currency = currency;
        AssetType = assetType;
    }

    public MarketDataBar AddMarketDataBar(
        DateOnly date,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal adjustedClose,
        decimal volume)
    {
        var marketDataBar = MarketDataBar.Create(
            date,
            open,
            high,
            low,
            close,
            adjustedClose,
            volume);

        _marketDataBars.Add(marketDataBar);
        return marketDataBar;
    }
}
