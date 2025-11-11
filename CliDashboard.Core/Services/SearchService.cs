namespace CliDashboard.Core.Services;

public class SearchService(PluginManager pluginManager, ScriptManager scriptManager)
{
    public class SearchResult
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Plugin" or "Script"
        public string Description { get; set; } = string.Empty;
        public object Item { get; set; } = null!; // The actual Plugin or Script object
    }

    public List<SearchResult> SearchAll(string query)
    {
        var results = new List<SearchResult>();
        var lowerQuery = query.ToLowerInvariant();

        // Search plugins
        var allPlugins = pluginManager.LoadPlugins()
            .SelectMany(kvp => kvp.Value)
            .ToList();

        foreach (var plugin in allPlugins)
        {
            if (plugin.Name.ToLowerInvariant().Contains(lowerQuery) ||
                (plugin.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false))
            {
                results.Add(new SearchResult
                {
                    Name = plugin.Name,
                    Type = "Plugin",
                    Description = plugin.Description,
                    Item = plugin
                });
            }
        }

        // Search scripts
        var allScripts = scriptManager.LoadScripts();
        foreach (var script in allScripts)
        {
            if (script.Name.ToLowerInvariant().Contains(lowerQuery) ||
                (script.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false))
            {
                results.Add(new SearchResult
                {
                    Name = script.Name,
                    Type = "Script",
                    Description = script.Description,
                    Item = script
                });
            }
        }

        return results.OrderBy(r => r.Name).ToList();
    }

    public void ShowSearchMenu()
    {
        ConsoleUtils.Clear();
        AnsiConsole.Write(new Rule("[cyan]Search[/]").RuleStyle("cyan"));

        var query = AnsiConsole.Ask<string>("\n[grey]Enter search term:[/]");

        if (string.IsNullOrWhiteSpace(query))
        {
            AnsiConsole.MarkupLine("[yellow]Search cancelled.[/]");
            ConsoleUtils.PauseForUser();
            return;
        }

        var results = SearchAll(query);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n[yellow]No results found for:[/] {query}");
            ConsoleUtils.PauseForUser();
            return;
        }

        AnsiConsole.MarkupLine($"\n[green]Found {results.Count} result(s):[/]\n");

        var choices = results.Select(r => r.Name).ToList();
        choices.Add("Back");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[grey]Select an item to run:[/]")
                .UseConverter(name =>
                {
                    if (name == "Back") return name;
                    var result = results.FirstOrDefault(r => r.Name == name);
                    if (result == null) return name;

                    var typeColor = result.Type == "Plugin" ? "blue" : "yellow";
                    var desc = string.IsNullOrWhiteSpace(result.Description)
                        ? ""
                        : $" — [grey]{result.Description}[/]";
                    return $"{name} [{typeColor}][dim][[{result.Type}]][/][/]{desc}";
                })
                .AddChoices(choices));

        if (selected == "Back")
            return;

        var selectedResult = results.First(r => r.Name == selected);

        if (selectedResult.Type == "Plugin")
        {
            pluginManager.ExecutePlugin((Plugin)selectedResult.Item);
        }
        else
        {
            // Execute script
            var script = (Script)selectedResult.Item;
            var processStartInfo = script.Path.EndsWith(".csx")
                ? new ProcessStartInfo("dotnet", $"script \"{script.Path}\"")
                {
                    UseShellExecute = false
                }
                : new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\"")
                {
                    UseShellExecute = false
                };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();

            ConsoleUtils.PauseForUser("Press any key to return to the main menu...");
        }
    }
}
