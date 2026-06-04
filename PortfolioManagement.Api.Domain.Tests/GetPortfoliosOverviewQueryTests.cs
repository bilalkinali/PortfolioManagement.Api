using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfoliosOverview;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Domain.Tests;

public class GetPortfoliosOverviewQueryTests
{
    private static readonly DateOnly Jan1 = new(2026, 1, 1);
    private static readonly DateOnly Jan2 = new(2026, 1, 2);

    [Fact]
    public async Task GetPortfoliosOverviewAsync_calculates_realized_unrealized_and_total_metrics()
    {
        await using var db = CreateDbContext();
        var longInstrument = Instrument.Create("MSFT", "Microsoft", currency: "USD");
        var shortInstrument = Instrument.Create("TSLA", "Tesla", currency: "USD");
        DomainTestIds.SetId(longInstrument, 1);
        DomainTestIds.SetId(shortInstrument, 2);
        var portfolio = Portfolio.Create("Growth", "Long and short positions", "user-1");
        portfolio.AddTrade(1, 10, 100m, Jan1);
        portfolio.AddTrade(1, -4, 130m, Jan2);
        portfolio.AddTrade(2, -5, 80m, Jan1);

        longInstrument.AddMarketDataBar(Jan2, MarketDataPeriod.Daily, 110m, 115m, 105m, 110m, 1000);
        shortInstrument.AddMarketDataBar(Jan2, MarketDataPeriod.Daily, 70m, 75m, 65m, 70m, 1000);

        db.Instruments.AddRange(longInstrument, shortInstrument);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var result = await new GetPortfoliosOverviewQuery(db).GetPortfoliosOverviewAsync("user-1", CancellationToken.None);

        var overview = Assert.Single(result);
        Assert.Equal(2, overview.PositionCount);
        Assert.Equal(2, overview.OpenPositionCount);
        Assert.Equal(0, overview.MissingPricePositionCount);
        Assert.Equal(1000m, overview.TotalCostBasis);
        Assert.Equal(120m, overview.TotalRealizedPnL);
        Assert.Equal(110m, overview.TotalUnrealizedPnL);
        Assert.Equal(230m, overview.TotalPnL);
        Assert.Equal(23m, overview.TotalPnLPercentage);
        Assert.Equal(310m, overview.TotalMarketValue);
    }

    [Fact]
    public async Task GetPortfoliosOverviewAsync_keeps_missing_price_values_nullable_and_marks_partial_metrics()
    {
        await using var db = CreateDbContext();
        var pricedInstrument = Instrument.Create("MSFT", "Microsoft", currency: "USD");
        var missingPriceInstrument = Instrument.Create("NVDA", "Nvidia", currency: "USD");
        DomainTestIds.SetId(pricedInstrument, 1);
        DomainTestIds.SetId(missingPriceInstrument, 2);
        var portfolio = Portfolio.Create("Growth", null, "user-1");
        portfolio.AddTrade(1, 10, 100m, Jan1);
        portfolio.AddTrade(2, 2, 50m, Jan1);

        pricedInstrument.AddMarketDataBar(Jan2, MarketDataPeriod.Daily, 110m, 115m, 105m, 110m, 1000);

        db.Instruments.AddRange(pricedInstrument, missingPriceInstrument);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var result = await new GetPortfoliosOverviewQuery(db).GetPortfoliosOverviewAsync("user-1", CancellationToken.None);

        var overview = Assert.Single(result);
        var missingPricePosition = Assert.Single(overview.Positions, position => position.Symbol == "NVDA");
        Assert.Equal(1, overview.MissingPricePositionCount);
        Assert.Null(missingPricePosition.LatestPrice);
        Assert.Null(missingPricePosition.MarketValue);
        Assert.Null(missingPricePosition.UnrealizedPnL);
        Assert.Equal(1100m, overview.TotalMarketValue);
        Assert.Equal(100m, overview.TotalUnrealizedPnL);
        Assert.Equal(100m, overview.TotalPnL);
        Assert.Equal(100m / 1100m * 100, overview.TotalPnLPercentage);
    }

    [Fact]
    public async Task GetPortfoliosOverviewAsync_returns_zero_percentage_when_total_cost_basis_is_zero()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create("MSFT", "Microsoft", currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        var portfolio = Portfolio.Create("Growth", null, "user-1");

        db.Instruments.Add(instrument);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var result = await new GetPortfoliosOverviewQuery(db).GetPortfoliosOverviewAsync("user-1", CancellationToken.None);

        var overview = Assert.Single(result);
        Assert.Equal(0m, overview.TotalCostBasis);
        Assert.Equal(0m, overview.TotalPnL);
        Assert.Equal(0m, overview.TotalPnLPercentage);
    }

    private static PortfolioDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PortfolioDbContext(options);
    }
}
