namespace Abstractions;

/// <summary>
/// User profile for personalization
/// </summary>
public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public Dictionary<string, string> Preferences { get; set; } = new();
    public List<string> AllowedModules { get; set; } = new();
    public string? Role { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Social features
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Friends { get; set; } = new();
    public List<SocialPost> Posts { get; set; } = new();
    public List<Activity> ActivityFeed { get; set; } = new();
}

/// <summary>
/// Social post on user profile
/// </summary>
public class SocialPost
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Likes { get; set; } = new();
    public List<SocialComment> Comments { get; set; } = new();
}

/// <summary>
/// Comment on a social post
/// </summary>
public class SocialComment
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Activity feed item
/// </summary>
public class Activity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}

/// <summary>
/// Types of activities
/// </summary>
public enum ActivityType
{
    PostCreated,
    CommentAdded,
    FriendAdded,
    BlogPostCreated,
    ForumPostCreated,
    ProfileUpdated
}

/// <summary>
/// User preference keys
/// </summary>
public static class UserPreferenceKeys
{
    public const string Language = "language";
    public const string ResponseStyle = "response_style";
    public const string VerbosityLevel = "verbosity";
    public const string Theme = "theme";
    public const string AutoSave = "auto_save";
}
