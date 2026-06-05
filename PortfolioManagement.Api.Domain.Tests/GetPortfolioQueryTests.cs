using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfolio;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Domain.Tests;

public class GetPortfolioQueryTests
{
    private static readonly DateOnly Jan1 = new(2026, 1, 1);
    private static readonly DateOnly Jan2 = new(2026, 1, 2);
    private static readonly DateOnly Jan3 = new(2026, 1, 3);

    [Fact]
    public async Task GetPortfolioAsync_returns_trade_display_fields_and_realized_gain_for_closing_trade()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create("MSFT", "Microsoft", currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        var portfolio = Portfolio.Create("Growth", null, "user-1");
        var buy = portfolio.AddTrade(1, 10, 100m, Jan1);
        var sell = portfolio.AddTrade(1, -4, 130m, Jan2);
        DomainTestIds.SetId(buy, 1);
        DomainTestIds.SetId(sell, 2);

        db.Instruments.Add(instrument);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var result = await new GetPortfolioQuery(db).GetPortfolioAsync(portfolio.Id, "user-1");

        var position = Assert.Single(result.Positions);
        var sellResponse = Assert.Single(position.Trades, trade => trade.Id == sell.Id);
        var buyResponse = Assert.Single(position.Trades, trade => trade.Id == buy.Id);
        Assert.Equal("Sell", sellResponse.Type);
        Assert.Equal(4, sellResponse.Shares);
        Assert.Equal(520m, sellResponse.TotalCost);
        Assert.Equal(120m, sellResponse.RealizedGain);
        Assert.Equal(30m, sellResponse.RealizedGainPercentage);
        Assert.Equal("Buy", buyResponse.Type);
        Assert.Equal(10, buyResponse.Shares);
        Assert.Equal(1000m, buyResponse.TotalCost);
        Assert.Null(buyResponse.RealizedGain);
        Assert.Null(buyResponse.RealizedGainPercentage);
    }

    [Fact]
    public async Task GetPortfolioAsync_calculates_realized_gain_for_partial_close_after_average_cost_changes()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create("MSFT", "Microsoft", currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        var portfolio = Portfolio.Create("Growth", null, "user-1");
        portfolio.AddTrade(1, 10, 100m, Jan1);
        portfolio.AddTrade(1, 10, 120m, Jan2);
        var sell = portfolio.AddTrade(1, -5, 132m, Jan3);
        DomainTestIds.SetId(sell, 3);

        db.Instruments.Add(instrument);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var result = await new GetPortfolioQuery(db).GetPortfolioAsync(portfolio.Id, "user-1");

        var position = Assert.Single(result.Positions);
        var sellResponse = Assert.Single(position.Trades, trade => trade.Id == sell.Id);
        Assert.Equal(110m, position.AverageCostBasis);
        Assert.Equal(110m, sellResponse.RealizedGain);
        Assert.Equal(20m, sellResponse.RealizedGainPercentage);
    }

    private static PortfolioDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PortfolioDbContext(options);
    }
}
