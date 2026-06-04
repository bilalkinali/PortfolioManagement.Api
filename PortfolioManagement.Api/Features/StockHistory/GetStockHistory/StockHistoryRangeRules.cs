using PortfolioManagement.Api.Domain;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public static class StockHistoryRangeRules
{
    private static readonly string[] ValidRanges =
    [
        "1m",
        "3m",
        "6m",
        "1y",
        "5y",
        "10y",
        "all"
    ];

    public static bool IsValidRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return true;
        }

        return ValidRanges.Contains(Normalize(range));
    }

    public static MarketDataPeriod? ResolvePeriod(string? range, string? timespan)
    {
        if (!string.IsNullOrWhiteSpace(range))
        {
            return Normalize(range) switch
            {
                "1m" or "3m" or "6m" or "1y" => MarketDataPeriod.Daily,
                "5y" => MarketDataPeriod.Weekly,
                "10y" or "all" => MarketDataPeriod.Monthly,
                _ => null
            };
        }

        return MapTimespanToPeriod(timespan);
    }

    public static string ResolveTimespan(string? range, string? timespan)
    {
        var period = ResolvePeriod(range, timespan);

        if (period is not null)
        {
            return MapPeriodToTimespan(period.Value);
        }

        return string.IsNullOrWhiteSpace(timespan)
            ? "day"
            : timespan.Trim().ToLowerInvariant();
    }

    private static MarketDataPeriod? MapTimespanToPeriod(string? timespan)
    {
        if (string.IsNullOrWhiteSpace(timespan))
        {
            return MarketDataPeriod.Daily;
        }

        return timespan.Trim().ToLowerInvariant() switch
        {
            "day" => MarketDataPeriod.Daily,
            "week" => MarketDataPeriod.Weekly,
            "month" => MarketDataPeriod.Monthly,
            _ => null
        };
    }

    private static string MapPeriodToTimespan(MarketDataPeriod period)
    {
        return period switch
        {
            MarketDataPeriod.Daily => "day",
            MarketDataPeriod.Weekly => "week",
            MarketDataPeriod.Monthly => "month",
            _ => "day"
        };
    }

    private static string Normalize(string value)
        => value.Trim().ToLowerInvariant();
}
