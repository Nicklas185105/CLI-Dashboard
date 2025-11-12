namespace CliDashboard.Core.Services;

public class PluginSyncManager(string pluginRoot, DashboardSettingsManager settingsManager)
{
    public enum SyncStatus
    {
        Success,
        NoSyncFolder,
        SyncDisabled,
        Error
    }

    public class SyncResult
    {
        public SyncStatus Status { get; set; }
        public int PluginsUploaded { get; set; }
        public int PluginsDownloaded { get; set; }
        public int PluginsSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }

    public SyncResult SyncPlugins()
    {
        var settings = settingsManager.LoadSettings();
        var syncSettings = settings.PluginSync;

        if (!syncSettings.Enabled)
        {
            return new SyncResult
            {
                Status = SyncStatus.SyncDisabled,
                Message = "Plugin sync is disabled in settings"
            };
        }

        if (string.IsNullOrEmpty(syncSettings.SyncFolderPath) || !Directory.Exists(syncSettings.SyncFolderPath))
        {
            return new SyncResult
            {
                Status = SyncStatus.NoSyncFolder,
                Message = "Sync folder not configured or does not exist"
            };
        }

        var result = new SyncResult { Status = SyncStatus.Success };

        try
        {
            // Download (sync from remote to local)
            if (syncSettings.SyncDirection is "Both" or "DownloadOnly")
            {
                result.PluginsDownloaded = DownloadPlugins(syncSettings.SyncFolderPath, syncSettings.ExcludedPlugins, result.Errors);
            }

            // Upload (sync from local to remote)
            if (syncSettings.SyncDirection is "Both" or "UploadOnly")
            {
                result.PluginsUploaded = UploadPlugins(syncSettings.SyncFolderPath, syncSettings.ExcludedPlugins, result.Errors);
            }

            result.Message = $"Sync complete: {result.PluginsUploaded} uploaded, {result.PluginsDownloaded} downloaded";
            
            if (result.Errors.Count > 0)
            {
                result.Status = SyncStatus.Error;
                result.Message += $", {result.Errors.Count} errors";
            }
        }
        catch (Exception ex)
        {
            result.Status = SyncStatus.Error;
            result.Errors.Add(ex.Message);
            result.Message = $"Sync failed: {ex.Message}";
        }

        return result;
    }

    private int DownloadPlugins(string syncFolder, List<string> excludedPlugins, List<string> errors)
    {
        var downloadCount = 0;

        if (!Directory.Exists(syncFolder))
            return downloadCount;

        foreach (var remotePluginDir in Directory.GetDirectories(syncFolder))
        {
            try
            {
                var pluginName = Path.GetFileName(remotePluginDir);
                
                if (excludedPlugins.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
                    continue;

                var localPluginDir = Path.Combine(pluginRoot, pluginName);

                // Check if remote plugin is newer or doesn't exist locally
                if (ShouldDownload(remotePluginDir, localPluginDir))
                {
                    CopyDirectory(remotePluginDir, localPluginDir);
                    downloadCount++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to download plugin {Path.GetFileName(remotePluginDir)}: {ex.Message}");
            }
        }

        return downloadCount;
    }

    private int UploadPlugins(string syncFolder, List<string> excludedPlugins, List<string> errors)
    {
        var uploadCount = 0;
        Directory.CreateDirectory(syncFolder);

        if (!Directory.Exists(pluginRoot))
            return uploadCount;

        foreach (var localPluginDir in Directory.GetDirectories(pluginRoot))
        {
            try
            {
                var pluginName = Path.GetFileName(localPluginDir);
                
                if (excludedPlugins.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
                    continue;

                var remotePluginDir = Path.Combine(syncFolder, pluginName);

                // Check if local plugin is newer or doesn't exist remotely
                if (ShouldUpload(localPluginDir, remotePluginDir))
                {
                    CopyDirectory(localPluginDir, remotePluginDir);
                    uploadCount++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to upload plugin {Path.GetFileName(localPluginDir)}: {ex.Message}");
            }
        }

        return uploadCount;
    }

    private static bool ShouldDownload(string remoteDir, string localDir)
    {
        if (!Directory.Exists(localDir))
            return true;

        var remoteYaml = Path.Combine(remoteDir, "plugin.yaml");
        var localYaml = Path.Combine(localDir, "plugin.yaml");

        if (!File.Exists(remoteYaml))
            return false;

        if (!File.Exists(localYaml))
            return true;

        // Compare last write times
        return File.GetLastWriteTimeUtc(remoteYaml) > File.GetLastWriteTimeUtc(localYaml);
    }

    private static bool ShouldUpload(string localDir, string remoteDir)
    {
        if (!Directory.Exists(remoteDir))
            return true;

        var localYaml = Path.Combine(localDir, "plugin.yaml");
        var remoteYaml = Path.Combine(remoteDir, "plugin.yaml");

        if (!File.Exists(localYaml))
            return false;

        if (!File.Exists(remoteYaml))
            return true;

        // Compare last write times
        return File.GetLastWriteTimeUtc(localYaml) > File.GetLastWriteTimeUtc(remoteYaml);
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
        }

        // Copy all subdirectories
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var destSubDir = Path.Combine(destDir, dirName);
            CopyDirectory(subDir, destSubDir);
        }
    }

    public void ConfigureSyncFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            var create = AnsiConsole.Confirm($"Folder does not exist. Create it at {folderPath}?");
            if (create)
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                return;
            }
        }

        settingsManager.UpdateSetting(
            s => s.PluginSync.SyncFolderPath,
            folderPath,
            (s, v) => s.PluginSync.SyncFolderPath = v
        );

        AnsiConsole.MarkupLine($"[green]Sync folder configured:[/] {folderPath}");
    }

    public void EnableSync(bool enabled)
    {
        settingsManager.UpdateSetting(
            s => s.PluginSync.Enabled,
            enabled,
            (s, v) => s.PluginSync.Enabled = v
        );

        AnsiConsole.MarkupLine($"[green]Plugin sync {(enabled ? "enabled" : "disabled")}[/]");
    }

    public void SetSyncDirection(string direction)
    {
        if (direction != "Both" && direction != "UploadOnly" && direction != "DownloadOnly")
        {
            AnsiConsole.MarkupLine("[red]Invalid sync direction. Must be: Both, UploadOnly, or DownloadOnly[/]");
            return;
        }

        settingsManager.UpdateSetting(
            s => s.PluginSync.SyncDirection,
            direction,
            (s, v) => s.PluginSync.SyncDirection = v
        );

        AnsiConsole.MarkupLine($"[green]Sync direction set to:[/] {direction}");
    }

    public void ShowSyncStatus()
    {
        var settings = settingsManager.LoadSettings();
        var syncSettings = settings.PluginSync;

        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn("Value");

        table.AddRow("Enabled", syncSettings.Enabled ? "[green]Yes[/]" : "[red]No[/]");
        table.AddRow("Sync Folder", syncSettings.SyncFolderPath ?? "[yellow]Not configured[/]");
        table.AddRow("Direction", syncSettings.SyncDirection);
        table.AddRow("Auto-sync on Start", syncSettings.AutoSyncOnStart ? "Yes" : "No");
        table.AddRow("Excluded Plugins", string.Join(", ", syncSettings.ExcludedPlugins));

        AnsiConsole.Write(table);
    }
}
