using System.IO.Compression;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CliDashboard.Core.Services;

public class PluginPackageManager(string pluginRoot)
{
    private readonly string _exportPath = Path.Combine(pluginRoot, "..", "exports");

    public string ExportPlugin(string pluginName)
    {
        var pluginDir = Path.Combine(pluginRoot, pluginName);
        
        if (!Directory.Exists(pluginDir))
        {
            throw new DirectoryNotFoundException($"Plugin '{pluginName}' not found at {pluginDir}");
        }

        // Ensure export directory exists
        Directory.CreateDirectory(_exportPath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var zipFileName = $"{pluginName}-{timestamp}.zip";
        var zipPath = Path.Combine(_exportPath, zipFileName);

        try
        {
            // Create ZIP file
            ZipFile.CreateFromDirectory(pluginDir, zipPath, CompressionLevel.Optimal, false);
            
            AnsiConsole.MarkupLine($"[green]Plugin exported successfully![/]");
            AnsiConsole.MarkupLine($"[cyan]Location:[/] {zipPath}");
            
            return zipPath;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Export failed: {ex.Message}[/]");
            throw;
        }
    }

    public void ImportPlugin(string zipPath, bool overwrite = false)
    {
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException($"ZIP file not found: {zipPath}");
        }

        try
        {
            // Create temporary extraction directory
            var tempDir = Path.Combine(Path.GetTempPath(), $"plugin-import-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Extract ZIP to temp directory
                ZipFile.ExtractToDirectory(zipPath, tempDir);

                // Find the plugin.yaml file
                var yamlFiles = Directory.GetFiles(tempDir, "plugin.yaml", SearchOption.AllDirectories);
                
                if (yamlFiles.Length == 0)
                {
                    throw new InvalidOperationException("Invalid plugin package: plugin.yaml not found");
                }

                var yamlPath = yamlFiles[0];
                var pluginDir = Path.GetDirectoryName(yamlPath);

                if (string.IsNullOrEmpty(pluginDir))
                {
                    throw new InvalidOperationException("Could not determine plugin directory");
                }

                // Read plugin metadata to get the plugin name
                var yaml = File.ReadAllText(yamlPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var plugin = deserializer.Deserialize<Plugin>(yaml);
                var safeName = plugin.Name.Replace(" ", "-").ToLowerInvariant();
                var targetDir = Path.Combine(pluginRoot, safeName);

                // Check if plugin already exists
                if (Directory.Exists(targetDir) && !overwrite)
                {
                    var confirm = AnsiConsole.Confirm(
                        $"Plugin '{plugin.Name}' already exists. Overwrite?",
                        false);

                    if (!confirm)
                    {
                        AnsiConsole.MarkupLine("[yellow]Import cancelled.[/]");
                        return;
                    }

                    // Delete existing plugin
                    Directory.Delete(targetDir, true);
                }
                else if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                // Copy plugin to target directory
                CopyDirectory(pluginDir, targetDir);

                AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' imported successfully![/]");
                AnsiConsole.MarkupLine($"[cyan]Version:[/] {plugin.Version}");
                AnsiConsole.MarkupLine($"[cyan]Menu:[/] {plugin.Menu}");
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Import failed: {ex.Message}[/]");
            throw;
        }
    }

    public List<string> ListExportedPlugins()
    {
        if (!Directory.Exists(_exportPath))
            return new List<string>();

        return Directory.GetFiles(_exportPath, "*.zip")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Select(f => f!)
            .OrderByDescending(f => f)
            .ToList();
    }

    public void OpenExportFolder()
    {
        Directory.CreateDirectory(_exportPath);
        
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _exportPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not open folder: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]Export folder: {_exportPath}[/]");
        }
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

    public string GetExportPath() => _exportPath;
}
