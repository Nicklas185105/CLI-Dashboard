namespace CliDashboard.Core.Services;

public class PluginManager(string pluginRoot)
{
    private Plugin LoadPluginFromYaml(string yamlPath)
    {
        var yaml = File.ReadAllText(yamlPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<Plugin>(yaml);
    }

    public Dictionary<string, List<Plugin>> LoadPlugins()
    {
        var plugins = new Dictionary<string, List<Plugin>>();

        if (!Directory.Exists(pluginRoot))
            return plugins;

        foreach (var folder in Directory.GetDirectories(pluginRoot))
        {
            var yamlPath = Path.Combine(folder, "plugin.yaml");
            var scriptPath = Path.Combine(folder, "main.csx");
            if (!File.Exists(yamlPath) || !File.Exists(scriptPath))
                continue;

            var plugin = LoadPluginFromYaml(yamlPath);
            plugin.ScriptPath = scriptPath;

            if (!plugins.ContainsKey(plugin.Menu))
                plugins[plugin.Menu] = new List<Plugin>();

            plugins[plugin.Menu].Add(plugin);
        }

        return plugins;
    }

    public List<string> GetPluginMenus()
    {
        return LoadPlugins()
            .Select(p => p.Key)
            .Distinct()
            .Order()
            .ToList();
    }

    public List<Plugin> GetPluginsForMenu(string menu)
    {
        return LoadPlugins()
            .Where(p => p.Key == menu)
            .OrderBy(p => p.Key)
            .SelectMany(selector => selector.Value)
            .ToList();
    }

    public void ExecutePlugin(Plugin plugin)
    {
        try
        {
            var psi = new ProcessStartInfo("dotnet", $"script \"{plugin.ScriptPath}\"")
            {
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to run plugin:[/] {plugin.Name}");
            AnsiConsole.WriteException(ex);
        }
    }

    public void CreateNewPlugin()
    {
        var name = AnsiConsole.Ask<string>("Plugin name:");
        var menu = AnsiConsole.Ask<string>("Menu category for plugin:");
        var safeName = name.Replace(" ", "-").ToLowerInvariant();
        var pluginDir = Path.Combine(pluginRoot, safeName);
        var yamlPath = Path.Combine(pluginDir, "plugin.yaml");
        var csxPath = Path.Combine(pluginDir, "main.csx");

        Directory.CreateDirectory(pluginDir);

        var plugin = new Plugin
        {
            Name = name,
            Menu = menu,
            ScriptPath = csxPath
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(yamlPath, serializer.Serialize(plugin));

        File.WriteAllText(csxPath, "// Write your plugin logic here\nConsole.WriteLine(\"Hello from plugin!\");");

        Process.Start(new ProcessStartInfo("code", csxPath) { UseShellExecute = true });
        AnsiConsole.MarkupLine("[green]Plugin created and opened in VS Code![/]");
    }
}
