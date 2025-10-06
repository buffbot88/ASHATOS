# Content Moderation Quick Start Guide

## Introduction

RaCore's Content Moderation system (Phase 4.6) provides real-time protection against harmful content across all interactive modules. This guide will help you get started quickly.

---

## Quick Overview

The Content Moderation Module:
- ✅ Automatically scans text content in real-time
- ✅ Detects 13 types of violations (hate, violence, harassment, spam, etc.)
- ✅ Auto-blocks harmful content and suspends violators
- ✅ Provides manual review queue for edge cases
- ✅ Integrates seamlessly with existing modules

---

## Getting Started

### 1. Module is Auto-Loaded

The ContentModerationModule is automatically loaded when RaCore starts. No manual configuration required!

### 2. Test Content Scanning

```bash
# Start RaCore
cd RaCore
dotnet run

# Test scanning content
> moderation help
> moderation scan "This is a safe test message"
> moderation stats
```

### 3. View Statistics

```bash
> moderation stats
```

Returns:
```json
{
  "TotalScans": 10,
  "Approved": 8,
  "Flagged": 1,
  "Blocked": 1,
  "AutoSuspended": 0,
  "PendingReview": 1,
  "ActiveSuspensions": 0,
  "ViolationsByType": {
    "Spam": 1
  }
}
```

---

## Using Content Moderation

### Console Commands

```bash
# Get help
moderation help

# Scan text content
moderation scan "Your content here"

# View user's moderation history
moderation history user123

# View pending reviews
moderation pending

# Manually suspend a user
moderation suspend user123 7 Repeated violations

# Unsuspend a user
moderation unsuspend user123

# View statistics
moderation stats
```

---

## Integration Example: Forum Posts

The Forum module already has content moderation integrated. Here's how it works:

### Creating a Post

```bash
# Normal forum post creation flow
> forum create "Hello World" "This is my first post"

# If content contains violations:
> forum create "Spam Post" "Buy now! Click here for free money!"
# Result: Post blocked due to content violations: Spam
```

### Behind the Scenes

When you create a forum post:

1. **Suspension Check**: System checks if user is suspended
2. **Content Scan**: Post title and content are scanned
3. **Action Decision**: Based on violation severity:
   - **Approved**: Post created normally
   - **Flagged**: Post created, marked for review
   - **Blocked**: Post rejected, reason provided
   - **Auto-Suspend**: Post blocked, user suspended

---

## Understanding Violation Scores

### Score Ranges

- **0.0 - 0.59**: ✅ Approved (no action)
- **0.60 - 0.84**: ⚠️ Flagged (needs review)
- **0.85 - 0.89**: ❌ Blocked (content rejected)
- **0.90 - 1.00**: 🚫 Auto-Suspended (user banned)

### Example Violations

```bash
# Low severity (Approved)
> moderation scan "Hello everyone!"
# Score: 0.0, Action: Approved

# Medium severity (Flagged)
> moderation scan "Click here for limited time offer"
# Score: 0.4, Action: Approved (Spam pattern detected but below threshold)

# High severity (Blocked)
> moderation scan "I hate all people from XYZ group"
# Score: 0.95, Action: AutoSuspended (Hate speech detected)
```

---

## Violation Types

The system detects these violation types:

| Type | Examples | Weight |
|------|----------|--------|
| **Hate** | Racist, bigoted content | 0.95 |
| **Violence** | Threats, weapons | 0.90 |
| **Self-Harm** | Suicide content | 1.00 |
| **Sexual** | Explicit content | 0.85 |
| **Harassment** | Bullying, stalking | 0.80 |
| **Spam** | Excessive ads | 0.40 |
| **Phishing** | Fake login pages | 1.00 |
| **Personal Info** | SSN, credit cards | 0.70 |
| **Malware** | Malicious links | 1.00 |
| **Copyright** | Pirated content | 0.60 |
| **Illegal** | Illicit activities | 1.00 |
| **Misinformation** | False claims | 0.50 |
| **Profanity** | Excessive cursing | 0.30 |

---

## Manual Moderation

### Review Flagged Content

```bash
# View pending reviews
> moderation pending

# Returns list of flagged content with IDs
```

### Take Action on Flagged Content

Use the API or extend the module to add review commands:

```csharp
var moderationModule = GetModule<IContentModerationModule>();

// Review and approve
await moderationModule.ReviewContentAsync(
    moderationId: "abc-123",
    finalAction: ModerationAction.Approved,
    reviewerId: "moderator1",
    notes: "False positive - content is acceptable"
);

// Review and block
await moderationModule.ReviewContentAsync(
    moderationId: "abc-124",
    finalAction: ModerationAction.Blocked,
    reviewerId: "moderator1",
    notes: "Confirmed violation"
);
```

---

## User Suspension Management

### Check if User is Suspended

```bash
# Via console (not implemented by default, use API)
# Or check in application code
```

```csharp
var moderationModule = GetModule<IContentModerationModule>();
bool isSuspended = await moderationModule.IsUserSuspendedAsync("user123");

if (isSuspended)
{
    var suspension = await moderationModule.GetActiveSuspensionAsync("user123");
    Console.WriteLine($"Suspended until: {suspension?.ExpiresAt}");
    Console.WriteLine($"Reason: {suspension?.Reason}");
}
```

### Suspend a User

```bash
# Temporary suspension (7 days)
> moderation suspend user123 7 Repeated spam violations

# Permanent suspension (0 days)
> moderation suspend user123 0 Severe violations
```

