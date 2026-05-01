using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Tools.ImportData.ImportInstruments;

public static class SecJsonImporter
{
    public static async Task ImportAsync(string path)
    {
        var connectionString = "";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new PortfolioDbContext(options);

        var json = await File.ReadAllTextAsync(path);

        var companies = JsonSerializer.Deserialize<Dictionary<string, SecCompany>>(json);

        if (companies is null)
        {
            Console.WriteLine("Could not parse JSON.");
            return;
        }

        var inserted = 0;
        var updated = 0;

        foreach (var company in companies.Values)
        {
            var symbol = company.Ticker.Trim().ToUpperInvariant();
            var name = company.Title.Trim();

            try
            {
                var existingInstrument = await dbContext.Instruments
                        .FirstOrDefaultAsync(x => x.Symbol == symbol);

                if (existingInstrument is null)
                {
                    var instrument = Instrument.Create(symbol: symbol, name: name, cik: company.Cik);

                    dbContext.Instruments.Add(instrument);
                    ++inserted;
                }
                else
                {
                    existingInstrument.UpdateMetadata(name: name, cik: company.Cik);
                    ++updated;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect to server.");
                return;
            }
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong: {ex.Message}", ex);
            return;
        }

        Console.WriteLine($"Inserted: {inserted}");
        Console.WriteLine($"Updated: {updated}");
        Console.WriteLine($"Total: {companies.Count}");
    }
}