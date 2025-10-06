namespace Abstractions;

/// <summary>
/// Represents parental control settings for a user account.
/// </summary>
public class ParentalControlSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public bool IsMinor { get; set; } = false;
    public DateTime? DateOfBirth { get; set; }
    public ContentRating MaxAllowedRating { get; set; } = ContentRating.Teen;
    public bool RequireParentalApproval { get; set; } = false;
    public string? ParentGuardianUserId { get; set; }
    public List<string> BlockedCategories { get; set; } = new();
    public List<string> AllowedModules { get; set; } = new();
    public TimeRestriction? TimeRestrictions { get; set; }
    public bool FilterProfanity { get; set; } = true;
    public bool FilterViolence { get; set; } = true;
    public bool FilterSexualContent { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

/// <summary>
/// Content rating system aligned with ESRB/PEGI standards.
/// </summary>
public enum ContentRating
{
    Everyone = 0,      // E - All ages
    Everyone10Plus = 10, // E10+ - Ages 10 and up
    Teen = 13,         // T - Ages 13 and up
    Mature = 17,       // M - Ages 17 and up
    Adults = 18        // AO - Adults only (18+)
}

/// <summary>
/// Time-based usage restrictions.
/// </summary>
public class TimeRestriction
{
    public TimeSpan? DailyTimeLimit { get; set; }
    public TimeOnly? AllowedStartTime { get; set; }
    public TimeOnly? AllowedEndTime { get; set; }
    public List<DayOfWeek> RestrictedDays { get; set; } = new();
    public TimeSpan UsedToday { get; set; } = TimeSpan.Zero;
    public DateTime LastResetUtc { get; set; } = DateTime.UtcNow.Date;
}

/// <summary>
/// Represents a content rating assigned to content.
/// </summary>
public class ContentRatingInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ContentId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // "post", "game", "comment", etc.
    public ContentRating Rating { get; set; } = ContentRating.Everyone;
    public List<ContentDescriptor> Descriptors { get; set; } = new();
    public string? RatingReason { get; set; }
    public DateTime RatedAtUtc { get; set; } = DateTime.UtcNow;
    public string RatedBy { get; set; } = "System";
}

/// <summary>
/// Content descriptors explaining why content has a particular rating.
/// </summary>
public enum ContentDescriptor
{
    None = 0,
    MildLanguage = 1,
    MildViolence = 2,
    CartoonViolence = 3,
    FantasyViolence = 4,
    IntenseViolence = 5,
    BloodAndGore = 6,
    SexualContent = 7,
    SuggestiveThemes = 8,
    NudityOrPartialNudity = 9,
    UseOfAlcoholOrTobacco = 10,
    DrugReference = 11,
    MatureHumor = 12,
    SimulatedGambling = 13,
    RealMoneyTransactions = 14,
    OnlineInteractions = 15,
    UserGeneratedContent = 16
}

/// <summary>
/// Interface for parental control module.
/// </summary>
public interface IParentalControlModule
{
    /// <summary>
    /// Get parental control settings for a user.
    /// </summary>
    Task<ParentalControlSettings?> GetSettingsAsync(string userId);
    
    /// <summary>
    /// Set parental control settings for a user.
    /// </summary>
    Task<bool> SetSettingsAsync(ParentalControlSettings settings);
    
    /// <summary>
    /// Check if user can access content based on rating.
    /// </summary>
    Task<bool> CanAccessContentAsync(string userId, ContentRating rating);
    
    /// <summary>
    /// Check if user can access a specific module.
    /// </summary>
    Task<bool> CanAccessModuleAsync(string userId, string moduleName);
    
    /// <summary>
    /// Rate content for age-appropriateness.
    /// </summary>
    Task<ContentRatingInfo> RateContentAsync(string contentId, string contentType, string content);
    
    /// <summary>
    /// Filter content for age-appropriate display.
    /// </summary>
    Task<string> FilterContentAsync(string content, string userId);
    
    /// <summary>
    /// Check time restrictions for a user.
    /// </summary>
    Task<(bool allowed, string? reason)> CheckTimeRestrictionsAsync(string userId);
    
    /// <summary>
    /// Record usage time for a user.
    /// </summary>
    Task RecordUsageTimeAsync(string userId, TimeSpan duration);
}

/// <summary>
/// Parental approval request for restricted actions.
/// </summary>
public class ParentalApprovalRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string ParentGuardianUserId { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty; // "module_access", "content_access", etc.
    public string RequestDetails { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAtUtc { get; set; }
    public string? ResponseNotes { get; set; }
}

/// <summary>
/// Status of parental approval requests.
/// </summary>
public enum ApprovalStatus
{
    Pending,
    Approved,
    Denied,
    Expired
}
