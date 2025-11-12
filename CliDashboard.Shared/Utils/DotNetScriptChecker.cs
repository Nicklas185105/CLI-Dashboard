using System.Diagnostics;
using Spectre.Console;

namespace CliDashboard.Shared.Utils;

public static class DotNetScriptChecker
{
    private static readonly string StateFile = Path.Combine(PathUtil.GetRoot(), ".dotnet-script-warning-shown");

    public static bool IsInstalled()
    {
        try
        {
            var psi = new ProcessStartInfo("dotnet", "script --version")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            var process = Process.Start(psi);
            if (process == null) return false;
            
            process.WaitForExit(5000); // 5 second timeout
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static void ShowWarningIfNeeded()
    {
        // Check if warning has already been shown
        if (File.Exists(StateFile))
            return;

        // Check if dotnet-script is installed
        if (IsInstalled())
        {
            // Mark as checked so we don't check again
            File.WriteAllText(StateFile, DateTime.UtcNow.ToString("o"));
            return;
        }

        // Show warning
        AnsiConsole.Write(new Rule("[yellow]⚠ Missing Dependency[/]").RuleStyle("yellow"));
        AnsiConsole.MarkupLine("\n[yellow]dotnet-script is not installed.[/]");
        AnsiConsole.MarkupLine("Plugins require [cyan]dotnet-script[/] to run.\n");
        AnsiConsole.MarkupLine("To install, run:");
        AnsiConsole.MarkupLine("[green]  dotnet tool install -g dotnet-script[/]\n");
        
        // Mark as shown
        File.WriteAllText(StateFile, DateTime.UtcNow.ToString("o"));
        
        ConsoleUtils.PauseForUser();
    }
}
