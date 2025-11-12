namespace CliDashboard.Core.Services;

/// <summary>
/// Simple cron expression parser supporting basic patterns
/// Format: minute hour day month dayOfWeek
/// Examples:
///   "* * * * *"     - Every minute
///   "0 * * * *"     - Every hour
///   "0 9 * * *"     - Daily at 9 AM
///   "0 9 * * 1"     - Every Monday at 9 AM
///   "*/5 * * * *"   - Every 5 minutes
/// </summary>
public class SimpleCronParser
{
    public static DateTime? GetNextOccurrence(string cronExpression, DateTime from)
    {
        try
        {
            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
                return null;

            var minute = parts[0];
            var hour = parts[1];
            var day = parts[2];
            var month = parts[3];
            var dayOfWeek = parts[4];

            var current = from.AddMinutes(1); // Start from next minute
            current = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);

            // Check up to 366 days in the future to find next match
            for (int i = 0; i < 366 * 24 * 60; i++)
            {
                if (Matches(current, minute, hour, day, month, dayOfWeek))
                    return current;

                current = current.AddMinutes(1);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool Matches(DateTime dateTime, string minute, string hour, string day, string month, string dayOfWeek)
    {
        return MatchesValue(dateTime.Minute, minute, 0, 59) &&
               MatchesValue(dateTime.Hour, hour, 0, 23) &&
               MatchesValue(dateTime.Day, day, 1, 31) &&
               MatchesValue(dateTime.Month, month, 1, 12) &&
               (dayOfWeek == "*" || MatchesDayOfWeek(dateTime.DayOfWeek, dayOfWeek));
    }

    private static bool MatchesValue(int value, string pattern, int min, int max)
    {
        if (pattern == "*")
            return true;

        // Handle step values like */5
        if (pattern.StartsWith("*/"))
        {
            if (int.TryParse(pattern.Substring(2), out var step))
            {
                return value % step == 0;
            }
        }

        // Handle specific value
        if (int.TryParse(pattern, out var specific))
        {
            return value == specific;
        }

        // Handle ranges like 1-5
        if (pattern.Contains('-'))
        {
            var range = pattern.Split('-');
            if (range.Length == 2 &&
                int.TryParse(range[0], out var start) &&
                int.TryParse(range[1], out var end))
            {
                return value >= start && value <= end;
            }
        }

        // Handle lists like 1,3,5
        if (pattern.Contains(','))
        {
            var values = pattern.Split(',');
            foreach (var v in values)
            {
                if (int.TryParse(v.Trim(), out var listValue) && value == listValue)
                    return true;
            }
        }

        return false;
    }

    private static bool MatchesDayOfWeek(DayOfWeek dayOfWeek, string pattern)
    {
        if (pattern == "*")
            return true;

        // Convert .NET DayOfWeek (Sunday=0) to cron (Sunday=0 or 7, Monday=1)
        var cronDay = (int)dayOfWeek;

        if (int.TryParse(pattern, out var specific))
        {
            return cronDay == specific || (specific == 7 && cronDay == 0);
        }

        return false;
    }

    public static string GetHumanReadable(string cronExpression)
    {
        try
        {
            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
                return "Invalid cron expression";

            var minute = parts[0];
            var hour = parts[1];
            var day = parts[2];
            var month = parts[3];
            var dayOfWeek = parts[4];

            if (cronExpression == "* * * * *")
                return "Every minute";

            if (minute == "0" && hour == "*" && day == "*" && month == "*" && dayOfWeek == "*")
                return "Every hour";

            if (minute.StartsWith("*/"))
            {
                var interval = minute.Substring(2);
                return $"Every {interval} minutes";
            }

            if (minute != "*" && hour != "*" && day == "*" && month == "*" && dayOfWeek == "*")
                return $"Daily at {hour.PadLeft(2, '0')}:{minute.PadLeft(2, '0')}";

            if (minute != "*" && hour != "*" && day == "*" && month == "*" && dayOfWeek != "*")
            {
                var dayName = GetDayName(dayOfWeek);
                return $"Every {dayName} at {hour.PadLeft(2, '0')}:{minute.PadLeft(2, '0')}";
            }

            return $"Custom: {cronExpression}";
        }
        catch
        {
            return "Invalid cron expression";
        }
    }

    private static string GetDayName(string dayOfWeek)
    {
        return dayOfWeek switch
        {
            "0" or "7" => "Sunday",
            "1" => "Monday",
            "2" => "Tuesday",
            "3" => "Wednesday",
            "4" => "Thursday",
            "5" => "Friday",
            "6" => "Saturday",
            _ => $"day {dayOfWeek}"
        };
    }

    public static bool ValidateCronExpression(string cronExpression)
    {
        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return false;

        return GetNextOccurrence(cronExpression, DateTime.Now) != null;
    }
}
