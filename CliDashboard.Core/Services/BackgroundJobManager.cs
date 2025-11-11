namespace CliDashboard.Core.Services;

public class BackgroundJobManager
{
    private readonly Dictionary<string, BackgroundJob> _runningJobs = new();
    private readonly object _lock = new();
    private readonly NotificationService _notificationService;

    public BackgroundJobManager(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public string StartJob(string name, string command, string arguments, string? workingDirectory = null)
    {
        lock (_lock)
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new BackgroundJob
            {
                Id = jobId,
                Name = name,
                Command = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                StartTime = DateTime.Now,
                Status = JobStatus.Running
            };

            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            try
            {
                job.Process = Process.Start(psi);
                if (job.Process != null)
                {
                    _runningJobs[jobId] = job;
                    
                    // Monitor process completion in background
                    Task.Run(() => MonitorJob(jobId));
                    
                    _notificationService.ShowBackgroundJobNotification(name, "Job started", NotificationType.Info);
                    return jobId;
                }
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                job.EndTime = DateTime.Now;
                job.Error = ex.Message;
                _notificationService.ShowBackgroundJobNotification(name, $"Failed to start: {ex.Message}", NotificationType.Error);
            }

            return jobId;
        }
    }

    public void StopJob(string jobId)
    {
        lock (_lock)
        {
            if (_runningJobs.TryGetValue(jobId, out var job))
            {
                try
                {
                    if (job.Process != null && !job.Process.HasExited)
                    {
                        job.Process.Kill(true); // Kill process tree
                        job.Status = JobStatus.Stopped;
                        job.EndTime = DateTime.Now;
                        _notificationService.ShowBackgroundJobNotification(job.Name, "Job stopped", NotificationType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    job.Error = ex.Message;
                }
            }
        }
    }

    public List<BackgroundJob> GetAllJobs()
    {
        lock (_lock)
        {
            return _runningJobs.Values.ToList();
        }
    }

    public BackgroundJob? GetJob(string jobId)
    {
        lock (_lock)
        {
            return _runningJobs.TryGetValue(jobId, out var job) ? job : null;
        }
    }

    public void RemoveJob(string jobId)
    {
        lock (_lock)
        {
            if (_runningJobs.TryGetValue(jobId, out var job))
            {
                if (job.Process != null && !job.Process.HasExited)
                {
                    StopJob(jobId);
                }
                _runningJobs.Remove(jobId);
            }
        }
    }

    private void MonitorJob(string jobId)
    {
        BackgroundJob? job;
        lock (_lock)
        {
            if (!_runningJobs.TryGetValue(jobId, out job))
                return;
        }

        try
        {
            job.Process?.WaitForExit();
            
            lock (_lock)
            {
                job.EndTime = DateTime.Now;
                if (job.Process != null)
                {
                    job.ExitCode = job.Process.ExitCode;
                    job.Output = job.Process.StandardOutput.ReadToEnd();
                    job.Error = job.Process.StandardError.ReadToEnd();
                    job.Status = job.Process.ExitCode == 0 ? JobStatus.Completed : JobStatus.Failed;
                    
                    var type = job.Status == JobStatus.Completed ? NotificationType.Success : NotificationType.Error;
                    _notificationService.ShowBackgroundJobNotification(
                        job.Name,
                        job.Status == JobStatus.Completed ? "Job completed successfully" : "Job failed",
                        type);
                }
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                job.Status = JobStatus.Failed;
                job.EndTime = DateTime.Now;
                job.Error = ex.Message;
            }
        }
    }
}

public class BackgroundJob
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string? WorkingDirectory { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public JobStatus Status { get; set; }
    public int? ExitCode { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public Process? Process { get; set; }
    
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
}

public enum JobStatus
{
    Running,
    Completed,
    Failed,
    Stopped
}
