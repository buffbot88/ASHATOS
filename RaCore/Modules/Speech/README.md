# RaCore SpeechModule

## Overview

The SpeechModule is the exclusive gateway for all external communication with RaCore.  
It parses natural language, commands, confirmations, and routes agentic requests to the core cognition (ConsciousModule).  
All responses are generated via LLM (AILanguage) or routed agentically.

---

## Directory Structure

```
Speech/
├── SpeechModule.cs               # Main speech module, agentic router & NLP gateway
├── ISpeechModule.cs              # Interface for agentic speech modules
├── SpeechModule.AgentCompat.cs   # Agentic command/confirmation helpers
├── SpeechModule.Events.cs        # Event hooks (OnMemoryReady, wake, etc.)
├── SpeechModule.Helpers.cs       # Static helpers, regex, normalization
├── SpeechModule.MemoryCompat.cs  # Memory interop helpers
├── README.md                     # Module documentation
```

---

## Key Components

- **SpeechModule.cs**:  
  The main agentic router for all external I/O. Parses, confirms, delegates to core cognition.

- **ISpeechModule.cs**:  
  Interface for agentic speech/NLP modules.

- **SpeechModule.AgentCompat.cs**:  
  Agentic command/confirmation helpers for delegated cognition.

- **SpeechModule.Events.cs**:  
  Event hooks for system readiness, memory binding, warmup.

- **SpeechModule.Helpers.cs**:  
  Static helpers for command parsing, normalization, module invocation.

- **SpeechModule.MemoryCompat.cs**:  
  Helpers for memory interop (remember, recall, etc.).

---

## API Highlights

- `GenerateResponseAsync(input)` - Entry point for all external communications.
- `Process(input)` - Synchronous wrapper for compatibility.
- Supports agentic commands: `think`, `remember`, `recall`, `probe`, `status`, `help`, confirmations, and skills/plugins.
- All outputs are routed through LLM (AILanguage) for natural language generation.

---

## Extending the Speech Subsystem

- Add new agentic commands and regexes as needed.
- Plug in new skills, plugins, or extensions via module routing.
- Integrate with diagnostics and UI via event hooks.

---

## Integration

- Designed to call into ConsciousModule for cognition, MemoryModule for storage, and SubconsciousModule for reasoning.
- All external I/O (API/user/system) must be routed here.

---

## Example Usage

```csharp
var speech = new SpeechModule();
var response = await speech.GenerateResponseAsync("think What is my current status?");
Console.WriteLine(response);
```

---

## Contributors

Please document all new agentic commands, extension points, and diagnostics in this README.
