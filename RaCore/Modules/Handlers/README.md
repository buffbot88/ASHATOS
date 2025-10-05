# RaCore Handler Modules

## Overview

These modules provide agentic, async, extensible info and error handling for RaCore, with multi-language support, diagnostics, and drop-in extensibility.

## Directory Structure

```
Handlers/
├── InfoHandlerModule.cs
├── ErrorHandlerModule.cs
├── IHandlerModule.cs
├── HandlerDiagnostics.cs
├── HandlerExtensions.cs
├── README.md
```

## Key Components

- **InfoHandlerModule.cs**: Handles agentic info output; multi-language via AILanguageModule.
- **ErrorHandlerModule.cs**: Handles errors/exceptions; agentic output via AILanguageModule.
- **IHandlerModule.cs**: Standard interface for info/error handlers.
- **HandlerDiagnostics.cs**: Event hooks for monitoring/diagnostics.
- **HandlerExtensions.cs**: Static helpers for agentic prompt formatting.

## API

- `ProcessAsync(input)` - Handle info or error input asynchronously.
- `Process(input)` - Synchronous wrapper.
- Diagnostics: Subscribe to `HandlerDiagnostics` for monitoring in UI/logs.

## Extending

- Add new handlers by implementing `IHandlerModule`.
- Extend diagnostics/events as needed for UI integration.
- Use handler extensions for custom prompt formatting.

## Integration

- Designed for tight integration with RaCore's ModuleManager, ConsciousModule, and AILanguageModule.
- All output is agentic and ready for LLM workflows.

## Contributors

Document new handler modules and diagnostics in this README.
