using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Features.Instruments.SearchInstruments.Proxy;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed class SearchInstrumentsHandler
{
    private readonly PortfolioDbContext _db;
    private readonly ILogger<SearchInstrumentsHandler> _logger;
    private readonly MassiveProxy _proxy;

    public SearchInstrumentsHandler(
        PortfolioDbContext db,
        MassiveProxy proxy,
        ILogger<SearchInstrumentsHandler> logger)
    {
        _db = db;
        _logger = logger;
        _proxy = proxy;
    }

    public async Task<SearchInstrumentsResponse?> Handle(SearchInstrumentsRequest request)
    {
        var query = request.Query.Trim();
        var limit = request.Limit ?? 10;
        var type = request.Type ?? SearchInstrumentType.CS;

        var localResults = await SearchLocalDatabase(query, limit);

        if (localResults.Count > 0)
        {
            return new SearchInstrumentsResponse(localResults);
        }

        _logger.LogInformation(
            "No local instruments found for query '{Query}'. Calling Massive API.",
            query);

        var massiveResponse = await _proxy.SearchAsync(query, limit, type);

        if (massiveResponse is null)
        {
            return null;
        }

        if (massiveResponse.Results.Count == 0)
        {
            _logger.LogInformation(
                "No instruments found locally or from Massive for query '{Query}'.",
                query);

            return new SearchInstrumentsResponse([]);
        }

        await SaveMissingMassiveInstruments(massiveResponse.Results);

        var results = massiveResponse.Results
            .Where(ticker =>
                !string.IsNullOrWhiteSpace(ticker.Ticker) &&
                !string.IsNullOrWhiteSpace(ticker.Name))
            .Select(ticker => new SearchInstrumentResult(
                ticker.Ticker!.Trim().ToUpperInvariant(),
                ticker.Name!,
                null,
                ticker.Market,
                ticker.PrimaryExchange,
                ticker.CurrencyName,
                ticker.Type,
                null,
                null))
            .ToList();

        return new SearchInstrumentsResponse(results);
    }

    private async Task<List<SearchInstrumentResult>> SearchLocalDatabase(string query, int limit)
    {
        var pattern = $"%{query}%";
        var startsWithPattern = $"{query}%";

        //var instruments = await _db.Instruments
        //    .AsNoTracking()
        //    .Where(i =>
        //        EF.Functions.ILike(i.Symbol, pattern) ||
        //        EF.Functions.ILike(i.Name, pattern))
        //    .OrderBy(i =>
        //        EF.Functions.ILike(i.Symbol, startsWithPattern) ? 0 :
        //        EF.Functions.ILike(i.Name, startsWithPattern) ? 1 :
        //        2)
        //    .ThenBy(i => i.Symbol)
        //    .Take(limit)
        //    .Select(i => new SearchInstrumentResult(
        //        i.Symbol,
        //        i.Name,
        //        i.Cik,
        //        i.Market,
        //        i.Exchange,
        //        i.Currency,
        //        i.Type))
        //    .ToListAsync();

        return await _db.Instruments
            .AsNoTracking()
            .Where(i =>
                EF.Functions.ILike(i.Symbol, pattern) ||
                EF.Functions.ILike(i.Name, pattern))
            .OrderBy(i =>
                EF.Functions.ILike(i.Symbol, startsWithPattern) ? 0 :
                EF.Functions.ILike(i.Name, startsWithPattern) ? 1 :
                2)
            .ThenBy(i => i.Symbol)
            .Take(limit)
            .Select(i => new
            {
                i.Symbol,
                i.Name,
                i.Cik,
                i.Market,
                i.Exchange,
                i.Currency,
                i.Type,
                LatestBar = _db.MarketDataBars
                    .Where(b => b.InstrumentId == i.Id &&
                                b.Period == MarketDataPeriod.Daily)
                    .OrderByDescending(b => b.Date)
                    .Select(b => new
                    {
                        b.Date,
                        b.Close
                    })
                    .FirstOrDefault()
            })
            .Select(x => new SearchInstrumentResult(
                x.Symbol,
                x.Name,
                x.Cik,
                x.Market,
                x.Exchange,
                x.Currency,
                x.Type,
                x.LatestBar != null ? x.LatestBar.Close : null,
                x.LatestBar != null ? x.LatestBar.Date : null))
            .ToListAsync();
    }


    private async Task SaveMissingMassiveInstruments(List<MassiveTickerResult> massiveResults)
    {
        var validResults = massiveResults
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Ticker) &&
                !string.IsNullOrWhiteSpace(x.Name))
            .ToList();

        var symbols = validResults
            .Select(x => x.Ticker!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var existingSymbols = await _db.Instruments
            .Where(i => symbols.Contains(i.Symbol))
            .Select(i => i.Symbol)
            .ToListAsync();

        var existingSymbolSet = existingSymbols.ToHashSet();

        foreach (var result in validResults)
        {
            var symbol = result.Ticker!.Trim().ToUpperInvariant();

            if (existingSymbolSet.Contains(symbol))
            {
                continue;
            }

            var instrument = Instrument.Create(
                symbol: symbol,
                name: result.Name!,
                cik: null,
                market: result.Market,
                exchange: result.PrimaryExchange,
                currency: result.CurrencyName,
                type: result.Type);


            _logger.LogInformation(
                "Added instrument from Massive. Symbol: {Symbol}, Name: {Name}, Market: {Market}, Exchange: {Exchange}, Currency: {Currency}, Type: {Type}",
                symbol,
                result.Name,
                result.Market,
                result.PrimaryExchange,
                result.CurrencyName,
                result.Type);

            _db.Instruments.Add(instrument);
            existingSymbolSet.Add(symbol);
        }

        await _db.SaveChangesAsync();
    }
}