namespace Abstractions;

/// <summary>
/// Represents a content moderation scan result.
/// </summary>
public class moderationResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string ContentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public moderationAction Action { get; set; }
    public double ConfidenceScore { get; set; }
    public List<ContentViolation> Violations { get; set; } = new();
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// Represents a specific content violation.
/// </summary>
public class ContentViolation
{
    public ViolationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Severity { get; set; } // 0.0 to 1.0
    public string? MatchedPattern { get; set; }
    public int? Position { get; set; } // Position in text, if applicable
}

/// <summary>
/// Types of content that can be modeRated.
/// </summary>
public enum ContentType
{
    Text,
    Image,
    Video,
    Audio,
    Link,
    File
}

/// <summary>
/// Types of content violations.
/// </summary>
public enum ViolationType
{
    None,
    HaASHATssment,
    Hate,
    Violence,
    SelfHarm,
    Sexual,
    Spam,
    Malware,
    Phishing,
    PersonalInfo,
    Copyright,
    Illegal,
    Misinformation,
    ExcessiveProfanity
}

/// <summary>
/// Actions that can be taken by the moderation system.
/// </summary>
public enum moderationAction
{
    Approved,
    Flagged,
    Blocked,
    AutoSuspended,
    RequiresReview
}

/// <summary>
/// Represents a user suspension record.
/// </summary>
public class SuspensionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public DateTime SuspendedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // Null = permanent
    public string Reason { get; set; } = string.Empty;
    public string SuspendedBy { get; set; } = "System";
    public bool IsActive { get; set; } = true;
    public List<string> ViolationIds { get; set; } = new(); // References to moderationResult IDs
    public string? AppealNotes { get; set; }
    public DateTime? AppealedAt { get; set; }
}

/// <summary>
/// Configuration for content moderation thresholds.
/// </summary>
public class moderationPolicy
{
    public double AutoBlockThreshold { get; set; } = 0.85;
    public double AutoSuspendThreshold { get; set; } = 0.90;
    public double FlagForReviewThreshold { get; set; } = 0.60;
    public int MaxViolationsBeforeSuspension { get; set; } = 3;
    public TimeSpan DefaultSuspensionduration { get; set; } = TimeSpan.FromDays(7);
    public Dictionary<ViolationType, double> ViolationWeights { get; set; } = new()
    {
        [ViolationType.HaASHATssment] = 0.8,
        [ViolationType.Hate] = 0.95,
        [ViolationType.Violence] = 0.9,
        [ViolationType.SelfHarm] = 1.0,
        [ViolationType.Sexual] = 0.85,
        [ViolationType.Spam] = 0.4,
        [ViolationType.Malware] = 1.0,
        [ViolationType.Phishing] = 1.0,
        [ViolationType.PersonalInfo] = 0.7,
        [ViolationType.Copyright] = 0.6,
        [ViolationType.Illegal] = 1.0,
        [ViolationType.Misinformation] = 0.5,
        [ViolationType.ExcessiveProfanity] = 0.3
    };
}

/// <summary>
/// Interface for content moderation module.
/// </summary>
public interface IContentmoderationModule
{
    /// <summary>
    /// Scan text content for violations.
    /// </summary>
    Task<moderationResult> ScanTextAsync(string text, string userId, string moduleName, string contentId);
    
    /// <summary>
    /// Check if a user is currently suspended.
    /// </summary>
    Task<bool> IsUserSuspendedAsync(string userId);
    
    /// <summary>
    /// Get active suspension for a user.
    /// </summary>
    Task<SuspensionRecord?> GetActiveSuspensionAsync(string userId);
    
    /// <summary>
    /// Get moderation history for a user.
    /// </summary>
    Task<List<moderationResult>> GetUsermoderationHistoryAsync(string userId, int limit = 50);
    
    /// <summary>
    /// Get pending reviews.
    /// </summary>
    Task<List<moderationResult>> GetPendingReviewsAsync(int limit = 100);
    
    /// <summary>
    /// Review a flagged content.
    /// </summary>
    Task<bool> ReviewContentAsync(string moderationId, moderationAction finalAction, string reviewerId, string notes);
    
    /// <summary>
    /// Manually suspend a user.
    /// </summary>
    Task<bool> SuspendUserAsync(string userId, string reason, TimeSpan? duration, string suspendedBy);
    
    /// <summary>
    /// Lift a suspension.
    /// </summary>
    Task<bool> UnsuspendUserAsync(string userId, string unsuspendedBy);
    
    /// <summary>
    /// Get moderation statistics.
    /// </summary>
    Task<moderationStats> GetStatsAsync();
}

/// <summary>
/// Statistics for content moderation.
/// </summary>
public class moderationStats
{
    public int TotalScans { get; set; }
    public int Approved { get; set; }
    public int Flagged { get; set; }
    public int Blocked { get; set; }
    public int AutoSuspended { get; set; }
    public int PendingReview { get; set; }
    public int ActiveSuspensions { get; set; }
    public Dictionary<ViolationType, int> ViolationsByType { get; set; } = new();
}
