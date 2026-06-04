using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

namespace PortfolioManagement.Api.Domain.Tests;

public class StockHistoryRangeRulesTests
{
    [Theory]
    [InlineData("1M")]
    [InlineData("3M")]
    [InlineData("6M")]
    [InlineData("1Y")]
    public void ResolvePeriod_maps_short_chart_ranges_to_daily(string range)
    {
        var period = StockHistoryRangeRules.ResolvePeriod(range, "month");

        Assert.Equal(MarketDataPeriod.Daily, period);
    }

    [Fact]
    public void ResolvePeriod_maps_5y_to_weekly()
    {
        var period = StockHistoryRangeRules.ResolvePeriod("5Y", "day");

        Assert.Equal(MarketDataPeriod.Weekly, period);
    }

    [Theory]
    [InlineData("10Y")]
    [InlineData("ALL")]
    public void ResolvePeriod_maps_long_chart_ranges_to_monthly(string range)
    {
        var period = StockHistoryRangeRules.ResolvePeriod(range, "day");

        Assert.Equal(MarketDataPeriod.Monthly, period);
    }

    [Fact]
    public void ResolvePeriod_treats_range_as_case_insensitive()
    {
        var period = StockHistoryRangeRules.ResolvePeriod("all", "day");

        Assert.Equal(MarketDataPeriod.Monthly, period);
    }
}
