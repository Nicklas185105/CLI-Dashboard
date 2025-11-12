# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2025-11-12

### Fixed
- The plugins were not executing correctly with the new setup.

### Changed
- Enhanced `ConsoleUtils.Clear` for better error handling.
- Both example plugins updated to use the `ConsoleUtils` methods.
	- Updated the plugin.yaml files.

## [1.0.0] - 2025-11-12

### Added
- Open source project structure
- README.md with comprehensive documentation
- CONTRIBUTING.md with contribution guidelines
- CODE_OF_CONDUCT.md for community standards
- Issue templates (bug report, feature request, plugin submission)
- Pull request template
- MIT License
- SECURITY.md for vulnerability reporting
- Core plugin system using `.csx` scripts with `.yaml` metadata
- Dynamic menu generation from plugin metadata
- Custom script management (Add, Edit, Run, Delete)
- PowerShell script (`.ps1`) support
- Plugin folders and "Create Plugin" scaffolding
- Git toolkit plugin as reference implementation
- `.csx` helper utilities (`ConsoleUtils`, `RunInNewTerminal`, etc.)
- Plugin metadata descriptions in UI
- Color-coded plugin categories in menus
- Plugin reload without restarting dashboard
- Fuzzy search for scripts/plugins
- Git-style command alias system
- Favorite scripts/plugins with quick access
- Plugin versioning support
- Plugin dependencies support
- Plugin-defined keyboard shortcuts
- Per-plugin output logs
- Plugin pinning to top of menu
- Plugin category browsing and tags
- Plugin configs in `%APPDATA%/cli-dashboard/configs/`
- Plugin sharing via export/import ZIP
- Configurable plugin sync folder (Dropbox/Git)
- Global settings file (`dashboard.yaml`)
- Full UI integration for all features
- Cron-like task scheduler for plugin/script execution
- Background worker support for plugin monitoring
- Desktop notifications via PowerShell
- Plugin/script background execution with monitoring
- Build and publish automation scripts (`publish.ps1`, `install.ps1`)

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

---

## Version History

- **[Unreleased]**: Current development version
- **[1.0.1]**: Small fixes and changes to improve plugin functionality
- **[1.0.0]**: Initial release with complete Phase 1-5 features

---

## How to Update This File

When making changes to the project:

1. Add your changes under the **[Unreleased]** section
2. Use the following categories:
   - **Added**: New features
   - **Changed**: Changes to existing functionality
   - **Deprecated**: Soon-to-be removed features
   - **Removed**: Removed features
   - **Fixed**: Bug fixes
   - **Security**: Security fixes or improvements

3. When releasing a new version:
   - Change **[Unreleased]** to **[Version Number] - YYYY-MM-DD**
   - Create a new **[Unreleased]** section at the top
   - Update the version links at the bottom

### Example Entry

```markdown
### Added
- New plugin hot-reload feature (#123)
- Support for background task cancellation (#124)

### Fixed
- Fixed plugin load error on Windows 11 (#125)
```
