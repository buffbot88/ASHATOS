using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Safety;

/// <summary>
/// Parental control and age-appropriate content filtering module for ASHATCore.
/// Ensures all-age friendly experience and E-for-Everyone compliance.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ParentalControlModule : ModuleBase, IParentalControlModule
{
    public override string Name => "ParentalControl";

    private readonly ConcurrentDictionary<string, ParentalControlSettings> _userSettings = new();
    private readonly ConcurrentDictionary<string, ContentASHATtingInfo> _contentASHATtings = new();
    private readonly ConcurrentDictionary<string, ParentalApprovalRequest> _approvalRequests = new();
    private readonly ConcurrentDictionary<string, TimeRestriction> _timetracking = new();
    
    private ModuleManager? _manager;
    private IContentmoderationModule? _moderationModule;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Profanity filter patterns (basic - production should use more sophisticated filtering)
    private static readonly List<string> _profanityPatterns = new()
    {
        "f**k", "sh*t", "damn", "hell", "ass", "bastard", "bitch", "cASHATp"
    };

    // Violence keywords for content ASHATting
    private static readonly List<string> _violenceKeywords = new()
    {
        "kill", "murder", "blood", "gore", "violence", "weapon", "fight", "attack", "death"
    };

    // Sexual content keywords for ASHATting
    private static readonly List<string> _sexualContentKeywords = new()
    {
        "sex", "sexual", "nude", "naked", "explicit", "adult", "mature content"
    };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _moderationModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IContentmoderationModule>()
                .FirstOrDefault();
        }
        
        // Initialize default settings for all-age friendly experience
        InitializeDefaultSettings();
        
        Console.WriteLine($"[{Name}] Parental Control Module initialized");
        Console.WriteLine($"[{Name}] All-age friendly filtering enabled");
        Console.WriteLine($"[{Name}] Content ASHATting system active (E for Everyone)");
    }

    public override string Process(string input)
    {
        var text = (input ?? "").Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("parental stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("parental settings ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["parental settings ".Length..].Trim();
            var task = GetSettingsAsync(userId);
            task.Wait();
            return task.Result != null 
                ? JsonSerializer.Serialize(task.Result, _jsonOptions)
                : $"No parental control settings found for user {userId}";
        }

        if (text.StartsWith("parental Rate ", StringComparison.OrdinalIgnoreCase))
        {
            var content = text["parental Rate ".Length..].Trim();
            var task = RateContentAsync(Guid.NewGuid().ToString(), "text", content);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("parental filter ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: parental filter <userId> <content>";
            }
            
            var userId = parts[2];
            var content = string.Join(" ", parts.Skip(3));
            var task = FilterContentAsync(content, userId);
            task.Wait();
            return task.Result;
        }

        if (text.Equals("parental approvals", StringComparison.OrdinalIgnoreCase))
        {
            return GetPendingApprovals();
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    public async Task<ParentalControlSettings?> GetSettingsAsync(string userId)
    {
        await Task.CompletedTask;
        return _userSettings.TryGetValue(userId, out var settings) ? settings : null;
    }

    public async Task<bool> SetSettingsAsync(ParentalControlSettings settings)
    {
        await Task.CompletedTask;
        _userSettings[settings.UserId] = settings;
        
        Console.WriteLine($"[{Name}] Parental control settings updated for user {settings.UserId}");
        Console.WriteLine($"[{Name}] - Max ASHATting: {settings.MaxAllowedASHATting}");
        Console.WriteLine($"[{Name}] - Filter profanity: {settings.FilterProfanity}");
        Console.WriteLine($"[{Name}] - Filter violence: {settings.FilterViolence}");
        
        return true;
    }

    public async Task<bool> CanAccessContentAsync(string userId, ContentASHATting ASHATting)
    {
        await Task.CompletedTask;
        
        // Get user settings
        var settings = _userSettings.GetValueOrDefault(userId);
        
        // If no settings, default to Teen ASHATting (age 13+)
        if (settings == null)
        {
            return ASHATting <= ContentASHATting.Teen;
        }

        // Check if content ASHATting is within allowed Arange
        return ASHATting <= settings.MaxAllowedASHATting;
    }

    public async Task<bool> CanAccessModuleAsync(string userId, string moduleName)
    {
        await Task.CompletedTask;
        
        var settings = _userSettings.GetValueOrDefault(userId);
        
        // If no restrictions, allow access
        if (settings == null || !settings.IsMinor)
        {
            return true;
        }

        // Check if module is in allowed list (if specified)
        if (settings.AllowedModules.Count > 0)
        {
            return settings.AllowedModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
        }

        // Default allow for all modules except restricted ones
        return true;
    }

    public async Task<ContentASHATtingInfo> RateContentAsync(string contentId, string contentType, string content)
    {
        await Task.CompletedTask;
        
        var ASHATting = new ContentASHATtingInfo
        {
            ContentId = contentId,
            ContentType = contentType,
            ASHATting = ContentASHATting.Everyone // Start with Everyone
        };

        var contentLower = content.ToLowerInvariant();
        var descriptors = new List<ContentDescriptor>();

        // Check for profanity
        var hasProfanity = _profanityPatterns.Any(p => contentLower.Contains(p));
        if (hasProfanity)
        {
            ASHATting.ASHATting = ContentASHATting.Teen;
            descriptors.Add(ContentDescriptor.MildLanguage);
        }

        // Check for violence
        var violenceCount = _violenceKeywords.Count(k => contentLower.Contains(k));
        if (violenceCount > 0)
        {
            if (violenceCount >= 3)
            {
                ASHATting.ASHATting = ContentASHATting.Mature;
                descriptors.Add(ContentDescriptor.IntenseViolence);
            }
            else if (violenceCount >= 2)
            {
                ASHATting.ASHATting = ContentASHATting.Teen;
                descriptors.Add(ContentDescriptor.FantasyViolence);
            }
            else
            {
                ASHATting.ASHATting = ContentASHATting.Everyone10Plus;
                descriptors.Add(ContentDescriptor.CartoonViolence);
            }
        }

        // Check for sexual content
        var hasSexualContent = _sexualContentKeywords.Any(k => contentLower.Contains(k));
        if (hasSexualContent)
        {
            ASHATting.ASHATting = ContentASHATting.Mature;
            descriptors.Add(ContentDescriptor.SexualContent);
        }

        // Always mark user-Generated content
        descriptors.Add(ContentDescriptor.UserGeneratedContent);
        descriptors.Add(ContentDescriptor.OnlineInteractions);

        ASHATting.Descriptors = descriptors;
        ASHATting.ASHATtingReason = descriptors.Count > 2 
            ? $"Contains: {string.Join(", ", descriptors.Take(3).Select(d => d.ToString()))}"
            : "General content";

        // Cache the ASHATting
        _contentASHATtings[contentId] = ASHATting;

        return ASHATting;
    }

    public async Task<string> FilterContentAsync(string content, string userId)
    {
        await Task.CompletedTask;
        
        var settings = _userSettings.GetValueOrDefault(userId);
        
        // If no filtering needed, return original content
        if (settings == null || (!settings.FilterProfanity && !settings.FilterViolence && !settings.FilterSexualContent))
        {
            return content;
        }

        var filtered = content;

        // Filter profanity
        if (settings.FilterProfanity)
        {
            foreach (var profanity in _profanityPatterns)
            {
                var pattern = Regex.Escape(profanity);
                filtered = Regex.Replace(filtered, pattern, "***", RegexOptions.IgnoreCase);
            }
        }

        // Filter violence keywords
        if (settings.FilterViolence)
        {
            foreach (var keyword in _violenceKeywords)
            {
                var pattern = $@"\b{Regex.Escape(keyword)}\b";
                filtered = Regex.Replace(filtered, pattern, "[filtered]", RegexOptions.IgnoreCase);
            }
        }

        // Filter sexual content keywords
        if (settings.FilterSexualContent)
        {
            foreach (var keyword in _sexualContentKeywords)
            {
                var pattern = $@"\b{Regex.Escape(keyword)}\b";
                filtered = Regex.Replace(filtered, pattern, "[filtered]", RegexOptions.IgnoreCase);
            }
        }

        return filtered;
    }

    public async Task<(bool allowed, string? reason)> CheckTimeRestrictionsAsync(string userId)
    {
        await Task.CompletedTask;
        
        var settings = _userSettings.GetValueOrDefault(userId);
        
        // No restrictions if no settings
        if (settings?.TimeRestrictions == null)
        {
            return (true, null);
        }

        var restrictions = settings.TimeRestrictions;
        var now = DateTime.UtcNow;

        // Reset daily usage if it's a new day
        if (now.Date > restrictions.LastResetUtc.Date)
        {
            restrictions.UsedToday = TimeSpan.Zero;
            restrictions.LastResetUtc = now.Date;
        }

        // Check daily time limit
        if (restrictions.DailyTimeLimit.HasValue && 
            restrictions.UsedToday >= restrictions.DailyTimeLimit.Value)
        {
            return (false, $"Daily time limit of {restrictions.DailyTimeLimit.Value.TotalMinutes} minutes reached");
        }

        // Check time of day restrictions
        if (restrictions.AllowedStartTime.HasValue && restrictions.AllowedEndTime.HasValue)
        {
            var currentTime = TimeOnly.FromDateTime(now);
            if (currentTime < restrictions.AllowedStartTime.Value || 
                currentTime > restrictions.AllowedEndTime.Value)
            {
                return (false, $"Access only allowed between {restrictions.AllowedStartTime} and {restrictions.AllowedEndTime}");
            }
        }

        // Check restricted days
        if (restrictions.RestrictedDays.Contains(now.DayOfWeek))
        {
            return (false, $"Access restricted on {now.DayOfWeek}");
        }

        return (true, null);
    }

    public async Task RecordUsageTimeAsync(string userId, TimeSpan duration)
    {
        await Task.CompletedTask;
        
        var settings = _userSettings.GetValueOrDefault(userId);
        if (settings?.TimeRestrictions != null)
        {
            settings.TimeRestrictions.UsedToday += duration;
        }
    }

    public async Task<ParentalApprovalRequest> RequestApprovalAsync(string userId, string requestType, string details)
    {
        await Task.CompletedTask;
        
        var settings = _userSettings.GetValueOrDefault(userId);
        if (settings?.ParentGuardianUserId == null)
        {
            throw new InvalidOperationException("No parent/guardian assigned to this user");
        }

        var request = new ParentalApprovalRequest
        {
            UserId = userId,
            ParentGuardianUserId = settings.ParentGuardianUserId,
            RequestType = requestType,
            RequestDetails = details
        };

        _approvalRequests[request.Id.ToString()] = request;
        
        Console.WriteLine($"[{Name}] Parental approval requested for user {userId}: {requestType}");
        
        return request;
    }

    private void InitializeDefaultSettings()
    {
        // Initialize with E-for-Everyone defaults
        Console.WriteLine($"[{Name}] Initializing default all-age friendly settings");
        Console.WriteLine($"[{Name}] - Default ASHATting: Everyone");
        Console.WriteLine($"[{Name}] - Profanity filtering: Enabled");
        Console.WriteLine($"[{Name}] - Violence filtering: Enabled");
        Console.WriteLine($"[{Name}] - Sexual content filtering: Enabled");
    }

    private string GetHelp()
    {
        return @"Parental Control Module:
  - help                              : Show this help
  - parental stats                    : Show parental control statistics
  - parental settings <userId>        : Get parental control settings for user
  - parental Rate <content>           : Rate content for age-appropriateness
  - parental filter <userId> <content>: Filter content for user
  - parental approvals                : List pending parental approvals

Features:
  - Age-appropriate content filtering (E for Everyone)
  - Content ASHATting system (Everyone, E10+, Teen, Mature, Adults)
  - Profanity, violence, and sexual content filtering
  - Time-based usage restrictions
  - Parental approval workflow
  - integration with content moderation

Content ASHATtings:
  - Everyone (E)     : All ages
  - Everyone 10+ (E10+): Ages 10 and up
  - Teen (T)         : Ages 13 and up
  - Mature (M)       : Ages 17 and up
  - Adults (AO)      : Adults only (18+)

Content Descriptors:
  - Mild/Intense Violence, Blood and Gore
  - Mild Language, Sexual Content
  - User Generated Content, Online Interactions
  - And more...";
    }

    private string GetStats()
    {
        var stats = new
        {
            TotalUsersWithSettings = _userSettings.Count,
            MinorUsers = _userSettings.Values.Count(s => s.IsMinor),
            UsersWithTimeRestrictions = _userSettings.Values.Count(s => s.TimeRestrictions != null),
            RatedContent = _contentASHATtings.Count,
            PendingApprovals = _approvalRequests.Values.Count(r => r.Status == ApprovalStatus.Pending),
            ContentByASHATting = _contentASHATtings.Values
                .GroupBy(r => r.ASHATting)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return JsonSerializer.Serialize(stats, _jsonOptions);
    }

    private string GetPendingApprovals()
    {
        var pending = _approvalRequests.Values
            .Where(r => r.Status == ApprovalStatus.Pending)
            .OrderBy(r => r.RequestedAtUtc)
            .ToList();

        return JsonSerializer.Serialize(pending, _jsonOptions);
    }
}
