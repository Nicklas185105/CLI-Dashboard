namespace CliDashboard.Shared.Models;

public class ScheduledTask
{
    /// <summary>
    /// Unique identifier for the task
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Task name/description
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of task: Plugin or Script
    /// </summary>
    public TaskType Type { get; set; }

    /// <summary>
    /// Plugin name or script name/path
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Cron expression for scheduling (e.g., "0 9 * * *" = daily at 9am)
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// Whether the task is currently enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Send notification when task completes
    /// </summary>
    public bool NotifyOnCompletion { get; set; } = false;

    /// <summary>
    /// Send notification only on failure
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Last execution time
    /// </summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>
    /// Next scheduled execution time
    /// </summary>
    public DateTime? NextExecutionTime { get; set; }

    /// <summary>
    /// Last execution status
    /// </summary>
    public TaskExecutionStatus? LastStatus { get; set; }

    /// <summary>
    /// Total number of executions
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    /// Task creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Additional arguments to pass to plugin/script
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// Working directory for execution
    /// </summary>
    public string? WorkingDirectory { get; set; }
}

public enum TaskType
{
    Plugin,
    Script
}

public enum TaskExecutionStatus
{
    Success,
    Failed,
    Timeout,
    Cancelled
}

public class TaskExecutionHistory
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public DateTime ExecutionTime { get; set; }
    public TaskExecutionStatus Status { get; set; }
    public int DurationMs { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}
