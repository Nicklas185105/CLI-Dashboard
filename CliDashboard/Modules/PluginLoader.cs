// File: PluginLoader.cs
using CliDashboard.Models;
using CliDashboard.Shared.Utils;
using Spectre.Console;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CliDashboard.Modules;

internal class PluginLoader(string pluginRoot)
{
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

    public List<Plugin> GetPluginsForMenu(string menu)
    {
        return LoadPlugins()
            .Where(p => p.Key == menu)
            .OrderBy(p => p.Key)
            .Select(selector => selector.Value)
            .FirstOrDefault();
    }

    private Plugin LoadPluginFromYaml(string yamlPath)
    {
        var yaml = File.ReadAllText(yamlPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<Plugin>(yaml);
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

    public void RenderDynamicMenu(Dictionary<string, List<Plugin>> groupedPlugins)
    {
        while (true)
        {
            ConsoleUtils.Clear();

            var topMenu = groupedPlugins.Keys.Order().ToList();
            topMenu.Add("Settings");
            topMenu.Add("Custom Scripts");
            topMenu.Add("Exit");

            var mainChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a category")
                    .AddChoices(topMenu));

            if (mainChoice == "Exit")
                return;

            if (mainChoice == "Settings")
            {
                var settings = new SettingsManager(pluginRoot);
                settings.SettingsMenu();
            }
            else if (mainChoice == "Custom Scripts")
            {
                var scriptRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cli-dashboard");
                var configPath = Path.Combine(scriptRoot, "scripts.yaml");
                new CustomScriptsManager(scriptRoot, configPath).CustomScriptsMenu();
            }
            else if (groupedPlugins.ContainsKey(mainChoice))
            {
                var submenu = groupedPlugins[mainChoice];

                var selectedPluginName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[blue]{mainChoice} Plugins:[/]")
                        .AddChoices(submenu.Select(p => p.Name).Append("Back")));

                if (selectedPluginName == "Back")
                    continue;

                var plugin = submenu.First(p => p.Name == selectedPluginName);
                ExecutePlugin(plugin);
            }
        }
    }
}
