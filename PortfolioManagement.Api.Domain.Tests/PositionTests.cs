using PortfolioManagement.Api.Domain;

namespace PortfolioManagement.Api.Domain.Tests;

public class PositionTests
{
    private static readonly DateOnly Jan1 = new(2026, 1, 1);
    private static readonly DateOnly Jan2 = new(2026, 1, 2);
    private static readonly DateOnly Jan3 = new(2026, 1, 3);

    [Fact]
    public void Create_creates_position_with_first_trade()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out var trade);

        Assert.Equal(1, position.InstrumentId);
        Assert.Same(trade, Assert.Single(position.Trades));
    }

    [Fact]
    public void Cannot_be_created_empty_through_public_api()
    {
        var publicConstructors = typeof(Position).GetConstructors();

        Assert.Empty(publicConstructors);
    }

    [Fact]
    public void Initial_positive_quantity_gives_long_status()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);

        Assert.Equal("Long", position.Status);
    }

    [Fact]
    public void Initial_negative_quantity_gives_short_status()
    {
        var position = Position.Create(1, -10, 100m, Jan1, out _);

        Assert.Equal("Short", position.Status);
    }

    [Fact]
    public void Quantity_is_signed_sum_of_trades()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);
        position.AddTrade(5, 120m, Jan2);
        position.AddTrade(-3, 130m, Jan3);

        Assert.Equal(12, position.Quantity);
    }

    [Fact]
    public void AverageCostBasis_uses_average_cost_when_adding_to_open_long_position()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);
        position.AddTrade(10, 120m, Jan2);

        Assert.Equal(110m, position.AverageCostBasis);
    }

    [Fact]
    public void AverageCostBasis_uses_average_cost_when_adding_to_open_short_position()
    {
        var position = Position.Create(1, -10, 100m, Jan1, out _);
        position.AddTrade(-10, 80m, Jan2);

        Assert.Equal(90m, position.AverageCostBasis);
    }

    [Fact]
    public void RealizedPnL_is_calculated_when_reducing_long_position()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);
        position.AddTrade(-4, 130m, Jan2);

        Assert.Equal(6, position.Quantity);
        Assert.Equal(100m, position.AverageCostBasis);
        Assert.Equal(120m, position.RealizedPnL);
    }

    [Fact]
    public void RealizedPnL_is_calculated_when_closing_short_position()
    {
        var position = Position.Create(1, -10, 100m, Jan1, out _);
        position.AddTrade(10, 70m, Jan2);

        Assert.Equal(0, position.Quantity);
        Assert.Equal(0m, position.AverageCostBasis);
        Assert.Equal(300m, position.RealizedPnL);
    }

    [Fact]
    public void Status_becomes_closed_when_quantity_reaches_zero()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);
        position.AddTrade(-10, 110m, Jan2);

        Assert.Equal("Closed", position.Status);
    }

    [Fact]
    public void OpenDate_is_earliest_trade_date()
    {
        var position = Position.Create(1, 10, 100m, Jan2, out _);
        position.AddTrade(5, 90m, Jan1);

        Assert.Equal(Jan1, position.OpenDate);
    }

    [Fact]
    public void CloseDate_is_set_only_when_position_is_closed()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);

        Assert.Null(position.CloseDate);

        position.AddTrade(-10, 110m, Jan3);

        Assert.Equal(Jan3, position.CloseDate);
    }

    [Fact]
    public void Long_position_can_be_closed_without_crossing_zero()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);

        var trade = position.AddTrade(-10, 110m, Jan2);

        Assert.Contains(trade, position.Trades);
        Assert.Equal(0, position.Quantity);
    }

    [Fact]
    public void Long_position_allows_sell_that_crosses_zero_and_opens_short_position()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out _);

        var trade = position.AddTrade(-15, 110m, Jan2);

        Assert.Contains(trade, position.Trades);
        Assert.Equal(-5, position.Quantity);
        Assert.Equal("Short", position.Status);
        Assert.Equal(110m, position.AverageCostBasis);
        Assert.Equal(100m, position.RealizedPnL);
    }

    [Fact]
    public void Short_position_can_be_closed_without_crossing_zero()
    {
        var position = Position.Create(1, -10, 100m, Jan1, out _);

        var trade = position.AddTrade(10, 90m, Jan2);

        Assert.Contains(trade, position.Trades);
        Assert.Equal(0, position.Quantity);
    }

    [Fact]
    public void Short_position_allows_buy_that_crosses_zero_and_opens_long_position()
    {
        var position = Position.Create(1, -10, 100m, Jan1, out _);

        var trade = position.AddTrade(15, 90m, Jan2);

        Assert.Contains(trade, position.Trades);
        Assert.Equal(5, position.Quantity);
        Assert.Equal("Long", position.Status);
        Assert.Equal(90m, position.AverageCostBasis);
        Assert.Equal(100m, position.RealizedPnL);
    }

    [Fact]
    public void EditTrade_allows_change_that_makes_ordered_history_cross_zero()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out var openingTrade);
        var reducingTrade = position.AddTrade(-5, 110m, Jan2);
        DomainTestIds.SetId(openingTrade, 1);
        DomainTestIds.SetId(reducingTrade, 2);

        position.EditTrade(2, -15, 110m, Jan2);

        Assert.Equal(-15, reducingTrade.Quantity);
        Assert.Equal(-5, position.Quantity);
        Assert.Equal("Short", position.Status);
        Assert.Equal(110m, position.AverageCostBasis);
        Assert.Equal(100m, position.RealizedPnL);
    }

    [Fact]
    public void DeleteTrade_allows_remaining_ordered_history_to_cross_zero()
    {
        var position = Position.Create(1, 10, 100m, Jan1, out var openingTrade);
        var laterBuy = position.AddTrade(5, 120m, Jan2);
        var sell = position.AddTrade(-12, 130m, Jan3);
        DomainTestIds.SetId(openingTrade, 1);
        DomainTestIds.SetId(laterBuy, 2);
        DomainTestIds.SetId(sell, 3);

        position.DeleteTrade(2);

        Assert.DoesNotContain(laterBuy, position.Trades);
        Assert.Equal(-2, position.Quantity);
        Assert.Equal("Short", position.Status);
        Assert.Equal(130m, position.AverageCostBasis);
        Assert.Equal(300m, position.RealizedPnL);
    }
}
