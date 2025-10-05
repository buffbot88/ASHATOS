# RaCore Safety Extensions

## Overview

The Safety subsystem provides agentic, async, extensible consent management, ethics/risk analysis, and skill harm/policy definition for RaCore.  
All modules are designed for agentic interaction, multi-module integration, and drop-in extension.

---

## Directory Structure

```
Extensions/
├── Safety/
│   ├── ConsentRegistryModule.cs          # Consent management & registry (async agentic)
│   ├── EthicsGuardModule.cs              # Ethics guard, risk and consent analysis (async agentic)
│   ├── SafetyPolicy.cs                   # Safety policies, harm types, skill defaults
│   ├── RiskScorer.cs                     # Risk scoring engine for skill/plan steps
│   ├── README.md                         # Documentation for contributors/extension
```

---

## Key Components

- **ConsentRegistryModule.cs**:  
  Registry for user/system consent for skills and scopes. Async, agentic, integrates with ThoughtProcessor.

- **EthicsGuardModule.cs**:  
  Analyzes skill plans for harms/risks, blocks, confirms, or approves actions. Delegates agentic output to ThoughtProcessor.

- **SafetyPolicy.cs**:  
  Central policy definition—skill harm types, severity, block/confirm thresholds, and defaults.

- **RiskScorer.cs**:  
  Fast risk calculation for skill step based on arguments, severity, harm type, and simple heuristics.

---

## API Highlights

- `ConsentRegistryModule.ProcessAsync(input)` — grant, revoke, list, or query consents.
- `EthicsGuardModule.ProcessAsync(planJson)` — analyze agentic plan, block/confirm/approve.
- Skill/harm/severity defaults and thresholds for extensibility.

---

## Extending

- Add new skills, harm types, severity levels in `SafetyPolicy.cs`.
- Extend consent/ethics analysis with custom heuristics or plugins.

---

## Integration

- Designed for tight integration with RaCore's Conscious, Planner, Skills.
- All outputs are agentic, routed through ThoughtProcessor for LLM/NLP interaction.

---

## Example Usage

```csharp
var consent = new ConsentRegistryModule();
await consent.ProcessAsync("consent grant File.Delete");
bool hasConsent = consent.HasConsent("File.Delete");

var ethics = new EthicsGuardModule();
var planJson = "{\"Steps\":[{\"Skill\":\"File.Delete\",\"ArgumentsJson\":\"{\\\"action\\\":\\\"delete\\\"}\"}]}";
var result = await ethics.ProcessAsync(planJson);
Console.WriteLine(result.Text);
```

---

## Contributors

Document new skills, policy extensions, and diagnostics here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
