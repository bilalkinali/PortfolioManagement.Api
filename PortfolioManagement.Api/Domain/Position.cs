namespace PortfolioManagement.Api.Domain;

public class Position
{
    protected Position() { }
    
    private Position(string symbol)
    {
        Symbol = symbol;
    }

    private readonly List<Trade> _trades = [];

    public int Id { get; protected set; }
    public string Symbol { get; protected set; } = null!;
    public int Quantity => _trades.Sum(t => t.Quantity);
    public decimal AvgCost => Quantity != 0 ? _trades.Sum(t => t.Quantity * t.Price) / Quantity : 0m;
    public decimal RealizedPnL => _trades.Where(t => t.Quantity < 0).Sum(t => -t.Quantity * (t.Price - AvgCost));
    public string Status => Quantity > 0 ? "Open" : "Closed";
    public DateOnly OpenDate => _trades.Min(t => t.ExecutedDate);
    public DateOnly? CloseDate => Quantity == 0 ? _trades.Max(t => t.ExecutedDate) : null;
    public int PortfolioId { get; protected set; }

    public IReadOnlyCollection<Trade> Trades => _trades;


    /**************************************************************************************/

    
    public static Position Create(string symbol)
    {
        return new Position(symbol);
    }


    // Trades
    public Trade AddTrade(int quantity, decimal price, DateOnly executedDate)
    {
        var trade = Trade.Create(quantity, price, executedDate);
        _trades.Add(trade);
        return trade;
    }
}