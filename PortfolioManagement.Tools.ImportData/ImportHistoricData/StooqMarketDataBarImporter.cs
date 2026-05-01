using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;
using System.Globalization;

namespace PortfolioManagement.Tools.ImportData.ImportHistoricData;

public static class StooqMarketDataBarImporter
{
    private const string ExpectedHeader =
        "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>,<OPENINT>";

    public static async Task ImportAsync(string[] files)
    {
        var connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        
        foreach (var file in files)
        {
            await using var dbContext = new PortfolioDbContext(options);
            await ImportFileAsync(dbContext, file);
        }
    }

    private static async Task ImportFileAsync(PortfolioDbContext dbContext, string path)
    {
        Console.WriteLine();
        Console.WriteLine($"Importing file: {Path.GetFileName(path)}");

        string[] lines = await File.ReadAllLinesAsync(path);

        if (lines.Length < 2)
        {
            Console.WriteLine("File does not contain any data rows.");
            return;
        }

        if (!string.Equals(lines[0].Trim(), ExpectedHeader, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Skipped. Header does not match expected Stooq format.");
            return;
        }

        var symbol = lines[1].Split(',')[0].Split('.')[0];

        Console.WriteLine(symbol);

        var instrument = await dbContext.Instruments
            .FirstOrDefaultAsync(i => i.Symbol == symbol);


        if (instrument is null)
        {
            Console.WriteLine($"Instrument with symbol: '{symbol}' was not found");
            return;
        }

        var existingDates = await dbContext.MarketDataBars
            .AsNoTracking()
            .Where(x => x.InstrumentId == instrument.Id &&
                x.Period == MarketDataPeriod.Daily)
            .Select(x => x.Date)
            .ToHashSetAsync();

        var processedCount = 0;
        var addedCount = 0;
        var skippedCount = 0;

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            processedCount++;

            var columns = line.Split(',');

            var date = DateOnly.ParseExact(columns[2], "yyyyMMdd", CultureInfo.InvariantCulture);

            if (existingDates.Contains(date))
            {
                skippedCount++;
            }
            else
            {
                var open = decimal.Round(decimal.Parse(columns[4], CultureInfo.InvariantCulture), 8);
                var high = decimal.Round(decimal.Parse(columns[5], CultureInfo.InvariantCulture), 8);
                var low = decimal.Round(decimal.Parse(columns[6], CultureInfo.InvariantCulture), 8);
                var close = decimal.Round(decimal.Parse(columns[7], CultureInfo.InvariantCulture), 8);
                var volume = (long)decimal.Parse(columns[8], CultureInfo.InvariantCulture);

                if (high < low)
                {
                    skippedCount++;
                    Console.WriteLine();
                    Console.WriteLine($"Skipped invalid OHLC row in {Path.GetFileName(path)}:");
                    Console.WriteLine($"Date: {date}, High: {high}, Low: {low}");
                    continue;
                }

                instrument.AddMarketDataBar(
                    date,
                    MarketDataPeriod.Daily,
                    open,
                    high,
                    low,
                    close,
                    volume
                );

                existingDates.Add(date);
                addedCount++;
            }
        }

        Console.WriteLine();

        try
        {
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while importing {Path.GetFileName(path)}:");
            Console.WriteLine(ex.ToString());

            dbContext.ChangeTracker.Clear();
            return;
        }

        Console.WriteLine($"Finished. Processed: {processedCount}, Added: {addedCount}, Skipped: {skippedCount}");
    }

    public static string[] ResolveImportFiles(string path)
    {
        if (File.Exists(path))
            return [path];

        if (Directory.Exists(path))
            return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);

        return [];
    }
}