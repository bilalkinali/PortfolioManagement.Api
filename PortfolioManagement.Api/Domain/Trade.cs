namespace PortfolioManagement.Api.Domain;

public class Trade
{
    protected Trade() { }

    private Trade(string symbol, int quantity, decimal price, DateOnly executedDate)
    {
        Symbol = symbol;
        Quantity = quantity;
        Price = price;
        ExecutedDate = executedDate;
    }

    public int Id { get; protected set; }
    public string Symbol { get; protected set; } = null!;
    public int Quantity { get; protected set; }
    public decimal Price { get; protected set; }
    public DateOnly ExecutedDate { get; protected set; }


    /**************************************************************************************/


    public static Trade Create(string symbol, int quantity, decimal price, DateOnly executedDate)
    {
        return new Trade(symbol.Trim().ToUpper(), quantity, price, executedDate);
    }
}