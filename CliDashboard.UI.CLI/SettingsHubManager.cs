namespace CliDashboard.UI.CLI;

internal class SettingsHubManager(DashboardSettingsManager settingsManager)
{
    public void ShowSettingsMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Settings").Centered().Color(Color.Cyan1));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Choose a settings category:[/]")
                    .AddChoices(new[]
                    {
                        "⚙️  General Settings",
                        "🔄 Plugin Sync Settings",
                        "🛒 Marketplace Settings",
                        "🎨 UI Settings",
                        "📝 Edit Settings File",
                        "🔄 Reset to Defaults",
                        "◀️  Back"
                    }));

            switch (choice)
            {
                case "⚙️  General Settings":
                    ManageGeneralSettings();
                    break;
                case "🔄 Plugin Sync Settings":
                    ManagePluginSyncSettings();
                    break;
                case "🛒 Marketplace Settings":
                    ManageMarketplaceSettings();
                    break;
                case "🎨 UI Settings":
                    ManageUiSettings();
                    break;
                case "📝 Edit Settings File":
                    settingsManager.OpenSettingsInEditor();
                    ConsoleUtils.PauseForUser();
                    break;
                case "🔄 Reset to Defaults":
                    if (AnsiConsole.Confirm("Are you sure you want to reset all settings to defaults?"))
                    {
                        settingsManager.ResetToDefaults();
                    }
                    ConsoleUtils.PauseForUser();
                    break;
                case "◀️  Back":
                    return;
            }
        }
    }

    private void ManageGeneralSettings()
    {
        while (true)
        {
            var settings = settingsManager.LoadSettings();
            var general = settings.General;

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]General Settings[/]\n");

            var table = new Table();
            table.AddColumn("Setting");
            table.AddColumn("Current Value");

            table.AddRow("Enable Telemetry", general.EnableTelemetry ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Auto-check Updates", general.AutoCheckUpdates ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Default Terminal", general.DefaultTerminal ?? "[grey]System Default[/]");
            table.AddRow("Auto-reload Plugins", general.AutoReloadPlugins ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Max Recent Items", general.MaxRecentItems.ToString());

            AnsiConsole.Write(table);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]What would you like to change?[/]")
                    .AddChoices(new[]
                    {
                        "Toggle Telemetry",
                        "Toggle Auto-check Updates",
                        "Set Default Terminal",
                        "Toggle Auto-reload Plugins",
                        "Set Max Recent Items",
                        "Back"
                    }));

            switch (choice)
            {
                case "Toggle Telemetry":
                    settingsManager.UpdateSetting(
                        s => s.General.EnableTelemetry,
                        !general.EnableTelemetry,
                        (s, v) => s.General.EnableTelemetry = v);
                    AnsiConsole.MarkupLine("[green]Telemetry setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Auto-check Updates":
                    settingsManager.UpdateSetting(
                        s => s.General.AutoCheckUpdates,
                        !general.AutoCheckUpdates,
                        (s, v) => s.General.AutoCheckUpdates = v);
                    AnsiConsole.MarkupLine("[green]Auto-check updates setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Default Terminal":
                    var terminal = AnsiConsole.Ask<string>("Enter terminal command (or leave empty for system default):");
                    settingsManager.UpdateSetting(
                        s => s.General.DefaultTerminal,
                        string.IsNullOrWhiteSpace(terminal) ? null : terminal,
                        (s, v) => s.General.DefaultTerminal = v);
                    AnsiConsole.MarkupLine("[green]Default terminal updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Auto-reload Plugins":
                    settingsManager.UpdateSetting(
                        s => s.General.AutoReloadPlugins,
                        !general.AutoReloadPlugins,
                        (s, v) => s.General.AutoReloadPlugins = v);
                    AnsiConsole.MarkupLine("[green]Auto-reload plugins setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Max Recent Items":
                    var maxItems = AnsiConsole.Ask<int>("Enter max recent items to track:", general.MaxRecentItems);
                    settingsManager.UpdateSetting(
                        s => s.General.MaxRecentItems,
                        maxItems,
                        (s, v) => s.General.MaxRecentItems = v);
                    AnsiConsole.MarkupLine("[green]Max recent items updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Back":
                    return;
            }
        }
    }

    private void ManagePluginSyncSettings()
    {
        while (true)
        {
            var settings = settingsManager.LoadSettings();
            var sync = settings.PluginSync;

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]Plugin Sync Settings[/]\n");

            var table = new Table();
            table.AddColumn("Setting");
            table.AddColumn("Current Value");

            table.AddRow("Sync Enabled", sync.Enabled ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Sync Folder Path", sync.SyncFolderPath ?? "[yellow]Not configured[/]");
            table.AddRow("Sync Direction", sync.SyncDirection);
            table.AddRow("Auto-sync on Start", sync.AutoSyncOnStart ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Auto-sync Interval", sync.AutoSyncIntervalMinutes > 0 ? $"{sync.AutoSyncIntervalMinutes} minutes" : "[grey]Disabled[/]");
            table.AddRow("Excluded Plugins", sync.ExcludedPlugins.Count > 0 ? string.Join(", ", sync.ExcludedPlugins) : "[grey]None[/]");

            AnsiConsole.Write(table);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]What would you like to change?[/]")
                    .AddChoices(new[]
                    {
                        "Toggle Sync Enabled",
                        "Set Sync Folder Path",
                        "Change Sync Direction",
                        "Toggle Auto-sync on Start",
                        "Set Auto-sync Interval",
                        "Manage Excluded Plugins",
                        "Back"
                    }));

            switch (choice)
            {
                case "Toggle Sync Enabled":
                    settingsManager.UpdateSetting(
                        s => s.PluginSync.Enabled,
                        !sync.Enabled,
                        (s, v) => s.PluginSync.Enabled = v);
                    AnsiConsole.MarkupLine($"[green]Plugin sync {(!sync.Enabled ? "enabled" : "disabled")}![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Sync Folder Path":
                    var path = AnsiConsole.Ask<string>("Enter sync folder path:");
                    if (Directory.Exists(path) || AnsiConsole.Confirm($"Folder doesn't exist. Create {path}?"))
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        
                        settingsManager.UpdateSetting(
                            s => s.PluginSync.SyncFolderPath,
                            path,
                            (s, v) => s.PluginSync.SyncFolderPath = v);
                        AnsiConsole.MarkupLine("[green]Sync folder path updated![/]");
                    }
                    ConsoleUtils.PauseForUser();
                    break;

                case "Change Sync Direction":
                    var direction = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select sync direction:")
                            .AddChoices("Both", "UploadOnly", "DownloadOnly"));
                    settingsManager.UpdateSetting(
                        s => s.PluginSync.SyncDirection,
                        direction,
                        (s, v) => s.PluginSync.SyncDirection = v);
                    AnsiConsole.MarkupLine("[green]Sync direction updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Auto-sync on Start":
                    settingsManager.UpdateSetting(
                        s => s.PluginSync.AutoSyncOnStart,
                        !sync.AutoSyncOnStart,
                        (s, v) => s.PluginSync.AutoSyncOnStart = v);
                    AnsiConsole.MarkupLine("[green]Auto-sync on start setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Auto-sync Interval":
                    var interval = AnsiConsole.Ask<int>("Enter auto-sync interval in minutes (0 to disable):", sync.AutoSyncIntervalMinutes);
                    settingsManager.UpdateSetting(
                        s => s.PluginSync.AutoSyncIntervalMinutes,
                        interval,
                        (s, v) => s.PluginSync.AutoSyncIntervalMinutes = v);
                    AnsiConsole.MarkupLine("[green]Auto-sync interval updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Manage Excluded Plugins":
                    ManageExcludedPlugins();
                    break;

                case "Back":
                    return;
            }
        }
    }

    private void ManageExcludedPlugins()
    {
        while (true)
        {
            var settings = settingsManager.LoadSettings();
            var excluded = settings.PluginSync.ExcludedPlugins;

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]Excluded Plugins[/]\n");

            if (excluded.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No plugins excluded from sync[/]\n");
            }
            else
            {
                foreach (var plugin in excluded)
                {
                    AnsiConsole.MarkupLine($"• {plugin}");
                }
                AnsiConsole.WriteLine();
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices("Add Plugin", excluded.Count > 0 ? "Remove Plugin" : null, "Back")
                    .AddChoices(new[] { "Back" }));

            switch (choice)
            {
                case "Add Plugin":
                    var pluginName = AnsiConsole.Ask<string>("Enter plugin name to exclude:");
                    if (!excluded.Contains(pluginName))
                    {
                        excluded.Add(pluginName);
                        settingsManager.SaveSettings(settings);
                        AnsiConsole.MarkupLine("[green]Plugin added to exclusion list![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Plugin already in exclusion list![/]");
                    }
                    ConsoleUtils.PauseForUser();
                    break;

                case "Remove Plugin":
                    if (excluded.Count > 0)
                    {
                        var toRemove = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Select plugin to remove from exclusion:")
                                .AddChoices(excluded));
                        excluded.Remove(toRemove);
                        settingsManager.SaveSettings(settings);
                        AnsiConsole.MarkupLine("[green]Plugin removed from exclusion list![/]");
                        ConsoleUtils.PauseForUser();
                    }
                    break;

                case "Back":
                    return;
            }
        }
    }

    private void ManageMarketplaceSettings()
    {
        while (true)
        {
            var settings = settingsManager.LoadSettings();
            var marketplace = settings.Marketplace;

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]Marketplace Settings[/]\n");

            var table = new Table();
            table.AddColumn("Setting");
            table.AddColumn("Current Value");

            table.AddRow("Marketplace Enabled", marketplace.Enabled ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Cache Expiration", $"{marketplace.CacheExpirationHours} hours");
            table.AddRow("Allow Unverified Plugins", marketplace.AllowUnverifiedPlugins ? "[yellow]Yes[/]" : "[green]No[/]");
            table.AddRow("Registry URLs", marketplace.RegistryUrls.Count.ToString());

            AnsiConsole.Write(table);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]What would you like to change?[/]")
                    .AddChoices(new[]
                    {
                        "Toggle Marketplace Enabled",
                        "Set Cache Expiration",
                        "Toggle Allow Unverified Plugins",
                        "Manage Registry URLs",
                        "Back"
                    }));

            switch (choice)
            {
                case "Toggle Marketplace Enabled":
                    settingsManager.UpdateSetting(
                        s => s.Marketplace.Enabled,
                        !marketplace.Enabled,
                        (s, v) => s.Marketplace.Enabled = v);
                    AnsiConsole.MarkupLine($"[green]Marketplace {(!marketplace.Enabled ? "enabled" : "disabled")}![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Set Cache Expiration":
                    var hours = AnsiConsole.Ask<int>("Enter cache expiration in hours:", marketplace.CacheExpirationHours);
                    settingsManager.UpdateSetting(
                        s => s.Marketplace.CacheExpirationHours,
                        hours,
                        (s, v) => s.Marketplace.CacheExpirationHours = v);
                    AnsiConsole.MarkupLine("[green]Cache expiration updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Allow Unverified Plugins":
                    settingsManager.UpdateSetting(
                        s => s.Marketplace.AllowUnverifiedPlugins,
                        !marketplace.AllowUnverifiedPlugins,
                        (s, v) => s.Marketplace.AllowUnverifiedPlugins = v);
                    AnsiConsole.MarkupLine("[green]Unverified plugins setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Manage Registry URLs":
                    AnsiConsole.MarkupLine("[yellow]Registry URL management coming in future marketplace phase![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Back":
                    return;
            }
        }
    }

    private void ManageUiSettings()
    {
        while (true)
        {
            var settings = settingsManager.LoadSettings();
            var ui = settings.UI;

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]UI Settings[/]\n");

            var table = new Table();
            table.AddColumn("Setting");
            table.AddColumn("Current Value");

            table.AddRow("Theme", ui.Theme);
            table.AddRow("Show Descriptions", ui.ShowDescriptions ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Compact Mode", ui.CompactMode ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Show Versions", ui.ShowVersions ? "[green]Yes[/]" : "[red]No[/]");
            table.AddRow("Animation Speed", ui.AnimationSpeed);

            AnsiConsole.Write(table);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[grey]What would you like to change?[/]")
                    .AddChoices(new[]
                    {
                        "Change Theme",
                        "Toggle Show Descriptions",
                        "Toggle Compact Mode",
                        "Toggle Show Versions",
                        "Change Animation Speed",
                        "Back"
                    }));

            switch (choice)
            {
                case "Change Theme":
                    var theme = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select theme:")
                            .AddChoices("Default", "Dark", "Light", "Custom"));
                    settingsManager.UpdateSetting(
                        s => s.UI.Theme,
                        theme,
                        (s, v) => s.UI.Theme = v);
                    AnsiConsole.MarkupLine("[green]Theme updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Show Descriptions":
                    settingsManager.UpdateSetting(
                        s => s.UI.ShowDescriptions,
                        !ui.ShowDescriptions,
                        (s, v) => s.UI.ShowDescriptions = v);
                    AnsiConsole.MarkupLine("[green]Show descriptions setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Compact Mode":
                    settingsManager.UpdateSetting(
                        s => s.UI.CompactMode,
                        !ui.CompactMode,
                        (s, v) => s.UI.CompactMode = v);
                    AnsiConsole.MarkupLine("[green]Compact mode setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Toggle Show Versions":
                    settingsManager.UpdateSetting(
                        s => s.UI.ShowVersions,
                        !ui.ShowVersions,
                        (s, v) => s.UI.ShowVersions = v);
                    AnsiConsole.MarkupLine("[green]Show versions setting updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Change Animation Speed":
                    var speed = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select animation speed:")
                            .AddChoices("Fast", "Normal", "Slow", "None"));
                    settingsManager.UpdateSetting(
                        s => s.UI.AnimationSpeed,
                        speed,
                        (s, v) => s.UI.AnimationSpeed = v);
                    AnsiConsole.MarkupLine("[green]Animation speed updated![/]");
                    ConsoleUtils.PauseForUser();
                    break;

                case "Back":
                    return;
            }
        }
    }
}
