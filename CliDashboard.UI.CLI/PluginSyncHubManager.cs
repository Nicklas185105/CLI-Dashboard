namespace CliDashboard.UI.CLI;

internal class PluginSyncHubManager(PluginSyncManager syncManager)
{
    public void ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Plugin Sync").Centered().Color(Color.Blue));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Plugin Synchronization[/]")
                    .AddChoices(new[]
                    {
                        "🔄 Sync Now",
                        "📊 View Sync Status",
                        "⚙️  Configure Sync",
                        "◀️  Back"
                    }));

            switch (choice)
            {
                case "🔄 Sync Now":
                    SyncNow();
                    break;
                case "📊 View Sync Status":
                    ViewSyncStatus();
                    break;
                case "⚙️  Configure Sync":
                    ConfigureSync();
                    break;
                case "◀️  Back":
                    return;
            }
        }
    }

    private void SyncNow()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[cyan]Starting Plugin Sync...[/]\n");

        var result = AnsiConsole.Status()
            .Start("Synchronizing plugins...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                return syncManager.SyncPlugins();
            });

        AnsiConsole.WriteLine();

        // Display results
        switch (result.Status)
        {
            case PluginSyncManager.SyncStatus.Success:
                AnsiConsole.MarkupLine("[green]✓ Sync completed successfully![/]\n");
                
                var table = new Table();
                table.AddColumn("Metric");
                table.AddColumn("Count");
                table.AddRow("Plugins Uploaded", result.PluginsUploaded.ToString());
                table.AddRow("Plugins Downloaded", result.PluginsDownloaded.ToString());
                table.AddRow("Plugins Skipped", result.PluginsSkipped.ToString());
                
                if (result.Errors.Count > 0)
                {
                    table.AddRow("[yellow]Errors[/]", $"[yellow]{result.Errors.Count}[/]");
                }
                
                AnsiConsole.Write(table);
                
                if (result.Errors.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[yellow]Errors encountered:[/]");
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLine($"[red]• {error}[/]");
                    }
                }
                break;

            case PluginSyncManager.SyncStatus.SyncDisabled:
                AnsiConsole.MarkupLine("[yellow]⚠ Plugin sync is disabled.[/]");
                AnsiConsole.MarkupLine("[grey]Enable it in sync configuration.[/]");
                break;

            case PluginSyncManager.SyncStatus.NoSyncFolder:
                AnsiConsole.MarkupLine("[yellow]⚠ Sync folder not configured.[/]");
                AnsiConsole.MarkupLine("[grey]Configure a sync folder in settings.[/]");
                break;

            case PluginSyncManager.SyncStatus.Error:
                AnsiConsole.MarkupLine($"[red]✗ Sync failed: {result.Message}[/]");
                if (result.Errors.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLine($"[red]• {error}[/]");
                    }
                }
                break;
        }

        ConsoleUtils.PauseForUser();
    }

    private void ViewSyncStatus()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[cyan]Plugin Sync Status[/]\n");
        
        syncManager.ShowSyncStatus();
        
        ConsoleUtils.PauseForUser();
    }

    private void ConfigureSync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]Configure Plugin Sync[/]\n");
            
            syncManager.ShowSyncStatus();

            AnsiConsole.WriteLine();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]What would you like to configure?[/]")
                    .AddChoices(new[]
                    {
                        "Toggle Sync Enabled",
                        "Set Sync Folder Path",
                        "Change Sync Direction",
                        "View Advanced Settings",
                        "Back"
                    }));

            switch (choice)
            {
                case "Toggle Sync Enabled":
                    var enable = AnsiConsole.Confirm("Enable plugin sync?");
                    syncManager.EnableSync(enable);
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Sync Folder Path":
                    var path = AnsiConsole.Ask<string>("Enter sync folder path:");
                    syncManager.ConfigureSyncFolder(path);
                    ConsoleUtils.PauseForUser();
                    break;

                case "Change Sync Direction":
                    var direction = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select sync direction:")
                            .AddChoices("Both", "UploadOnly", "DownloadOnly"));
                    syncManager.SetSyncDirection(direction);
                    ConsoleUtils.PauseForUser();
                    break;

                case "View Advanced Settings":
                    AnsiConsole.MarkupLine("\n[yellow]For advanced sync settings (auto-sync, exclusions, etc.),[/]");
                    AnsiConsole.MarkupLine("[yellow]please use the main Settings → Plugin Sync Settings menu.[/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Back":
                    return;
            }
        }
    }
}
