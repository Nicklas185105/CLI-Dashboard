namespace CliDashboard.Shared.Models;

public class Favorites
{
    public List<string> PluginNames { get; set; } = new();
    public List<string> ScriptNames { get; set; } = new();
}
