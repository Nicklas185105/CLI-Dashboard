namespace CliDashboard.UI.CLI;

public class CommandHandler(
    PluginManager pluginManager,
    ScriptManager scriptManager,
    SettingsManager settingsManager,
    SearchService searchService)
{
    public bool HandleCommand(string[] args)
    {
        if (args.Length == 0)
            return false; // Launch interactive mode

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "add":
                HandleAdd(args.Skip(1).ToArray());
                return true;

            case "run":
                HandleRun(args.Skip(1).ToArray());
                return true;

            case "list":
                HandleList(args.Skip(1).ToArray());
                return true;

            case "search":
                HandleSearch(args.Skip(1).ToArray());
                return true;

            case "reload":
                HandleReload();
                return true;

            case "help":
            case "--help":
            case "-h":
                ShowHelp();
                return true;

            default:
                AnsiConsole.MarkupLine($"[red]Unknown command:[/] {command}");
                AnsiConsole.MarkupLine("Run [cyan]CliDashboard.UI.CLI help[/] for usage information.");
                return true;
        }
    }

    private void HandleAdd(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] add <script|plugin>");
            return;
        }

        var type = args[0].ToLowerInvariant();
        switch (type)
        {
            case "script":
                AnsiConsole.MarkupLine("[cyan]Opening script creation wizard...[/]");
                var scriptType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose script type")
                        .AddChoices("PowerShell (.ps1)", "C# Spectre (.csx)"));

                if (scriptType.StartsWith("PowerShell"))
                    scriptManager.AddCustomScript();
                else
                    scriptManager.AddSpectreScript();
                break;

            case "plugin":
                AnsiConsole.MarkupLine("[cyan]Opening plugin creation wizard...[/]");
                settingsManager.CreateNewPlugin();
                break;

            default:
                AnsiConsole.MarkupLine($"[red]Unknown type:[/] {type}");
                AnsiConsole.MarkupLine("Use: add <script|plugin>");
                break;
        }
    }

    private void HandleRun(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] run <name>");
            return;
        }

        var name = string.Join(" ", args);
        
        // Search for matching plugin or script
        var allPlugins = pluginManager.LoadPlugins().SelectMany(kvp => kvp.Value).ToList();
        var plugin = allPlugins.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (plugin != null)
        {
            AnsiConsole.MarkupLine($"[cyan]Running plugin:[/] {plugin.Name}");
            pluginManager.ExecutePlugin(plugin);
            return;
        }

        var allScripts = scriptManager.LoadScripts();
        var script = allScripts.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (script != null)
        {
            AnsiConsole.MarkupLine($"[cyan]Running script:[/] {script.Name}");
            var psi = script.Path.EndsWith(".csx")
                ? new ProcessStartInfo("dotnet", $"script \"{script.Path}\"")
                : new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\"");
            psi.UseShellExecute = false;
            var process = new Process { StartInfo = psi };
            process.Start();
            process.WaitForExit();
            return;
        }

        AnsiConsole.MarkupLine($"[red]Not found:[/] {name}");
        AnsiConsole.MarkupLine("Use [cyan]list plugins[/] or [cyan]list scripts[/] to see available items.");
    }

    private void HandleList(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] list <plugins|scripts>");
            return;
        }

        var type = args[0].ToLowerInvariant();
        
        switch (type)
        {
            case "plugins":
                var allPlugins = pluginManager.LoadPlugins();
                if (allPlugins.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No plugins found.[/]");
                    return;
                }

                AnsiConsole.Write(new Rule("[cyan]Plugins[/]").LeftJustified());
                foreach (var category in allPlugins)
                {
                    AnsiConsole.MarkupLine($"\n[blue]{category.Key}[/]");
                    foreach (var plugin in category.Value)
                    {
                        var desc = string.IsNullOrWhiteSpace(plugin.Description) ? "" : $" - [grey]{plugin.Description}[/]";
                        AnsiConsole.MarkupLine($"  • {plugin.Name}{desc}");
                    }
                }
                break;

            case "scripts":
                var allScripts = scriptManager.LoadScripts();
                if (allScripts.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No scripts found.[/]");
                    return;
                }

                AnsiConsole.Write(new Rule("[cyan]Scripts[/]").LeftJustified());
                foreach (var script in allScripts)
                {
                    var desc = string.IsNullOrWhiteSpace(script.Description) ? "" : $" - [grey]{script.Description}[/]";
                    AnsiConsole.MarkupLine($"  • {script.Name}{desc}");
                }
                break;

            default:
                AnsiConsole.MarkupLine($"[red]Unknown type:[/] {type}");
                AnsiConsole.MarkupLine("Use: list <plugins|scripts>");
                break;
        }
    }

    private void HandleSearch(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] search <term>");
            return;
        }

        var query = string.Join(" ", args);
        var results = searchService.SearchAll(query);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No results found for:[/] {query}");
            return;
        }

        AnsiConsole.Write(new Rule($"[cyan]Search Results: \"{query}\"[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[green]Found {results.Count} result(s):[/]\n");

        foreach (var result in results)
        {
            var typeColor = result.Type == "Plugin" ? "blue" : "yellow";
            var desc = string.IsNullOrWhiteSpace(result.Description) ? "" : $" - [grey]{result.Description}[/]";
            AnsiConsole.MarkupLine($"  [{typeColor}][{result.Type}][/] {result.Name}{desc}");
        }
    }

    private void HandleReload()
    {
        AnsiConsole.MarkupLine("[cyan]Reloading plugins...[/]");
        pluginManager.ReloadPlugins();
        AnsiConsole.MarkupLine("[green]✓[/] Plugins reloaded successfully!");
    }

    private void ShowHelp()
    {
        AnsiConsole.Write(new FigletText("CLI Dashboard").Centered().Color(Color.Cyan1));
        AnsiConsole.Write(new Rule("[grey]Command Line Usage[/]"));

        AnsiConsole.MarkupLine("\n[bold]Available Commands:[/]\n");
        
        AnsiConsole.MarkupLine("[cyan]add <script|plugin>[/]");
        AnsiConsole.MarkupLine("  Create a new script or plugin\n");
        
        AnsiConsole.MarkupLine("[cyan]run <name>[/]");
        AnsiConsole.MarkupLine("  Execute a plugin or script by name\n");
        
        AnsiConsole.MarkupLine("[cyan]list <plugins|scripts>[/]");
        AnsiConsole.MarkupLine("  List all available plugins or scripts\n");
        
        AnsiConsole.MarkupLine("[cyan]search <term>[/]");
        AnsiConsole.MarkupLine("  Search for plugins and scripts\n");
        
        AnsiConsole.MarkupLine("[cyan]reload[/]");
        AnsiConsole.MarkupLine("  Reload plugins from disk\n");
        
        AnsiConsole.MarkupLine("[cyan]help[/]");
        AnsiConsole.MarkupLine("  Show this help message\n");

        AnsiConsole.MarkupLine("\n[bold]Interactive Mode:[/]");
        AnsiConsole.MarkupLine("Run without arguments to launch the interactive menu.\n");
    }
}
