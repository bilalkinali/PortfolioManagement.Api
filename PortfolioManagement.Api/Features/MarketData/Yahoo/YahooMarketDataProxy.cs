using System.Globalization;
using YahooFinanceApi;
using YahooClient = YahooFinanceApi.Yahoo;

namespace PortfolioManagement.Api.Features.MarketData.Yahoo;

public sealed class YahooMarketDataProxy
{
    private readonly ILogger<YahooMarketDataProxy> _logger;
    private readonly YahooRequestGate _requestGate;

    public YahooMarketDataProxy(
        YahooRequestGate requestGate,
        ILogger<YahooMarketDataProxy> logger)
    {
        _requestGate = requestGate;
        _logger = logger;
        YahooClient.IgnoreEmptyRows = true;
    }

    internal async Task<IReadOnlyList<MarketDataInstrumentLookupResult>?> LookupAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var quote = await GetQuoteAsync(symbol, cancellationToken);

        if (quote is null)
        {
            return null;
        }

        return
        [
            new MarketDataInstrumentLookupResult(
                Symbol: quote.Symbol,
                Name: quote.Name ?? quote.Symbol,
                ProviderSymbol: quote.ProviderSymbol,
                Cik: null,
                Market: quote.Market,
                ExchangeCode: quote.ExchangeCode,
                Currency: quote.Currency,
                Type: quote.Type)
        ];
    }

    internal async Task<MarketDataQuote?> GetLatestQuoteAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var quote = await GetQuoteAsync(symbol, cancellationToken);

        if (quote is null || quote.CurrentPrice is null || quote.CurrentPrice <= 0)
        {
            return null;
        }

        return new MarketDataQuote(
            Symbol: quote.Symbol,
            ProviderSymbol: quote.ProviderSymbol,
            CurrentPrice: quote.CurrentPrice.Value,
            PreviousClose: quote.PreviousClose,
            Open: quote.Open,
            High: quote.High,
            Low: quote.Low,
            Volume: quote.Volume,
            TimestampUtc: quote.TimestampUtc,
            Currency: quote.Currency);
    }

    internal async Task<IReadOnlyList<MarketDataHistoricalCandle>?> GetDailyHistoryAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Calling Yahoo Finance history for symbol {Symbol}. From: {From}, To: {To}",
                symbol,
                from,
                to);

            var history = await _requestGate.ExecuteAsync(
                async token =>
                {
                    token.ThrowIfCancellationRequested();

                    return await YahooClient.GetHistoricalAsync(
                        symbol,
                        from.ToDateTime(TimeOnly.MinValue),
                        to.AddDays(1).ToDateTime(TimeOnly.MinValue),
                        Period.Daily);
                },
                cancellationToken);

            return history?
                .Where(x =>
                    x.Close > 0 &&
                    DateOnly.FromDateTime(x.DateTime) >= from &&
                    DateOnly.FromDateTime(x.DateTime) <= to)
                .Select(x => new MarketDataHistoricalCandle(
                    Date: DateOnly.FromDateTime(x.DateTime),
                    Open: Convert.ToDecimal(x.Open, CultureInfo.InvariantCulture),
                    High: Convert.ToDecimal(x.High, CultureInfo.InvariantCulture),
                    Low: Convert.ToDecimal(x.Low, CultureInfo.InvariantCulture),
                    Close: Convert.ToDecimal(x.Close, CultureInfo.InvariantCulture),
                    Volume: Convert.ToInt64(x.Volume, CultureInfo.InvariantCulture)))
                .OrderBy(x => x.Date)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Yahoo Finance history call failed for symbol {Symbol}.", symbol);
            return null;
        }
    }

    internal async Task<MarketDataStockProfileSummary?> GetProfileAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var quote = await GetQuoteAsync(symbol, cancellationToken);

        if (quote is null)
        {
            return null;
        }

        return new MarketDataStockProfileSummary(
            Ticker: quote.Symbol,
            Active: true,
            Cik: null,
            CurrencyName: quote.Currency,
            Description: null,
            HomepageUrl: null,
            ListDate: null,
            Locale: null,
            Market: quote.Market,
            MarketCap: quote.MarketCap,
            Name: quote.Name,
            PhoneNumber: null,
            PrimaryExchange: quote.ExchangeCode,
            Type: quote.Type,
            WeightedSharesOutstanding: null,
            IconUrl: null,
            LogoUrl: null);
    }

    private async Task<YahooQuote?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calling Yahoo Finance quote for symbol {Symbol}.", symbol);

            var normalizedSymbol = symbol.Trim().ToUpperInvariant();

            var securities = await _requestGate.ExecuteAsync(
                async token =>
                {
                    token.ThrowIfCancellationRequested();

                    return await YahooClient
                        .Symbols(normalizedSymbol)
                        .Fields(
                            Field.Symbol,
                            Field.LongName,
                            Field.ShortName,
                            Field.Currency,
                            Field.FinancialCurrency,
                            Field.RegularMarketPrice,
                            Field.RegularMarketPreviousClose,
                            Field.RegularMarketOpen,
                            Field.RegularMarketDayHigh,
                            Field.RegularMarketDayLow,
                            Field.RegularMarketVolume,
                            Field.RegularMarketTime,
                            Field.FullExchangeName,
                            Field.Exchange,
                            Field.Market,
                            Field.QuoteType,
                            Field.MarketCap)
                        .QueryAsync();
                },
                cancellationToken);

            if (securities is null || !securities.TryGetValue(normalizedSymbol, out var security))
            {
                return null;
            }

            var providerSymbol = GetString(security, Field.Symbol) ?? normalizedSymbol;
            var name = GetString(security, Field.LongName) ?? GetString(security, Field.ShortName);
            var currency = GetString(security, Field.Currency) ?? GetString(security, Field.FinancialCurrency);
            var exchange = GetString(security, Field.FullExchangeName) ?? GetString(security, Field.Exchange);

            return new YahooQuote(
                Symbol: normalizedSymbol,
                ProviderSymbol: providerSymbol,
                Name: name,
                Currency: currency,
                CurrentPrice: GetDecimal(security, Field.RegularMarketPrice),
                PreviousClose: GetDecimal(security, Field.RegularMarketPreviousClose),
                Open: GetDecimal(security, Field.RegularMarketOpen),
                High: GetDecimal(security, Field.RegularMarketDayHigh),
                Low: GetDecimal(security, Field.RegularMarketDayLow),
                Volume: GetLong(security, Field.RegularMarketVolume),
                TimestampUtc: GetDateTimeOffset(security, Field.RegularMarketTime),
                ExchangeCode: exchange,
                Market: GetString(security, Field.Market),
                Type: GetString(security, Field.QuoteType),
                MarketCap: GetDecimal(security, Field.MarketCap));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Yahoo Finance quote call failed for symbol {Symbol}.", symbol);
            return null;
        }
    }

    private static string? GetString(dynamic security, Field field)
    {
        var value = GetValue(security, field);
        return value?.ToString();
    }

    private static decimal? GetDecimal(dynamic security, Field field)
    {
        var value = GetValue(security, field);

        if (value is null)
        {
            return null;
        }

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    private static long? GetLong(dynamic security, Field field)
    {
        var value = GetValue(security, field);

        if (value is null)
        {
            return null;
        }

        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset? GetDateTimeOffset(dynamic security, Field field)
    {
        var value = GetValue(security, field);

        return value switch
        {
            null => null,
            DateTime dateTime => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToUniversalTime(),
            long unixSeconds => DateTimeOffset.FromUnixTimeSeconds(unixSeconds),
            int unixSeconds => DateTimeOffset.FromUnixTimeSeconds(unixSeconds),
            _ => null
        };
    }

    private static object? GetValue(dynamic security, Field field)
    {
        try
        {
            return security[field];
        }
        catch
        {
            return null;
        }
    }

    private sealed record YahooQuote(
        string Symbol,
        string ProviderSymbol,
        string? Name,
        string? Currency,
        decimal? CurrentPrice,
        decimal? PreviousClose,
        decimal? Open,
        decimal? High,
        decimal? Low,
        long? Volume,
        DateTimeOffset? TimestampUtc,
        string? ExchangeCode,
        string? Market,
        string? Type,
        decimal? MarketCap);
}
