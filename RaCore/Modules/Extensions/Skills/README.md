# RaCore Skills Module

## Overview

The Skills subsystem provides agentic, async, extensible skill registration, management, and safe invocation for RaCore.  
Skills are modular, registry-driven, and can be invoked agentically and safely via the registry.

---

## Directory Structure

```
Extensions/
├── Skills/
│   ├── SkillsModule.cs                # Skill interface and result type
│   ├── SkillsRegistryModule.cs        # Registry and router for all registered skills
│   ├── SystemOpenSkillModule.cs       # Example skill: open file, URL, or app
│   ├── README.md                      # Documentation for contributors/extension
```

---

## Key Components

- **SkillsModule.cs**:  
  Interface for agentic, async skills and result type.

- **SkillsRegistryModule.cs**:  
  Registry and router for all registered skills, agentic management, safe invocation, and description.

- **SystemOpenSkillModule.cs**:  
  Example agentic skill: open file, URL, or app on the host system.

---

## API Highlights

- `ISkill.InvokeAsync(argumentsJson, CancellationToken)` — Async agentic skill invocation.
- `SkillsRegistryModule.ProcessAsync(input)` — List, describe, and route skills.
- All outputs are agentic and routed via ThoughtProcessor.

---

## Extending

- Implement new skills by inheriting `ISkill`.
- Add new skills and register via registry or ModuleManager.

---

## Example Usage

```csharp
var registry = new SkillsRegistryModule();
await registry.ProcessAsync("skills list");

var openSkill = new SystemOpenSkillModule();
await openSkill.InvokeAsync("{\"target\":\"Calculator.exe\"}");
```

---

## Contributors

Document new skill types, registry extensions, and diagnostics here.
