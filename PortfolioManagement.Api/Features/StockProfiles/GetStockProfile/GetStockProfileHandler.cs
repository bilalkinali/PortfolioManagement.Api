using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;
using System.Text.Json;
using PortfolioManagement.Api.Features.StockProfiles.GetStockProfile.Proxy;

namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public sealed class GetStockProfileHandler
{
    private readonly PortfolioDbContext _db;
    private readonly MassiveProfileProxy _proxy;

    public GetStockProfileHandler(
        PortfolioDbContext db,
        MassiveProfileProxy proxy)
    {
        _db = db;
        _proxy = proxy;
    }

    public async Task<GetStockProfileResponse?> Handle(
        GetStockProfileRequest request,
        CancellationToken cancellationToken)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();

        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(x => x.Symbol == ticker, cancellationToken);

        if (instrument is null)
        {
            return null;
        }

        var profile = await _db.StockProfiles
            .FirstOrDefaultAsync(x => x.InstrumentId == instrument.Id, cancellationToken);

        if (profile is not null)
        {
            return MapToResponse(profile);
        }

        var massiveTickerInfo = await _proxy.GetProfileFromProxyAsync(ticker, cancellationToken);

        if (massiveTickerInfo is null)
        {
            return null;
        }

        var newProfile = await CreateAndSaveProfileAsync(
            instrument.Id,
            ticker,
            massiveTickerInfo,
            cancellationToken);

        return MapToResponse(newProfile);
    }

    private static GetStockProfileResponse MapToResponse(StockProfile profile)
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
                ? new AddressResponse(profile.AddressLine1, profile.City, profile.State, profile.PostalCode)
                : null,
            Branding: profile.IconUrl is not null || profile.LogoUrl is not null
                ? new BrandingResponse(profile.IconUrl, profile.LogoUrl)
                : null,
            DelistedUtc: profile.DelistedUtc,
            LastSyncedDate: profile.LastSyncedAtUtc);
    }

    private async Task<StockProfile> CreateAndSaveProfileAsync(
    int instrumentId,
    string fallbackTicker,
    MassiveTickerOverviewResponse.TickerInfo massiveTickerInfo,
    CancellationToken cancellationToken)
    {
        var profile = StockProfile.Create(
            instrumentId: instrumentId,
            ticker: massiveTickerInfo.Ticker ?? fallbackTicker,
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
            delistedUtc: massiveTickerInfo.DelistedUtc,
            lastSyncedAtUtc: DateOnly.FromDateTime(DateTime.UtcNow));

        _db.StockProfiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);

        return profile;
    }
}