### Unsuspend a User

```bash
> moderation unsuspend user123
```

---

## Integration with Your Own Modules

### Step 1: Add Module Reference

```csharp
public class YourModule : ModuleBase
{
    private IContentModerationModule? _moderationModule;
    
    public override void Initialize(object? manager)
    {
        var moduleManager = manager as ModuleManager;
        _moderationModule = moduleManager?.Modules
            .Select(m => m.Instance)
            .OfType<IContentModerationModule>()
            .FirstOrDefault();
    }
}
```

### Step 2: Check Before User Actions

```csharp
public async Task<bool> ProcessUserAction(string userId, string content)
{
    // Check suspension
    if (_moderationModule != null && 
        await _moderationModule.IsUserSuspendedAsync(userId))
    {
        return false; // Deny action
    }
    
    // Scan content
    var result = await _moderationModule.ScanTextAsync(
        content, 
        userId, 
        Name, 
        Guid.NewGuid().ToString()
    );
    
    // Act on result
    if (result.Action == ModerationAction.Blocked ||
        result.Action == ModerationAction.AutoSuspended)
    {
        return false; // Reject content
    }
    
    // Allow action
    return true;
}
```

---

## Configuration

### Default Policy (in ContentModerationModule)

```csharp
public class ModerationPolicy
{
    public double AutoBlockThreshold = 0.85;
    public double AutoSuspendThreshold = 0.90;
    public double FlagForReviewThreshold = 0.60;
    public int MaxViolationsBeforeSuspension = 3;
    public TimeSpan DefaultSuspensionDuration = TimeSpan.FromDays(7);
}
```

### Customizing Thresholds

To customize, modify `ContentModerationModule.cs`:

```csharp
private readonly ModerationPolicy _policy = new()
{
    AutoBlockThreshold = 0.80,           // More strict
    AutoSuspendThreshold = 0.85,         // More strict
    FlagForReviewThreshold = 0.50,       // More sensitive
    MaxViolationsBeforeSuspension = 5,   // More lenient
    DefaultSuspensionDuration = TimeSpan.FromDays(14) // Longer bans
};
```

---

## Best Practices

### For Administrators

1. **Review Flagged Content**: Check pending reviews regularly
2. **Monitor Stats**: Use `moderation stats` to track violations
3. **Adjust Thresholds**: Fine-tune based on false positive rates
4. **Appeal Process**: Establish clear appeal procedures
5. **Document Policies**: Maintain clear community guidelines

### For Developers

1. **Always Check Suspension**: Before allowing user actions
2. **Scan All User Content**: Text, titles, descriptions, etc.
3. **Handle Gracefully**: Provide clear feedback on rejections
4. **Log Moderations**: Track all moderation events
5. **Test Thoroughly**: Test with various violation scenarios

### For Users

1. **Follow Guidelines**: Understand community standards
2. **Self-Moderate**: Review content before posting
3. **Report Violations**: Use reporting features (when available)
4. **Appeal if Needed**: Contact moderators for false positives

---

## Troubleshooting

### False Positives

**Issue**: Legitimate content being blocked

**Solution**:
- Lower detection thresholds
- Refine pattern matching
- Add whitelist for common false positives
- Implement manual review for edge cases

### False Negatives

**Issue**: Harmful content not detected

**Solution**:
- Add more patterns to detection
- Lower thresholds for critical violations
- Implement ML-based detection
- Encourage user reporting

### Performance Issues

**Issue**: Slow content processing

**Solution**:
- Profile scanning operations
- Optimize pattern matching
- Consider async scanning for large content
- Cache common patterns

---

## Testing

### Test Scenarios

```bash
# Safe content
> moderation scan "Hello, how are you?"

# Spam detection
> moderation scan "Click here now! Limited time offer! Buy today!"

# Hate speech detection
> moderation scan "I hate all people from XYZ group"

# Violence detection
> moderation scan "I will attack and hurt people"

# Personal info detection
> moderation scan "My SSN is 123-45-6789"
```

### Expected Results

Each test should return a JSON response with:
- Violations detected
- Confidence score
- Action taken (Approved, Flagged, Blocked, AutoSuspended)

---

## Next Steps

1. **Review Documentation**: See `PHASE4_6_SUMMARY.md` for full details
2. **Integrate with Modules**: Add moderation to blogs, chat, comments
3. **Customize Patterns**: Add domain-specific violation patterns
4. **Build Dashboard**: Create web UI for moderation queue
5. **Add ML Models**: Integrate AI for better detection
6. **Multi-language**: Extend to support multiple languages

---

## Support

### Resources

- Full Documentation: `PHASE4_6_SUMMARY.md`
- Safety Module Docs: `RaCore/Modules/Extensions/Safety/README.md`
- API Reference: See `IContentModerationModule` interface

### Common Issues

**Q: How do I adjust sensitivity?**  
A: Modify thresholds in `ModerationPolicy` within `ContentModerationModule.cs`

**Q: Can I add custom patterns?**  
A: Yes, edit `_harmfulPatterns` dictionary in `ContentModerationModule.cs`

**Q: How do I integrate with my module?**  
A: Follow the integration guide in Step 1 and Step 2 above

**Q: Can I use ML models?**  
A: Yes, extend `ScanTextAsync` to call external ML services

---

**Version:** 4.6.0  
**Last Updated:** 2025-01-05  
**Status:** Production Ready
