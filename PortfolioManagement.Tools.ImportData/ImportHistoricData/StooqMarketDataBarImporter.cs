using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;
using System.Globalization;

namespace PortfolioManagement.Tools.ImportData.ImportHistoricData;

public static class StooqMarketDataBarImporter
{
    public static async Task ImportAsync(string path)
    {
        var connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;";

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new PortfolioDbContext(options);

        string[] lines = await File.ReadAllLinesAsync(path);

        var symbol = lines[1].Split(',')[0].Split('.')[0];

        Console.WriteLine(symbol);

        var instrument = await dbContext.Instruments.FirstOrDefaultAsync(i => i.Symbol == symbol);

        if (instrument is null)
        {
            Console.WriteLine($"Instrument with symbol: '{symbol}' was not found");
        }

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = line.Split(',');

            var marketDataBar = instrument.AddMarketDataBar(
                DateOnly.ParseExact(columns[2], "yyyyMMdd", CultureInfo.InvariantCulture),
                MarketDataPeriod.Daily,
                decimal.Parse(columns[4], CultureInfo.InvariantCulture),
                decimal.Parse(columns[5], CultureInfo.InvariantCulture),
                decimal.Parse(columns[6], CultureInfo.InvariantCulture),
                decimal.Parse(columns[7], CultureInfo.InvariantCulture),
                long.Parse(columns[8], CultureInfo.InvariantCulture)
            );

            Console.WriteLine(
                $"{marketDataBar.Date} | {marketDataBar.Period} | " +
                $"O:{marketDataBar.Open} H:{marketDataBar.High} L:{marketDataBar.Low} C:{marketDataBar.Close} | " +
                $"V:{marketDataBar.Volume}"
            );
        }

        /*
        AAPL
           07/09/1984 | Daily | O:0.0991725 H:0.10039 L:0.0979751 C:0.0991725 | V:99242379
           10/09/1984 | Daily | O:0.0991725 H:0.0994767 L:0.096788 C:0.0985838 | V:77028276
           11/09/1984 | Daily | O:0.0994767 H:0.102175 L:0.0994767 C:0.10039 | V:181637249
        */

        Console.ReadLine();
    }
}