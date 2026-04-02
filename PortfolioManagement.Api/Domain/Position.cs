namespace PortfolioManagement.Api.Domain;

public class Position
{
    protected Position() { }
    
    private Position(int id)
    {
        Id = id;
    }

    private readonly List<Trade> _trades = [];

    public int Id { get; protected set; }
    public IReadOnlyCollection<Trade> Trades => _trades;


    /**************************************************************************************/

    
    public static Position Create(int id)
    {
        return new Position(id);
    }


    // Trades
    public Trade AddTrade(string symbol, int quantity, decimal price, DateOnly executedDate)
    {
        var trade = Trade.Create(symbol, quantity, price, executedDate);
        _trades.Add(trade);
        return trade;
    }
}