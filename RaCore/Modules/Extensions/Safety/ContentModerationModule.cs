using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Safety;

/// <summary>
/// Real-time content moderation and harm detection module for RaCore.
/// Scans content across all interactive modules to detect and prevent harmful activity.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ContentModerationModule : ModuleBase, IContentModerationModule
{
    public override string Name => "ContentModeration";

    private readonly ModerationPolicy _policy = new();
    private readonly ConcurrentDictionary<string, ModerationResult> _moderationHistory = new();
    private readonly ConcurrentDictionary<string, SuspensionRecord> _suspensions = new();
    private readonly ConcurrentDictionary<string, List<string>> _userViolations = new();
    
    private ModuleManager? _manager;
    private IAuthenticationModule? _authModule;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Harmful content patterns (basic keyword-based detection)
    private static readonly Dictionary<ViolationType, List<string>> _harmfulPatterns = new()
    {
        [ViolationType.Hate] = new() 
        { 
            "hate", "racist", "bigot", "nazi", "supremacist", "genocide", "ethnic cleansing"
        },
        [ViolationType.Violence] = new() 
        { 
            "kill", "murder", "assault", "attack", "bomb", "weapon", "terrorist", "massacre"
        },
        [ViolationType.SelfHarm] = new() 
        { 
            "suicide", "self-harm", "cut myself", "end my life", "kill myself"
        },
        [ViolationType.Sexual] = new() 
        { 
            "explicit sexual content", "child abuse", "grooming", "predator"
        },
        [ViolationType.Harassment] = new() 
        { 
            "bully", "harass", "stalk", "threaten", "intimidate", "doxx", "dox"
        },
        [ViolationType.Spam] = new() 
        { 
            "click here", "limited time offer", "buy now", "free money", "congratulations you won"
        },
        [ViolationType.Phishing] = new() 
        { 
            "verify your account", "confirm your password", "urgent security alert", "account suspended"
        },
        [ViolationType.PersonalInfo] = new() 
        { 
            "ssn:", "social security", "credit card", "cvv:", "passport number"
        },
        [ViolationType.ExcessiveProfanity] = new()
        {
            // Basic profanity patterns - in production, use more sophisticated filtering
            "f**k", "sh*t", "damn", "hell"
        }
    };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _authModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IAuthenticationModule>()
                .FirstOrDefault();
        }
        
        Console.WriteLine($"[{Name}] Content Moderation Module initialized");
        Console.WriteLine($"[{Name}] Auto-block threshold: {_policy.AutoBlockThreshold:F2}");
        Console.WriteLine($"[{Name}] Auto-suspend threshold: {_policy.AutoSuspendThreshold:F2}");
        Console.WriteLine($"[{Name}] Max violations before suspension: {_policy.MaxViolationsBeforeSuspension}");
    }

    public override string Process(string input)
    {
        var text = (input ?? "").Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("moderation stats", StringComparison.OrdinalIgnoreCase))
        {
            var task = GetStatsAsync();
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("moderation scan ", StringComparison.OrdinalIgnoreCase))
        {
            var content = text["moderation scan ".Length..].Trim();
            var task = ScanTextAsync(content, "system", "console", Guid.NewGuid().ToString());
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("moderation history ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["moderation history ".Length..].Trim();
            var task = GetUserModerationHistoryAsync(userId);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.Equals("moderation pending", StringComparison.OrdinalIgnoreCase))
        {
            var task = GetPendingReviewsAsync();
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("moderation suspend ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: moderation suspend <userId> <days> <reason>";
            }
            
            var userId = parts[2];
            if (!int.TryParse(parts[3], out var days))
            {
                return "Invalid duration. Usage: moderation suspend <userId> <days> <reason>";
            }
            
            var reason = parts.Length > 4 ? string.Join(" ", parts.Skip(4)) : "Manual suspension";
            var duration = days > 0 ? TimeSpan.FromDays(days) : (TimeSpan?)null;
            
            var task = SuspendUserAsync(userId, reason, duration, "manual");
            task.Wait();
            return task.Result ? $"User {userId} suspended" : $"Failed to suspend user {userId}";
        }

        if (text.StartsWith("moderation unsuspend ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["moderation unsuspend ".Length..].Trim();
            var task = UnsuspendUserAsync(userId, "manual");
            task.Wait();
            return task.Result ? $"User {userId} unsuspended" : $"Failed to unsuspend user {userId}";
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    public async Task<ModerationResult> ScanTextAsync(string text, string userId, string moduleName, string contentId)
    {
        await Task.CompletedTask; // Async placeholder
        
        var result = new ModerationResult
        {
            ContentId = contentId,
            UserId = userId,
            ModuleName = moduleName,
            ContentType = ContentType.Text,
            TimestampUtc = DateTime.UtcNow
        };

        var violations = new List<ContentViolation>();
        var textLower = text.ToLowerInvariant();

        // Check for harmful patterns
        foreach (var (violationType, patterns) in _harmfulPatterns)
        {
            foreach (var pattern in patterns)
            {
                if (textLower.Contains(pattern))
                {
                    var weight = _policy.ViolationWeights.GetValueOrDefault(violationType, 0.5);
                    violations.Add(new ContentViolation
                    {
                        Type = violationType,
                        Description = $"Detected {violationType} content",
                        Severity = weight,
                        MatchedPattern = pattern,
                        Position = textLower.IndexOf(pattern)
                    });
                }
            }
        }

        // Calculate confidence score based on violations
        if (violations.Count == 0)
        {
            result.Action = ModerationAction.Approved;
            result.ConfidenceScore = 0.0;
        }
        else
        {
            // Take the maximum severity as confidence score
            result.ConfidenceScore = violations.Max(v => v.Severity);
            result.Violations = violations;

            // Determine action based on policy thresholds
            if (result.ConfidenceScore >= _policy.AutoSuspendThreshold)
            {
                result.Action = ModerationAction.AutoSuspended;
                
                // Auto-suspend the user
                await HandleAutoSuspensionAsync(userId, result);
            }
            else if (result.ConfidenceScore >= _policy.AutoBlockThreshold)
            {
                result.Action = ModerationAction.Blocked;
                await LogSecurityEventAsync(SecurityEventType.ContentBlocked, userId, 
                    $"Content blocked: {string.Join(", ", violations.Select(v => v.Type))}");
            }
            else if (result.ConfidenceScore >= _policy.FlagForReviewThreshold)
            {
                result.Action = ModerationAction.Flagged;
                await LogSecurityEventAsync(SecurityEventType.ContentFlagged, userId, 
                    $"Content flagged: {string.Join(", ", violations.Select(v => v.Type))}");
            }
            else
            {
                result.Action = ModerationAction.Approved;
            }
        }

        // Store result
        _moderationHistory[result.Id.ToString()] = result;
        
        // Track user violations
        if (result.Action != ModerationAction.Approved)
        {
            if (!_userViolations.TryGetValue(userId, out var userViolationsList))
            {
                userViolationsList = new List<string>();
                _userViolations[userId] = userViolationsList;
            }
            userViolationsList.Add(result.Id.ToString());
        }

        return result;
    }

    public async Task<bool> IsUserSuspendedAsync(string userId)
    {
        await Task.CompletedTask;
        
        var suspension = _suspensions.Values.FirstOrDefault(s => 
            s.UserId == userId && 
            s.IsActive && 
            (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));
            
        return suspension != null;
    }

    public async Task<SuspensionRecord?> GetActiveSuspensionAsync(string userId)
    {
        await Task.CompletedTask;
        
        return _suspensions.Values.FirstOrDefault(s => 
            s.UserId == userId && 
            s.IsActive && 
            (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<List<ModerationResult>> GetUserModerationHistoryAsync(string userId, int limit = 50)
    {
        await Task.CompletedTask;
        
        return _moderationHistory.Values
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.TimestampUtc)
            .Take(limit)
            .ToList();
    }

    public async Task<List<ModerationResult>> GetPendingReviewsAsync(int limit = 100)
    {
        await Task.CompletedTask;
        
        return _moderationHistory.Values
            .Where(m => m.Action == ModerationAction.Flagged && m.ReviewedBy == null)
            .OrderByDescending(m => m.TimestampUtc)
            .Take(limit)
            .ToList();
    }

    public async Task<bool> ReviewContentAsync(string moderationId, ModerationAction finalAction, string reviewerId, string notes)
    {
        await Task.CompletedTask;
        
        if (!_moderationHistory.TryGetValue(moderationId, out var result))
        {
            return false;
        }

        result.Action = finalAction;
        result.ReviewedBy = reviewerId;
        result.ReviewedAt = DateTime.UtcNow;
        result.ReviewNotes = notes;

        await LogSecurityEventAsync(SecurityEventType.ModerationReview, result.UserId, 
            $"Content reviewed by {reviewerId}: {finalAction}");

        return true;
    }

    public async Task<bool> SuspendUserAsync(string userId, string reason, TimeSpan? duration, string suspendedBy)
    {
        var suspension = new SuspensionRecord
        {
            UserId = userId,
            Reason = reason,
            SuspendedBy = suspendedBy,
            SuspendedAt = DateTime.UtcNow,
            ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null
        };

        _suspensions[suspension.Id.ToString()] = suspension;

        // Deactivate user in authentication system
        if (_authModule != null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
            }
        }

        await LogSecurityEventAsync(SecurityEventType.UserManuallySuspended, userId, 
            $"Suspended by {suspendedBy}: {reason} (Duration: {duration?.TotalDays ?? -1} days)");

        Console.WriteLine($"[{Name}] User {userId} suspended: {reason}");
        
        return true;
    }

    public async Task<bool> UnsuspendUserAsync(string userId, string unsuspendedBy)
    {
        var activeSuspension = await GetActiveSuspensionAsync(userId);
        if (activeSuspension == null)
        {
            return false;
        }

        activeSuspension.IsActive = false;

        // Reactivate user in authentication system
        if (_authModule != null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
            }
        }

        await LogSecurityEventAsync(SecurityEventType.UserUnsuspended, userId, 
            $"Unsuspended by {unsuspendedBy}");

        Console.WriteLine($"[{Name}] User {userId} unsuspended");
        
        return true;
    }

    public async Task<ModerationStats> GetStatsAsync()
    {
        await Task.CompletedTask;
        
        var stats = new ModerationStats
        {
            TotalScans = _moderationHistory.Count,
            Approved = _moderationHistory.Values.Count(m => m.Action == ModerationAction.Approved),
            Flagged = _moderationHistory.Values.Count(m => m.Action == ModerationAction.Flagged),
            Blocked = _moderationHistory.Values.Count(m => m.Action == ModerationAction.Blocked),
            AutoSuspended = _moderationHistory.Values.Count(m => m.Action == ModerationAction.AutoSuspended),
            PendingReview = _moderationHistory.Values.Count(m => m.Action == ModerationAction.Flagged && m.ReviewedBy == null),
            ActiveSuspensions = _suspensions.Values.Count(s => s.IsActive && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
        };

        // Count violations by type
        foreach (var result in _moderationHistory.Values)
        {
            foreach (var violation in result.Violations)
            {
                if (!stats.ViolationsByType.ContainsKey(violation.Type))
                {
                    stats.ViolationsByType[violation.Type] = 0;
                }
                stats.ViolationsByType[violation.Type]++;
            }
        }

        return stats;
    }

    private async Task HandleAutoSuspensionAsync(string userId, ModerationResult result)
    {
        // Check violation count
        var userViolationCount = _userViolations.GetValueOrDefault(userId, new List<string>()).Count;
        
        if (userViolationCount >= _policy.MaxViolationsBeforeSuspension || 
            result.ConfidenceScore >= _policy.AutoSuspendThreshold)
        {
            var violations = string.Join(", ", result.Violations.Select(v => v.Type));
            await SuspendUserAsync(
                userId, 
                $"Automatic suspension due to content violation: {violations}", 
                _policy.DefaultSuspensionDuration,
                "System"
            );

            await LogSecurityEventAsync(SecurityEventType.UserAutoSuspended, userId, 
                $"Auto-suspended: {violations} (Score: {result.ConfidenceScore:F2})");
        }
    }

    private async Task<User?> GetUserByIdAsync(string userId)
    {
        await Task.CompletedTask;
        
        if (_authModule == null || !Guid.TryParse(userId, out var userGuid))
        {
            return null;
        }

        // Try to get user from auth module
        // Note: This is a simplified approach. In production, AuthenticationModule should expose GetUserById
        return null;
    }

    private async Task LogSecurityEventAsync(SecurityEventType type, string userId, string details)
    {
        if (_authModule == null)
        {
            return;
        }

        var securityEvent = new SecurityEvent
        {
            Type = type,
            UserId = Guid.TryParse(userId, out var guid) ? guid : null,
            Username = userId,
            IpAddress = "system",
            Details = details,
            Success = true,
            TimestampUtc = DateTime.UtcNow
        };

        await _authModule.LogSecurityEventAsync(securityEvent);
    }

    private string GetHelp()
    {
        return @"Content Moderation Module:
  - help                          : Show this help
  - moderation stats              : Show moderation statistics
  - moderation scan <text>        : Scan text for violations
  - moderation history <userId>   : Get user's moderation history
  - moderation pending            : List content pending review
  - moderation suspend <userId> <days> <reason> : Suspend a user
  - moderation unsuspend <userId> : Unsuspend a user

Features:
  - Real-time text scanning for harmful content
  - Automated content blocking and user suspension
  - Moderation queue for manual review
  - Comprehensive audit logs
  - Configurable violation thresholds

Violation Types:
  - Harassment, Hate, Violence, Self-Harm
  - Sexual content, Spam, Phishing
  - Personal Information, Copyright, Illegal content
  - Misinformation, Excessive Profanity

Policy:
  - Auto-block threshold: " + _policy.AutoBlockThreshold.ToString("F2") + @"
  - Auto-suspend threshold: " + _policy.AutoSuspendThreshold.ToString("F2") + @"
  - Flag for review threshold: " + _policy.FlagForReviewThreshold.ToString("F2") + @"
  - Max violations before suspension: " + _policy.MaxViolationsBeforeSuspension + @"
  - Default suspension duration: " + _policy.DefaultSuspensionDuration.TotalDays + @" days";
    }
}
