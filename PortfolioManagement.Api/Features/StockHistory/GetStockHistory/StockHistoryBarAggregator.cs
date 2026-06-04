using System.Globalization;
using PortfolioManagement.Api.Domain;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed record StockHistoryDailyBar(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);

public static class StockHistoryBarAggregator
{
    public static List<StockBar> Aggregate(
        IReadOnlyList<StockHistoryDailyBar> dailyBars,
        MarketDataPeriod period)
    {
        var orderedBars = dailyBars
            .OrderBy(x => x.Date)
            .ToList();

        if (period == MarketDataPeriod.Daily)
        {
            return orderedBars
                .Select(MapToStockBar)
                .ToList();
        }

        if (period == MarketDataPeriod.Weekly)
        {
            return orderedBars
                .GroupBy(x => new
                {
                    Year = ISOWeek.GetYear(x.Date.ToDateTime(TimeOnly.MinValue)),
                    Week = ISOWeek.GetWeekOfYear(x.Date.ToDateTime(TimeOnly.MinValue))
                })
                .Select(x => MapAggregatedBucket(x.ToList()))
                .OrderBy(x => x.Timestamp)
                .ToList();
        }

        return orderedBars
            .GroupBy(x => new
            {
                x.Date.Year,
                Month = x.Date.Month
            })
            .Select(x => MapAggregatedBucket(x.ToList()))
            .OrderBy(x => x.Timestamp)
            .ToList();
    }

    private static StockBar MapAggregatedBucket(IReadOnlyList<StockHistoryDailyBar> bars)
    {
        var firstBar = bars[0];
        var lastBar = bars[^1];

        return new StockBar(
            lastBar.Close,
            bars.Max(x => x.High),
            bars.Min(x => x.Low),
            null,
            firstBar.Open,
            ToUnixTimeMilliseconds(firstBar.Date),
            bars.Sum(x => x.Volume),
            null);
    }

    private static StockBar MapToStockBar(StockHistoryDailyBar bar)
    {
        return new StockBar(
            bar.Close,
            bar.High,
            bar.Low,
            null,
            bar.Open,
            ToUnixTimeMilliseconds(bar.Date),
            bar.Volume,
            null);
    }

    private static long ToUnixTimeMilliseconds(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeMilliseconds();
    }
}
