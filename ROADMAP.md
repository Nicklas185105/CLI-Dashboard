# CLI Dashboard Roadmap

> **Goal**: Build a fully-featured, plugin-based developer productivity CLI tool, with dynamic menus, script execution, and extensibility as its foundation. The project will be CLI-first, with optional future transition into a GUI once core features have matured.

---

## ✅ Phase 1: Core Foundation (Complete)

* Plugin system using `.csx` scripts with `.yaml` metadata
* Dynamic menu generation from plugin metadata
* Custom Script management (Add, Edit, Run, Delete)
* PS1 support (for legacy/quick commands)
* Plugin folders and `Create Plugin` scaffold
* Git toolkit plugin as a first reference plugin
* `.csx` helper utilities (`ConsoleUtils`, `RunInNewTerminal`, etc.)

---

## ✅ Phase 2: Developer Experience Boost (Complete)

* [x] Support plugin metadata descriptions in main UI
* [x] Color-coded or styled plugin categories in menus
* [x] Plugin reload without restarting dashboard
* [x] Fuzzy search for scripts/plugins from main menu
* [x] Git-style command alias system (e.g., `cli-dashboard add script`)
* [x] Add "Favorite Scripts/Plugins" feature with quick access

---

## ✅ Phase 3: Plugin System Enhancement (Complete)

* [x] Plugin versioning support
* [x] Optional "Plugin Dependencies" (shared libs, etc.)
* [x] Plugin-defined keyboard shortcuts (optional)
* [x] Plugin output logs (per plugin)
* [x] Plugin pinning to top of menu
* [x] Plugin category browsing / tags
* [x] Support plugin configs placed in "%appdata%/configs/<pluginname>"/"%appdata%/plugins/<pluginname>/<configfile>"

*Note: DLL/NuGet plugin support moved to future phases*
---

## ✅ Phase 4: Ecosystem & Sync (Complete)

* [x] Plugin sharing: export/import ZIP
* [x] Configurable plugin sync folder (e.g., Dropbox/Git repo)
* [x] Global settings file (e.g., `dashboard.yaml`) to persist state
* [x] Full UI integration for all features

*Note: GitHub marketplace moved to new Phase 7*

---

## ✅ Phase 5: Automation & Scheduling (Complete)

* [x] Cron-like system to schedule plugin/script execution
* [x] Background worker support for plugin monitoring (e.g., alert plugin)
* [x] Desktop notifications via PowerShell or toast
* [x] Plugin/script background execution - Should be able to shut them down from the plugin monitoring

---

## 🧪 Phase 6: Experimental UX Enhancements

* [ ] Menu animations/transitions via Spectre.Console markup
* [ ] Interactive dashboards via nested Spectre.Console UIs
* [ ] Multi-pane CLI layout (if feasible)

---

## 🖼️ GUI (Future Phase)

*GUI development will be deferred until CLI ecosystem is mature.*

* [ ] Evaluate Avalonia UI for cross-platform native GUI
* [ ] Build UI shell with sidebar/menu
* [ ] Maintain YAML+CSX compatibility across CLI/GUI
* [ ] Shared plugin engine DLL for both CLI and GUI

---

## 🎯 Vision

> Become a universal, plugin-based launcher and script hub tailored for power users, developers, and automation-focused engineers.

---

Let me know when you want to begin implementation of any phase!
