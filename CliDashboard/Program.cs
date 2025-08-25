using CliDashboard.Modules;
using CliDashboard.Shared.Utils;
using Spectre.Console;

string scriptRoot = Setup.GetScriptRoot();
string configPath = Path.Combine(scriptRoot, "scripts.yaml");
string launcherPath = Path.Combine(scriptRoot, "launch-cli-dashboard.ps1");
string pluginRoot = Path.Combine(scriptRoot, "plugins");
Setup.EnsureDirectoryExists(scriptRoot);
Setup.EnsureFileExists(configPath);
Setup.CreateLauncherScript(launcherPath);
CustomScriptsManager scriptsManager = new(scriptRoot, configPath);
SettingsManager settingsManager = new(pluginRoot);
PluginLoader pluginLoader = new(pluginRoot);
PluginHubManager pluginHubManager = new(pluginRoot);

while (true)
{
    ConsoleUtils.Clear();

    var baseChoices = new List<string> { "Custom Scripts", "Plugin Hub", "Settings", "Exit" };
    var pluginMenus = pluginLoader.LoadPlugins();
    baseChoices.InsertRange(1, pluginMenus.Select(x => x.Key));

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What do you want to do?")
            .AddChoices(baseChoices));

    switch (choice)
    {
        case "Custom Scripts":
            scriptsManager.CustomScriptsMenu();
            break;
        case "Plugin Hub":
            pluginHubManager.ShowPluginHub();
            break;
        case "Settings":
            settingsManager.SettingsMenu();
            break;
        case "Exit":
            AnsiConsole.Clear();
            return;
        default:
            ShowPluginMenu(choice);
            break;
    }
}

void ShowPluginMenu(string menu)
{
    var plugins = pluginLoader.GetPluginsForMenu(menu);
    if (plugins.Count == 0)
    {
        AnsiConsole.MarkupLine("[red]No plugins found under this menu.[/]");
        return;
    }

    var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"[grey]Select a plugin from [blue]{menu}[/]:[/]")
            .AddChoices(plugins.Select(p => p.Name)));

    var plugin = plugins.First(p => p.Name == selected);
    pluginLoader.ExecutePlugin(plugin);
}