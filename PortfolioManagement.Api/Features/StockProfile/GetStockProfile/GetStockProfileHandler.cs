using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.StockProfile.GetStockProfile;

public sealed class GetStockProfileHandler
{
    private readonly PortfolioDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GetStockProfileHandler> _logger;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GetStockProfileHandler(
        PortfolioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILogger<GetStockProfileHandler> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClientFactory.CreateClient("Massive");
        _logger = logger;
    }

    public async Task<GetStockProfileResponse?> Handle(GetStockProfileRequest request)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();

        var instrument = await _dbContext.Instruments
            .FirstOrDefaultAsync(x => x.Symbol == ticker);

        if (instrument is null)
        {
            return null;
        }

        var profile = await _dbContext.StockProfiles
            .FirstOrDefaultAsync(x => x.InstrumentId == instrument.Id);

        if (profile is not null)
        {
            return MapToResponse(profile);
        }

        var url = request.Date is not null
            ? $"/v3/reference/tickers/{ticker}?date={Uri.EscapeDataString(request.Date)}"
            : $"/v3/reference/tickers/{ticker}";

        _logger.LogInformation("Calling Massive API for ticker profile: {Ticker}", ticker);

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        var apiResult = JsonSerializer.Deserialize<MassiveTickerOverviewResponse>(
            content,
            JsonSerializerOptions);

        if (apiResult?.TickerData is null)
        {
            return null;
        }

        var massiveTickerInfo = apiResult.TickerData;

        var newProfile = Domain.StockProfile.Create(
            instrumentId: instrument.Id,
            ticker: massiveTickerInfo.Ticker ?? ticker,
            active: massiveTickerInfo.Active,
            cik: massiveTickerInfo.Cik,
            compositeFigi: massiveTickerInfo.CompositeFigi,
            currencyName: massiveTickerInfo.CurrencyName,
            description: massiveTickerInfo.Description,
            homepageUrl: massiveTickerInfo.HomepageUrl,
            listDate: massiveTickerInfo.ListDate,
            locale: massiveTickerInfo.Locale,
            market: massiveTickerInfo.Market,
            marketCap: massiveTickerInfo.MarketCap,
            name: massiveTickerInfo.Name,
            phoneNumber: massiveTickerInfo.PhoneNumber,
            primaryExchange: massiveTickerInfo.PrimaryExchange,
            roundLot: massiveTickerInfo.RoundLot,
            shareClassFigi: massiveTickerInfo.ShareClassFigi,
            shareClassSharesOutstanding: massiveTickerInfo.ShareClassSharesOutstanding,
            sicCode: massiveTickerInfo.SicCode,
            sicDescription: massiveTickerInfo.SicDescription,
            tickerRoot: massiveTickerInfo.TickerRoot,
            tickerSuffix: massiveTickerInfo.TickerSuffix,
            totalEmployees: massiveTickerInfo.TotalEmployees,
            type: massiveTickerInfo.Type,
            weightedSharesOutstanding: massiveTickerInfo.WeightedSharesOutstanding,
            addressLine1: massiveTickerInfo.Address?.Address1,
            city: massiveTickerInfo.Address?.City,
            state: massiveTickerInfo.Address?.State,
            postalCode: massiveTickerInfo.Address?.PostalCode,
            iconUrl: massiveTickerInfo.Branding?.IconUrl,
            logoUrl: massiveTickerInfo.Branding?.LogoUrl,
            delistedUtc: massiveTickerInfo.DelistedUtc);

        _dbContext.StockProfiles.Add(newProfile);
        await _dbContext.SaveChangesAsync();

        return MapToResponse(newProfile);
    }

    private static GetStockProfileResponse MapToResponse(Domain.StockProfile profile)
    {
        return new GetStockProfileResponse(
            Active: profile.Active,
            Cik: profile.Cik,
            CompositeFigi: profile.CompositeFigi,
            CurrencyName: profile.CurrencyName,
            Description: profile.Description,
            HomepageUrl: profile.HomepageUrl,
            ListDate: profile.ListDate,
            Locale: profile.Locale,
            Market: profile.Market,
            MarketCap: profile.MarketCap,
            Name: profile.Name,
            PhoneNumber: profile.PhoneNumber,
            PrimaryExchange: profile.PrimaryExchange,
            RoundLot: profile.RoundLot,
            ShareClassFigi: profile.ShareClassFigi,
            ShareClassSharesOutstanding: profile.ShareClassSharesOutstanding,
            SicCode: profile.SicCode,
            SicDescription: profile.SicDescription,
            Ticker: profile.Ticker,
            TickerRoot: profile.TickerRoot,
            TickerSuffix: profile.TickerSuffix,
            TotalEmployees: profile.TotalEmployees,
            Type: profile.Type,
            WeightedSharesOutstanding: profile.WeightedSharesOutstanding,
            Address: profile.AddressLine1 is not null || profile.City is not null
                ? new Address(profile.AddressLine1, profile.City, profile.State, profile.PostalCode)
                : null,
            Branding: profile.IconUrl is not null || profile.LogoUrl is not null
                ? new Branding(profile.IconUrl, profile.LogoUrl)
                : null,
            DelistedUtc: profile.DelistedUtc);
    }
}
