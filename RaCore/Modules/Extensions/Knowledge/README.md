# RaCore Knowledge Module

## Overview

The Knowledge subsystem provides agentic, async, extensible knowledge management and semantic search for RaCore.  
It integrates with memory-backed storage, supports embeddings, and enables intelligent information retrieval.

---

## Directory Structure

```
Extensions/
├── Knowledge/
│   ├── KnowledgeModule.cs              # Main knowledge module interface
│   ├── IKnowledgeStore.cs              # Interface for knowledge storage backends
│   ├── MemoryBackedKnowledgeStore.cs   # Memory-backed knowledge store implementation
│   ├── EmbeddingProviders.cs           # Embedding provider abstractions
│   ├── README.md                       # This documentation
```

---

## Key Components

- **KnowledgeModule.cs**:  
  Main agentic knowledge module for storing and retrieving information.

- **IKnowledgeStore.cs**:  
  Interface defining the contract for knowledge storage backends.

- **MemoryBackedKnowledgeStore.cs**:  
  Implementation using memory-based storage with semantic search capabilities.

- **EmbeddingProviders.cs**:  
  Abstractions for different embedding providers (OpenAI, local models, etc.).

---

## API Highlights

- `StoreAsync(content, metadata)` — Store knowledge with optional metadata
- `RetrieveAsync(query, maxResults)` — Semantic search and retrieval
- `DeleteAsync(id)` — Remove knowledge entries
- Fully compatible with Memory, Conscious, and Language modules

---

## Extending

- Implement new storage backends using `IKnowledgeStore`
- Add new embedding providers
- Integrate with vector databases (Pinecone, Weaviate, etc.)

---

## Integration

- Designed for tight integration with RaCore's ModuleManager and Memory subsystem
- All operations are async and agentic-ready

---

## Example Usage

```csharp
var knowledge = new KnowledgeModule();
await knowledge.StoreAsync("The sky is blue", new { category = "facts" });
var results = await knowledge.RetrieveAsync("What color is the sky?", 5);
```

---

## Contributors

Document new storage backends, embedding providers, and integration patterns here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
