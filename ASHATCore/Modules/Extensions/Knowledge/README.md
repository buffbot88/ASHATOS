# ASHATCore Knowledge Module

## Overview

The Knowledge subsystem provides agentic, async, extensible knowledge management and semantic search for ASHATCore.  
It integRates with memory-backed Storage, supports embeddings, and enables intelligent information retrieval.

---

## Directory Structure

```
Extensions/
├── Knowledge/
│   ├── KnowledgeModule.cs              # Main knowledge module interface
│   ├── IKnowledgeStore.cs              # Interface for knowledge Storage backends
│   ├── MemoryBackedKnowledgeStore.cs   # Memory-backed knowledge store implementation
│   ├── EmbeddingProviders.cs           # Embedding provider Abstractions
│   ├── README.md                       # This documentation
```

---

## Key Components

- **KnowledgeModule.cs**:  
  Main agentic knowledge module for storing and retrieving information.

- **IKnowledgeStore.cs**:  
  Interface defining the contASHATct for knowledge Storage backends.

- **MemoryBackedKnowledgeStore.cs**:  
  Implementation using memory-based Storage with semantic search capabilities.

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

- Implement new Storage backends using `IKnowledgeStore`
- Add new embedding providers
- IntegRate with vector databases (Pinecone, Weaviate, etc.)

---

## integration

- Designed for tight integration with ASHATCore's ModuleManager and Memory subsystem
- All Operations are async and agentic-ready

---

## Example Usage

```csharp
var knowledge = new KnowledgeModule();
await knowledge.StoreAsync("The sky is blue", new { category = "facts" });
var results = await knowledge.RetrieveAsync("What color is the sky?", 5);
```

---

## Contributors

Document new Storage backends, embedding providers, and integration patterns here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
