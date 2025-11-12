using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Timers;

namespace CliDashboard.Core.Services;

public class TaskSchedulerService : IDisposable
{
    private readonly string _tasksPath;
    private readonly string _historyPath;
    private readonly PluginManager _pluginManager;
    private readonly ScriptManager _scriptManager;
    private readonly NotificationService _notificationService;
    private readonly System.Timers.Timer _checkTimer;
    private readonly object _lock = new();
    private List<ScheduledTask> _tasks = new();
    private bool _isRunning = false;

    public TaskSchedulerService(
        string dashboardRoot,
        PluginManager pluginManager,
        ScriptManager scriptManager,
        NotificationService notificationService)
    {
        _tasksPath = Path.Combine(dashboardRoot, "scheduled-tasks.yaml");
        _historyPath = Path.Combine(dashboardRoot, "logs", "task-history.yaml");
        _pluginManager = pluginManager;
        _scriptManager = scriptManager;
        _notificationService = notificationService;

        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath)!);

        // Check every minute for tasks to run
        _checkTimer = new System.Timers.Timer(60000); // 60 seconds
        _checkTimer.Elapsed += CheckAndExecuteTasks;
        _checkTimer.AutoReset = true;
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning) return;

            LoadTasks();
            UpdateNextExecutionTimes();
            _checkTimer.Start();
            _isRunning = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            _checkTimer.Stop();
            _isRunning = false;
        }
    }

    public List<ScheduledTask> GetAllTasks()
    {
        lock (_lock)
        {
            return new List<ScheduledTask>(_tasks);
        }
    }

    public void AddTask(ScheduledTask task)
    {
        lock (_lock)
        {
            task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
            _tasks.Add(task);
            SaveTasks();
        }
    }

    public void UpdateTask(ScheduledTask task)
    {
        lock (_lock)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing != null)
            {
                var index = _tasks.IndexOf(existing);
                task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
                _tasks[index] = task;
                SaveTasks();
            }
        }
    }

    public void DeleteTask(string taskId)
    {
        lock (_lock)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
            SaveTasks();
        }
    }

    public void ToggleTask(string taskId)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Enabled = !task.Enabled;
                if (task.Enabled)
                {
                    task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
                }
                SaveTasks();
            }
        }
    }

    public List<TaskExecutionHistory> GetTaskHistory(string? taskId = null, int maxRecords = 50)
    {
        try
        {
            if (!File.Exists(_historyPath))
                return new List<TaskExecutionHistory>();

            var yaml = File.ReadAllText(_historyPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var history = deserializer.Deserialize<List<TaskExecutionHistory>>(yaml) ?? new List<TaskExecutionHistory>();

            if (taskId != null)
            {
                history = history.Where(h => h.TaskId == taskId).ToList();
            }

            return history.OrderByDescending(h => h.ExecutionTime).Take(maxRecords).ToList();
        }
        catch
        {
            return new List<TaskExecutionHistory>();
        }
    }

    private void CheckAndExecuteTasks(object? sender, ElapsedEventArgs e)
    {
        lock (_lock)
        {
            var now = DateTime.Now;
            var tasksToExecute = _tasks.Where(t =>
                t.Enabled &&
                t.NextExecutionTime.HasValue &&
                t.NextExecutionTime.Value <= now
            ).ToList();

            foreach (var task in tasksToExecute)
            {
                ExecuteTask(task);
            }
        }
    }

    private void ExecuteTask(ScheduledTask task)
    {
        var startTime = DateTime.Now;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            bool success;
            string? output = null;
            string? error = null;

            if (task.Type == TaskType.Plugin)
            {
                success = ExecutePlugin(task, out output, out error);
            }
            else
            {
                success = ExecuteScript(task, out output, out error);
            }

            sw.Stop();

            // Update task stats
            task.LastExecutionTime = startTime;
            task.ExecutionCount++;
            task.LastStatus = success ? TaskExecutionStatus.Success : TaskExecutionStatus.Failed;
            task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
            SaveTasks();

            // Log history
            LogExecution(task, startTime, success, sw.Elapsed.Milliseconds, output, error);

            // Send notifications
            if ((task.NotifyOnCompletion && success) || (task.NotifyOnFailure && !success))
            {
                _notificationService.ShowTaskCompletedNotification(task.Name, success, error);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            task.LastExecutionTime = startTime;
            task.LastStatus = TaskExecutionStatus.Failed;
            task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
            SaveTasks();

            LogExecution(task, startTime, false, sw.Elapsed.Milliseconds, null, ex.Message);

            if (task.NotifyOnFailure)
            {
                _notificationService.ShowTaskCompletedNotification(task.Name, false, ex.Message);
            }
        }
    }

    private bool ExecutePlugin(ScheduledTask task, out string? output, out string? error)
    {
        output = null;
        error = null;

        try
        {
            var plugins = _pluginManager.LoadPlugins().SelectMany(p => p.Value).ToList();
            var plugin = plugins.FirstOrDefault(p => p.Name.Equals(task.Target, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
            {
                error = $"Plugin '{task.Target}' not found";
                return false;
            }

            var psi = new ProcessStartInfo("dotnet", $"script \"{plugin.ScriptPath}\" {task.Arguments ?? ""}")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = task.WorkingDirectory ?? Path.GetDirectoryName(plugin.ScriptPath)
            };

            var process = Process.Start(psi);
            if (process != null)
            {
                // Read streams asynchronously to avoid deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                process.WaitForExit(300000); // 5 minute timeout
                
                output = outputTask.Result;
                error = errorTask.Result;

                return process.ExitCode == 0;
            }

            error = "Failed to start process";
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool ExecuteScript(ScheduledTask task, out string? output, out string? error)
    {
        output = null;
        error = null;

        try
        {
            var scripts = _scriptManager.LoadScripts();
            var script = scripts.FirstOrDefault(s => s.Name.Equals(task.Target, StringComparison.OrdinalIgnoreCase));

            if (script == null)
            {
                error = $"Script '{task.Target}' not found";
                return false;
            }

            var psi = script.Path.EndsWith(".csx")
                ? new ProcessStartInfo("dotnet", $"script \"{script.Path}\" {task.Arguments ?? ""}")
                : new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -File \"{script.Path}\" {task.Arguments ?? ""}");

            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WorkingDirectory = task.WorkingDirectory ?? Path.GetDirectoryName(script.Path);

            var process = Process.Start(psi);
            if (process != null)
            {
                // Read streams asynchronously to avoid deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                process.WaitForExit(300000); // 5 minute timeout
                
                output = outputTask.Result;
                error = errorTask.Result;

                return process.ExitCode == 0;
            }

            error = "Failed to start process";
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private void LogExecution(ScheduledTask task, DateTime executionTime, bool success, int durationMs, string? output, string? error)
    {
        try
        {
            var history = GetTaskHistory();
            history.Add(new TaskExecutionHistory
            {
                TaskId = task.Id,
                TaskName = task.Name,
                ExecutionTime = executionTime,
                Status = success ? TaskExecutionStatus.Success : TaskExecutionStatus.Failed,
                DurationMs = durationMs,
                Output = output != null ? output.Substring(0, Math.Min(1000, output.Length)) : null, // Limit output size
                Error = error != null ? error.Substring(0, Math.Min(1000, error.Length)) : null
            });

            // Keep only last 500 records
            if (history.Count > 500)
            {
                history = history.OrderByDescending(h => h.ExecutionTime).Take(500).ToList();
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(_historyPath, serializer.Serialize(history));
        }
        catch
        {
            // Ignore history logging errors
        }
    }

    private void LoadTasks()
    {
        try
        {
            if (!File.Exists(_tasksPath))
            {
                _tasks = new List<ScheduledTask>();
                return;
            }

            var yaml = File.ReadAllText(_tasksPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _tasks = deserializer.Deserialize<List<ScheduledTask>>(yaml) ?? new List<ScheduledTask>();
        }
        catch
        {
            _tasks = new List<ScheduledTask>();
        }
    }

    private void SaveTasks()
    {
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(_tasksPath, serializer.Serialize(_tasks));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error saving tasks: {ex.Message}[/]");
        }
    }

    private void UpdateNextExecutionTimes()
    {
        foreach (var task in _tasks.Where(t => t.Enabled))
        {
            if (!task.NextExecutionTime.HasValue || task.NextExecutionTime.Value < DateTime.Now)
            {
                task.NextExecutionTime = SimpleCronParser.GetNextOccurrence(task.CronExpression, DateTime.Now);
            }
        }
        SaveTasks();
    }

    public void Dispose()
    {
        _checkTimer?.Dispose();
    }
}
