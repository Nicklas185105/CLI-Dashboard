using System.Diagnostics;

namespace CliDashboard.Shared.Utils;

public static class Run
{
    public static void InTerminal(string command, string workingDir)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass {command}",
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };

            using var process = Process.Start(processInfo);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            Log.Error($"Error executing command '{command}' in '{workingDir}': {ex.Message}");
        }
    }

    public static void InNewTerminal(string command, string workingDir, string windowTitle = "CLI Script")
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoExit -Command \"cd '{workingDir}'; $host.UI.RawUI.WindowTitle = '{windowTitle}'; {command}\"",
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            Log.Error($"Error executing command '{command}' in new terminal at '{workingDir}': {ex.Message}");
        }
    }
}
