# Support Chat & AI-Driven Appeals - Quick Start Guide

## Overview

The Support Chat Module provides AI-driven appeal handling for suspended users. RaAI conducts automated interviews and makes fair judgments about account reactivation.

---

## Getting Started

### 1. Module Auto-Loads

The SupportChatModule is automatically loaded when RaCore starts. No configuration required!

### 2. Prerequisites

For the appeal system to work, you need:
- ✅ ContentModerationModule (handles suspensions)
- ✅ SpeechModule/AILanguageModule (provides AI capabilities)
- ✅ A suspended user to test with

---

## Basic Usage

### Test the Appeal System

```bash
# Start RaCore
cd RaCore
dotnet run

# Check if module loaded
> help
# Should show SupportChat in module list

# Get appeal help
> appeal help
```

---

## Complete Appeal Walkthrough

### Step 1: Suspend a Test User

First, create a test user suspension:

```bash
# Suspend user for testing
> moderation suspend testuser 7 Test suspension for appeal demo
```

### Step 2: Start an Appeal

```bash
> appeal start testuser "I made a mistake and want to make things right"
```

**Returns**:
```json
{
  "Id": "...",
  "UserId": "testuser",
  "SuspensionId": "...",
  "RequestedAt": "2024-01-15T10:30:00Z",
  "InitialStatement": "I made a mistake and want to make things right",
  "Status": "InProgress"
}
```

**AI asks first question**: 
"Can you explain in your own words why your account was suspended?"

### Step 3: Answer Interview Questions

```bash
# Answer question 1
> appeal respond testuser "I posted spam content without realizing it violated the rules"

# AI analyzes and asks question 2
# "Do you understand which community rules were violated?"

# Answer question 2
> appeal respond testuser "Yes, I now understand I violated the anti-spam policy"

# Continue for questions 3, 4, and 5...
> appeal respond testuser "I will read the rules carefully and ask before posting promotional content"
> appeal respond testuser "I was trying to help share a resource but should have checked the guidelines first"
> appeal respond testuser "Yes, I acknowledge that future violations may result in permanent suspension"
```

### Step 4: Receive Decision

After the final response, AI makes a decision:

```
=== Appeal Decision ===
Outcome: Approved
Reasoning: User demonstrates clear understanding of violation, shows genuine remorse, and commits to following rules
Confidence: 0.85

Your appeal has been approved. Your suspension has been lifted.
Please remember to follow community guidelines going forward.
```

### Step 5: Verify Suspension Lifted

```bash
# Check if user is still suspended
> moderation history testuser
# Should show suspension is no longer active
```

---

## Common Scenarios

### Scenario 1: Appeal Approved

**User shows**: Understanding, remorse, commitment to change

**Result**: 
- Outcome: Approved
- Confidence: High (≥ 0.7)
- Action: Suspension lifted automatically

### Scenario 2: Appeal Denied

**User shows**: No understanding, no remorse, deflects blame

**Result**:
- Outcome: Denied
- Confidence: High (≥ 0.7)
- Action: Suspension remains

### Scenario 3: Appeal Escalated

**User shows**: Conflicting info, complex situation, ambiguous responses

**Result**:
- Outcome: EscalateToHuman
- Confidence: Low (< 0.7)
- Action: Sent to human moderator review

---

## Human Moderator Review

### View Pending Appeals

```bash
> appeal pending
```

**Returns**:
```json
[
  {
    "Id": "...",
    "AppealRequestId": "...",
    "Outcome": "EscalateToHuman",
    "Reasoning": "Complex case requires human judgment",
    "ConfidenceScore": 0.65,
    "RequiresHumanReview": true
  }
]
```

### Review Escalated Appeal

```bash
> appeal review <appealId> Approved moderator1 User provided valid context
```

**Outcomes**:
- `Approved` - Lift suspension
- `PartiallyApproved` - May reduce suspension duration
- `Denied` - Keep suspension in effect

---

## Console Commands Reference

```bash
# Show help
appeal help

# Start appeal
appeal start <userId> <initial statement>

# Respond to question
appeal respond <userId> <response>

# Check appeal status
appeal status <userId>

# View escalated appeals (moderators)
appeal pending

# Review appeal (moderators)
appeal review <appealId> <outcome> <reviewerId> <notes>
```

---

## API Usage

### In Your Code

```csharp
// Get module
var supportModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ISupportChatModule>()
    .FirstOrDefault();

if (supportModule != null)
{
    // Start appeal
    var appeal = await supportModule.StartAppealAsync(userId, statement);
    
    // Submit response
    var response = await supportModule.SubmitAppealResponseAsync(userId, answer);
    
    // Get decision
    var decision = await supportModule.GetAppealDecisionAsync(userId);
    
    if (decision?.Outcome == AppealOutcome.Approved)
    {
        Console.WriteLine("Appeal approved!");
    }
}
```

---

