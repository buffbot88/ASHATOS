# RaStudio.py Pages

This folder contains top-level **UI pages** for RaStudio.py, hosting the main modular panels and supporting phase 3 agentic extension.

## Pages Overview

- **DashboardPage**  
  Hosts the DashboardPanel and LogsPanel, supports plugin extension cards and agentic context.

- **ModulesPage**  
  Hosts the ModulesPanel, supports plugin module cards and extension points.

- **MonitorPage**  
  Hosts the MonitorPanel, supports plugin metrics, logs, diagnostics, and agentic extension cards.

## Extending Pages

- Add new pages as needed for additional workflows or plugin integrations.
- Inject plugin extension cards and modular components via panel manifest and registry.
- All pages are ready for agentic context and modular extension.

## Example

```python
from pages.dashboard_page import DashboardPage
dashboard_page = DashboardPage(dashboard_vm, logs_vm)
```

---

**Ready for modular, extensible, and agentic UI—just add new Python page files and wire them up in your main window or router!**