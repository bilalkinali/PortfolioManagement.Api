using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Finnhub;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Api.Features.StockProfiles.GetStockProfile.Proxy;

namespace PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;

public sealed class GetStockProfileHandler
{
    private readonly PortfolioDbContext _db;
    private readonly FinnhubProfileProxy _finnhubProfileProxy;
    private readonly MassiveProfileProxy _massiveProfileProxy;
    private readonly MarketDataProviderRouter _providerRouter;
    private readonly YahooMarketDataProxy _yahooMarketDataProxy;

    public GetStockProfileHandler(
        PortfolioDbContext db,
        MassiveProfileProxy massiveProfileProxy,
        FinnhubProfileProxy finnhubProfileProxy,
        YahooMarketDataProxy yahooMarketDataProxy,
        MarketDataProviderRouter providerRouter)
    {
        _db = db;
        _massiveProfileProxy = massiveProfileProxy;
        _finnhubProfileProxy = finnhubProfileProxy;
        _yahooMarketDataProxy = yahooMarketDataProxy;
        _providerRouter = providerRouter;
    }

    public async Task<GetStockProfileResponse?> Handle(
        GetStockProfileRequest request,
        CancellationToken cancellationToken)
    {
        var ticker = request.Ticker.Trim().ToUpperInvariant();

        var instrument = await _db.Instruments
            .Include(i => i.StockProfile)
            .FirstOrDefaultAsync(i => i.Symbol == ticker, cancellationToken);

        instrument ??= await CreateInstrumentFromProviderAsync(ticker, cancellationToken);

        if (instrument is null)
        {
            return null;
        }
        
        if (instrument.StockProfile is not null)
        {
            return MapToResponse(instrument.StockProfile);
        }

        var provider = _providerRouter.ResolveQuoteProvider(ticker, instrument.ExchangeCode);
        var providerSymbol = _providerRouter.ResolveProviderSymbol(provider, ticker, instrument.ProviderSymbol);
        var profileSummary = provider == MarketDataProvider.Yahoo
            ? await _yahooMarketDataProxy.GetProfileAsync(providerSymbol, cancellationToken)
            : await _finnhubProfileProxy.GetProfileAsync(providerSymbol, cancellationToken);

        if (profileSummary is null && provider == MarketDataProvider.Finnhub)
        {
            var massiveTickerInfo = await _massiveProfileProxy.GetProfileFromProxyAsync(ticker, cancellationToken);

            if (massiveTickerInfo is not null)
            {
                var massiveProfile = await CreateAndSaveMassiveProfileAsync(
                    instrument.Id,
                    ticker,
                    massiveTickerInfo,
                    cancellationToken);

                return MapToResponse(massiveProfile);
            }
        }

        if (profileSummary is null)
        {
            return null;
        }

        var newProfile = await CreateAndSaveProfileAsync(
            instrument.Id,
            profileSummary,
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

    private async Task<Instrument?> CreateInstrumentFromProviderAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var provider = _providerRouter.ResolveSearchProvider(ticker);
        IReadOnlyList<MarketDataInstrumentLookupResult>? lookupResults = null;

        if (provider == MarketDataProvider.Yahoo)
        {
            lookupResults = await _yahooMarketDataProxy.LookupAsync(ticker, cancellationToken);
        }
        else
        {
            var profile = await _finnhubProfileProxy.GetProfileAsync(ticker, cancellationToken);

            if (profile is not null)
            {
                lookupResults =
                [
                    new MarketDataInstrumentLookupResult(
                        Symbol: profile.Ticker,
                        Name: profile.Name ?? profile.Ticker,
                        ProviderSymbol: profile.Ticker,
                        Cik: profile.Cik is not null && int.TryParse(profile.Cik, out var cik) ? cik : null,
                        Market: profile.Market,
                        ExchangeCode: profile.PrimaryExchange,
                        Currency: profile.CurrencyName,
                        Type: profile.Type)
                ];
            }
        }

        var lookup = lookupResults?.FirstOrDefault();

        if (lookup is null)
        {
            return null;
        }

        var instrument = Instrument.Create(
            symbol: lookup.Symbol,
            name: lookup.Name,
            providerSymbol: lookup.ProviderSymbol,
            cik: lookup.Cik,
            market: lookup.Market,
            exchangeCode: lookup.ExchangeCode,
            currency: lookup.Currency,
            type: lookup.Type);

        _db.Instruments.Add(instrument);
        await _db.SaveChangesAsync(cancellationToken);

        return instrument;
    }

    private async Task<StockProfile> CreateAndSaveProfileAsync(
        int instrumentId,
        MarketDataStockProfileSummary profileSummary,
        CancellationToken cancellationToken)
    {
        var profile = StockProfile.Create(
            instrumentId: instrumentId,
            ticker: profileSummary.Ticker,
            active: profileSummary.Active,
            cik: profileSummary.Cik,
            compositeFigi: null,
            currencyName: profileSummary.CurrencyName,
            description: profileSummary.Description,
            homepageUrl: profileSummary.HomepageUrl,
            listDate: profileSummary.ListDate,
            locale: profileSummary.Locale,
            market: profileSummary.Market,
            marketCap: profileSummary.MarketCap,
            name: profileSummary.Name,
            phoneNumber: profileSummary.PhoneNumber,
            primaryExchange: profileSummary.PrimaryExchange,
            roundLot: null,
            shareClassFigi: null,
            shareClassSharesOutstanding: null,
            sicCode: null,
            sicDescription: null,
            tickerRoot: null,
            tickerSuffix: null,
            totalEmployees: null,
            type: profileSummary.Type,
            weightedSharesOutstanding: profileSummary.WeightedSharesOutstanding,
            addressLine1: null,
            city: null,
            state: null,
            postalCode: null,
            iconUrl: profileSummary.IconUrl,
            logoUrl: profileSummary.LogoUrl,
            delistedUtc: null,
            lastSyncedAtUtc: DateOnly.FromDateTime(DateTime.UtcNow));

        _db.StockProfiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);

        return profile;
    }

    private async Task<StockProfile> CreateAndSaveMassiveProfileAsync(
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
