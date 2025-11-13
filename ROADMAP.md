# 🪯 CLI Dashboard — Project Roadmap

> **Status:** Post-Launch Development
> **Version:** 2.x Series
> **Focus:** Stability, Ecosystem, and User Experience

This roadmap outlines the next major milestones for the CLI Dashboard project.
The initial 1.x release focused on core foundations — plugin loading, scripting, and menu management.
The 2.x roadmap focuses on stability, extensibility, and ecosystem growth.

---

## 🚀 Phase 2.0 — Core Stability & Plugin Architecture Refinement

**Goal:** Strengthen the plugin foundation and ensure long-term stability

**Planned Work**

* Refactor plugin loader:

  * Support metadata validation (version, dependencies)
  * Add lazy loading for faster startup
  * Integrate dependency injection for plugins
* Introduce standardized `plugin.yml` manifest schema
* Implement plugin sandboxing (configurable safety mode)
* Centralized `LoggingService` shared by plugins and scripts
* Add unit tests for:

  * Plugin loader
  * Config manager
  * Scheduler
* Performance profiling (startup, menu generation, execution)

---

## 🧑‍💻 Phase 2.1 — Developer Experience & Documentation

**Goal:** Empower developers to easily create and publish plugins

**Planned Work**

* Write an official **Plugin Development Guide**:

  * Step-by-step tutorial
  * Example plugin template
* Support better CLI commands:

  ```bash
  cli-dashboard new plugin <Name>
  ```
* Publish a lightweight **Plugin SDK** NuGet package (`CliDashboard.SDK`)
* Improve README and CONTRIBUTING with code examples
* Automate docs generation from XML → Markdown

---

## 🧹 Phase 2.2 — Plugin & Script Ecosystem

**Goal:** Foster community-driven plugins and expand built-in functionality

**Planned Work**

* Update built-in **Plugin Manager Menu**
  * List installed plugins
  * Enable/disable/uninstall
  * Check for updates
* Add **Online Plugin Registry** integration
  * Allow submitting plugin manifests via GitHub URL
  * Host community plugin index (e.g., GitHub Pages)
* CLI plugin search/filter support
* Introduce official “Core Plugins”:
  * Task Scheduler
  * File Watcher
  * System Monitor
  * Git Helper
  * Azure DevOps Dashboard

---

## 🎨 Phase 3.0 — UX & Visual Enhancements

**Goal:** Improve usability, interactivity, and visual presentation

**Planned Work**

* Add dynamic color themes (light/dark/custom)
* Improve navigation:
  * Breadcrumbs
  * Keyboard shortcuts
* Animated Spectre.Console elements:
  * Spinners
  * Progress bars
  * Charts
* Configurable dashboard “widgets”:
  * System info
  * Plugin status
  * Network stats
* Persist user layout and theme preferences
* `cli-dashboard config` interactive settings editor

---

## 🐧 Phase 3.1 — Cross-Platform & Advanced Runtime Support

**Goal:** Expand reach and enable more scripting environments

**Planned Work**

* Linux/macOS support (remove Windows-only dependencies)
* Publish as a global .NET tool:
  ```bash
  dotnet tool install -g cli-dashboard
  ```
* Replace hard PowerShell dependency with modular shell integration
* Add optional embedded scripting runtimes:
  * Python (IronPython)
  * PowerShell Core
  * F# Interactive
* Optional plugin sandboxing via AppDomain isolation

---

## ⚙️ Phase 4.0 — Extended Functionality

**Goal:** Transform CLI Dashboard into a modular productivity platform

**Planned Work**

* Remote dashboards (REST API-based)
* Optional GUI client (Avalonia UI or Blazor)
* Introduce **Workflow Mode** (chain plugins/scripts)
* Global event bus for plugin communication
* Per-plugin settings persistence
* Basic Plugin Store (hosted on GitHub Org)

---

## 🌱 Phase 5.0 — Community & Growth

**Goal:** Grow the community and sustain open-source contributions

**Planned Work**

* Create demo video or GIFs for the README
* Add contributor recognition section in CLI Dashboard
* Launch Discord or GitHub Discussions server
* Label onboarding issues (“good first issue”, “help wanted”)
* Add automated contributor acknowledgments

---

## 💡 Future Exploration (Stretch Goals)

* AI-assisted plugin generation (prompt → scaffold)
* Remote script execution with token authentication
* Cloud-sync for dashboard config and plugins
* Plugin telemetry dashboard (opt-in analytics)

---

### 🧱 Versioning Strategy

| Version      | Focus                               | Status         |
| ------------ | ----------------------------------- | -------------- |
| **v1.x**     | Foundation & First Release          | ✅ Complete     |
| **v2.0–2.2** | Core Improvements & Ecosystem       | 🛠 In Progress |
| **v3.x**     | UX & Cross-Platform Support         | ⏳ Planned      |
| **v4.x**     | Advanced Features & GUI Integration | 🧭 Future      |
| **v5.x**     | Community Growth & Sustainability   | 🌱 Long-term   |

---

**Last Updated:** *November 2025*
**Maintainer:** [@Nicklas185105](https://github.com/Nicklas185105)
