namespace PortfolioManagement.Api.Domain;

public class Position
{
    protected Position() { }
    
    private Position(int instrumentId)
    {
        InstrumentId = instrumentId;
    }

    private readonly List<Trade> _trades = [];

    public int Id { get; protected set; }
    public int Quantity => _trades.Sum(t => t.Quantity);
    public decimal AvgCost => Quantity != 0 ? _trades.Sum(t => t.Quantity * t.Price) / Quantity : 0m;
    public decimal RealizedPnL => _trades.Where(t => t.Quantity < 0).Sum(t => -t.Quantity * (t.Price - AvgCost));
    public string Status => Quantity == 0 ? "Closed" : "Open"; // Just temporary, since shorting would result in negative quantity but still open position
    public DateOnly OpenDate => _trades.Min(t => t.ExecutedDate);
    public DateOnly? CloseDate => Quantity == 0 ? _trades.Max(t => t.ExecutedDate) : null;
    public int PortfolioId { get; protected set; }
    public int InstrumentId { get; protected set; }
    public Instrument Instrument { get; protected set; } = null!;

    public IReadOnlyCollection<Trade> Trades => _trades;
    public bool HasTrades => _trades.Count > 0;

    /**************************************************************************************/

    
    public static Position Create(int instrumentId)
    {
        return new Position(instrumentId);
    }


    // Trades
    public Trade AddTrade(int quantity, decimal price, DateOnly executedDate)
    {
        var trade = Trade.Create(quantity, price, executedDate);
        _trades.Add(trade);
        return trade;
    }

    public void DeleteTrade(int tradeId)
    {
        var trade = _trades.FirstOrDefault(t => t.Id == tradeId);

        if (trade is null)
        {
            throw new InvalidOperationException("Trade does not exist");
        }

        _trades.Remove(trade);
    }
}
