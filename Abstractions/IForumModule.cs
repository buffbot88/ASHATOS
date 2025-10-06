namespace Abstractions;

/// <summary>
/// Forum module interface for managing forum posts, users, and moderation.
/// </summary>
public interface IForumModule
{
    /// <summary>
    /// Get all forum posts with optional filtering.
    /// </summary>
    Task<List<ForumPost>> GetPostsAsync(int page = 1, int perPage = 20, bool includeDeleted = false);
    
    /// <summary>
    /// Delete a forum post.
    /// </summary>
    Task<bool> DeletePostAsync(string postId, string moderatorId, string reason);
    
    /// <summary>
    /// Lock or unlock a forum thread.
    /// </summary>
    Task<bool> LockThreadAsync(string postId, bool locked, string moderatorId);
    
    /// <summary>
    /// Get warnings for a user.
    /// </summary>
    Task<List<ForumWarning>> GetUserWarningsAsync(string userId);
    
    /// <summary>
    /// Issue a warning to a user.
    /// </summary>
    Task<bool> IssueWarningAsync(string userId, string reason, string moderatorId);
    
    /// <summary>
    /// Ban or unban a user.
    /// </summary>
    Task<bool> BanUserAsync(string userId, bool banned, string reason, string moderatorId);
    
    /// <summary>
    /// Check if a user is banned.
    /// </summary>
    Task<bool> IsUserBannedAsync(string userId);
    
    /// <summary>
    /// Get forum statistics.
    /// </summary>
    Task<ForumStats> GetStatsAsync();
}

public class ForumPost
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
    public bool IsLocked { get; set; }
    public int ReplyCount { get; set; }
    public int ViewCount { get; set; }
}

public class ForumWarning
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string IssuedBy { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ForumStats
{
    public int TotalPosts { get; set; }
    public int ActiveThreads { get; set; }
    public int DeletedPosts { get; set; }
    public int LockedThreads { get; set; }
    public int BannedUsers { get; set; }
    public int TotalWarnings { get; set; }
}
