namespace CliDashboard.UI.CLI;

internal class Main(ScriptManager scriptsManager,
                    PluginManager pluginManager,
                    PluginHubManager pluginHubManager,
                    SearchService searchService,
                    FavoritesManager favoritesManager,
                    SettingsHubManager settingsHubManager,
                    PluginImportExportManager importExportManager,
                    PluginSyncHubManager syncHubManager,
                    SchedulerHubManager schedulerHubManager,
                    BackgroundJobsHubManager backgroundJobsHubManager)
{
    public void Run()
    {
        string root = PathUtil.GetRoot();
        string configPath = Path.Combine(root, "scripts.yaml");
        Setup.EnsureDirectoryExists(root);
        Setup.EnsureFileExists(configPath);
        Setup.CreateLauncherScript(root);

        while (true)
        {
            ConsoleUtils.Clear();

            var baseChoices = new List<string> { "Favorites", "Search", "Custom Scripts", "Plugin Hub", "Import/Export", "Sync Plugins", "Scheduled Tasks", "Background Jobs", "Settings", "Exit" };
            var pluginMenus = pluginManager.LoadPlugins();
            var coloredMenus = pluginMenus.Select(x => GetColoredCategory(x.Key)).ToList();
            baseChoices.InsertRange(1, coloredMenus);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What do you want to do?")
                    .AddChoices(baseChoices));

            switch (choice)
            {
                case "Favorites":
                    ShowFavoritesMenu();
                    break;
                case "Search":
                    searchService.ShowSearchMenu();
                    break;
                case "Custom Scripts":
                    scriptsManager.CustomScriptsMenu();
                    break;
                case "Plugin Hub":
                    pluginHubManager.ShowPluginHub();
                    break;
                case "Import/Export":
                    importExportManager.ShowMenu();
                    break;
                case "Sync Plugins":
                    syncHubManager.ShowMenu();
                    break;
                case "Scheduled Tasks":
                    schedulerHubManager.ShowMenu();
                    break;
                case "Background Jobs":
                    backgroundJobsHubManager.ShowMenu();
                    break;
                case "Settings":
                    settingsHubManager.ShowSettingsMenu();
                    break;
                case "Exit":
                    AnsiConsole.Clear();
                    return;
                default:
                    // Strip color markup to get actual menu name
                    var actualMenu = StripColorMarkup(choice);
                    ShowPluginMenu(actualMenu);
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
                    .UseConverter(name =>
                    {
                        if (name == "Back") return name;
                        var plugin = plugins.FirstOrDefault(p => p.Name == name);
                        if (plugin == null) return name;

                        var star = favoritesManager.IsPluginFavorite(plugin.Name) ? "★ " : "";
                        var desc = string.IsNullOrWhiteSpace(plugin.Description) ? "" : $" — [grey]{plugin.Description}[/]";
                        return $"{star}{name}{desc}";
                    })
                    .AddChoices(pluginNames));

            if (selected == "Back")
                return;

            var plugin = plugins.First(p => p.Name == selected);

            // Ask: Run or Toggle Favorite
            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[grey]What do you want to do with [cyan]{plugin.Name}[/]?[/]")
                    .AddChoices([
                        "Run",
                        favoritesManager.IsPluginFavorite(plugin.Name) ? "Remove from Favorites" : "Add to Favorites",
                        "Back"
                    ]));

            if (action == "Run")
            {
                pluginManager.ExecutePlugin(plugin);
            }
            else if (action.Contains("Favorite"))
            {
                favoritesManager.TogglePluginFavorite(plugin.Name);
                var msg = favoritesManager.IsPluginFavorite(plugin.Name)
                    ? $"[green]★[/] Added [cyan]{plugin.Name}[/] to favorites"
                    : $"Removed [cyan]{plugin.Name}[/] from favorites";
                AnsiConsole.MarkupLine(msg);
                ConsoleUtils.PauseForUser();
            }
        }

        void ShowFavoritesMenu()
        {
            ConsoleUtils.Clear();
            AnsiConsole.Write(new Rule(Emoji.Replace("[yellow]:star: Favorites[/]")).RuleStyle("yellow"));

            var favorites = favoritesManager.LoadFavorites();
            var allPlugins = pluginManager.LoadPlugins().SelectMany(kvp => kvp.Value).ToList();
            var allScripts = scriptsManager.LoadScripts();

            var favoriteItems = new List<(string Name, string Type, object? Item)>();

            foreach (var pluginName in favorites.PluginNames)
            {
                var plugin = allPlugins.FirstOrDefault(p => p.Name == pluginName);
                if (plugin != null)
                    favoriteItems.Add((plugin.Name, "Plugin", plugin));
            }

            foreach (var scriptName in favorites.ScriptNames)
            {
                var script = allScripts.FirstOrDefault(s => s.Name == scriptName);
                if (script != null)
                    favoriteItems.Add((script.Name, "Script", script));
            }

            if (favoriteItems.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[yellow]No favorites yet.[/]");
                AnsiConsole.MarkupLine("[grey]Add favorites from plugin or script menus![/]");
                ConsoleUtils.PauseForUser();
                return;
            }

            var choices = favoriteItems.Select(f => f.Name).ToList();
            choices.Add("Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]Select a favorite:[/]")
                    .UseConverter(name =>
                    {
                        if (name == "Back") return name;
                        var item = favoriteItems.FirstOrDefault(f => f.Name == name);
                        if (item == default) return name;
                        var typeColor = item.Type == "Plugin" ? "blue" : "yellow";
                        return Emoji.Replace($":star: {name} [{typeColor}][dim][[{item.Type}]][/][/]");
                    })
                    .AddChoices(choices));

            if (selected == "Back")
                return;

            var selectedItem = favoriteItems.First(f => f.Name == selected);

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[grey]What do you want to do?[/]")
                    .AddChoices(["Run", "Remove from Favorites", "Back"]));

            if (action == "Run")
            {
                if (selectedItem.Type == "Plugin")
                {
                    pluginManager.ExecutePlugin((Plugin)selectedItem.Item);
                }
                else
                {
                    var script = (Script)selectedItem.Item;
                    var psi = script.Path.EndsWith(".csx")
                        ? new ProcessStartInfo("dotnet", $"script \"{script.Path}\"")
                        : new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\"");
                    psi.UseShellExecute = false;
                    var process = new Process { StartInfo = psi };
                    process.Start();
                    process.WaitForExit();
                    ConsoleUtils.PauseForUser();
                }
            }
            else if (action == "Remove from Favorites")
            {
                if (selectedItem.Type == "Plugin")
                    favoritesManager.TogglePluginFavorite(selectedItem.Name);
                else
                    favoritesManager.ToggleScriptFavorite(selectedItem.Name);

                AnsiConsole.MarkupLine($"[yellow]Removed [cyan]{selectedItem.Name}[/] from favorites[/]");
                ConsoleUtils.PauseForUser();
            }
        }

        string GetColoredCategory(string category)
        {
            // Apply colors based on category keywords
            var lower = category.ToLowerInvariant();
            if (lower.Contains("git")) return $"[green]{category}[/]";
            if (lower.Contains("system")) return $"[blue]{category}[/]";
            if (lower.Contains("dev") || lower.Contains("development")) return $"[yellow]{category}[/]";
            if (lower.Contains("docker") || lower.Contains("container")) return $"[cyan]{category}[/]";
            if (lower.Contains("database") || lower.Contains("db")) return $"[magenta]{category}[/]";
            if (lower.Contains("custom")) return $"[purple]{category}[/]";
            return $"[white]{category}[/]"; // Default color
        }

        string StripColorMarkup(string text)
        {
            // Remove Spectre.Console markup like [green]text[/]
            return System.Text.RegularExpressions.Regex.Replace(text, @"\[.*?\]", "");
        }
    }
}
