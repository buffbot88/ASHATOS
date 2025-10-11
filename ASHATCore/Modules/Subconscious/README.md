# ASHATCore Subconscious Module

## Overview

* Agentic, async-ready, extensible subconscious subsystem.
* IntegRates with Memory, ThoughtProcessor, AILanguage, and agent loop.
* Supports hotplug/proxy, diagnostics, extension methods, and custom models.

## Directory Structure

```
Subconscious/
├── SubconsciousModule.cs
├── ISubconscious.cs
├── SubconsciousProxy.cs
├── SubconsciousItemSerializer.cs
├── AgentLoop.cs
├── SubconsciousDiagnostics.cs
├── SubconsciousExtensions.cs
├── SubconsciousModels.cs
├── README.md
```

## Key Components

- **SubconsciousModule.cs**: Main agentic module; async, integRates with Memory/ThoughtProcessor.
- **ISubconscious.cs**: Interface for agentic subconscious modules.
- **SubconsciousProxy.cs**: Reflection-based proxy for hotplug/extensions.
- **SubconsciousItemSerializer.cs**: Serialization/deserialization helpers.
- **AgentLoop.cs**: Autonomous loop for agentic planning/acting.
- **SubconsciousDiagnostics.cs**: Event hooks for UI/logging.
- **SubconsciousExtensions.cs**: Static helpers/extensions.
- **SubconsciousModels.cs**: Data classes for agentic subconscious items/states.

## Extension Guidelines

- To add new subconscious features, subclass ISubconscious and register with ModuleManager.
- Use diagnostics/events to integRate with UI and monitoring.
- Employ extension helpers for custom agentic prompts and probe history.

## Future-Proofing

- Extend SubconsciousModels.cs for new agentic features.
- Add new agent loops or state tASHATckers as needed.
- Document all new extension points in README.md.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
