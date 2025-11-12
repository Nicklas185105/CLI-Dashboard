using CliDashboard.UI.CLI;

var services = new ServiceCollection();
var scriptRoot = PathUtil.GetRoot();
var pluginRoot = Path.Combine(scriptRoot, "plugins");
var pluginLogsRoot = Path.Combine(scriptRoot, "logs", "plugins");
var configPath = Path.Combine(scriptRoot, "scripts.yaml");

// Register core services
services.AddSingleton<DashboardLogger>();
services.AddSingleton<FavoritesManager>(provider => new FavoritesManager(scriptRoot));
services.AddSingleton<PluginLogger>(provider => new PluginLogger(pluginLogsRoot));
services.AddSingleton<PluginConfigManager>();
services.AddSingleton<DashboardSettingsManager>(provider => new DashboardSettingsManager(scriptRoot));
services.AddSingleton<PluginPackageManager>(provider => new PluginPackageManager(pluginRoot));
services.AddSingleton<PluginSyncManager>(provider => new PluginSyncManager(
    pluginRoot,
    provider.GetRequiredService<DashboardSettingsManager>()));
services.AddSingleton<PluginManager>(provider => new PluginManager(
    pluginRoot, 
    provider.GetRequiredService<PluginLogger>()));
services.AddSingleton<SettingsManager>(provider => new SettingsManager(
    pluginRoot, 
    provider.GetRequiredService<PluginManager>()));
services.AddSingleton<ScriptManager>(provider => new ScriptManager(
    scriptRoot, 
    configPath, 
    provider.GetRequiredService<FavoritesManager>()));
services.AddSingleton<PluginHubManager>(provider => new PluginHubManager(
    pluginRoot, 
    provider.GetRequiredService<PluginManager>(),
    provider.GetRequiredService<PluginLogger>()));
services.AddSingleton<SearchService>();
services.AddSingleton<CommandHandler>();
services.AddSingleton<SettingsHubManager>(provider => new SettingsHubManager(
    provider.GetRequiredService<DashboardSettingsManager>()));
services.AddSingleton<PluginImportExportManager>(provider => new PluginImportExportManager(
    provider.GetRequiredService<PluginPackageManager>(),
    provider.GetRequiredService<PluginManager>()));
services.AddSingleton<PluginSyncHubManager>(provider => new PluginSyncHubManager(
    provider.GetRequiredService<PluginSyncManager>()));
services.AddSingleton<NotificationService>();
services.AddSingleton<TaskSchedulerService>(provider => new TaskSchedulerService(
    scriptRoot,
    provider.GetRequiredService<PluginManager>(),
    provider.GetRequiredService<ScriptManager>(),
    provider.GetRequiredService<NotificationService>()));
services.AddSingleton<BackgroundJobManager>(provider => new BackgroundJobManager(
    provider.GetRequiredService<NotificationService>()));
services.AddSingleton<SchedulerHubManager>(provider => new SchedulerHubManager(
    provider.GetRequiredService<TaskSchedulerService>(),
    provider.GetRequiredService<PluginManager>(),
    provider.GetRequiredService<ScriptManager>()));
services.AddSingleton<BackgroundJobsHubManager>(provider => new BackgroundJobsHubManager(
    provider.GetRequiredService<BackgroundJobManager>(),
    provider.GetRequiredService<PluginManager>(),
    provider.GetRequiredService<ScriptManager>()));

services.AddSingleton<Main>();

// Build the DI container
var provider = services.BuildServiceProvider();

// Check for dotnet-script installation
DotNetScriptChecker.ShowWarningIfNeeded();

// Start the task scheduler
var taskScheduler = provider.GetRequiredService<TaskSchedulerService>();
taskScheduler.Start();

// Handle CLI commands or launch interactive mode
var commandHandler = provider.GetRequiredService<CommandHandler>();
if (!commandHandler.HandleCommand(args))
{
    // No CLI arguments provided, launch interactive mode
    provider.GetRequiredService<Main>().Run();
}
