using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Tools.ImportSecInstruments;

Console.WriteLine("Enter path to SEC company_tickers.json:");

var path = Console.ReadLine();

if (string.IsNullOrWhiteSpace(path))
{
    Console.WriteLine("No path entered");
    return;
}

path = path.Trim('"');

if (!File.Exists(path))
{
    Console.WriteLine($"File not found: {path}");
    return;
}

Console.WriteLine("Working...");

var connectionString = "";

var options = new DbContextOptionsBuilder<PortfolioDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new PortfolioDbContext(options);

var importer = new SecJsonImporter(path, dbContext);

await importer.ImportAsync();

Console.ReadLine();