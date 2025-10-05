# RaCore Execution Module

## Overview

The Execution subsystem provides agentic, async, extensible orchestration of Planner plans for RaCore.  
It routes plans step-by-step to SkillsRegistry and handles agentic output via ThoughtProcessor.

---

## Directory Structure

```
Extensions/
├── Execution/
│   ├── ExecutionModule.cs            # Main agentic executor for Planner plans
│   ├── README.md                     # Documentation for contributors/extension
```

---

## Key Components

- **ExecutionModule.cs**:  
  Agentic async executor for Planner plans. Routes each step to SkillsRegistry, integrates with ThoughtProcessor.

---

## API Highlights

- `ProcessAsync(input)` — Accepts Planner plan JSON, executes steps via SkillsRegistry.
- `status`, `help` — Friendly commands for admin/developer use.
- Fully agentic output (structured, natural language).

---

## Extending

- Integrate with SafetyModule for permission/risk control.
- Add advanced orchestration, distributed execution, or multi-agent routing.
- Extend for game client/server plugin orchestration.

---

## Example Usage

```csharp
var executor = new ExecutionModule();
var planJson = "{ \"Goal\": \"Open Calculator\", \"Steps\": [ { \"Skill\": \"System.Open\", \"ArgumentsJson\": \"{ \\\"target\\\": \\\"Calculator.exe\\\" }\" } ] }";
var result = await executor.ProcessAsync(planJson);
Console.WriteLine(result.Text);
```

---

## Contributors

Document new orchestration patterns, extension points, and diagnostics here.
