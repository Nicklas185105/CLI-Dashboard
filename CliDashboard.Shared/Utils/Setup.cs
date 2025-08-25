namespace CliDashboard.Shared.Utils;

public static class Setup
{
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "scripts"));
    }

    public static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");
    }

    public static void CreateLauncherScript(string path)
    {
        if (!File.Exists(path))
        {
            var projectPath = Directory.GetCurrentDirectory();
            //var content = $"Start-Process powershell -ArgumentList \"-NoExit\", \"-Command\", \"dotnet run --project '{projectPath}'\"";
            File.WriteAllText(path, projectPath);
        }
    }

    public static string GetScriptRoot()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "cli-dashboard");
    }
}
