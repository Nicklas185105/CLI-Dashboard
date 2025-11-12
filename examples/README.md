# Example Plugins

This directory contains example plugins to help you learn how to create your own plugins for CLI Dashboard.

## Available Examples

### 1. Hello World (`plugins/hello-world/`)
**Difficulty**: Beginner  
**Purpose**: Learn the basic structure of a plugin

A simple example that displays a colorful message using Spectre.Console. Perfect for understanding:
- Plugin metadata (YAML)
- Basic script structure (CSX)
- Simple output formatting

### 2. System Info (`plugins/system-info/`)
**Difficulty**: Intermediate  
**Purpose**: Learn system interaction and data display

Displays comprehensive system information with formatted tables. Demonstrates:
- System API integration
- Advanced Spectre.Console features (Panels, Tables, FigletText)
- Data collection and formatting
- Conditional formatting based on values

## Getting Started

### Quick Start

1. **Copy a plugin** to your CLI Dashboard plugins folder:
   ```powershell
   Copy-Item -Recurse examples/plugins/hello-world "$env:APPDATA\cli-dashboard\plugins\hello-world"
   ```

2. **Reload plugins** in CLI Dashboard or restart the application

3. **Run the plugin** from the menu

### Plugin Structure

Every plugin needs at minimum:

```
my-plugin/
├── plugin.yaml          # Metadata
└── script.csx          # C# script
```

**Optional files:**
- `README.md` - Documentation
- `config.yaml` - Plugin configuration
- Additional `.csx` files

### Plugin Metadata (plugin.yaml)

```yaml
name: My Plugin
description: What my plugin does
version: 1.0.0
author: Your Name
category: Category Name
tags:
  - tag1
  - tag2
icon: 🚀
```

### Plugin Script (script.csx)

```csharp
#!/usr/bin/env dotnet-script
#r "nuget: Spectre.Console, 0.50.0"

using Spectre.Console;

AnsiConsole.MarkupLine("[green]Hello from my plugin![/]");
Console.ReadKey(true);
```

## Learning Path

1. **Start with `hello-world`**
   - Understand basic structure
   - Learn simple output

2. **Move to `system-info`**
   - System interaction
   - Advanced formatting
   - Data collection

3. **Check out real plugins**
   - Look in `%APPDATA%\cli-dashboard\plugins\`
   - See how Git Toolkit and other plugins work

4. **Create your own**
   - Start simple
   - Add features incrementally
   - Share with the community!

## Tips for Plugin Development

### 1. Use Spectre.Console
Spectre.Console provides rich formatting:
```csharp
#r "nuget: Spectre.Console, 0.50.0"
using Spectre.Console;

AnsiConsole.MarkupLine("[bold green]Success![/]");
```

### 2. Handle Errors Gracefully
```csharp
try
{
    // Your code
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
}
```

### 3. Use Configuration Files
Store settings in `%APPDATA%\cli-dashboard\configs\your-plugin\`:
```csharp
var configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "cli-dashboard", "configs", "your-plugin", "config.yaml");
```

### 4. Log Important Actions
```csharp
// Logs go to %APPDATA%\cli-dashboard\logs\plugins\your-plugin.log
Console.WriteLine($"[{DateTime.Now}] Action performed");
```

### 5. Test Thoroughly
- Test with different inputs
- Test error conditions
- Test on a clean install

## Common Patterns

### User Input
```csharp
var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");
var confirm = AnsiConsole.Confirm("Are you sure?");
```

### Selection Menu
```csharp
var choice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Choose an option:")
        .AddChoices(new[] { "Option 1", "Option 2", "Option 3" }));
```

### Execute External Commands
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = "status",
        RedirectStandardOutput = true,
        UseShellExecute = false
    }
};
process.Start();
var output = process.StandardOutput.ReadToEnd();
process.WaitForExit();
```

### Progress Indicator
```csharp
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var task = ctx.AddTask("[green]Processing[/]");
        while (!task.IsFinished)
        {
            await Task.Delay(100);
            task.Increment(1.5);
        }
    });
```

## Need Help?

- Check [CONTRIBUTING.md](../CONTRIBUTING.md) for coding standards
- Look at existing plugins for inspiration
- Open an issue on GitHub for questions
- Submit your plugin via Pull Request!

## Contributing Your Plugin

Created a useful plugin? Share it with the community!

1. Add it to `examples/plugins/`
2. Include a complete README.md
3. Test thoroughly
4. Submit a PR using the plugin submission template

See [CONTRIBUTING.md](../CONTRIBUTING.md) for details.

---

Happy plugin building! 🚀
