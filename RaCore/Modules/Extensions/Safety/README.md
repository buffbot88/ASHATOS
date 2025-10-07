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
│   ├── ContentModerationModule.cs        # Real-time content moderation & harm detection (Phase 4.6)
│   ├── ParentalControlModule.cs          # Parental controls & content filtering (NEW in Phase 4.8)
│   ├── ComplianceModule.cs               # COPPA, GDPR, CCPA compliance (NEW in Phase 4.8)
│   ├── SafetyPolicy.cs                   # Safety policies, harm types, skill defaults
│   ├── RiskScorer.cs                     # Risk scoring engine for skill/plan steps
│   ├── README.md                         # Documentation for contributors/extension
```

---

## Key Components

- **ParentalControlModule.cs** (NEW in Phase 4.8):  
  Complete parental control system with content rating (E, E10+, T, M, AO), profanity/violence/sexual content filtering, time-based usage restrictions, and parental approval workflows. ESRB/PEGI-aligned content descriptors.

- **ComplianceModule.cs** (NEW in Phase 4.8):  
  Regulatory compliance framework supporting COPPA, GDPR, and CCPA. Age verification (5 methods), parental consent management (6 verification methods), compliance incident tracking, and automated reporting.

- **ContentModerationModule.cs** (Phase 4.6):  
  Real-time content scanning and harm detection across all interactive modules (forums, blogs, games, etc.). Automatically detects and blocks harmful content, suspends violating users, and maintains audit trails.

- **ConsentRegistryModule.cs**:  
  Registry for user/system consent for skills and scopes. Async, agentic, integrates with ThoughtProcessor.

- **EthicsGuardModule.cs**:  
  Analyzes skill plans for harms/risks, blocks, confirms, or approves actions. Delegates agentic output to ThoughtProcessor.

- **SafetyPolicy.cs**:  
  Central policy definition—skill harm types, severity, block/confirm thresholds, and defaults.

- **RiskScorer.cs**:  
  Fast risk calculation for skill step based on arguments, severity, harm type, and simple heuristics.

---

## Content Moderation (Phase 4.6)

### Features

- **Real-time Content Scanning**: Analyzes text content for harmful patterns before accepting
- **Multi-violation Detection**: Detects hate speech, violence, harassment, spam, phishing, and more
- **Automated Actions**: Auto-blocks or auto-suspends based on severity thresholds
- **Manual Review Queue**: Flags questionable content for human moderation
- **User Suspension System**: Temporary or permanent suspensions with appeal tracking
- **Audit Trails**: Complete logging of all moderation actions
- **Integration Hooks**: Easy integration with forums, blogs, chat, and other modules

### Violation Types

- Harassment, Hate Speech, Violence, Self-Harm
- Sexual Content, Spam, Phishing, Malware
- Personal Information, Copyright, Illegal Content
- Misinformation, Excessive Profanity

### Console Commands

```bash
moderation stats              # View moderation statistics
moderation scan <text>        # Scan text manually
moderation history <userId>   # View user's history
moderation pending            # List pending reviews
moderation suspend <userId> <days> <reason>  # Suspend user
moderation unsuspend <userId> # Unsuspend user
```

---

## API Highlights

- `ContentModerationModule.ScanTextAsync(text, userId, module, contentId)` — scan content for violations.
- `ContentModerationModule.IsUserSuspendedAsync(userId)` — check suspension status.
- `ContentModerationModule.GetPendingReviewsAsync()` — retrieve flagged content for review.
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
// Consent
var consent = new ConsentRegistryModule();
await consent.ProcessAsync("consent grant File.Delete");
bool hasConsent = consent.HasConsent("File.Delete");

// Ethics
var ethics = new EthicsGuardModule();
var planJson = "{\"Steps\":[{\"Skill\":\"File.Delete\",\"ArgumentsJson\":\"{\\\"action\\\":\\\"delete\\\"}\"}]}";
var result = await ethics.ProcessAsync(planJson);
Console.WriteLine(result.Text);

// Content Moderation (NEW in v4.6)
var moderation = new ContentModerationModule();
var scanResult = await moderation.ScanTextAsync(
    "User posted content here",
    "user123",
    "Forum",
    "post456"
);

if (scanResult.Action == ModerationAction.Blocked)
{
    Console.WriteLine($"Content blocked: {string.Join(", ", scanResult.Violations.Select(v => v.Type))}");
}

// Forum integration example
var forumModule = new ForumModule();
var (success, message, postId) = await forumModule.CreatePostAsync(
    "user123",
    "JohnDoe",
    "My post title",
    "Post content that will be moderated automatically"
);
```

---

## Contributors

Document new skills, policy extensions, and diagnostics here.

---

**Last Updated:** 2025-01-13  
**Version:** 9.3.9 (See RaCore/Version.cs for unified version)  
**Phase:** 4.8 - All-Age Compliance & Parental Controls
