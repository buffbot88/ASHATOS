# RaCore Conscious Module

## Overview

The ConsciousModule is the agentic, async-ready cognitive subsystem for RaCore.  
It handles thought processing, memory and subconscious integration, event-driven cognition, and LLM-based responses.

---

## Directory Structure

```
Conscious/
├── ConsciousModule.cs           # Main agentic, async, cognitive module
├── ConsciousModule.Types.cs     # Data types: Thought, Mood, config, processors
├── ConsciousModule.Events.cs    # Event hooks: OnMemoryReady, OnSystemEvent, etc.
├── ConsciousModule.MemoryCompat.cs # Memory interop helpers
├── ThoughtProcessor.cs          # Agentic thought processing, LLM interfacing
├── LlamaCppProcessor.cs         # Llama.cpp backend integration (optional)
├── README.md                    # This documentation
```

---

## Key Components

- **ConsciousModule.cs**:  
  The main agentic cognitive module. Routes commands (think, remember, recall, status, help) and integrates with memory and subconscious modules.

- **ConsciousModule.Types.cs**:  
  Agents' thought models, mood types, config, and processors for cognition.

- **ConsciousModule.Events.cs**:  
  Handles event-driven hooks like memory ready, wake, and custom agent events.

- **ConsciousModule.MemoryCompat.cs**:  
  Helpers for memory interoperability, cross-module compatibility.

- **ThoughtProcessor.cs**:  
  Central thought processing and agentic output pipeline using LLM.

- **LlamaCppProcessor.cs**:  
  Optional backend for local LLM generation (Llama.cpp).

---

## API Highlights

- `ProcessAsync(input)` - Main entry for agentic cognition, routing commands and thoughts.
- Event hooks for system, memory, and agent state changes.
- Direct integration with memory, subconscious, and error/info handler modules.

---

## Extending the Cognitive Subsystem

- Add new cognitive states, plugins, or event handlers.
- Extend types and processors for richer agentic behavior.
- Use ThoughtProcessor for all LLM agentic output.

---

## Integration

- Designed for tight integration with RaCore's ModuleManager, Memory, Subconscious, and Handlers.
- All responses are agentic and ready for conversational AI workflows.

---

## Future-Proofing

- Ready for multi-language, metadata-rich, and sentient AI mainframe scenarios.
- Easy to subclass, extend, and plugin for new types of cognition and reasoning.

---

## Example Usage

```csharp
var conscious = new ConsciousModule();
var response = await conscious.ProcessAsync("think What is my current status?");
Console.WriteLine(response.Text);
```

---

## Contributors

Please document all new cognitive features, extension points, and diagnostics in this README.
