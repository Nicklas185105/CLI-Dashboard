using Spectre.Console;
using System.Diagnostics;

namespace CliDashboard.Modules;

internal class SettingsManager(string pluginFolder)
{
    public void SettingsMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Settings").RuleStyle("grey"));

            var subChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a settings option:")
                    .AddChoices([
                        "Open Plugin Folder",
                        "Create New Plugin",
                        "Back"
                    ]));

            switch (subChoice)
            {
                case "Open Plugin Folder":
                    OpenPluginFolder();
                    break;
                case "Create New Plugin":
                    CreateNewPlugin();
                    break;
                case "Back":
                    return;
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to the settings menu...[/]");
            Console.ReadKey(true);
        }
    }

    private void OpenPluginFolder()
    {
        if (!Directory.Exists(pluginFolder))
            Directory.CreateDirectory(pluginFolder);

        Process.Start("explorer", pluginFolder);
        AnsiConsole.MarkupLine("[green]Plugin folder opened.[/]");
    }

    private void CreateNewPlugin()
    {
        var name = AnsiConsole.Ask<string>("Plugin name:");
        var safeName = name.Replace(" ", "-").ToLowerInvariant();
        var folderPath = Path.Combine(pluginFolder, safeName);
        Directory.CreateDirectory(folderPath);

        var yamlPath = Path.Combine(folderPath, "plugin.yaml");
        var scriptPath = Path.Combine(folderPath, "main.csx");

        var yamlContent = $$"""
        name: {{name}}
        menu: Custom Plugins
        scriptPath: main.csx
        description: Describe what this plugin does
        """;
        File.WriteAllText(yamlPath, yamlContent);

        var csxTemplate = $$"""
        // {{name}} Plugin Entry Point
        #r "nuget: Spectre.Console, 0.47.0"
        using Spectre.Console;

        AnsiConsole.Write(new Rule("{{name}} Plugin").RuleStyle("cyan"));
        AnsiConsole.MarkupLine("[green]Hello from your plugin![/]");
        """;
        File.WriteAllText(scriptPath, csxTemplate);

        Process.Start(new ProcessStartInfo("code", yamlPath) { UseShellExecute = true });
        Process.Start(new ProcessStartInfo("code", scriptPath) { UseShellExecute = true });
        AnsiConsole.MarkupLine("[green]Plugin created and opened in VS Code![/]");
    }
}