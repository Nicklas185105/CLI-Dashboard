using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CliDashboard.UI.CLI;

internal class PluginHubManager(string pluginRoot, PluginManager? pluginManager = null, PluginLogger? pluginLogger = null)
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

        var actions = new List<string>
        {
            "Edit Plugin",
            "View Logs",
            "Toggle Pin",
            "Manage Tags",
            "Manage Dependencies",
            "View Plugin Info",
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
                yamlContent = yamlContent.Replace("[", "[[");
                yamlContent = yamlContent.Replace("]", "]]");
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
                case "Edit Plugin":
                    Process.Start(new ProcessStartInfo("code", pluginPath) { UseShellExecute = true });
                    break;
                case "View Logs":
                    ViewPluginLogs(pluginName);
                    break;
                case "Toggle Pin":
                    TogglePluginPin(pluginName);
                    break;
                case "Manage Tags":
                    ManagePluginTags(pluginName);
                    break;
                case "Manage Dependencies":
                    ManagePluginDependencies(pluginName);
                    break;
                case "View Plugin Info":
                    ViewPluginInfo(pluginName);
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

            ConsoleUtils.PauseForUser();
        }
    }

    private void ViewPluginLogs(string pluginName)
    {
        if (pluginLogger == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin logger not available.[/]");
            return;
        }

        var log = pluginLogger.ReadLastLog(pluginName);
        if (string.IsNullOrEmpty(log))
        {
            AnsiConsole.MarkupLine("[yellow]No logs found for this plugin.[/]");
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Panel(log)
            .Header($"Recent logs for {pluginName}")
            .BorderColor(Color.Yellow));
    }

    private void TogglePluginPin(string pluginName)
    {
        if (pluginManager == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin manager not available.[/]");
            return;
        }

        var plugin = pluginManager.LoadPlugins()
            .SelectMany(p => p.Value)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(p.ScriptPath)) == pluginName);

        if (plugin != null)
        {
            pluginManager.TogglePin(plugin);
            var status = plugin.IsPinned ? "pinned" : "unpinned";
            AnsiConsole.MarkupLine($"[green]Plugin {status} successfully![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Plugin not found.[/]");
        }
    }

    private void ManagePluginTags(string pluginName)
    {
        if (pluginManager == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin manager not available.[/]");
            return;
        }

        var plugin = pluginManager.LoadPlugins()
            .SelectMany(p => p.Value)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(p.ScriptPath)) == pluginName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin not found.[/]");
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[cyan]Current tags:[/] {string.Join(", ", plugin.Tags)}");

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose an action:")
                .AddChoices(new[] { "Add Tag", "Remove Tag", "Back" }));

        switch (action)
        {
            case "Add Tag":
                var newTag = AnsiConsole.Ask<string>("Enter tag name:");
                if (!plugin.Tags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
                {
                    plugin.Tags.Add(newTag);
                    SavePluginToYaml(plugin);
                    AnsiConsole.MarkupLine("[green]Tag added![/]");
                }
                break;
            case "Remove Tag":
                if (plugin.Tags.Count > 0)
                {
                    var tagToRemove = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select tag to remove:")
                            .AddChoices(plugin.Tags));
                    plugin.Tags.Remove(tagToRemove);
                    SavePluginToYaml(plugin);
                    AnsiConsole.MarkupLine("[green]Tag removed![/]");
                }
                break;
        }
    }

    private void ManagePluginDependencies(string pluginName)
    {
        if (pluginManager == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin manager not available.[/]");
            return;
        }

        var plugin = pluginManager.LoadPlugins()
            .SelectMany(p => p.Value)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(p.ScriptPath)) == pluginName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin not found.[/]");
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[cyan]Current dependencies:[/] {string.Join(", ", plugin.Dependencies)}");

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose an action:")
                .AddChoices(new[] { "Add Dependency", "Remove Dependency", "Back" }));

        switch (action)
        {
            case "Add Dependency":
                var newDep = AnsiConsole.Ask<string>("Enter dependency name (plugin name or file path):");
                if (!plugin.Dependencies.Contains(newDep))
                {
                    plugin.Dependencies.Add(newDep);
                    SavePluginToYaml(plugin);
                    AnsiConsole.MarkupLine("[green]Dependency added![/]");
                }
                break;
            case "Remove Dependency":
                if (plugin.Dependencies.Count > 0)
                {
                    var depToRemove = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select dependency to remove:")
                            .AddChoices(plugin.Dependencies));
                    plugin.Dependencies.Remove(depToRemove);
                    SavePluginToYaml(plugin);
                    AnsiConsole.MarkupLine("[green]Dependency removed![/]");
                }
                break;
        }
    }

    private void ViewPluginInfo(string pluginName)
    {
        if (pluginManager == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin manager not available.[/]");
            return;
        }

        var plugin = pluginManager.LoadPlugins()
            .SelectMany(p => p.Value)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(p.ScriptPath)) == pluginName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine("[red]Plugin not found.[/]");
            return;
        }

        AnsiConsole.Clear();
        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Name", plugin.Name);
        table.AddRow("Version", plugin.Version);
        table.AddRow("Menu", plugin.Menu);
        table.AddRow("Description", plugin.Description);
        table.AddRow("Pinned", plugin.IsPinned ? "Yes" : "No");
        table.AddRow("Tags", string.Join(", ", plugin.Tags));
        table.AddRow("Dependencies", string.Join(", ", plugin.Dependencies));
        table.AddRow("Keyboard Shortcut", plugin.KeyboardShortcut ?? "None");
        table.AddRow("Script Path", plugin.ScriptPath);

        AnsiConsole.Write(table);
    }

    private void SavePluginToYaml(Plugin plugin)
    {
        var pluginFolder = Path.GetDirectoryName(plugin.ScriptPath);
        if (string.IsNullOrEmpty(pluginFolder))
            return;

        var yamlPath = Path.Combine(pluginFolder, "plugin.yaml");
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(yamlPath, serializer.Serialize(plugin));

        pluginManager?.ReloadPlugins();
    }
}
