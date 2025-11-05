# RaStudio.py Assets (Phase 3)

This folder contains all static and dynamic assets used by RaStudio.py and its plugins/extensions.  
Assets are loaded by the client at runtime and may be referenced or managed by RaServer in distributed deployments.

---

## **Structure Overview**

```
/assets
  /images/         # UI icons, logos, backgrounds
  /sounds/         # Notification, feedback, and UI sounds
  /fonts/          # Custom fonts for UI and themes
  /scripts/        # Plugin, extension, or agentic scripts (Python, YAML, JSON)
  /themes/         # Theme files (see /themes for built-in XAML, here for plugin themes e.g. JSON/YAML)
  /sample_data/    # Example data files for plugins, modules, UI demos
  /docs/           # Asset-specific documentation, licenses
  README.md
```

---

## **Phase 3 Features**

- **Plugin-Ready:**  
  - Assets can be loaded dynamically by plugins/extensions (e.g., custom icons, sounds, or scripts).
  - Third-party plugins may register assets here or in their own package.

- **Agentic Workflows:**  
  - Scripts and sample data can be used for automated workflows, demo jobs, or content generation.

- **Theme Extensibility:**  
  - Custom theme assets (images, colors, fonts) can be added and automatically discovered by the ThemeManager.

---

## **Usage Examples**

- **Add a plugin icon:**  
  Place in `/assets/images/your_plugin_icon.png` and reference in the plugin manifest.

- **Add a notification sound:**  
  Place in `/assets/sounds/notify.wav` and reference from your panel or extension.

- **Add a custom theme:**  
  Place a JSON or YAML theme file in `/assets/themes/` and register via the ThemeManager.

- **Add a sample workflow script:**  
  Place in `/assets/scripts/sample_workflow.py` for demo jobs or plugin development.

---

## **Best Practices**

- Keep asset subfolders organized by type and usage.
- For plugins/extensions, either place assets in `/assets` or inside the plugin package with a manifest reference.
- Document any new asset types or usage in `/assets/docs/` for future contributors.

---

**Ready for modular, distributed, and agentic workflows—just add assets and reference them from your plugins, panels, or RaServer integration!**
