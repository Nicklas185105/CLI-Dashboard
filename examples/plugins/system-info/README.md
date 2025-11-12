# System Info Plugin

A more advanced example plugin that demonstrates system interaction and data collection.

## What It Does

Displays comprehensive system information in beautifully formatted tables:
- **Operating System**: OS version, architecture, machine name, uptime
- **Processor**: CPU count, architecture details
- **Memory**: Current process memory usage
- **Disk Drives**: All available drives with space usage

## Features Demonstrated

### 1. System API Integration
```csharp
RuntimeInformation.OSDescription
Environment.ProcessorCount
DriveInfo.GetDrives()
```

### 2. Advanced Spectre.Console Features
- `FigletText` for ASCII art headers
- `Panel` components for organized sections
- `Table` with multiple columns and formatting
- Color-coded output based on data (e.g., disk usage)

### 3. Data Formatting
- Converting bytes to MB/GB
- Percentage calculations
- Time formatting for uptime
- Right-aligned numbers in tables

## How to Use

1. Copy this folder to `%APPDATA%\cli-dashboard\plugins\system-info`
2. Restart CLI Dashboard or reload plugins
3. Navigate to "Examples" category
4. Select "System Info"

## Code Highlights

### Conditional Formatting
```csharp
var barColor = usedPercent > 90 ? Color.Red : 
               usedPercent > 75 ? Color.Yellow : 
               Color.Green;
```

### Nested Spectre Components
```csharp
AnsiConsole.Write(
    new Panel(table)
        .Header("[yellow]Operating System[/]")
        .BorderColor(Color.Yellow));
```

### Iterating System Resources
```csharp
foreach (var drive in DriveInfo.GetDrives())
{
    if (drive.IsReady)
    {
        // Collect and display drive info
    }
}
```

## Customization Ideas

1. **Add more system info**:
   - Network adapters
   - Running processes
   - Environment variables
   - Installed software

2. **Export to file**:
   - Save system info as JSON
   - Generate HTML report
   - Export to CSV

3. **Add monitoring**:
   - Track changes over time
   - Alert on low disk space
   - Monitor CPU usage

4. **Interactive features**:
   - Let user select which info to display
   - Refresh data on keypress
   - Compare with previous snapshots

## Dependencies

- **Spectre.Console 0.50.0** - For rich console UI

## Related Plugins

- **hello-world** - Basic plugin structure
- Git toolkit - External tool interaction
