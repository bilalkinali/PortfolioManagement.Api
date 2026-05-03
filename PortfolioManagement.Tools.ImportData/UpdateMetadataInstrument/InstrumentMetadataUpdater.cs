using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Tools.ImportData.UpdateMetadataInstrument;

public static class InstrumentMetadataUpdater
{
    private const string ExpectedHeader =
        "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>,<OPENINT>";

    public static async Task UpdateExchangeAsync(string[] files, string exchangeCode)
    {
        var connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new PortfolioDbContext(options);

        await UpdateAsync(dbContext, files, exchangeCode);
    }

    private static async Task UpdateAsync(PortfolioDbContext dbContext, string[] files, string exchangeCode)
    {
        var symbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var symbol = await TryGetSymbolFromFileAsync(file);

            if (symbol is null)
            {
                Console.WriteLine($"Could not read symbol from file: {Path.GetFileName(file)}");
                continue;
            }

            symbols.Add(symbol);
        }

        if (symbols.Count == 0)
        {
            Console.WriteLine("No symbols found from files.");
            return;
        }

        var symbolsArray = symbols.ToArray();

        var instruments = await dbContext.Instruments
            .Where(i => symbolsArray.Contains(i.Symbol))
            .ToListAsync();

        var updatedCount = 0;
        var skippedCount = 0;

        // Hardcoded for now since it's the same
        var currency = "usd";
        var market = "Stocks";
        var type = "CS";

        var instrumentsBySymbol = instruments.ToDictionary(
            i => i.Symbol,
            StringComparer.OrdinalIgnoreCase);

        foreach (var symbol in symbols)
        {
            if (!instrumentsBySymbol.TryGetValue(symbol, out var instrument))
            {
                skippedCount++;
                Console.WriteLine($"Instrument with symbol '{symbol}' was not found.");
                continue;
            }

            instrument.Enrich(exchangeCode, currency, market, type);
            updatedCount++;
        }

        await dbContext.SaveChangesAsync();

        Console.WriteLine();
        Console.WriteLine($"Finished metadata update. Updated: {updatedCount}, Skipped: {skippedCount}");
    }

    private static async Task<string?> TryGetSymbolFromFileAsync(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);

        if (lines.Length < 2)
            return null;

        if (!string.Equals(lines[0].Trim(), ExpectedHeader, StringComparison.OrdinalIgnoreCase))
            return null;

        var columns = lines[1].Split(',');

        if (columns.Length == 0 || string.IsNullOrWhiteSpace(columns[0]))
            return null;

        return columns[0].Split('.')[0].Trim();
    }
}