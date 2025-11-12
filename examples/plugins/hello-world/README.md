# Hello World Plugin

A simple example plugin that demonstrates the basic structure of a CLI Dashboard plugin.

## What It Does

Displays a colorful "Hello World" message with a table showing key plugin features. This is perfect for learning how plugins work.

## Files

- `plugin.yaml` - Plugin metadata (name, description, version, category, tags)
- `hello.csx` - C# script that runs when the plugin is executed
- `README.md` - This file

## How to Use

1. Copy this folder to `%APPDATA%\cli-dashboard\plugins\hello-world`
2. Restart CLI Dashboard or reload plugins
3. Navigate to "Examples" category in the main menu
4. Select "Hello World"

## Learning Points

### Plugin Metadata (plugin.yaml)

```yaml
name: Hello World
description: A simple example plugin
version: 1.0.0
category: Examples
tags:
  - example
  - tutorial
```

The YAML file tells CLI Dashboard:
- What to display in menus (`name`, `description`)
- How to organize it (`category`, `tags`)
- Version tracking (`version`)

### Plugin Script (hello.csx)

```csharp
#!/usr/bin/env dotnet-script
#r "nuget: Spectre.Console, 0.50.0"

using Spectre.Console;

AnsiConsole.MarkupLine("[bold green]Hello![/]");
```

The script:
- Uses `dotnet-script` to run C# code
- Can reference NuGet packages
- Uses Spectre.Console for rich formatting

## Next Steps

Check out more complex examples:
- **system-info** - Demonstrates system calls and data collection
- **git-toolkit** - Shows how to interact with external tools

## Customization

Try modifying this plugin:
1. Change the colors in the markup
2. Add more rows to the table
3. Add user input with `AnsiConsole.Ask<string>("Question?")`
4. Execute external commands with `Process.Start()`

Happy plugin building! 🚀
