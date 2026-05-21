using PortfolioManagement.Api.Domain;

namespace PortfolioManagement.Api.Domain.Tests;

public class PortfolioTests
{
    private static readonly DateOnly Jan1 = new(2026, 1, 1);
    private static readonly DateOnly Jan2 = new(2026, 1, 2);

    [Fact]
    public void AddTrade_creates_new_position_when_no_position_exists_for_instrument()
    {
        var portfolio = CreatePortfolio();

        var trade = portfolio.AddTrade(1, 10, 100m, Jan1);

        var position = Assert.Single(portfolio.Positions);
        Assert.Equal(1, position.InstrumentId);
        Assert.Same(trade, Assert.Single(position.Trades));
    }

    [Fact]
    public void AddTrade_reuses_existing_position_for_same_instrument()
    {
        var portfolio = CreatePortfolio();

        portfolio.AddTrade(1, 10, 100m, Jan1);
        portfolio.AddTrade(1, 5, 120m, Jan2);

        var position = Assert.Single(portfolio.Positions);
        Assert.Equal(15, position.Quantity);
        Assert.Equal(2, position.Trades.Count);
    }

    [Fact]
    public void Portfolio_enforces_one_position_per_instrument()
    {
        var portfolio = CreatePortfolio();

        portfolio.AddTrade(1, 10, 100m, Jan1);
        portfolio.AddTrade(1, -5, 110m, Jan2);

        Assert.Single(portfolio.Positions, position => position.InstrumentId == 1);
    }

    [Fact]
    public void EditTrade_delegates_to_correct_position_and_trade()
    {
        var portfolio = CreatePortfolio();
        var firstTrade = portfolio.AddTrade(1, 10, 100m, Jan1);
        var secondTrade = portfolio.AddTrade(2, 20, 50m, Jan1);
        var firstPosition = portfolio.Positions.Single(position => position.InstrumentId == 1);
        var secondPosition = portfolio.Positions.Single(position => position.InstrumentId == 2);
        DomainTestIds.SetId(firstPosition, 1);
        DomainTestIds.SetId(secondPosition, 2);
        DomainTestIds.SetId(firstTrade, 10);
        DomainTestIds.SetId(secondTrade, 20);

        portfolio.EditTrade(2, 20, 25, 55m, Jan2);

        Assert.Equal(10, firstPosition.Quantity);
        Assert.Equal(25, secondPosition.Quantity);
        Assert.Equal(55m, secondTrade.Price);
        Assert.Equal(Jan2, secondTrade.ExecutedDate);
    }

    [Fact]
    public void DeleteTrade_removes_trade()
    {
        var portfolio = CreatePortfolio();
        var firstTrade = portfolio.AddTrade(1, 10, 100m, Jan1);
        var secondTrade = portfolio.AddTrade(1, 5, 120m, Jan2);
        var position = Assert.Single(portfolio.Positions);
        DomainTestIds.SetId(position, 1);
        DomainTestIds.SetId(firstTrade, 10);
        DomainTestIds.SetId(secondTrade, 20);

        portfolio.DeleteTrade(1, 20);

        Assert.Single(position.Trades);
        Assert.DoesNotContain(secondTrade, position.Trades);
        Assert.Contains(firstTrade, position.Trades);
    }

    [Fact]
    public void DeleteTrade_removes_position_when_last_trade_is_deleted()
    {
        var portfolio = CreatePortfolio();
        var trade = portfolio.AddTrade(1, 10, 100m, Jan1);
        var position = Assert.Single(portfolio.Positions);
        DomainTestIds.SetId(position, 1);
        DomainTestIds.SetId(trade, 10);

        portfolio.DeleteTrade(1, 10);

        Assert.Empty(portfolio.Positions);
    }

    private static Portfolio CreatePortfolio()
    {
        return Portfolio.Create("Growth", "Long-term holdings", "user-1");
    }
}
