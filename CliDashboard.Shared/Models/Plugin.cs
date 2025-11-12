namespace CliDashboard.Shared.Models;

public class Plugin
{
    public string Name { get; set; } = string.Empty;            // Display name in menu
    public string Menu { get; set; } = string.Empty;            // Menu path (e.g., "Git Tools", "System")
    public string ScriptPath { get; set; } = string.Empty;      // .csx file path
    public string Description { get; set; } = string.Empty;     // Optional tooltip/description
    public string Version { get; set; } = "1.0.0";              // Plugin version (semver format)
    public List<string> Dependencies { get; set; } = new();     // Plugin dependencies (other plugins or libs)
    public string? KeyboardShortcut { get; set; }               // Optional keyboard shortcut (e.g., "Ctrl+G")
    public List<string> Tags { get; set; } = new();             // Tags for categorization and search
    public bool IsPinned { get; set; } = false;                 // Pin to top of menu
    public string? ConfigPath { get; set; }                     // Optional path to plugin-specific config
}
