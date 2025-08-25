using CliDashboard.Shared.Utils;
using Spectre.Console;
using System.Diagnostics;

namespace CliDashboard.Modules;

internal class PluginHubManager(string pluginRoot)
{
    public void ShowPluginHub()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Plugin Hub").Centered().Color(Color.Cyan1));

            var pluginDirs = Directory.GetDirectories(pluginRoot).Select(Path.GetFileName).ToList();
            if (pluginDirs.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No plugins found.[/]");
                return;
            }

            pluginDirs.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Select a plugin to manage:[/]")
                    .AddChoices(pluginDirs));

            if (selected == "Back")
                return;

            ShowPluginActions(selected);
        }
    }

    private void ShowPluginActions(string pluginName)
    {
        var pluginPath = Path.Combine(pluginRoot, pluginName);
        var pluginYaml = Path.Combine(pluginPath, "plugin.yaml");
        var pluginCsx = Path.Combine(pluginPath, "plugin.csx");

        var actions = new[]
        {
            //"Run Plugin",
            "Edit plugin.csx",
            "Edit plugin.yaml",
            "Delete Plugin",
            "Back"
        };

        while (true)
        {
            ConsoleUtils.Clear();
            AnsiConsole.MarkupLine($"[cyan]{pluginName}[/]");

            if (File.Exists(pluginYaml))
            {
                var yamlContent = File.ReadAllText(pluginYaml);
                AnsiConsole.Write(new Panel(yamlContent.Trim()).Header("plugin.yaml preview").Padding(1, 1, 1, 1));
                // Read the "scriptPath" from YAML
                var scriptPath = yamlContent.Split(new[] { "scriptPath:" }, StringSplitOptions.None)
                                            .Skip(1)
                                            .FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(scriptPath))
                    pluginCsx = Path.Combine(pluginPath, scriptPath);
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices(actions));

            switch (action)
            {
                //case "Run Plugin":
                //    PluginRunner.Run(pluginPath);
                //    break;
                case "Edit plugin.csx":
                    Process.Start(new ProcessStartInfo("code", pluginCsx) { UseShellExecute = true });
                    break;
                case "Edit plugin.yaml":
                    Process.Start(new ProcessStartInfo("code", pluginYaml) { UseShellExecute = true });
                    break;
                case "Delete Plugin":
                    if (AnsiConsole.Confirm("Are you sure you want to delete this plugin?"))
                    {
                        Directory.Delete(pluginPath, true);
                        AnsiConsole.MarkupLine("[red]Plugin deleted.[/]");
                        return;
                    }
                    break;
                case "Back":
                    return;
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
}
