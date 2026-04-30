using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Tools.ImportData;

/*------------------------------------------------------------------------------------------------*/
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Select the context to import:");
Console.ResetColor();

Console.WriteLine("\n" +
    "-- Use the keys 'Up' and 'Down' to navigate and press \u001b[32mEnter\u001b[0m to select:\n");

var selectionOptions = new Dictionary<int, string>
{
    {1, "Instruments"},
    {2, "Market Data Bars"}
};

int option = 1;
bool isSelected = false;
(int left, int top) = Console.GetCursorPosition();
string color = "[X] \u001b[32m";

Console.CursorVisible = false;

while (!isSelected)
{
    Console.SetCursorPosition(left, top);
    Console.WriteLine($"{(option == 1 ? color : "[ ] ")}Instruments\u001b[0m");
    Console.WriteLine($"{(option == 2 ? color : "[ ] ")}Market Data Bars\u001b[0m");

    ConsoleKeyInfo key = Console.ReadKey(true);

    switch (key.Key)
    {
        case ConsoleKey.DownArrow:
            option = (option == 2 ? 2 : option + 1);
            break;

        case ConsoleKey.UpArrow:
            option = (option == 1 ? 1 : option - 1);
            break;

        case ConsoleKey.Enter:
            isSelected = true;
            break;
    }
}

var selected = selectionOptions.First(o => o.Key == option).Value;

Console.WriteLine($"\nSelected option: \u001b[32m{selected}\u001b[0m");
/*------------------------------------------------------------------------------------------------*/

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