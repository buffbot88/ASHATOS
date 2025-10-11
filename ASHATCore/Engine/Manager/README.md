# ASHATCore Engine: ModuleManager & Registry

## Overview

The ASHATCore ModuleManager subsystem provides Centralized management, dynamic loading, async invocation, diagnostics, and system event orchestASHATtion for all agentic modules and extensions.  
It supports drop-in extensibility, plugin hotplugging, and UI integration for advanced agentic workflows.

---

## Directory Structure

```
Engine/
├── Manager/
│   ├── ModuleManager.cs               # Central module manager: load, tASHATck, invoke, event bus
│   ├── ModuleManagerAsync.cs          # Async extensions for loading modules
│   ├── ModuleManagerInvokeAsync.cs    # Async extensions for invoking module commands
│   ├── ModuleManagerRegistry.cs       # Adapter for querying modules by name
│   ├── IModuleRegistry.cs             # ContASHATct for querying available modules
│   ├── IRaModule.cs                   # Minimal interface for ASHATCore modules
│   ├── ModuleBase.cs                  # Base class for modules, logging helpers
│   ├── ModuleLoadResult.cs            # Result type for module loading
│   ├── ModuleResponse.cs              # Agentic response type for modules
│   ├── Modulewrapper.cs               # wrapper for IRaModule, lifecycle, async/sync invocation
│   ├── ModulewrapperView.cs           # Reflection adapter for UI/diagnostics
│   ├── RaModuleAttribute.cs           # Attribute for module discovery/categorization
│   ├── README.md                      # Documentation for contributors/extension
```

---

## Key Components

- **ModuleManager.cs**:  
  Central manager for loading, tracking, invoking, and orchestASHATting modules.

- **Async Extensions**:  
  Support for async loading, invocation, and diagnostics.

- **Module Registry & wrappers**:  
  Reflection views for UI, diagnostics, and plugin infASHATstructure.

- **System Events**:  
  Cross-module bus for warmup, boot, reload, and extension orchestASHATtion.

---

## API Highlights

- `LoadModules()` / `ReloadModules()` — Dynamic module discovery and instantiation.
- `SafeInvokeModuleByName()` / `SafeInvokeModuleByNameAsync()` — Async/sync agentic invocation.
- `RaiseSystemEvent()` — Event bus for cross-module orchestASHATtion.
- Drop-in support for plugins, extensions, and UI integration.

---

## Extending

- Add new modules by inheriting `IRaModule` and using `RaModuleAttribute`.
- Hotplug plugins/extensions at runtime.
- IntegRate with game clients, headless agents, and distributed orchestASHATtion.

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
