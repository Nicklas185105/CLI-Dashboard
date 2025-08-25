namespace CliDashboard.Models;

internal class Plugin
{
    public string Name { get; set; } = string.Empty;            // Display name in menu
    public string Menu { get; set; } = string.Empty;            // Menu path (e.g., "Git Tools", "System")
    public string ScriptPath { get; set; } = string.Empty;      // .csx file path
    public string Description { get; set; } = string.Empty;     // Optional tooltip/description
}
