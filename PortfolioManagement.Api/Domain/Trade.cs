namespace PortfolioManagement.Api.Domain;

public class Trade
{
    protected Trade() { }

    private Trade(int quantity, decimal price, DateOnly executedDate)
    {
        Quantity = quantity;
        Price = price;
        ExecutedDate = executedDate;
    }

    public int Id { get; protected set; }
    public bool IsBuy => Quantity > 0;
    public int Quantity { get; protected set; }
    public decimal Price { get; protected set; }
    public DateOnly ExecutedDate { get; protected set; }
    public int PositionId { get; protected set; }


    /**************************************************************************************/


    public static Trade Create(int quantity, decimal price, DateOnly executedDate)
    {
        return new Trade(quantity, price, executedDate);
    }
}