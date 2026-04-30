using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Tools.ImportSecInstruments;

public class SecJsonImporter
{
    private readonly string _path;
    private readonly PortfolioDbContext _dbContext;

    public SecJsonImporter(string path, PortfolioDbContext dbContext)
    {
        _path = path;
        _dbContext = dbContext;
    }

    public async Task ImportAsync()
    {
        var json = await File.ReadAllTextAsync(_path);

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

            var existingInstrument = await _dbContext.Instruments
                .FirstOrDefaultAsync(x => x.Symbol == symbol);

            if (existingInstrument is null)
            {
                var instrument = Instrument.Create(symbol, name, company.Cik);

                _dbContext.Instruments.Add(instrument);
                ++inserted;
            }
            else
            {
                existingInstrument.UpdateMetadata(name, company.Cik);
                ++updated;
            }
        }

        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"Inserted: {inserted}");
        Console.WriteLine($"Updated: {updated}");
        Console.WriteLine($"Total: {companies.Count}");
    }
}