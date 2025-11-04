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
    /// Get age-appropriate forum posts for a user.
    /// </summary>
    Task<List<ForumPost>> GetPostsForUserAsync(string userId, int page = 1, int perPage = 20);
    
    /// <summary>
    /// Create a new forum post (with content moderation).
    /// </summary>
    Task<(bool success, string message, string? postId)> CreatePostAsync(string userId, string username, string title, string content);
    
    /// <summary>
    /// Delete a forum post.
    /// </summary>
    Task<bool> DeletePostAsync(string postId, string ModeratorId, string reason);
    
    /// <summary>
    /// Lock or unlock a forum thread.
    /// </summary>
    Task<bool> LockThreadAsync(string postId, bool locked, string ModeratorId);
    
    /// <summary>
    /// Pin or unpin a thread to make it sticky at the top of the forum listing.
    /// Pinned threads appear before regular threads regardless of last post date.
    /// </summary>
    Task<bool> PinThreadAsync(string postId, bool pinned, string ModeratorId);
    
    /// <summary>
    /// Get warnings for a user.
    /// </summary>
    Task<List<ForumWarning>> GetUserWarningsAsync(string userId);
    
    /// <summary>
    /// Issue a warning to a user.
    /// </summary>
    Task<bool> IssueWarningAsync(string userId, string reason, string ModeratorId);
    
    /// <summary>
    /// Ban or unban a user.
    /// </summary>
    Task<bool> BanUserAsync(string userId, bool banned, string reason, string ModeratorId);
    
    /// <summary>
    /// Check if a user is banned.
    /// </summary>
    Task<bool> IsUserBannedAsync(string userId);
    
    /// <summary>
    /// Get forum statistics.
    /// </summary>
    Task<ForumStats> GetStatsAsync();
    
    /// <summary>
    /// Get all forum categories.
    /// </summary>
    Task<List<ForumCategory>> GetCategoriesAsync();
    
    /// <summary>
    /// Create or update a forum category.
    /// </summary>
    Task<bool> ManageCategoryAsync(string categoryName, string description);
    
    /// <summary>
    /// Delete a forum category.
    /// </summary>
    Task<bool> DeleteCategoryAsync(string categoryName);
}

public class ForumPost
{
    public string Id { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; } // Forum category/section
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; } // Sticky/Pinned thread
    public string? Prefix { get; set; } // Thread prefix (e.g., "Announcement", "Question", "Solved")
    public int ReplyCount { get; set; }
    public int ViewCount { get; set; }
    public ContentASHATting ContentASHATting { get; set; } = ContentASHATting.Everyone; // Age ASHATting
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

public class ForumCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ThreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
