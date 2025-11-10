# RaStudio.py Panels

This folder contains **built-in and plugin-ready UI panels** for RaStudio.py.
Panels are the main building blocks of your modular, agentic, and extensible UI.

---

## Panels Overview

### Built-in Panels

- **DashboardPanel**  
  Mainframe output, async input, modular extension, and plugin diagnostics.

- **ModulesPanel**  
  Displays all loaded modules, supports reload, drop-in extension, plugin cards, and agentic diagnostics.

- **MonitorPanel**  
  Modular monitoring UI for diagnostics, metrics, logs, and plugin/agentic extensions.

- **SettingsPanel**  
  Manages theme, language, plugin/extensible settings, agentic user/config preferences.

- **DiagnosticsPanel**  
  Displays error logs, diagnostics, agentic traces, plugin diagnostics, and extensible error display.

### Plugin Panels

- Place dynamic plugin panels in `plugins/` or use manifest metadata for discovery.
- Plugin panels support manifest metadata, agentic context, diagnostics, and extension UI.

---

## Extending Panels

- Panels can be discovered and loaded via manifest (see `PanelManifest`, `PluginManifest` in `/models`).
- Plugins/extensions can provide their own panels with agentic actions, diagnostics, and metadata.
- All panels support async agentic flows and diagnostics for advanced modular AI UI.

---

## Example Usage

Panels are loaded via `PanelLoader`:

```python
from panels.panel_loader import PanelLoader
panels = PanelLoader.load_panels()
```

## Adding New Panels

- Create a new panel in the `panels` or `plugins/` folder.
- For plugins, provide manifest metadata and a `register_panel()` function.
- Bind ViewModels for agentic data, diagnostics, and extension actions.

---

**Ready for modular, extensible, and agentic UI workflows—just add new Python panel files and register them in your panel loader or plugin manifest!**