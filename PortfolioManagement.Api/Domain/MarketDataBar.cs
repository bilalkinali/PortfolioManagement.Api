namespace PortfolioManagement.Api.Domain;

public class MarketDataBar
{
    protected MarketDataBar() { }

    private MarketDataBar(
        DateOnly date,
        MarketDataPeriod period,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume)
    {
        Date = date;
        Period = period;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    public int Id { get; protected set; }
    public int InstrumentId { get; protected set; }

    public DateOnly Date { get; protected set; }
    public MarketDataPeriod Period { get; protected set; }

    public decimal Open { get; protected set; }
    public decimal High { get; protected set; }
    public decimal Low { get; protected set; }
    public decimal Close { get; protected set; }

    public long Volume { get; protected set; }


    /**************************************************************************************/


    public static MarketDataBar Create(DateOnly date, MarketDataPeriod period, decimal open, decimal high, decimal low, decimal close, long volume) 
    {
        if (high < low)
            throw new ArgumentException("High cannot be lower than low.");

        return new MarketDataBar(date, period, open, high, low, close, volume);
    }
}

public enum MarketDataPeriod
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}
