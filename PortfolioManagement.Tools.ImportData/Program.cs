using PortfolioManagement.Tools.ImportData.Helpers;
using PortfolioManagement.Tools.ImportData.ImportHistoricData;
using PortfolioManagement.Tools.ImportData.ImportInstruments;
using PortfolioManagement.Tools.ImportData.UpdateMetadataInstrument;

/*V------------------------------------- Main Console Interaction -------------------------------------V*/

int option = 1;
bool isSelected = false;
string colorGreen = "[X] \u001b[32m";

Console.CursorVisible = false;

try
{
    while (!isSelected)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("This tool is used to Import Data or Update Metadata");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("\n-- Use the keys 'Up' and 'Down' to navigate and press ");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Enter");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" to select:\n");

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("-- Import data --");
        Console.ResetColor();
        Console.WriteLine($"{(option == 1 ? colorGreen : "[ ] ")}Instruments\u001b[0m");
        Console.WriteLine($"{(option == 2 ? colorGreen : "[ ] ")}Historic Data\u001b[0m");

        Console.WriteLine("");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("-- Update Metadata --");
        Console.ResetColor();
        Console.WriteLine($"{(option == 3 ? colorGreen : "[ ] ")}Instruments Metadata\u001b[0m");

        ConsoleKeyInfo key = Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.DownArrow:
                option = (option == 3 ? 3 : option + 1);
                break;

            case ConsoleKey.UpArrow:
                option = (option == 1 ? 1 : option - 1);
                break;

            case ConsoleKey.Enter:
                isSelected = true;
                break;
        }
    }
}
finally
{
    Console.CursorVisible = true;
    Console.ResetColor();
}

var selected = option switch
{
    1 => "Instruments",
    2 => "Historic Data",
    3 => "Instruments Metadata",
    _ => throw new InvalidOperationException("Invalid selection")
};

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n***************************************************************\n");
Console.ResetColor();

/*^--------------------------------------- ******************** ---------------------------------------^*/

/*V--------------------------------------- Instruments from SEC ---------------------------------------V*/

if (selected == "Instruments")
{
    var path = ConsolePrompts.AskForExistingFilePath("SEC file");

    if (path is null) return;

    if (!ConsolePrompts.ConfirmProceedImport()) return;

    Console.WriteLine("Working...\n");

    await SecJsonImporter.ImportAsync(path);

    Console.WriteLine("\nImport process has ended.");
    Console.ReadLine(); 
}

/*^--------------------------------------- ******************** ---------------------------------------^*/

/*V------------------------------------- Historic Data from Stooq -------------------------------------V*/

if (selected == "Historic Data")
{
    var path = ConsolePrompts.AskForExistingFileOrDirectoryPath("Historic Data (file or directory)");

    if (path is null) return;

    var files = StooqMarketDataBarImporter.ResolveImportFiles(path);

    if (files.Length == 0)
    {
        Console.WriteLine("No .txt files found.");
        Console.ReadLine();
        return;
    }

    if (Directory.Exists(path))
    {
        Console.WriteLine($"Found {files.Length} .txt files in directory:");
        Console.WriteLine(path);
    }
    else
    {
        Console.WriteLine($"Found 1 file:");
        Console.WriteLine(files[0]);
    }

    if (!ConsolePrompts.ConfirmProceedImport()) return;

    Console.WriteLine("Working...\n");

    await StooqMarketDataBarImporter.ImportAsync(files);

    Console.WriteLine("\nImport process has ended.");
    Console.ReadLine();
}

/*^--------------------------------------- ******************** ---------------------------------------^*/

/*V----------------------------------- Update Metadata - Instrument -----------------------------------V*/

if (selected == "Instruments Metadata")
{
    var folderPath = ConsolePrompts.AskForExistingDirectoryPath("Instruments metadata folder");

    if (folderPath is null) return;

    var detectedExchangeName = ConsolePrompts.DetectExchangeName(folderPath);

    if (detectedExchangeName is null)
    {
        Console.WriteLine("Could not detect exchange name from folder.");
        Console.ReadLine();
        return;
    }

    var exchangeCode = ConsolePrompts.AskForExchangeCode(detectedExchangeName);

    if (exchangeCode is null) return;

    var files = Directory.GetFiles(folderPath);
    var fileCount = files.Length;

    if (fileCount == 0)
    {
        Console.WriteLine("No files found in directory.");
        Console.ReadLine();
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Found {fileCount} file(s).");
    Console.WriteLine($"Exchange code to apply: {exchangeCode}");

    if (!ConsolePrompts.ConfirmProceedUpdate($"updating {fileCount} instrument(s) with exchange '{exchangeCode}'"))
        return;

    Console.WriteLine("Working...\n");

    await InstrumentMetadataUpdater.UpdateExchangeAsync(files, exchangeCode);

    Console.WriteLine("\nUpdate process ended.");
    Console.ReadLine();
}

/*^--------------------------------------- ******************** ---------------------------------------^*/