namespace CliDashboard.Shared.Models;

public class DashboardSettings
{
    /// <summary>
    /// Version of the dashboard settings schema
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// General application preferences
    /// </summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>
    /// Plugin synchronization settings
    /// </summary>
    public PluginSyncSettings PluginSync { get; set; } = new();

    /// <summary>
    /// Marketplace and registry settings
    /// </summary>
    public MarketplaceSettings Marketplace { get; set; } = new();

    /// <summary>
    /// UI/Theme preferences
    /// </summary>
    public UiSettings UI { get; set; } = new();
}

public class GeneralSettings
{
    /// <summary>
    /// Enable telemetry and analytics
    /// </summary>
    public bool EnableTelemetry { get; set; } = false;

    /// <summary>
    /// Auto-check for dashboard updates
    /// </summary>
    public bool AutoCheckUpdates { get; set; } = true;

    /// <summary>
    /// Default terminal for script execution
    /// </summary>
    public string? DefaultTerminal { get; set; }

    /// <summary>
    /// Enable plugin auto-reload on file changes
    /// </summary>
    public bool AutoReloadPlugins { get; set; } = true;

    /// <summary>
    /// Maximum number of recent items to track
    /// </summary>
    public int MaxRecentItems { get; set; } = 10;
}

public class PluginSyncSettings
{
    /// <summary>
    /// Enable plugin synchronization
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Path to sync folder (can be Dropbox, OneDrive, Git repo, etc.)
    /// </summary>
    public string? SyncFolderPath { get; set; }

    /// <summary>
    /// Auto-sync on dashboard start
    /// </summary>
    public bool AutoSyncOnStart { get; set; } = false;

    /// <summary>
    /// Auto-sync interval in minutes (0 = disabled)
    /// </summary>
    public int AutoSyncIntervalMinutes { get; set; } = 0;

    /// <summary>
    /// Sync direction: Both, UploadOnly, DownloadOnly
    /// </summary>
    public string SyncDirection { get; set; } = "Both";

    /// <summary>
    /// List of plugin names to exclude from sync
    /// </summary>
    public List<string> ExcludedPlugins { get; set; } = new();
}

public class MarketplaceSettings
{
    /// <summary>
    /// Enable marketplace features
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// GitHub repository URLs for plugin registries
    /// </summary>
    public List<string> RegistryUrls { get; set; } = new()
    {
        "https://github.com/cli-dashboard/plugin-registry"
    };

    /// <summary>
    /// Cache marketplace data for X hours
    /// </summary>
    public int CacheExpirationHours { get; set; } = 24;

    /// <summary>
    /// Allow installing plugins from unverified sources
    /// </summary>
    public bool AllowUnverifiedPlugins { get; set; } = false;
}

public class UiSettings
{
    /// <summary>
    /// Color theme: Default, Dark, Light, Custom
    /// </summary>
    public string Theme { get; set; } = "Default";

    /// <summary>
    /// Show plugin descriptions in menus
    /// </summary>
    public bool ShowDescriptions { get; set; } = true;

    /// <summary>
    /// Compact mode (less spacing)
    /// </summary>
    public bool CompactMode { get; set; } = false;

    /// <summary>
    /// Show plugin versions in menu
    /// </summary>
    public bool ShowVersions { get; set; } = false;

    /// <summary>
    /// Animation speed: Fast, Normal, Slow, None
    /// </summary>
    public string AnimationSpeed { get; set; } = "Normal";
}
