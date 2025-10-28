namespace CliDashboard.Shared.Utils;

public static class PathUtil
{
    public static string GetRoot()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "cli-dashboard");
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetRoot(), "configs");
    }
}
