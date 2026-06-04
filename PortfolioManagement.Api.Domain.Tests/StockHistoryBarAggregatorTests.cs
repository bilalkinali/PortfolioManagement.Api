using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

namespace PortfolioManagement.Api.Domain.Tests;

public class StockHistoryBarAggregatorTests
{
    [Fact]
    public void Aggregate_groups_daily_bars_into_weekly_candles()
    {
        var bars = new List<StockHistoryDailyBar>
        {
            new(new DateOnly(2026, 1, 5), 10m, 15m, 9m, 12m, 100),
            new(new DateOnly(2026, 1, 6), 12m, 18m, 11m, 17m, 200),
            new(new DateOnly(2026, 1, 12), 20m, 25m, 19m, 22m, 300)
        };

        var result = StockHistoryBarAggregator.Aggregate(bars, MarketDataPeriod.Weekly);

        Assert.Equal(2, result.Count);
        AssertStockBar(result[0], new DateOnly(2026, 1, 5), 10m, 18m, 9m, 17m, 300);
        AssertStockBar(result[1], new DateOnly(2026, 1, 12), 20m, 25m, 19m, 22m, 300);
    }

    [Fact]
    public void Aggregate_groups_daily_bars_into_monthly_candles()
    {
        var bars = new List<StockHistoryDailyBar>
        {
            new(new DateOnly(2026, 1, 2), 10m, 13m, 8m, 11m, 100),
            new(new DateOnly(2026, 1, 30), 11m, 20m, 10m, 19m, 200),
            new(new DateOnly(2026, 2, 2), 21m, 24m, 18m, 22m, 300)
        };

        var result = StockHistoryBarAggregator.Aggregate(bars, MarketDataPeriod.Monthly);

        Assert.Equal(2, result.Count);
        AssertStockBar(result[0], new DateOnly(2026, 1, 2), 10m, 20m, 8m, 19m, 300);
        AssertStockBar(result[1], new DateOnly(2026, 2, 2), 21m, 24m, 18m, 22m, 300);
    }

    [Fact]
    public void Aggregate_returns_monthly_candles_for_all_range_resolution()
    {
        var period = StockHistoryRangeRules.ResolvePeriod("ALL", null);
        var bars = new List<StockHistoryDailyBar>
        {
            new(new DateOnly(2025, 12, 31), 90m, 95m, 89m, 92m, 100),
            new(new DateOnly(2026, 1, 2), 100m, 110m, 99m, 108m, 200),
            new(new DateOnly(2026, 1, 30), 108m, 115m, 105m, 112m, 300)
        };

        var result = StockHistoryBarAggregator.Aggregate(bars, period!.Value);

        Assert.Equal(MarketDataPeriod.Monthly, period);
        Assert.Equal(2, result.Count);
        AssertStockBar(result[0], new DateOnly(2025, 12, 31), 90m, 95m, 89m, 92m, 100);
        AssertStockBar(result[1], new DateOnly(2026, 1, 2), 100m, 115m, 99m, 112m, 500);
    }

    private static void AssertStockBar(
        StockBar bar,
        DateOnly date,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal volume)
    {
        Assert.Equal(open, bar.Open);
        Assert.Equal(high, bar.High);
        Assert.Equal(low, bar.Low);
        Assert.Equal(close, bar.Close);
        Assert.Equal(volume, bar.Volume);
        Assert.Equal(ToUnixTimeMilliseconds(date), bar.Timestamp);
    }

    private static long ToUnixTimeMilliseconds(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeMilliseconds();
    }
}
