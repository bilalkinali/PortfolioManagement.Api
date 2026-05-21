namespace PortfolioManagement.Api.Domain.Tests;

public class TradeTests
{
    private static readonly DateOnly ExecutedDate = new(2026, 1, 2);

    [Fact]
    public void Create_allows_positive_quantity_as_buy()
    {
        var trade = global::Trade.Create(10, 100m, ExecutedDate);

        Assert.Equal(10, trade.Quantity);
        Assert.True(trade.IsBuy);
        Assert.False(trade.IsSell);
    }

    [Fact]
    public void Create_allows_negative_quantity_as_sell()
    {
        var trade = global::Trade.Create(-10, 100m, ExecutedDate);

        Assert.Equal(-10, trade.Quantity);
        Assert.False(trade.IsBuy);
        Assert.True(trade.IsSell);
    }

    [Fact]
    public void Create_rejects_zero_quantity()
    {
        Assert.Throws<ArgumentException>(() => global::Trade.Create(0, 100m, ExecutedDate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_price_less_than_or_equal_to_zero(decimal price)
    {
        Assert.Throws<ArgumentException>(() => global::Trade.Create(10, price, ExecutedDate));
    }

    [Fact]
    public void Create_rejects_future_executed_date()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        Assert.Throws<ArgumentException>(() => global::Trade.Create(10, 100m, futureDate));
    }
}
