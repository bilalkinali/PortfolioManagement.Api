namespace PortfolioManagement.Api.Domain;

public class MarketDataBar
{
    protected MarketDataBar() { }

    private MarketDataBar(
        DateOnly date,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal adjustedClose,
        decimal volume)
    {
        Date = date;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        AdjustedClose = adjustedClose;
        Volume = volume;
    }

    public int Id { get; protected set; }
    public int InstrumentId { get; protected set; }
    public Instrument Instrument { get; protected set; } = null!;
    public DateOnly Date { get; protected set; }
    public decimal Open { get; protected set; }
    public decimal High { get; protected set; }
    public decimal Low { get; protected set; }
    public decimal Close { get; protected set; }
    public decimal AdjustedClose { get; protected set; }
    public decimal Volume { get; protected set; }


    /**************************************************************************************/


    public static MarketDataBar Create(
        DateOnly date,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal adjustedClose,
        decimal volume)
    {
        return new MarketDataBar(
            date,
            open,
            high,
            low,
            close,
            adjustedClose,
            volume);
    }
}
