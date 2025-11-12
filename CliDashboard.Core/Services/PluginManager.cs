using System.Text;

namespace CliDashboard.Core.Services;

public class PluginManager(string pluginRoot, PluginLogger? pluginLogger = null)
{
    private Dictionary<string, List<Plugin>>? _cachedPlugins;
    private readonly object _cacheLock = new();
    private readonly PluginLogger? _logger = pluginLogger;

    private Plugin LoadPluginFromYaml(string yamlPath)
    {
        var yaml = File.ReadAllText(yamlPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<Plugin>(yaml);
    }

    public Dictionary<string, List<Plugin>> LoadPlugins(bool forceReload = false)
    {
        lock (_cacheLock)
        {
            if (_cachedPlugins != null && !forceReload)
                return _cachedPlugins;

            var plugins = new Dictionary<string, List<Plugin>>();

            if (!Directory.Exists(pluginRoot))
            {
                _cachedPlugins = plugins;
                return plugins;
            }

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

            _cachedPlugins = plugins;
            return _cachedPlugins;
        }
    }

    public void ReloadPlugins()
    {
        LoadPlugins(forceReload: true);
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
        var plugins = LoadPlugins()
            .Where(p => p.Key == menu)
            .OrderBy(p => p.Key)
            .SelectMany(selector => selector.Value)
            .ToList();

        // Sort by pinned status first, then by name
        return plugins
            .OrderByDescending(p => p.IsPinned)
            .ThenBy(p => p.Name)
            .ToList();
    }

    public void ExecutePlugin(Plugin plugin)
    {
        var output = new StringBuilder();
        var error = new StringBuilder();
        var success = false;

        try
        {
            // Check dependencies before execution
            if (!CheckDependencies(plugin, out var missingDeps))
            {
                var errorMsg = $"Missing dependencies: {string.Join(", ", missingDeps)}";
                AnsiConsole.MarkupLine($"[red]{errorMsg}[/]");
                _logger?.LogExecution(plugin.Name, false, error: errorMsg);
                return;
            }

            var psi = new ProcessStartInfo("dotnet", $"script \"{plugin.ScriptPath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            var process = Process.Start(psi);
            if (process != null)
            {
                // Read streams asynchronously to avoid deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                process.WaitForExit();
                
                output.Append(outputTask.Result);
                error.Append(errorTask.Result);
                success = process.ExitCode == 0;
                
                if (!string.IsNullOrEmpty(output.ToString()))
                    AnsiConsole.WriteLine(output.ToString());
                if (!string.IsNullOrEmpty(error.ToString()))
                    AnsiConsole.MarkupLine($"[red]{error}[/]");
            }
            
            _logger?.LogExecution(plugin.Name, success, output.ToString(), error.ToString());
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to run plugin:[/] {plugin.Name}");
            AnsiConsole.WriteException(ex);
            _logger?.LogExecution(plugin.Name, false, error: ex.ToString());
        }
        finally
        {
            ConsoleUtils.PauseForUser();
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
            ScriptPath = csxPath,
            Version = "1.0.0"
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(yamlPath, serializer.Serialize(plugin));

        File.WriteAllText(csxPath, "// Write your plugin logic here\nConsole.WriteLine(\"Hello from plugin!\");");

        Process.Start(new ProcessStartInfo("code", csxPath) { UseShellExecute = true });
        AnsiConsole.MarkupLine("[green]Plugin created and opened in VS Code![/]");
    }

    private bool CheckDependencies(Plugin plugin, out List<string> missingDependencies)
    {
        missingDependencies = new List<string>();
        
        if (plugin.Dependencies == null || plugin.Dependencies.Count == 0)
            return true;

        var allPlugins = LoadPlugins().SelectMany(p => p.Value).ToList();
        
        foreach (var dependency in plugin.Dependencies)
        {
            // Check if dependency is another plugin
            if (!allPlugins.Any(p => p.Name.Equals(dependency, StringComparison.OrdinalIgnoreCase)))
            {
                // Could also be a file dependency - check if file exists
                if (!File.Exists(dependency))
                {
                    missingDependencies.Add(dependency);
                }
            }
        }

        return missingDependencies.Count == 0;
    }

    public void TogglePin(Plugin plugin)
    {
        plugin.IsPinned = !plugin.IsPinned;
        SavePluginMetadata(plugin);
        ReloadPlugins();
    }

    public List<Plugin> GetPluginsByTag(string tag)
    {
        return LoadPlugins()
            .SelectMany(p => p.Value)
            .Where(p => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    public List<string> GetAllTags()
    {
        return LoadPlugins()
            .SelectMany(p => p.Value)
            .SelectMany(p => p.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    private void SavePluginMetadata(Plugin plugin)
    {
        var pluginFolder = Path.GetDirectoryName(plugin.ScriptPath);
        if (string.IsNullOrEmpty(pluginFolder))
            return;

        var yamlPath = Path.Combine(pluginFolder, "plugin.yaml");
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(yamlPath, serializer.Serialize(plugin));
    }
}
