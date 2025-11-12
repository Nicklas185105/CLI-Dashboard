namespace CliDashboard.Core.Services;

public class NotificationService
{
    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        try
        {
            // Use PowerShell to show Windows Toast notification
            var icon = type switch
            {
                NotificationType.Success => "✓",
                NotificationType.Warning => "⚠",
                NotificationType.Error => "✗",
                _ => "ℹ"
            };

            var fullTitle = $"{icon} {title}";

            // Build PowerShell command for Windows Toast
            var ps = $@"
                [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
                [Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
                [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] > $null
                
                $APP_ID = '{{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}}\WindowsPowerShell\v1.0\powershell.exe'
                
                $template = @""
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{fullTitle.Replace("\"", "\\\"")}</text>
                            <text>{message.Replace("\"", "\\\"")}</text>
                        </binding>
                    </visual>
                </toast>
""@
                
                $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
                $xml.LoadXml($template)
                $toast = New-Object Windows.UI.Notifications.ToastNotification $xml
                [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($APP_ID).Show($toast)
            ";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{ps}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit(5000); // Wait max 5 seconds
        }
        catch (Exception ex)
        {
            // Fallback: log to console if toast fails
            Console.WriteLine($"[Notification] {title}: {message}");
            Console.WriteLine($"[Notification Error] {ex.Message}");
        }
    }

    public void ShowTaskCompletedNotification(string taskName, bool success, string? error = null)
    {
        if (success)
        {
            ShowNotification(
                "Task Completed",
                $"'{taskName}' executed successfully",
                NotificationType.Success);
        }
        else
        {
            ShowNotification(
                "Task Failed",
                $"'{taskName}' failed: {error ?? "Unknown error"}",
                NotificationType.Error);
        }
    }

    public void ShowBackgroundJobNotification(string jobName, string message, NotificationType type = NotificationType.Info)
    {
        ShowNotification($"Background Job: {jobName}", message, type);
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
