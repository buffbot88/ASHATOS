# RaCore MemoryModule

## Overview

The RaCore **MemoryModule** provides an agentic, async-ready, extensible subsystem for persistent memory storage, recall, and reasoning.  
It is designed for sentient AI workflows, supporting natural language, metadata, multi-language, diagnostics, and drop-in extension.

---

## Directory Structure

```
Memory/
├── MemoryModule.cs           # Main agentic, async, persistent memory implementation
├── BinMemory.cs             # Channelized/bin memory for logs, IO, errors, etc.
├── MemoryModels.cs          # Data classes: MemoryItem, BinEntry, Candidate, ConsciousEntry, BinChannel
├── IMemoryModule.cs         # Interfaces: IMemory, IBinMemory
├── MemoryDiagnostics.cs     # Diagnostics/event hooks for UI/logging
├── MemoryExtensions.cs      # Static helpers/extensions for memory operations
├── README.md                # This documentation
```

---

## Key Components

- **MemoryModule.cs**:  
  The main module for agentic, async, natural language-based memory operations. Stores and recalls key-value pairs with metadata.

- **BinMemory.cs**:  
  Channelized/bin memory implementation for logging, IO, error records, and other non-standard memory entries.

- **MemoryModels.cs**:  
  Contains core data types to represent memory items, bin entries, candidates (pending memory), and conscious (prioritized) memories.

- **IMemoryModule.cs**:  
  Defines the main interfaces for memory modules (`IMemory`, `IBinMemory`).

- **MemoryDiagnostics.cs**:  
  Centralized event hooks for diagnostics, logging, and UI integration.

- **MemoryExtensions.cs**:  
  Static extension methods and helpers for querying and manipulating memory items and bin entries.

---

## API Highlights

### Memory Operations

- `RememberAsync(key, value, metadata)` - Store a memory item.
- `RecallAsync(key)` - Retrieve the latest value for a key.
- `GetAllItemsAsync()` - List all memory items.
- `Remove(id or key)` - Remove a memory item.
- `Clear()` - Wipe all memory.
- All methods return a `ModuleResponse` for agentic, natural language output.

### Bin Memory Operations

- `AddEntryAsync(path, key, value, metadata, channel)` - Add a bin entry (log, IO, error record).
- `GetEntries(path, channel)` - Query bin entries by path/channel.
- `RemoveEntry(id)` - Remove a bin entry.

### Diagnostics

- Subscribe to events in `MemoryDiagnostics` for UI/logging:  
  - `OnMemoryEvent`
  - `OnMemoryError`
  - `OnMemoryStored`
  - `OnMemoryRemoved`
  - `OnBinEntryAdded`

### Extensions

- Use `MemoryExtensions` for advanced queries:
  - Find latest item by key
  - Search by metadata
  - Filter bin entries by channel/path

---

## Extending the Memory Subsystem

- Add new memory types by implementing `IMemory` or `IBinMemory`.
- Use manifest and capabilities for easy hotplug and runtime discovery.
- Extend `MemoryModels.cs` for new agentic features (episodic, semantic memory, etc.).
- Add diagnostics and extension hooks as needed.

---

## Integration

- Designed for tight integration with RaCore's ModuleManager, ThoughtProcessor, Subconscious, and AILanguage modules.
- All responses are agentic and ready for conversational AI workflows.

---

## Future-Proofing

- Ready for multi-language, metadata-rich, and sentient AI mainframe scenarios.
- Easy to subclass and extend for new types of memory and reasoning.
- Supports drop-in modules and runtime extension with minimal rewiring.

---

## Example Usage

```csharp
var memory = new MemoryModule();
await memory.RememberAsync("user_name", "Aika");
var recall = await memory.RecallAsync("user_name");
Console.WriteLine(recall.Text); // "Memory recall: \"user_name\" = \"Aika\"."
```

---

## Contributors

Please document all new memory types, extension points, and diagnostics in this README.
