public class Trade
{
    protected Trade() { }

    private Trade(int quantity, decimal price, DateOnly executedDate)
    {
        Validate(quantity, price, executedDate);

        Quantity = quantity;
        Price = price;
        ExecutedDate = executedDate;
    }

    public int Id { get; protected set; }

    // Positive = buy, negative = sell
    public int Quantity { get; protected set; }

    public decimal Price { get; protected set; }
    public DateOnly ExecutedDate { get; protected set; }
    public int PositionId { get; protected set; }

    public bool IsBuy => Quantity > 0;
    public bool IsSell => Quantity < 0;

    public decimal TradeValue => Math.Abs(Quantity) * Price;

    public static Trade Create(int quantity, decimal price, DateOnly executedDate)
    {
        return new Trade(quantity, price, executedDate);
    }

    public void Edit(int quantity, decimal price, DateOnly executedDate)
    {
        Validate(quantity, price, executedDate);

        Quantity = quantity;
        Price = price;
        ExecutedDate = executedDate;
    }

    private static void Validate(int quantity, decimal price, DateOnly executedDate)
    {
        if (quantity == 0)
        {
            throw new ArgumentException("Trade quantity cannot be zero.", nameof(quantity));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Trade price must be greater than zero.", nameof(price));
        }

        if (executedDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Trade executed date cannot be in the future.", nameof(executedDate));
        }
    }
}