// File: Modules/PluginManager.cs
using CliDashboard.Models;
using Spectre.Console;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CliDashboard.Modules;

internal class PluginManager(string pluginRoot)
{
    private readonly PluginLoader _loader = new(pluginRoot);

    public List<string> GetPluginMenus()
    {
        return _loader.LoadPlugins()
            .Select(p => p.Key)
            .Distinct()
            .Order()
            .ToList();
    }

    public List<Plugin> GetPluginsForMenu(string menu)
    {
        return _loader.LoadPlugins()
            .Where(p => p.Key == menu)
            .OrderBy(p => p.Key)
            .SelectMany(selector => selector.Value)
            .ToList();
    }

    public void RunPlugin(Plugin plugin)
    {
        if (!File.Exists(plugin.ScriptPath))
        {
            AnsiConsole.MarkupLine("[red]Script file not found:[/] " + plugin.ScriptPath);
            return;
        }

        var startInfo = new ProcessStartInfo("dotnet", $"script \"{plugin.ScriptPath}\"")
        {
            UseShellExecute = false
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to run plugin:[/] {ex.Message}");
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
