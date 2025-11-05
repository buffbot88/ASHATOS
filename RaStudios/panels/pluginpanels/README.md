# RaStudio.py Plugin Panels

This folder contains agentic, plugin-ready UI panels for RaStudio.py, loaded dynamically at runtime via manifest metadata or plugin registration.

## Structure

- **iplugin_panel.py**  
  Defines the base interface for plugin panels (initialization, diagnostics, metadata).

- **plugin_registry.py**  
  Registry for plugin panels; enables manifest-based discovery and diagnostic context.

- **hello_world_plugin_panel.py**  
  Example plugin panel. Plugins should subclass IPluginPanel and register themselves.

## Usage

- **Register a plugin panel:**
  ```python
  from panels.pluginpanels.plugin_registry import PluginRegistry
  from panels.pluginpanels.hello_world_plugin_panel import HelloWorldPluginPanel

  panel = HelloWorldPluginPanel()
  PluginRegistry.register_panel(panel, metadata={"name": "HelloWorldDemo"})
  ```

- **Discover plugin panels:**
  ```python
  found = PluginRegistry.find_panel_by_metadata({"name": "HelloWorldDemo"})
  panels = PluginRegistry.find_panels(lambda meta: meta and meta.get("name") == "HelloWorldDemo")
  ```

## Extending

- Add new plugin panel classes in this folder or in your plugin package.
- Use manifest metadata for agentic context, diagnostics, and UI extension.
- For full agentic workflows, connect plugin panels to RaServer via services.

---

**Ready for modular, extensible, and agentic UI workflows—just add new Python plugin panel files and register them with the PluginRegistry!**