namespace PortfolioManagement.Tools.ImportData.Helpers;

public static class ConsoleImportPrompt
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

    public static bool ConfirmProceed(string actionDescription = "importing this file")
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
}