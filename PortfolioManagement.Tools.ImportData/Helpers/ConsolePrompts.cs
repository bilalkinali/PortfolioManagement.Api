namespace PortfolioManagement.Tools.ImportData.Helpers;

public static class ConsolePrompts
{
    public static string? AskForExistingFilePath(string fileDescription)
    {
        Console.Write($"Enter path for the {fileDescription}: ");

        var path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("No path entered");
            Console.ReadLine();
            return null;
        }

        path = path.Trim().Trim('"');

        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            Console.ReadLine();
            return null;
        }

        return path;
    }

    public static string? AskForExistingFileOrDirectoryPath(string label)
    {
        Console.Write($"{label} path: ");
        var path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
            return null;

        path = path.Trim().Trim('"');

        if (File.Exists(path) || Directory.Exists(path))
            return path;

        Console.WriteLine($"Path was not found: {path}");
        return null;
    }

    public static bool ConfirmProceedImport(string actionDescription = "importing this file")
    {
        Console.WriteLine($"\nProceed with {actionDescription}? (y/n)\n");

        var proceed = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(proceed))
        {
            Console.WriteLine("Import cancelled.");
            Console.ReadLine();
            return false;
        }

        proceed = proceed.Trim().ToLowerInvariant();

        if (proceed is not ("y" or "yes"))
        {
            Console.WriteLine("Import cancelled.");
            Console.ReadLine();
            return false;
        }

        return true;
    }

    public static string? AskForExistingDirectoryPath(string label)
    {
        Console.Write($"{label} path: ");
        var path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("No path entered.");
            Console.ReadLine();
            return null;
        }

        path = path.Trim().Trim('"');

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Directory not found: {path}");
            Console.ReadLine();
            return null;
        }

        return path;
    }

    public static string? AskForExchangeCode(string detectedExchangeName)
    {
        var suggestedExchangeCode = detectedExchangeName.Trim().ToLowerInvariant() switch
        {
            "nasdaq" => "XNAS",
            "nyse" => "XNYS",
            "new york stock exchange" => "XNYS",
            "nyse arca" => "ARCX",
            "arca" => "ARCX",
            "nysemkt" => "XASE",
            "nyse mkt" => "XASE",
            "nyse american" => "XASE",
            "amex" => "XASE",

            _ => null
        };

        Console.WriteLine();

        if (suggestedExchangeCode is not null)
        {
            Console.WriteLine($"Detected exchange: {detectedExchangeName}");
            Console.WriteLine($"Suggested exchange code: {suggestedExchangeCode}");
            Console.Write("Use this exchange code? (y/n): ");

            var answer = Console.ReadLine();

            if (answer?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true ||
                answer?.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                return suggestedExchangeCode;
            }
        }
        else
        {
            Console.WriteLine($"Could not suggest exchange code from: {detectedExchangeName}");
        }

        Console.Write("Enter exchange code: ");
        var exchangeCode = Console.ReadLine()?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(exchangeCode))
        {
            Console.WriteLine("Exchange code cannot be empty.");
            Console.ReadLine();
            return null;
        }

        return exchangeCode;
    }

    public static string? DetectExchangeName(string folderPath)
    {
        var folderInfo = new DirectoryInfo(folderPath);

        var folderNameForExchange =
            !string.IsNullOrWhiteSpace(folderInfo.Name)
            && char.IsDigit(folderInfo.Name[0])
            && folderInfo.Parent is not null
                ? folderInfo.Parent.Name
                : folderInfo.Name;

        var detectedExchangeName = folderNameForExchange
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        return detectedExchangeName;
    }

    public static bool ConfirmProceedUpdate(string actionDescription = "updating metadata")
    {
        Console.WriteLine($"\nProceed with {actionDescription}? (y/n)\n");

        var proceed = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(proceed))
        {
            Console.WriteLine("Update cancelled.");
            Console.ReadLine();
            return false;
        }

        proceed = proceed.Trim().ToLowerInvariant();

        if (proceed is not ("y" or "yes"))
        {
            Console.WriteLine("Update cancelled.");
            Console.ReadLine();
            return false;
        }

        return true;
    }
}