## Integration Example

### Add Appeal Button to User Dashboard

```csharp
public async Task<string> CheckUserStatus(string userId)
{
    var moderationModule = GetModule<IContentModerationModule>();
    var supportModule = GetModule<ISupportChatModule>();
    
    if (await moderationModule.IsUserSuspendedAsync(userId))
    {
        var suspension = await moderationModule.GetActiveSuspensionAsync(userId);
        var activeSession = await supportModule.GetActiveSessionAsync(userId);
        
        if (activeSession == null)
        {
            return "You are suspended. Click here to start an appeal.";
        }
        else
        {
            return "Your appeal is in progress. Please answer the questions.";
        }
    }
    
    return "Account active";
}
```

---

## Testing Tips

### Test Different Appeal Outcomes

**For Approved**:
- Show understanding of violation
- Express genuine remorse
- Commit to behavior change
- Provide context without excuses
- Acknowledge consequences

**For Denied**:
- Claim ignorance or innocence
- Blame others or system
- Show no remorse
- Refuse to change behavior
- Argue with questions

**For Escalated**:
- Provide complex explanations
- Give conflicting information
- Present edge cases
- Discuss technical issues
- Request special consideration

---

## Troubleshooting

### Issue: "User is not currently suspended"

**Solution**: The user must be suspended before starting an appeal. Suspend them first:
```bash
> moderation suspend <userId> <days> <reason>
```

### Issue: "User already has an active appeal"

**Solution**: Complete or abandon the existing appeal before starting a new one.

### Issue: "No active appeal session found"

**Solution**: Start an appeal first with `appeal start`.

### Issue: AI analysis not working

**Check**:
- Is SpeechModule loaded?
- Is AILanguageModule configured?
- Check console for AI module errors

**Fallback**: System will escalate to human review if AI unavailable.

---

## Best Practices

### For Users
1. **Be honest** - AI can detect insincere responses
2. **Be specific** - Explain exactly what happened
3. **Take responsibility** - Don't deflect blame
4. **Show understanding** - Explain what you learned
5. **Commit to change** - Outline specific steps

### For Moderators
1. **Review escalated cases promptly**
2. **Use appeal pending regularly**
3. **Provide clear review notes**
4. **Be consistent with decisions**
5. **Log complex cases for future reference**

### For Developers
1. **Integrate early** - Add appeal flows to user dashboards
2. **Test all outcomes** - Approved, denied, escalated
3. **Monitor confidence scores** - Adjust thresholds if needed
4. **Log all interactions** - Useful for audits
5. **Handle edge cases** - AI unavailable, network issues

---

## Advanced Configuration

### Customize Questions

Edit `SupportChatModule.cs`:

```csharp
private static readonly List<string> _appealQuestions = new()
{
    "Your custom question 1",
    "Your custom question 2",
    // ...
};
```

### Adjust Confidence Threshold

```csharp
// In MakeAppealDecisionAsync method
decision.RequiresHumanReview = 
    decision.Outcome == AppealOutcome.EscalateToHuman ||
    decision.ConfidenceScore < 0.7; // Adjust this value
```

### Add Persistence

Replace ConcurrentDictionary with database calls:

```csharp
// Current: In-memory
private readonly ConcurrentDictionary<string, AppealRequest> _appeals = new();

// Future: Database
private async Task<AppealRequest?> LoadAppealAsync(string id)
{
    // Load from database
}
```

---

## Performance

- **Appeal start**: < 100ms
- **Response analysis**: 1-3 seconds (AI dependent)
- **Final decision**: 2-5 seconds (AI dependent)
- **Escalation check**: < 50ms

---

## Support

Need help? Check:
- **Documentation**: PHASE4_7_SUMMARY.md
- **Source Code**: RaCore/Modules/Extensions/Support/
- **Models**: Abstractions/AppealModels.cs
- **Integration**: CONTENT_MODERATION_QUICKSTART.md

---

## Example: Full Appeal Flow

```bash
# 1. Setup
> moderation suspend user123 7 Posted spam content

# 2. Start appeal
> appeal start user123 "I apologize for violating the rules"

# 3. Answer all questions
> appeal respond user123 "I posted promotional links which is against the spam policy"
> appeal respond user123 "Yes, I understand I violated rule 5 about unsolicited advertising"
> appeal respond user123 "I will only share content that provides value and ask moderators first"
> appeal respond user123 "I was excited about a product and didn't think about the community rules"
> appeal respond user123 "Yes, I acknowledge future violations may result in permanent ban"

# 4. Decision
=== Appeal Decision ===
Outcome: Approved
Reasoning: User shows clear understanding, genuine remorse, and commitment to change
Confidence: 0.88
Your appeal has been approved. Your suspension has been lifted.

# 5. Verify
> moderation history user123
# Shows suspension is no longer active
```

---

**Ready to Handle Appeals with AI!** 🤖⚖️
