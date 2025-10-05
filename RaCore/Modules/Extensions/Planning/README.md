# RaCore Planner Module

## Overview

The Planner subsystem provides agentic, async, extensible intent-to-plan translation for RaCore.  
It can process natural language, JSON intent, or NLU-driven requests and output structured agentic plans.

---

## Directory Structure

```
Extensions/
├── Planning/
│   ├── PlannerModule.cs              # Main agentic planner module
│   ├── PlannerModule.Types.cs        # Plan and Step data types
│   ├── README.md                     # Documentation for contributors/extension
```

---

## Key Components

- **PlannerModule.cs**:  
  Agentic planner, async-ready, processes natural language and JSON intent, outputs plans and steps.

- **PlannerModule.Types.cs**:  
  Plan and Step data types for structured agentic plan representation.

---

## API Highlights

- `ProcessAsync(input)` — Accepts natural language or JSON intent, outputs agentic plan via ThoughtProcessor.
- `status`, `help`, and friendly commands for developer use.
- Fully compatible with NLU, Conscious, and Safety modules.

---

## Extending

- Add new plan/step types, intent mappings, or skill integrations as needed.
- Easily integrate with new agentic modules and extensions.

---

## Example Usage

```csharp
var planner = new PlannerModule();
var result = await planner.ProcessAsync("{\"intent\":\"system.open\",\"target\":\"Calculator\"}");
Console.WriteLine(result.Text);
```

---

## Contributors

Document new plan/step types, extension points, and diagnostics here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
