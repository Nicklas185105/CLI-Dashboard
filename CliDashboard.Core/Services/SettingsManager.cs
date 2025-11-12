
namespace CliDashboard.Core.Services;

public class SettingsManager(string pluginFolder, PluginManager pluginManager)
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
                        "Reload Plugins",
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
                case "Reload Plugins":
                    ReloadPlugins();
                    break;
                case "Back":
                    return;
            }

            ConsoleUtils.PauseForUser("Press any key to return to the settings menu...");
        }
    }

    private void OpenPluginFolder()
    {
        if (!Directory.Exists(pluginFolder))
            Directory.CreateDirectory(pluginFolder);

        Process.Start("explorer", pluginFolder);
        AnsiConsole.MarkupLine("[green]Plugin folder opened.[/]");
    }

    public void CreateNewPlugin()
    {
        var name = AnsiConsole.Ask<string>("Plugin name:");
        var safeName = name.Replace(" ", "-").ToLowerInvariant();
        var folderPath = Path.Combine(pluginFolder, safeName);
        Directory.CreateDirectory(folderPath);

        var yamlPath = Path.Combine(folderPath, "plugin.yaml");
        var scriptPath = Path.Combine(folderPath, "main.csx");
        var omnisharpPath = Path.Combine(folderPath, "omnisharp.json");

        var yamlContent = $$"""
    name: {{name}}
    menu: Custom Plugins
    scriptPath: main.csx
    description: Describe what this plugin does
    """;
        File.WriteAllText(yamlPath, yamlContent);

        var csxTemplate = $$"""
    // {{name}} Plugin Entry Point
    #r "nuget: Spectre.Console, 0.50.0"
    #r "../../CliDashboard.Shared.dll"

    using Spectre.Console;

    AnsiConsole.Write(new Rule("{{name}} Plugin").RuleStyle("cyan"));
    AnsiConsole.MarkupLine("[green]Hello from your plugin![/]");
    """;
        File.WriteAllText(scriptPath, csxTemplate);

        var omnisharpTemplate = $$"""
    {
      "script": {
        "enableScriptNuGetReferences": true,
        "defaultTargetFramework": "net8.0"
      }
    }
    """;
        File.WriteAllText(omnisharpPath, omnisharpTemplate);

        //Process.Start(new ProcessStartInfo("code", yamlPath) { UseShellExecute = true });
        //Process.Start(new ProcessStartInfo("code", scriptPath) { UseShellExecute = true });
        Process.Start(new ProcessStartInfo("code", $"--new-window \"{folderPath}\"") { UseShellExecute = true });
        AnsiConsole.MarkupLine("[green]Plugin created and opened in VS Code![/]");
    }

    private void ReloadPlugins()
    {
        pluginManager.ReloadPlugins();
        AnsiConsole.MarkupLine("[green]✓[/] Plugins reloaded successfully!");
    }
}
