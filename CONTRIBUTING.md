# Contributing to CLI Dashboard

Thank you for your interest in contributing to CLI Dashboard! This document provides guidelines and instructions for contributing.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Creating Plugins](#creating-plugins)

## Code of Conduct

This project adheres to a Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a new branch for your feature or bugfix
4. Make your changes
5. Submit a pull request

## Development Setup

### Prerequisites

- **.NET 8.0 SDK** or later
- **PowerShell** (pwsh recommended)
- **Git**
- **Visual Studio 2022** or **VS Code** (optional but recommended)
- **dotnet-script** (for testing `.csx` plugins)

### Clone and Build

```powershell
# Clone your fork
git clone https://github.com/Nicklas185105/CliDashboard.git
cd CliDashboard

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project CliDashboard.UI.CLI
```

### Project Structure

```
CliDashboard/
├── CliDashboard.Core/          # Core engine (plugins, scripts, services)
├── CliDashboard.Shared/        # Shared utilities for plugins
├── CliDashboard.UI.CLI/        # CLI interface and menus
├── examples/                   # Example plugins
└── .github/                    # GitHub templates and workflows
```

## How to Contribute

### Reporting Bugs

- Use the GitHub issue tracker
- Use the bug report template
- Include detailed steps to reproduce
- Include your environment (OS, .NET version, PowerShell version)
- Attach logs if available (`%APPDATA%\cli-dashboard\logs\`)

### Suggesting Features

- Use the GitHub issue tracker
- Use the feature request template
- Clearly describe the use case
- Explain why this feature would be useful

### Submitting Code

1. Check existing issues and PRs to avoid duplicates
2. Discuss major changes in an issue first
3. Follow the coding standards below
4. Write tests for new functionality
5. Update documentation as needed
6. Submit a pull request using the PR template

## Coding Standards

### C# Style

- **Indentation**: 4 spaces (tabs converted to spaces)
- **Naming Conventions**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - Prefix private fields with `_` (e.g., `_pluginManager`)
- **Null handling**: Use nullable reference types (`string?`)
- **Comments**: XML documentation for public APIs

### Example

```csharp
/// <summary>
/// Manages plugin lifecycle and execution.
/// </summary>
public class PluginManager
{
    private readonly string _pluginRoot;
    private readonly ILogger _logger;

    public PluginManager(string pluginRoot, ILogger logger)
    {
        _pluginRoot = pluginRoot ?? throw new ArgumentNullException(nameof(pluginRoot));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads all plugins from the plugin directory.
    /// </summary>
    /// <returns>List of loaded plugins.</returns>
    public List<Plugin> LoadPlugins()
    {
        // Implementation
    }
}
```

### Plugin Development

- Use YAML for metadata (`plugin.yaml`)
- Use `.csx` for C# script plugins
- Follow the plugin structure in `/examples`
- Test plugins thoroughly before submitting

## Testing

### Running Tests

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test CliDashboard.Tests
```

### Writing Tests

- Place tests in a corresponding `.Tests` project
- Use xUnit for unit tests
- Name tests clearly: `MethodName_Scenario_ExpectedResult`
- Mock external dependencies

Example:
```csharp
[Fact]
public void LoadPlugins_ValidDirectory_ReturnsPluginList()
{
    // Arrange
    var manager = new PluginManager(testDirectory, mockLogger);

    // Act
    var plugins = manager.LoadPlugins();

    // Assert
    Assert.NotEmpty(plugins);
}
```

## Submitting Changes

### Pull Request Process

1. **Update documentation** if you're changing functionality
2. **Update CHANGELOG.md** under "Unreleased" section
3. **Ensure all tests pass**: `dotnet test`
4. **Ensure code builds**: `dotnet build`
5. **Fill out the PR template** completely
6. **Link related issues** using keywords (e.g., "Fixes #123")

### PR Checklist

- [ ] Code follows the project's coding standards
- [ ] Tests added/updated for changes
- [ ] Documentation updated (README, XML comments)
- [ ] CHANGELOG.md updated
- [ ] All tests pass locally
- [ ] No unnecessary dependencies added
- [ ] Commit messages are clear and descriptive

### Commit Messages

Use clear, descriptive commit messages:

```
Add plugin import/export functionality

- Implement ZIP-based plugin packaging
- Add import wizard in plugin hub
- Update PluginPackageManager with compression logic

Fixes #42
```

### Branch Naming

- `feature/short-description` - for new features
- `bugfix/issue-number-description` - for bug fixes
- `docs/description` - for documentation updates

## Creating Plugins

### Plugin Submission

If you're contributing a plugin:

1. Place it in `examples/plugins/your-plugin-name/`
2. Include:
   - `plugin.yaml` with complete metadata
   - `.csx` script file(s)
   - `README.md` explaining what it does
   - Any configuration file examples
3. Test thoroughly in different scenarios
4. Document any external dependencies

### Plugin Guidelines

- Keep plugins focused and single-purpose
- Use descriptive names and categories
- Provide clear error messages
- Handle edge cases gracefully
- Don't hardcode paths (use environment variables)
- Log important actions using `PluginLogger`

## Questions?

If you have questions:
- Check existing issues and discussions
- Open a new discussion on GitHub
- Reach out to maintainers

---

Thank you for contributing to CLI Dashboard! 🎉
