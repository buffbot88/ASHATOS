# RaStudio Services (Phase 3, Python)

This folder contains core service classes that power the RaStudio.py agentic UI and its integration with the RaCore v2.5 server.  
Services are responsible for managing application-wide concerns such as **agentic events, distributed settings, themes, and central infrastructure for modular features**.  
All settings and persistent state are now stored and managed **in RaServer memory** for true headless, distributed operation.

---

## Services Overview

- **ModuleBus (`module_bus.py`)**  
  Agentic event bus for communication between UI panels, plugins, and RaCore modules.  
  Supports diagnostics, async notifications, plugin hooks, and modular extensibility.  
  All events can be routed to RaServer for distributed clients and global diagnostics.

- **SettingsManager (`settings_manager.py`)**  
  Manages user, client, and plugin settings, including theme, language, and extension/plugin settings.  
  Provides persistent storage and runtime access for user preferences by syncing with RaServer memory.

- **ThemeManager (`theme_manager.py`)**  
  Centralized manager for theme switching, resource loading, and custom/plugin theme support.  
  Handles theme persistence and runtime updates, preferences are stored in RaServer.

- **SpeechPipelineService (`speech_pipeline_service.py`)**  
  Pipeline service for agentic input and output, communicating with RaCore's SpeechModule.  
  Supports both sync and async calls, routes all commands through RaServer.

- **RaAttributes (`ra_attributes.py`)**  
  Python decorators for marking plugin panels, modules, and extension points.  
  Supports manifest metadata, agentic context, and plugin/module discovery.

---

## Usage

These services are used throughout RaStudio.py to provide global features:

- **Event-driven communication** between panels and modules (`ModuleBus`)
- **User and client settings** (`SettingsManager`)—all changes sync with RaServer
- **Theme switching and customization** (`ThemeManager`)
- **Agentic input/output pipeline** (`SpeechPipelineService`)
- **Plugin/module/extension registration** (`RaAttributes`)

---

## Extending Services

- **Plugins and extensions** can register their settings and themes via `SettingsManager` and `ThemeManager`—changes propagate to RaServer.
- The event bus (`ModuleBus`) supports dynamic subscription, async publishing, and diagnostics context for advanced modularity.
- All persistent state (settings, preferences, diagnostics, plugin metadata) is managed by RaServer memory for multi-client consistency.

---

## Example

```python
# Subscribe to an event
ModuleBus.subscribe("Diagnostics", lambda evt: handle_diagnostics(evt))

# Change theme
ThemeManager.apply_theme("Dark")

# Get and set a user/client setting (syncs with RaServer)
current_lang = SettingsManager.language
SettingsManager.language = "fr"

# Send agentic input/command to SpeechModule on RaCore server
response = SpeechPipelineService(server_api).send("status")
```

## Phase 3 Architecture Principles

- **Headless, distributed RaServer**: All persistent state is managed centrally.
- **Stateless, modular UI clients**: RaStudio.py only caches or proxies settings—no local storage.
- **Extensible, plugin-ready**: New modules/extensions register via service decorators and sync state with RaServer.
- **Multi-client, real-time**: All diagnostics, events, and preferences propagate across all connected clients.

---

**For further details or to add new features, see the individual service files and RaCore server memory API documentation.**