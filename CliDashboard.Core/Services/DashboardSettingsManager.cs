using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CliDashboard.Core.Services;

public class DashboardSettingsManager
{
    private readonly string _settingsPath;
    private DashboardSettings? _cachedSettings;
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;

    public event EventHandler<DashboardSettings>? SettingsChanged;

    public DashboardSettingsManager(string dashboardRoot)
    {
        _settingsPath = Path.Combine(dashboardRoot, "dashboard.yaml");
        InitializeFileWatcher();
    }

    private void InitializeFileWatcher()
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (string.IsNullOrEmpty(directory))
            return;

        _watcher = new FileSystemWatcher(directory)
        {
            Filter = "dashboard.yaml",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += (s, e) =>
        {
            lock (_lock)
            {
                _cachedSettings = null;
                var settings = LoadSettings();
                SettingsChanged?.Invoke(this, settings);
            }
        };

        _watcher.EnableRaisingEvents = true;
    }

    public DashboardSettings LoadSettings()
    {
        lock (_lock)
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            if (!File.Exists(_settingsPath))
            {
                var defaultSettings = new DashboardSettings();
                SaveSettings(defaultSettings);
                _cachedSettings = defaultSettings;
                return defaultSettings;
            }

            try
            {
                var yaml = File.ReadAllText(_settingsPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                _cachedSettings = deserializer.Deserialize<DashboardSettings>(yaml);
                return _cachedSettings ?? new DashboardSettings();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: Failed to load settings: {ex.Message}[/]");
                AnsiConsole.MarkupLine("[yellow]Using default settings.[/]");
                return new DashboardSettings();
            }
        }
    }

    public void SaveSettings(DashboardSettings settings)
    {
        lock (_lock)
        {
            try
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(settings);
                
                // Temporarily disable watcher to avoid triggering Changed event
                if (_watcher != null)
                    _watcher.EnableRaisingEvents = false;

                File.WriteAllText(_settingsPath, yaml);
                _cachedSettings = settings;

                if (_watcher != null)
                    _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error saving settings: {ex.Message}[/]");
            }
        }
    }

    public void UpdateSetting<T>(Func<DashboardSettings, T> selector, T value, Action<DashboardSettings, T> setter)
    {
        var settings = LoadSettings();
        setter(settings, value);
        SaveSettings(settings);
    }

    public T GetSetting<T>(Func<DashboardSettings, T> selector)
    {
        var settings = LoadSettings();
        return selector(settings);
    }

    public void ResetToDefaults()
    {
        SaveSettings(new DashboardSettings());
        AnsiConsole.MarkupLine("[green]Settings reset to defaults.[/]");
    }

    public string GetSettingsPath() => _settingsPath;

    public void OpenSettingsInEditor()
    {
        if (!File.Exists(_settingsPath))
        {
            SaveSettings(new DashboardSettings());
        }

        try
        {
            Process.Start(new ProcessStartInfo("code", _settingsPath) { UseShellExecute = true });
        }
        catch
        {
            // Fallback to notepad on Windows
            try
            {
                Process.Start(new ProcessStartInfo("notepad", _settingsPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Could not open editor: {ex.Message}[/]");
                AnsiConsole.MarkupLine($"[yellow]Settings file location: {_settingsPath}[/]");
            }
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
