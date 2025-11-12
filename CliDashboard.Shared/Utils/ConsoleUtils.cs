using Spectre.Console;

namespace CliDashboard.Shared.Utils;

public static class ConsoleUtils
{
    public static void Clear()
    {
        try
        {
            AnsiConsole.Clear();
        }
        catch (System.IO.IOException)
        {
            // Console handle not available (e.g., redirected output or non-interactive terminal)
            // Skip clearing and just write a newline for separation
            Console.WriteLine();
        }
        AnsiConsole.Write(new FigletText("CLI Dashboard").Centered().Color(Color.Cyan1));
    }

    public static void PauseForUser(string message = "Press any key to continue...")
    {
        AnsiConsole.MarkupLine($"\n[grey]{message}[/]");
        Console.ReadKey(true);
    }
}
