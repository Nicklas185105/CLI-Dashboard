namespace CliDashboard.Shared.Logging;

public class DashboardLogger
{
    private readonly List<Action<string>> _customSinks = new();

    DashboardLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console() // basic default
            .WriteTo.File("cli-dashboard.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public void AddSink(Action<string> sink)
    {
        _customSinks.Add(sink);
    }

    public void Info(string message)
    {
        Log.Information(message);
        BroadcastToCustomSinks($"[INFO] {message}");
    }

    public void Warn(string message)
    {
        Log.Warning(message);
        BroadcastToCustomSinks($"[WARN] {message}");
    }

    public void Error(string message)
    {
        Log.Error(message);
        BroadcastToCustomSinks($"[ERROR] {message}");
    }

    public void Debug(string message)
    {
        Log.Debug(message);
        BroadcastToCustomSinks($"[DEBUG] {message}");
    }

    private void BroadcastToCustomSinks(string formatted)
    {
        foreach (var sink in _customSinks)
            sink.Invoke(formatted);
    }
}
