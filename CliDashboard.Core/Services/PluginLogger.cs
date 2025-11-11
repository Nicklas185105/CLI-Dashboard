using System.Text;

namespace CliDashboard.Core.Services;

public class PluginLogger(string logRoot)
{
    private readonly object _logLock = new();

    public void LogExecution(string pluginName, bool success, string? output = null, string? error = null)
    {
        lock (_logLock)
        {
            Directory.CreateDirectory(logRoot);
            
            var safeName = pluginName.Replace(" ", "-").ToLowerInvariant();
            var logFile = Path.Combine(logRoot, $"{safeName}.log");
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            logEntry.AppendLine($"Status: {(success ? "SUCCESS" : "FAILED")}");
            
            if (!string.IsNullOrEmpty(output))
            {
                logEntry.AppendLine("Output:");
                logEntry.AppendLine(output);
            }
            
            if (!string.IsNullOrEmpty(error))
            {
                logEntry.AppendLine("Error:");
                logEntry.AppendLine(error);
            }
            
            logEntry.AppendLine();
            
            File.AppendAllText(logFile, logEntry.ToString());
        }
    }

    public string GetLogPath(string pluginName)
    {
        var safeName = pluginName.Replace(" ", "-").ToLowerInvariant();
        return Path.Combine(logRoot, $"{safeName}.log");
    }

    public string? ReadLastLog(string pluginName, int lineCount = 50)
    {
        var logFile = GetLogPath(pluginName);
        
        if (!File.Exists(logFile))
            return null;

        var lines = File.ReadAllLines(logFile);
        var lastLines = lines.TakeLast(lineCount);
        return string.Join(Environment.NewLine, lastLines);
    }

    public void ClearLog(string pluginName)
    {
        var logFile = GetLogPath(pluginName);
        if (File.Exists(logFile))
            File.Delete(logFile);
    }

    public List<(string PluginName, DateTime LastExecution)> GetRecentExecutions()
    {
        if (!Directory.Exists(logRoot))
            return new List<(string, DateTime)>();

        return Directory.GetFiles(logRoot, "*.log")
            .Select(f => (
                PluginName: Path.GetFileNameWithoutExtension(f),
                LastExecution: File.GetLastWriteTime(f)
            ))
            .OrderByDescending(x => x.LastExecution)
            .ToList();
    }
}
