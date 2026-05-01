using PortfolioManagement.Tools.ImportData.Helpers;
using PortfolioManagement.Tools.ImportData.ImportInstruments;

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
        Console.WriteLine("Select the context to import:");
        Console.ResetColor();

        Console.WriteLine("\n-- Use the keys 'Up' and 'Down' to navigate and press \u001b[32mEnter\u001b[0m to select:\n");

        Console.WriteLine($"{(option == 1 ? colorGreen : "[ ] ")}Instruments\u001b[0m");
        Console.WriteLine($"{(option == 2 ? colorGreen : "[ ] ")}Historic Data\u001b[0m");

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
    _ => throw new InvalidOperationException("Invalid selection")
};

Console.WriteLine($"\n\u001b[36m***************************************************************\u001b[0m\n");

if (selected == "Instruments")
{
    var path = ConsoleImportPrompt.AskForExistingFilePath("Sec file");

    if (path is null) return;

    if (!ConsoleImportPrompt.ConfirmProceed()) return;

    Console.WriteLine("Working...\n");

    await SecJsonImporter.ImportAsync(path);

    Console.WriteLine("\nImport process has ended.");
    Console.ReadLine(); 
}

if (selected == "Historic Data")
{
    var path = ConsoleImportPrompt.AskForExistingFilePath("Historic Data file");

    if (path is null) return;

    if (!ConsoleImportPrompt.ConfirmProceed()) return;

    Console.WriteLine("Working...\n");
}