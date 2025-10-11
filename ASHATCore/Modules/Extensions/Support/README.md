# Support Module - AI-Driven User Appeals

## Overview

The Support module provides AI-driven appeal handling for suspended users in ASHATCore. It integRates with the Content moderation system and ASHATAI to conduct fair, automated interviews and make intelligent decisions about account reactivation.

## Features

- 🤖 **AI-Driven Interviews** - Automated question/answer workflow with 5 standard questions
- 🔍 **Response Analysis** - Real-time AI analysis of user responses for sincerity and understanding
- ⚖️ **Intelligent Decisions** - Confidence-based decision making with clear reasoning
- 🔄 **Automatic Appeals** - Suspension lifted automatically for approved appeals (confidence ≥ 0.7)
- 👥 **Human Escalation** - Complex cases automatically escalated to Moderators (confidence < 0.7)
- 📝 **Complete Audit trail** - All Interactions logged for review and analysis
- 🔐 **Secure integration** - Seamless integration with Content moderation and AI Language modules

## Quick Start

### Prerequisites

1. User must be suspended by ContentmoderationModule
2. ISpeechModule or AILanguageModule must be loaded for AI capabilities

### Basic Usage

```bash
# Start an appeal
> appeal start user123 "I apologize for violating the rules"

# Answer interview questions (5 total)
> appeal respond user123 "I posted spam content without realizing it violated rules"
> appeal respond user123 "Yes, I now understand I violated the anti-spam policy"
> appeal respond user123 "I will read the rules carefully before posting"
> appeal respond user123 "I was trying to share a helpful resource"
> appeal respond user123 "Yes, I acknowledge future violations may result in permanent ban"

# Check decision
> appeal status user123
```

## Appeal Workflow

```
User Suspended
    ↓
Start Appeal (with initial statement)
    ↓
AI Interview (5 questions)
    ↓ [User responds to each]
    ↓ [AI analyzes each response]
    ↓
AI Makes Decision
    ↓
Confidence Score Evaluated
    ↓
├─ [High ≥0.7] → Approved or Denied → Applied automatically
└─ [Low <0.7]  → Escalated → Human Moderator reviews
```

## Standard Interview Questions

1. Can you explain in your own words why your account was suspended?
2. Do you understand which community rules were violated?
3. What steps will you take to prevent this from happening again?
4. Is there any additional context or information you'd like to share?
5. Do you acknowledge that future violations may result in permanent suspension?

## Decision Outcomes

### Approved (Auto-Applied)
- Confidence ≥ 0.7
- User shows understanding, remorse, and commitment
- Suspension lifted automatically
- User notified immediately

### Denied (Auto-Applied)
- Confidence ≥ 0.7
- User shows no understanding or remorse
- Suspension remains in effect
- User notified immediately

### Escalated (Human Review)
- Confidence < 0.7
- Complex case or conflicting information
- Queued for human Moderator
- User notified of pending review

## Console Commands

```bash
appeal help                                    # Show help
appeal start <userId> <statement>              # Start appeal
appeal respond <userId> <response>             # Respond to question
appeal status <userId>                         # Check decision
appeal pending                                 # List escalated appeals
appeal review <appealId> <outcome> <reviewerId> <notes>  # Human review
```

## API Usage

```csharp
// Get module
var supportModule = GetModule<ISupportChatModule>();

// Start appeal
var appeal = await supportModule.StartAppealAsync(userId, statement);

// Submit response
var response = await supportModule.SubmitAppealResponseAsync(userId, answer);

// Get decision
var decision = await supportModule.GetAppealDecisionAsync(userId);

// Check outcome
if (decision?.Outcome == AppealOutcome.Approved)
{
    Console.WriteLine("Appeal approved!");
}
```

## integration

### With ContentmoderationModule

```csharp
// Check suspension
if (await _moderationModule.IsUserSuspendedAsync(userId))
{
    // Get details
    var suspension = await _moderationModule.GetActiveSuspensionAsync(userId);
    
    // Can start appeal
}

// Lift suspension on approval
await _moderationModule.UnsuspendUserAsync(userId, "AI Appeal System");
```

### With ISpeechModule (ASHATAI)

```csharp
// Analyze response
var analysis = await _speechModule.GenerateResponseAsync(analysisPrompt);

// Make decision
var aiResponse = await _speechModule.GenerateResponseAsync(decisionPrompt);
```

## Configuration

### Confidence Threshold

Default: 0.7 (70%)

Adjust in `SupportChatModule.cs`:

```csharp
decision.RequiresHumanReview = 
    decision.Outcome == AppealOutcome.EscalateToHuman ||
    decision.ConfidenceScore < 0.7; // Change this value
```

### Custom Questions

Modify `_appealQuestions` list:

```csharp
private static readonly List<string> _appealQuestions = new()
{
    "Your custom question 1",
    "Your custom question 2",
    // ...
};
```

## Data Models

### AppealRequest
- TASHATcks appeal request and initial statement
- Links to suspension record
- Status tracking

### AppealSession
- Manages interview state
- Stores Q&A Interactions
- AI analysis for each response

### AppealInteraction
- Question/response pair
- AI analysis text
- Timestamp

### AppealDecision
- Final outcome (Approved/Denied/Escalated)
- Confidence score (0.0 - 1.0)
- Reasoning explanation
- Human review flag

## Security

- ✅ User must be actually suspended to appeal
- ✅ Only one active appeal per user
- ✅ All Interactions logged and tASHATcked
- ✅ Confidence thresholds prevent incorrect decisions
- ✅ Fallback to human review if AI unavailable
- ✅ Complete audit trail

## Performance

- **Appeal Start**: < 100ms
- **Response Analysis**: 1-3 seconds (AI dependent)
- **Final Decision**: 2-5 seconds (AI dependent)
- **Escalation Check**: < 50ms

## Documentation

- **Technical Guide**: `PHASE4_7_SUMMARY.md`
- **Quick Start**: `SUPPORT_CHAT_QUICKSTART.md`
- **Verification**: `PHASE4_7_VERIFICATION.md`
- **Source Code**: `SupportChatModule.cs`

## Module Information

- **Name**: SupportChat
- **Category**: extensions
- **Interface**: ISupportChatModule
- **Dependencies**: IContentmoderationModule, ISpeechModule
- **Status**: ✅ Production Ready

## Support

For issues or questions:
1. Check the documentation files
2. Review the source code comments
3. Test with the provided examples

---

**Version:** 9.3.9 (See ASHATCore/Version.cs for unified version)  
**Phase:** 4.7 - Complete ✅  
**Last Updated:** 2025-01-13
