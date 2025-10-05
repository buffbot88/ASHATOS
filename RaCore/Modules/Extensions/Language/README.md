# RaCore Extensions: Agentic Language Module

## Overview

This module provides agentic, async, multi-language natural language generation for RaCore using llama.cpp or LlamaSharp.  
It supports context-aware prompts, multi-language output, and is fully extensible via clear interfaces.

---

## Directory Structure

```
Extensions/
├── Language/
│   ├── AILanguageModule.cs
│   ├── IAILanguageModule.cs
│   ├── AILanguageConstants.cs
│   ├── AILanguageModelLoader.cs
│   ├── AILanguageReflectionUtils.cs
│   ├── README.md
```

---

## Key Components

- **AILanguageModule.cs**: Main agentic language module; async, context-rich, llama.cpp/LlamaSharp-based.
- **IAILanguageModule.cs**: Interface for async agentic language generation.
- **AILanguageConstants.cs**: Constants for description, commands, languages.
- **AILanguageModelLoader.cs**: Reflection-based loader for llama models.
- **AILanguageReflectionUtils.cs**: Reflection helpers, disposal, context correction.

---

## API Highlights

- `GenerateAsync(intent, context, language, metadata)` - Generate agentic output using LLM.
- `ProcessAsync(input)` - Direct input, command, and agentic prompt handling.
- Supports reload, model switching, context/tokens/exe changes, and help/status reporting.

---

## Extending

- Add new commands and supported languages in `AILanguageConstants.cs`.
- Extend the loader and reflection helpers for new model APIs.

---

## Example Usage

```csharp
var aiLang = new AILanguageModule();
var response = await aiLang.GenerateAsync("chat", "What is the meaning of life?", "en");
Console.WriteLine(response.Text);
```

---

## Contributors

Document new commands, model support, and capabilities here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
