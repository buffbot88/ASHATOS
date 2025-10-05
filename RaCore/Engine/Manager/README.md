# RaCore Engine: ModuleManager & Registry

## Overview

The RaCore ModuleManager subsystem provides centralized management, dynamic loading, async invocation, diagnostics, and system event orchestration for all agentic modules and extensions.  
It supports drop-in extensibility, plugin hotplugging, and UI integration for advanced agentic workflows.

---

## Directory Structure

```
Engine/
├── Manager/
│   ├── ModuleManager.cs               # Central module manager: load, track, invoke, event bus
│   ├── ModuleManagerAsync.cs          # Async extensions for loading modules
│   ├── ModuleManagerInvokeAsync.cs    # Async extensions for invoking module commands
│   ├── ModuleManagerRegistry.cs       # Adapter for querying modules by name
│   ├── IModuleRegistry.cs             # Contract for querying available modules
│   ├── IRaModule.cs                   # Minimal interface for RaCore modules
│   ├── ModuleBase.cs                  # Base class for modules, logging helpers
│   ├── ModuleLoadResult.cs            # Result type for module loading
│   ├── ModuleResponse.cs              # Agentic response type for modules
│   ├── ModuleWrapper.cs               # Wrapper for IRaModule, lifecycle, async/sync invocation
│   ├── ModuleWrapperView.cs           # Reflection adapter for UI/diagnostics
│   ├── RaModuleAttribute.cs           # Attribute for module discovery/categorization
│   ├── README.md                      # Documentation for contributors/extension
```

---

## Key Components

- **ModuleManager.cs**:  
  Central manager for loading, tracking, invoking, and orchestrating modules.

- **Async Extensions**:  
  Support for async loading, invocation, and diagnostics.

- **Module Registry & Wrappers**:  
  Reflection views for UI, diagnostics, and plugin infrastructure.

- **System Events**:  
  Cross-module bus for warmup, boot, reload, and extension orchestration.

---

## API Highlights

- `LoadModules()` / `ReloadModules()` — Dynamic module discovery and instantiation.
- `SafeInvokeModuleByName()` / `SafeInvokeModuleByNameAsync()` — Async/sync agentic invocation.
- `RaiseSystemEvent()` — Event bus for cross-module orchestration.
- Drop-in support for plugins, extensions, and UI integration.

---

## Extending

- Add new modules by inheriting `IRaModule` and using `RaModuleAttribute`.
- Hotplug plugins/extensions at runtime.
- Integrate with game clients, headless agents, and distributed orchestration.

---

## Example Usage

```csharp
var manager = new ModuleManager();
manager.LoadModules();
var response = manager.SafeInvokeModuleByName("Memory", "recall user_name");

await manager.SafeInvokeModuleByNameAsync("Speech", "think What is my status?");
```

---

## Contributors

Document new registry/manager extensions, diagnostics, and plugin patterns here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
