# RaCore TestRunner Module

## Overview

The TestRunner subsystem provides agentic, async integration testing and diagnostics for RaCore's modular mainframe.  
It validates initialization, communication, and basic memory/skill/module flows, reporting results via ThoughtProcessor for UI/NLP integration.

---

## Directory Structure

```
Extensions/
├── TestRunner/
│   ├── TestRunnerModule.cs             # Main agentic integration test runner
│   ├── README.md                       # Documentation for contributors/extension
```

---

## Key Components

- **TestRunnerModule.cs**:  
  Main agentic test runner for modular mainframe integration, boot checks, and diagnostics.

---

## API Highlights

- `ProcessAsync(input)` — Supports `start`, `start fast`, `start json`, `start seed <n>`, `start verify`, `clean`, `status`, and `help`.
- Runs integration suite: checks module presence, memory, wake signals, skill/communication, and order.
- Cleans up test artifacts from memory and supports deterministic seeding.

---

## Extending

- Add new test cases for modules, plugins, and core flows.
- Integrate with CI pipelines or admin dashboards for health checks.

---

## Example Usage

```csharp
var runner = new TestRunnerModule();
var result = await runner.ProcessAsync("start fast");
Console.WriteLine(result.Text);

var resultJson = await runner.ProcessAsync("start json");
Console.WriteLine(resultJson.Text);
```

---

## Contributors

Document new test cases, diagnostics, and integration patterns here.
