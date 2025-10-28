namespace CliDashboard.Shared.Utils;

public static class Setup
{
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "scripts"));
        Directory.CreateDirectory(Path.Combine(path, "configs"));
    }

    public static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");
    }

    public static void CreateLauncherScript(string path)
    {
        var launchPath = Path.Combine(path, "launch-cli-dashboard.ps1");
        if (!File.Exists(launchPath))
            File.WriteAllText(launchPath, $"{path}\\CliDashboard.UI.CLI.exe");
    }
}
