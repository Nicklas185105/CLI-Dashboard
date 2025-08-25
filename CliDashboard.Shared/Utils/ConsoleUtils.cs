using Spectre.Console;

namespace CliDashboard.Shared.Utils;

public static class ConsoleUtils
{
    public static void Clear()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("CLI Dashboard").Centered().Color(Color.Cyan1));
    }
}
