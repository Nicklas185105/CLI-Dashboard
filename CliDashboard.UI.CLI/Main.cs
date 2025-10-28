namespace CliDashboard.UI.CLI;

internal class Main(ScriptManager scriptsManager,
                    SettingsManager settingsManager,
                    PluginManager pluginManager)
{
    public void Run()
    {
        string root = PathUtil.GetRoot();
        string configPath = Path.Combine(root, "scripts.yaml");
        string pluginRoot = Path.Combine(root, "plugins");
        Setup.EnsureDirectoryExists(root);
        Setup.EnsureFileExists(configPath);
        Setup.CreateLauncherScript(root);
        PluginHubManager pluginHubManager = new(pluginRoot);

        while (true)
        {
            ConsoleUtils.Clear();

            var baseChoices = new List<string> { "Custom Scripts", "Plugin Hub", "Settings", "Exit" };
            var pluginMenus = pluginManager.LoadPlugins();
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
            var plugins = pluginManager.GetPluginsForMenu(menu);
            if (plugins.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No plugins found under this menu.[/]");
                return;
            }

            var pluginNames = plugins.Select(s => s.Name).ToList();
            pluginNames.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[grey]Select a plugin from [blue]{menu}[/]:[/]")
                    .AddChoices(pluginNames));

            if (selected == "Back")
                return;

            var plugin = plugins.First(p => p.Name == selected);
            pluginManager.ExecutePlugin(plugin);
        }
    }
}
