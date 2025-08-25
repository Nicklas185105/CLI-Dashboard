using CliDashboard.UI.CLI;

var services = new ServiceCollection();
var scriptRoot = Setup.GetScriptRoot();
var pluginRoot = Path.Combine(scriptRoot, "plugins");
var configPath = Path.Combine(scriptRoot, "scripts.yaml");

// Register core services
services.AddSingleton<DashboardLogger>();
services.AddSingleton(provider => new PluginManager(pluginRoot));
services.AddSingleton(provider => new SettingsManager(pluginRoot));
services.AddSingleton(provider => new ScriptManager(scriptRoot, configPath));

services.AddSingleton<Main>();

// Build the DI container
var provider = services.BuildServiceProvider();

// Run the main menu
provider.GetRequiredService<Main>().Run();
