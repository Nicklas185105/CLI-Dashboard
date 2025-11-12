namespace CliDashboard.Core.Services;

public class ScriptManager(string? scriptRoot, string? configPath, FavoritesManager favoritesManager)
{
    public void CustomScriptsMenu()
    {
        while (true)
        {
            ConsoleUtils.Clear();
            AnsiConsole.Write(new Rule("Custom Scripts").RuleStyle("grey"));

            var subChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an option:")
                    .AddChoices([
                        "Run Script",
                        "Add Script",
                        "Edit Script",
                        "Delete Script",
                        "Back"
                    ]));

            switch (subChoice)
            {
                case "Run Script":
                    RunCustomScript();
                    break;
                case "Add Script":
                    var scriptType = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Choose script type")
                            .AddChoices("PowerShell (.ps1)", "C# Spectre (.csx)", "Back"));

                    if (scriptType == "Back")
                        break;

                    if (scriptType.StartsWith("PowerShell"))
                        AddCustomScript();
                    else
                        AddSpectreScript();
                    break;
                case "Edit Script":
                    EditCustomScript();
                    break;
                case "Delete Script":
                    DeleteCustomScript();
                    break;
                case "Back":
                    return;
            }
        }
    }

    public void RunCustomScript()
    {
        var scripts = LoadScripts();
        if (scripts.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No scripts found. Add one first![/]");
            return;
        }

        var scriptNames = scripts.Select(s => s.Name).ToList();
        scriptNames.Add("Back");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a script to run:")
                .UseConverter(name =>
                {
                    if (name == "Back") return name;
                    var script = scripts.FirstOrDefault(s => s.Name == name);
                    if (script == null) return name;
                    
                    var star = favoritesManager.IsScriptFavorite(script.Name) ? "★ " : "";
                    var desc = string.IsNullOrWhiteSpace(script.Description) ? "" : $" — [grey]{script.Description}[/]";
                    return $"{star}{script.Name}{desc}";
                })
                .AddChoices(scriptNames));

        if (selected == "Back")
            return;

        var script = scripts.First(s => s.Name == selected);
        
        // Ask: Run or Toggle Favorite
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[grey]What do you want to do with [cyan]{script.Name}[/]?[/]")
                .AddChoices([
                    "Run",
                    favoritesManager.IsScriptFavorite(script.Name) ? "Remove from Favorites" : "Add to Favorites",
                    "Back"
                ]));

        if (action == "Back")
            return;
            
        if (action.Contains("Favorite"))
        {
            favoritesManager.ToggleScriptFavorite(script.Name);
            var msg = favoritesManager.IsScriptFavorite(script.Name) 
                ? $"[green]★[/] Added [cyan]{script.Name}[/] to favorites" 
                : $"Removed [cyan]{script.Name}[/] from favorites";
            AnsiConsole.MarkupLine(msg);
            ConsoleUtils.PauseForUser();
            return;
        }
        
        // Run the script
        var processStartInfo = script.Path.EndsWith(".csx")
            ? new ProcessStartInfo("dotnet", $"script \"{script.Path}\"")
            {
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            }
            : new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\"");

        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = false;
        processStartInfo.RedirectStandardError = false;
        processStartInfo.CreateNoWindow = false;
        var process = new Process
        {
            StartInfo = processStartInfo,
        };
        process.Start();
        process.WaitForExit();

        ConsoleUtils.PauseForUser("Press any key to return to the main menu...");
    }

    public void AddCustomScript()
    {
        var name = AnsiConsole.Ask<string>("Script name:");
        var description = AnsiConsole.Ask<string>("Optional description:");
        string filename = name.Replace(" ", "-").ToLowerInvariant() + ".ps1";
        string scriptPath = Path.Combine(scriptRoot, "scripts", filename);

        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);
        File.WriteAllText(scriptPath, "Write-Host 'Hello from your script!'");
        Process.Start(new ProcessStartInfo("code", scriptPath) { UseShellExecute = true });

        var scripts = LoadScripts();
        scripts.Add(new Script { Name = name, Path = scriptPath, Description = description });
        SaveScripts(scripts);
        AnsiConsole.MarkupLine("[green]Script added![/]");
    }

    public void EditCustomScript()
    {
        var scripts = LoadScripts();
        if (scripts.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No scripts found to edit.[/]");
            return;
        }

        var scriptNames = scripts.Select(s => s.Name).ToList();
        scriptNames.Add("Back");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a script to edit:")
                .UseConverter(name =>
                {
                    var script = scripts.FirstOrDefault(s => s.Name == name);
                    return script is null
                        ? name
                        : string.IsNullOrWhiteSpace(script.Description)
                            ? script.Name
                            : $"{script.Name} — [grey]{script.Description}[/]";
                })
                .AddChoices(scriptNames));

        if (selected == "Back")
            return;

        var script = scripts.First(s => s.Name == selected);
        Process.Start(new ProcessStartInfo("code", script.Path) { UseShellExecute = true });
        AnsiConsole.MarkupLine("[green]Opened in VS Code![/]");
    }

    public List<Script> LoadScripts()
    {
        if (!File.Exists(configPath)) return new();
        var yaml = File.ReadAllText(configPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<List<Script>>(yaml) ?? new();
    }

    public void SaveScripts(List<Script> scripts)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(scripts);
        File.WriteAllText(configPath, yaml);
    }

    public void DeleteCustomScript()
    {
        var scripts = LoadScripts();
        if (scripts.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No scripts found to delete.[/]");
            return;
        }

        var scriptNames = scripts.Select(s => s.Name).ToList();
        scriptNames.Add("Back");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a script to delete:")
                .UseConverter(name =>
                {
                    var script = scripts.FirstOrDefault(s => s.Name == name);
                    return script is null
                        ? name
                        : string.IsNullOrWhiteSpace(script.Description)
                            ? script.Name
                            : $"{script.Name} — [grey]{script.Description}[/]";
                })
                .AddChoices(scriptNames));

        var script = scripts.First(s => s.Name == selected);

        if (selected == "Back")
            return;

        var confirm = AnsiConsole.Confirm($"Are you sure you want to delete [yellow]{script.Name}[/]?");
        if (!confirm) return;

        // Remove from list
        scripts.Remove(script);
        SaveScripts(scripts);

        // Optionally delete file
        if (File.Exists(script.Path))
        {
            var fileConfirm = AnsiConsole.Confirm("Do you also want to delete the script file from disk?");
            if (fileConfirm)
            {
                try
                {
                    File.Delete(script.Path);
                    AnsiConsole.MarkupLine("[green]Script and file deleted.[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to delete file: {ex.Message}[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Script entry deleted, but file left untouched.[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Script entry deleted, but file not found on disk.[/]");
        }
    }

    public void AddSpectreScript()
    {
        var name = AnsiConsole.Ask<string>("Script name:");
        var description = AnsiConsole.Ask<string>("Optional description:");
        string filename = name.Replace(" ", "-").ToLowerInvariant() + ".csx";
        string scriptPath = Path.Combine(scriptRoot, "scripts", filename);

        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);

        File.WriteAllText(scriptPath, """
        #r "nuget: Spectre.Console, 0.48.0"

        using Spectre.Console;

        AnsiConsole.MarkupLine("[bold green]Hello from your Spectre script![/]");
        """);

        Process.Start(new ProcessStartInfo("code", scriptPath) { UseShellExecute = true });

        var scripts = LoadScripts();
        scripts.Add(new Script { Name = name, Path = scriptPath, Description = description });
        SaveScripts(scripts);
        AnsiConsole.MarkupLine("[green]Spectre script added![/]");
    }
}
