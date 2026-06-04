using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory.Proxy;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Domain.Tests;

public class GetStockHistoryHandlerTests
{
    [Fact]
    public async Task Handle_returns_monthly_candles_for_all_range()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create("AAPL", "Apple", currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        instrument.AddMarketDataBar(new DateOnly(2025, 12, 31), MarketDataPeriod.Daily, 90m, 95m, 89m, 92m, 100);
        instrument.AddMarketDataBar(new DateOnly(2026, 1, 2), MarketDataPeriod.Daily, 100m, 110m, 99m, 108m, 200);
        instrument.AddMarketDataBar(new DateOnly(2026, 1, 30), MarketDataPeriod.Daily, 108m, 115m, 105m, 112m, 300);
        db.Instruments.Add(instrument);
        await db.SaveChangesAsync();

        var handler = new GetStockHistoryHandler(
            db,
            new MassiveStockHistoryProxy(
                new TestHttpClientFactory(),
                NullLogger<MassiveStockHistoryProxy>.Instance));
        var request = new GetStockHistoryRequest
        {
            Ticker = "AAPL",
            From = "2025-12-01",
            To = "2026-01-31",
            Timespan = "day",
            Range = "ALL"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.ResultsCount);
        var bars = result.Results!;
        Assert.NotNull(bars);
        Assert.Equal(2, bars.Count);
        Assert.Equal(90m, bars[0].Open);
        Assert.Equal(92m, bars[0].Close);
        Assert.Equal(100m, bars[0].Volume);
        Assert.Equal(100m, bars[1].Open);
        Assert.Equal(115m, bars[1].High);
        Assert.Equal(99m, bars[1].Low);
        Assert.Equal(112m, bars[1].Close);
        Assert.Equal(500m, bars[1].Volume);
    }

    private static PortfolioDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PortfolioDbContext(options);
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
            => new();
    }
}
