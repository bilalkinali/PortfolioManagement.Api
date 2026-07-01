using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
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
                NullLogger<MassiveStockHistoryProxy>.Instance),
            new MarketDataProviderRouter(),
            new YahooMarketDataProxy(
                new YahooRequestGate(),
                NullLogger<YahooMarketDataProxy>.Instance),
            new ConfigurationBuilder().Build());
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

    [Fact]
    public async Task Handle_returns_local_bars_without_proxy_call_when_local_cache_covers_range()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create(
            symbol: "AAPL",
            name: "Apple",
            providerSymbol: "AAPL.US",
            currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        instrument.AddMarketDataBar(new DateOnly(2026, 5, 19), MarketDataPeriod.Daily, 90m, 100m, 89m, 98m, 100);
        instrument.AddMarketDataBar(new DateOnly(2026, 6, 1), MarketDataPeriod.Daily, 100m, 110m, 99m, 108m, 200);
        instrument.AddMarketDataBar(new DateOnly(2026, 6, 15), MarketDataPeriod.Daily, 108m, 112m, 105m, 110m, 300);
        db.Instruments.Add(instrument);
        await db.SaveChangesAsync();

        var httpClientFactory = new TestHttpClientFactory();
        var handler = CreateHandler(db, httpClientFactory, new Dictionary<string, string?>
        {
            ["Massive:ApiKey"] = "configured"
        });
        var request = new GetStockHistoryRequest
        {
            Ticker = "AAPL",
            From = "2026-05-19",
            To = "2026-06-15",
            Timespan = "day",
            Range = "1M"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result!.ResultsCount);
        Assert.Equal(0, httpClientFactory.RequestCount);
    }

    [Fact]
    public async Task Handle_fetches_tail_range_when_local_cache_is_missing_newer_bars()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create(
            symbol: "AAPL",
            name: "Apple",
            providerSymbol: "AAPL.US",
            currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        instrument.AddMarketDataBar(new DateOnly(2026, 5, 19), MarketDataPeriod.Daily, 90m, 100m, 89m, 98m, 100);
        instrument.AddMarketDataBar(new DateOnly(2026, 6, 1), MarketDataPeriod.Daily, 100m, 110m, 99m, 108m, 200);
        db.Instruments.Add(instrument);
        await db.SaveChangesAsync();

        var httpClientFactory = new TestHttpClientFactory(CreateMassiveHistoryResponse(
            ("2026-06-15", 108m, 112m, 105m, 110m, 300)));
        var handler = CreateHandler(db, httpClientFactory, new Dictionary<string, string?>
        {
            ["Massive:ApiKey"] = "configured"
        });
        var request = new GetStockHistoryRequest
        {
            Ticker = "AAPL",
            From = "2026-05-19",
            To = "2026-06-15",
            Timespan = "day",
            Range = "1M"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result!.ResultsCount);
        Assert.Equal(1, httpClientFactory.RequestCount);
        Assert.Equal(
            [new DateOnly(2026, 5, 19), new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 15)],
            result.Results!.Select(x => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).UtcDateTime)));
        Assert.Equal(3, await db.MarketDataBars.CountAsync());
        Assert.Contains("/v2/aggs/ticker/AAPL/range/1/day/2026-06-02/2026-06-15", httpClientFactory.RequestUris.Single());
    }

    [Fact]
    public async Task Handle_fetches_tail_range_for_HYLN_with_massive_symbol()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create(
            symbol: "HYLN",
            name: "Hyliion",
            providerSymbol: "HYLN.US",
            exchangeCode: "NYSE American",
            currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        instrument.AddMarketDataBar(new DateOnly(2026, 5, 19), MarketDataPeriod.Daily, 1.90m, 2.00m, 1.89m, 1.98m, 100);
        instrument.AddMarketDataBar(new DateOnly(2026, 6, 1), MarketDataPeriod.Daily, 2.00m, 2.10m, 1.99m, 2.08m, 200);
        db.Instruments.Add(instrument);
        await db.SaveChangesAsync();

        var httpClientFactory = new TestHttpClientFactory(CreateMassiveHistoryResponse(
            ("2026-06-15", 2.08m, 2.12m, 2.05m, 2.10m, 300)));
        var handler = CreateHandler(db, httpClientFactory, new Dictionary<string, string?>
        {
            ["Massive:ApiKey"] = "configured"
        });
        var request = new GetStockHistoryRequest
        {
            Ticker = "HYLN",
            From = "2026-05-19",
            To = "2026-06-15",
            Timespan = "day",
            Range = "1M"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result!.ResultsCount);
        Assert.Equal(1, httpClientFactory.RequestCount);
        Assert.Equal(
            [new DateOnly(2026, 5, 19), new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 15)],
            result.Results!.Select(x => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).UtcDateTime)));
        Assert.Equal(3, await db.MarketDataBars.CountAsync());
        Assert.Contains("/v2/aggs/ticker/HYLN/range/1/day/2026-06-02/2026-06-15", httpClientFactory.RequestUris.Single());
    }

    [Fact]
    public async Task Handle_fetches_head_range_when_local_cache_is_missing_older_bars()
    {
        await using var db = CreateDbContext();
        var instrument = Instrument.Create(
            symbol: "AAPL",
            name: "Apple",
            providerSymbol: "AAPL.US",
            currency: "USD");
        DomainTestIds.SetId(instrument, 1);
        instrument.AddMarketDataBar(new DateOnly(2026, 6, 15), MarketDataPeriod.Daily, 108m, 112m, 105m, 110m, 300);
        db.Instruments.Add(instrument);
        await db.SaveChangesAsync();

        var httpClientFactory = new TestHttpClientFactory(CreateMassiveHistoryResponse(
            ("2026-05-19", 90m, 100m, 89m, 98m, 100)));
        var handler = CreateHandler(db, httpClientFactory, new Dictionary<string, string?>
        {
            ["Massive:ApiKey"] = "configured"
        });
        var request = new GetStockHistoryRequest
        {
            Ticker = "AAPL",
            From = "2026-05-19",
            To = "2026-06-15",
            Timespan = "day",
            Range = "1M"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.ResultsCount);
        Assert.Equal(1, httpClientFactory.RequestCount);
        Assert.Equal(
            [new DateOnly(2026, 5, 19), new DateOnly(2026, 6, 15)],
            result.Results!.Select(x => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).UtcDateTime)));
        Assert.Equal(2, await db.MarketDataBars.CountAsync());
        Assert.Contains("/v2/aggs/ticker/AAPL/range/1/day/2026-05-19/2026-06-14", httpClientFactory.RequestUris.Single());
    }

    private static PortfolioDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PortfolioDbContext(options);
    }

    private static GetStockHistoryHandler CreateHandler(
        PortfolioDbContext db,
        TestHttpClientFactory httpClientFactory,
        Dictionary<string, string?>? configurationValues = null)
    {
        return new GetStockHistoryHandler(
            db,
            new MassiveStockHistoryProxy(
                httpClientFactory,
                NullLogger<MassiveStockHistoryProxy>.Instance),
            new MarketDataProviderRouter(),
            new YahooMarketDataProxy(
                new YahooRequestGate(),
                NullLogger<YahooMarketDataProxy>.Instance),
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string?>())
                .Build());
    }

    private static string CreateMassiveHistoryResponse(
        params (string Date, decimal Open, decimal High, decimal Low, decimal Close, long Volume)[] bars)
    {
        return JsonSerializer.Serialize(new
        {
            adjusted = true,
            queryCount = bars.Length,
            request_id = "test",
            results = bars.Select(x => new
            {
                c = x.Close,
                h = x.High,
                l = x.Low,
                n = 1,
                o = x.Open,
                t = ToUnixTimeMilliseconds(DateOnly.Parse(x.Date)),
                v = x.Volume,
                vw = (decimal?)null
            }),
            resultsCount = bars.Length,
            status = "OK",
            ticker = "AAPL"
        });
    }

    private static long ToUnixTimeMilliseconds(DateOnly date)
        => new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds();

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly string? _responseBody;

        public TestHttpClientFactory(string? responseBody = null)
        {
            _responseBody = responseBody;
        }

        public int RequestCount { get; private set; }
        public List<string> RequestUris { get; } = [];

        public HttpClient CreateClient(string name)
            => new(new CountingHandler(this))
            {
                BaseAddress = new Uri("https://example.test")
            };

        private sealed class CountingHandler(TestHttpClientFactory factory) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                factory.RequestCount++;
                factory.RequestUris.Add(request.RequestUri?.ToString() ?? string.Empty);

                if (factory._responseBody is null)
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(factory._responseBody)
                });
            }
        }
    }
}
