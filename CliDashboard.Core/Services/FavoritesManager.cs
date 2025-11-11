namespace CliDashboard.Core.Services;

public class FavoritesManager
{
    private readonly string _favoritesPath;
    private Favorites? _cachedFavorites;

    public FavoritesManager(string configRoot)
    {
        _favoritesPath = Path.Combine(configRoot, "favorites.yaml");
    }

    public Favorites LoadFavorites()
    {
        if (_cachedFavorites != null)
            return _cachedFavorites;

        if (!File.Exists(_favoritesPath))
        {
            _cachedFavorites = new Favorites();
            return _cachedFavorites;
        }

        try
        {
            var yaml = File.ReadAllText(_favoritesPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _cachedFavorites = deserializer.Deserialize<Favorites>(yaml) ?? new Favorites();
            return _cachedFavorites;
        }
        catch
        {
            _cachedFavorites = new Favorites();
            return _cachedFavorites;
        }
    }

    public void SaveFavorites(Favorites favorites)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(favorites);
        File.WriteAllText(_favoritesPath, yaml);
        _cachedFavorites = favorites;
    }

    public void TogglePluginFavorite(string pluginName)
    {
        var favorites = LoadFavorites();
        if (favorites.PluginNames.Contains(pluginName))
            favorites.PluginNames.Remove(pluginName);
        else
            favorites.PluginNames.Add(pluginName);
        SaveFavorites(favorites);
    }

    public void ToggleScriptFavorite(string scriptName)
    {
        var favorites = LoadFavorites();
        if (favorites.ScriptNames.Contains(scriptName))
            favorites.ScriptNames.Remove(scriptName);
        else
            favorites.ScriptNames.Add(scriptName);
        SaveFavorites(favorites);
    }

    public bool IsPluginFavorite(string pluginName)
    {
        return LoadFavorites().PluginNames.Contains(pluginName);
    }

    public bool IsScriptFavorite(string scriptName)
    {
        return LoadFavorites().ScriptNames.Contains(scriptName);
    }

    public void ClearCache()
    {
        _cachedFavorites = null;
    }
}
