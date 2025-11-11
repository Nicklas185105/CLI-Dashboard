namespace CliDashboard.UI.CLI;

internal class PluginImportExportManager(
    PluginPackageManager packageManager,
    PluginManager pluginManager)
{
    public void ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Plugin Import/Export").Centered().Color(Color.Green));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]What would you like to do?[/]")
                    .AddChoices(new[]
                    {
                        "📦 Export Plugin",
                        "📥 Import Plugin from ZIP",
                        "📂 Browse Exported Plugins",
                        "🗂️  Open Export Folder",
                        "◀️  Back"
                    }));

            switch (choice)
            {
                case "📦 Export Plugin":
                    ExportPlugin();
                    break;
                case "📥 Import Plugin from ZIP":
                    ImportPlugin();
                    break;
                case "📂 Browse Exported Plugins":
                    BrowseExportedPlugins();
                    break;
                case "🗂️  Open Export Folder":
                    packageManager.OpenExportFolder();
                    ConsoleUtils.PauseForUser();
                    break;
                case "◀️  Back":
                    return;
            }
        }
    }

    private void ExportPlugin()
    {
        var plugins = pluginManager.LoadPlugins();
        var allPlugins = plugins.SelectMany(p => p.Value).ToList();

        if (allPlugins.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No plugins available to export.[/]");
            ConsoleUtils.PauseForUser();
            return;
        }

        var pluginNames = allPlugins
            .Select(p => $"{p.Name} ({p.Menu}) - v{p.Version}")
            .ToList();

        pluginNames.Add("Cancel");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Select a plugin to export:[/]")
                .PageSize(15)
                .AddChoices(pluginNames));

        if (selected == "Cancel")
            return;

        // Extract plugin name from selection
        var pluginName = allPlugins[pluginNames.IndexOf(selected)].Name;
        var safeName = pluginName.Replace(" ", "-").ToLowerInvariant();

        try
        {
            var exportPath = packageManager.ExportPlugin(safeName);
            AnsiConsole.MarkupLine($"\n[green]✓ Plugin exported successfully![/]");
            
            if (AnsiConsole.Confirm("Would you like to open the export folder?"))
            {
                packageManager.OpenExportFolder();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Export failed: {ex.Message}[/]");
        }

        ConsoleUtils.PauseForUser();
    }

    private void ImportPlugin()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[cyan]Import Plugin from ZIP[/]\n");

        var importChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("How would you like to import?")
                .AddChoices(new[]
                {
                    "Browse exported plugins",
                    "Enter custom path",
                    "Cancel"
                }));

        string? zipPath = null;

        switch (importChoice)
        {
            case "Browse exported plugins":
                var exported = packageManager.ListExportedPlugins();
                if (exported.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No exported plugins found.[/]");
                    ConsoleUtils.PauseForUser();
                    return;
                }

                exported.Add("Cancel");
                var selected = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a plugin package:")
                        .PageSize(15)
                        .AddChoices(exported));

                if (selected == "Cancel")
                    return;

                zipPath = Path.Combine(packageManager.GetExportPath(), selected);
                break;

            case "Enter custom path":
                zipPath = AnsiConsole.Ask<string>("Enter the full path to the ZIP file:");
                break;

            case "Cancel":
                return;
        }

        if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
        {
            AnsiConsole.MarkupLine("[red]File not found or invalid path.[/]");
            ConsoleUtils.PauseForUser();
            return;
        }

        try
        {
            packageManager.ImportPlugin(zipPath);
            AnsiConsole.MarkupLine("\n[green]✓ Plugin imported successfully![/]");
            AnsiConsole.MarkupLine("[yellow]Note: Plugins will be available after reloading.[/]");
            
            if (AnsiConsole.Confirm("Would you like to reload plugins now?"))
            {
                pluginManager.ReloadPlugins();
                AnsiConsole.MarkupLine("[green]Plugins reloaded![/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Import failed: {ex.Message}[/]");
        }

        ConsoleUtils.PauseForUser();
    }

    private void BrowseExportedPlugins()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[cyan]Exported Plugins[/]\n");

        var exported = packageManager.ListExportedPlugins();
        
        if (exported.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No exported plugins found.[/]");
            AnsiConsole.MarkupLine($"[grey]Export folder: {packageManager.GetExportPath()}[/]");
            ConsoleUtils.PauseForUser();
            return;
        }

        var table = new Table();
        table.AddColumn("Filename");
        table.AddColumn("Size");
        table.AddColumn("Created");

        foreach (var file in exported)
        {
            var fullPath = Path.Combine(packageManager.GetExportPath(), file);
            var fileInfo = new FileInfo(fullPath);
            
            table.AddRow(
                file,
                FormatFileSize(fileInfo.Length),
                fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm"));
        }

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Total: {exported.Count} plugin(s)[/]");
        AnsiConsole.MarkupLine($"[grey]Location: {packageManager.GetExportPath()}[/]");

        ConsoleUtils.PauseForUser();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
