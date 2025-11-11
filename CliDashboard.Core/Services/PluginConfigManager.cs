namespace CliDashboard.Core.Services;

public class PluginConfigManager
{
    private readonly string _appDataRoot;

    public PluginConfigManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _appDataRoot = Path.Combine(appData, "CliDashboard");
        Directory.CreateDirectory(_appDataRoot);
    }

    public string GetPluginConfigDirectory(string pluginName)
    {
        var safeName = pluginName.Replace(" ", "-").ToLowerInvariant();
        var configDir = Path.Combine(_appDataRoot, "plugins", safeName);
        Directory.CreateDirectory(configDir);
        return configDir;
    }

    public string GetPluginConfigPath(string pluginName, string configFileName)
    {
        var configDir = GetPluginConfigDirectory(pluginName);
        return Path.Combine(configDir, configFileName);
    }

    public void SaveConfig(string pluginName, string configFileName, string content)
    {
        var configPath = GetPluginConfigPath(pluginName, configFileName);
        File.WriteAllText(configPath, content);
    }

    public string? ReadConfig(string pluginName, string configFileName)
    {
        var configPath = GetPluginConfigPath(pluginName, configFileName);
        return File.Exists(configPath) ? File.ReadAllText(configPath) : null;
    }

    public bool ConfigExists(string pluginName, string configFileName)
    {
        var configPath = GetPluginConfigPath(pluginName, configFileName);
        return File.Exists(configPath);
    }

    public void DeleteConfig(string pluginName, string configFileName)
    {
        var configPath = GetPluginConfigPath(pluginName, configFileName);
        if (File.Exists(configPath))
            File.Delete(configPath);
    }

    public List<string> GetPluginConfigFiles(string pluginName)
    {
        var configDir = GetPluginConfigDirectory(pluginName);
        if (!Directory.Exists(configDir))
            return new List<string>();

        return Directory.GetFiles(configDir)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Select(f => f!)
            .ToList();
    }

    public void InitializePluginConfig(Plugin plugin)
    {
        if (!string.IsNullOrEmpty(plugin.ConfigPath))
        {
            var configDir = GetPluginConfigDirectory(plugin.Name);
            plugin.ConfigPath = configDir;
        }
    }
}
