# CLI Dashboard

A fully-featured, plugin-based developer productivity CLI tool with dynamic menus, script execution, scheduling, and extensibility built for power users and automation-focused engineers.

## Features

### 🔌 Plugin System
- **Dynamic Plugin Loading**: Create plugins using `.csx` scripts with `.yaml` metadata
- **Plugin Management**: Add, edit, reload, and manage plugins without restarting
- **Plugin Categories & Tags**: Organize plugins with categories and tags for easy browsing
- **Plugin Versioning**: Track and manage plugin versions
- **Plugin Dependencies**: Support for shared libraries and dependencies
- **Custom Shortcuts**: Define keyboard shortcuts per plugin
- **Plugin Logs**: Per-plugin output logging for debugging
- **Import/Export**: Share plugins via ZIP packages
- **Plugin Sync**: Sync plugins across machines using Dropbox/Git repos

### 📜 Script Management
- **Custom Scripts**: Add, edit, run, and delete custom scripts
- **PowerShell Support**: Legacy `.ps1` script support for quick commands
- **Favorites**: Quick access to frequently used scripts and plugins
- **Fuzzy Search**: Fast search across all scripts and plugins
- **Git-Style Commands**: CLI alias system (e.g., `cli-dashboard add script`)

### ⏰ Automation & Scheduling
- **Task Scheduler**: Cron-like system for scheduling plugin/script execution
- **Background Workers**: Monitor and run plugins in the background
- **Desktop Notifications**: Toast notifications via PowerShell
- **Job Management**: Control background execution and shut down jobs

### ⚙️ Configuration & Sync
- **Global Settings**: Persistent configuration via `dashboard.yaml`
- **Plugin Configs**: Per-plugin configuration in `%APPDATA%\cli-dashboard\configs\`
- **Sync Support**: Configurable sync folders for cross-device plugin sharing

### 🎨 User Experience
- **Dynamic Menus**: Auto-generated menus from plugin metadata
- **Color-Coded UI**: Styled plugin categories using Spectre.Console
- **Interactive Dashboard**: Rich CLI interface with selection prompts
- **Search Integration**: Fuzzy search for quick navigation

## Requirements

- **.NET 8.0** runtime (included in self-contained build)
- **Windows** (x64)
- **PowerShell** (pwsh recommended)
- **dotnet-script** (optional, for executing `.csx` plugins)

## Installation

### Option 1: Quick Install (From Release)

1. Download the latest release from the releases page
2. Extract to a folder
3. Run the install script:
   ```powershell
   .\install.ps1
   ```
4. Open a new terminal and run:
   ```powershell
   CliDashboard.UI.CLI.exe
   ```

### Option 2: Build from Source

1. Clone the repository:
   ```powershell
   git clone https://github.com/yourusername/CliDashboard.git
   cd CliDashboard
   ```

2. Build and publish:
   ```powershell
   .\publish.ps1
   ```

3. Install:
   ```powershell
   .\install.ps1
   ```

The installer will:
- Copy the application to `%APPDATA%\cli-dashboard`
- Add the install directory to your user PATH
- Make `CliDashboard.UI.CLI.exe` available from any terminal

## Usage

### Interactive Mode

Simply run the executable to launch the interactive menu:
```powershell
CliDashboard.UI.CLI.exe
```

Navigate through the menus to:
- Manage plugins
- Run scripts
- Configure settings
- Schedule tasks
- Monitor background jobs

### CLI Commands

Use Git-style commands for quick actions:
```powershell
# Add a new script
CliDashboard.UI.CLI.exe add script

# Search for plugins
CliDashboard.UI.CLI.exe search <query>

# View favorites
CliDashboard.UI.CLI.exe favorites

# And more...
```

## Creating Plugins

### Basic Plugin Structure

1. Create a folder in `%APPDATA%\cli-dashboard\plugins\<plugin-name>\`
2. Add a `plugin.yaml` metadata file:
   ```yaml
   name: My Plugin
   description: Description of what the plugin does
   version: 1.0.0
   category: Development
   tags:
     - git
     - automation
   ```

3. Add a `.csx` script with your logic:
   ```csharp
   using CliDashboard.Shared;
   
   ConsoleUtils.WriteSuccess("Plugin executed!");
   // Your plugin logic here
   ```

### Plugin Helper Utilities

The `CliDashboard.Shared` library provides:
- `ConsoleUtils`: Formatted console output
- `RunInNewTerminal()`: Execute commands in separate terminals
- Logging utilities
- And more...

## Project Structure

```
CliDashboard/
├── CliDashboard.Core/          # Core plugin and script engine
├── CliDashboard.Shared/        # Shared utilities for plugins
├── CliDashboard.UI.CLI/        # CLI interface and menus
├── publish/                    # Build output
├── publish.ps1                 # Build script
├── install.ps1                 # Installation script
└── ROADMAP.md                  # Development roadmap
```

## Configuration

### Global Settings

Located at `%APPDATA%\cli-dashboard\dashboard.yaml`:
```yaml
sync_folder: C:\Users\YourName\Dropbox\cli-dashboard-plugins
auto_reload_plugins: true
# Additional settings...
```

### Plugin Configuration

Per-plugin configs in `%APPDATA%\cli-dashboard\configs\<plugin-name>\`:
- Store API keys, paths, or other plugin-specific settings
- Accessed by plugins at runtime

## Development

### Building

```powershell
# Release build with auto-deploy
dotnet build CliDashboard.UI.CLI -c Release

# Manual publish
.\publish.ps1
```

### Dependencies

- **Spectre.Console**: Rich CLI UI framework
- **YamlDotNet**: YAML parsing
- **Serilog**: Logging
- **Microsoft.Extensions.DependencyInjection**: Dependency injection

## Roadmap

See [ROADMAP.md](ROADMAP.md) for detailed development plans.

Current focus areas:
- ✅ Phases 1-5: Core foundation, plugins, ecosystem, automation (Complete)
- 🧪 Phase 6: Experimental UX enhancements (In Progress)
- 🖼️ Future: GUI with Avalonia UI

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - see [LICENSE](LICENSE) file for details

## Support

For issues, questions, or feature requests, please open an issue on GitHub.

---

**Built with ❤️ for developers who love automation**
