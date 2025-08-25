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

## 🛠️ Phase 2: Developer Experience Boost

* [ ] Support plugin metadata descriptions in main UI
* [ ] Color-coded or styled plugin categories in menus
* [ ] Plugin reload without restarting dashboard
* [ ] Fuzzy search for scripts/plugins from main menu
* [ ] Git-style command alias system (e.g., `cli-dashboard add script`)
* [ ] Add "Favorite Scripts/Plugins" feature with quick access

---

## 📁 Phase 3: Plugin System Enhancement

* [ ] Plugin versioning support
* [ ] Optional "Plugin Dependencies" (shared libs, etc.)
* [ ] Plugin-defined keyboard shortcuts (optional)
* [ ] Plugin output logs (per plugin)
* [ ] Plugin pinning to top of menu
* [ ] Plugin category browsing / tags

---

## 🌍 Phase 4: Ecosystem & Sync

* [ ] GitHub-based plugin registry/marketplace (manual import or URL based)
* [ ] Plugin sharing: export/import ZIP
* [ ] Configurable plugin sync folder (e.g., Dropbox/Git repo)
* [ ] Global settings file (e.g., `dashboard.yaml`) to persist state

---

## 🤖 Phase 5: Automation & Scheduling

* [ ] Cron-like system to schedule plugin/script execution
* [ ] Background worker support for plugin monitoring (e.g., alert plugin)
* [ ] Desktop notifications via PowerShell or toast

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
