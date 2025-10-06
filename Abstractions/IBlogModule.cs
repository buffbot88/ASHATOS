namespace Abstractions;

/// <summary>
/// Blog module interface for managing blog posts, comments, and categories.
/// </summary>
public interface IBlogModule
{
    /// <summary>
    /// Get all blog posts with optional filtering.
    /// </summary>
    Task<List<BlogPost>> GetPostsAsync(int page = 1, int perPage = 20, string? category = null);
    
    /// <summary>
    /// Get a specific blog post by ID.
    /// </summary>
    Task<BlogPost?> GetPostByIdAsync(string postId);
    
    /// <summary>
    /// Create a new blog post (with content moderation).
    /// </summary>
    Task<(bool success, string message, string? postId)> CreatePostAsync(string userId, string username, string title, string content, string? category = null);
    
    /// <summary>
    /// Update an existing blog post.
    /// </summary>
    Task<bool> UpdatePostAsync(string postId, string userId, string title, string content, string? category = null);
    
    /// <summary>
    /// Delete a blog post.
    /// </summary>
    Task<bool> DeletePostAsync(string postId, string userId);
    
    /// <summary>
    /// Get comments for a blog post.
    /// </summary>
    Task<List<BlogComment>> GetCommentsAsync(string postId);
    
    /// <summary>
    /// Add a comment to a blog post.
    /// </summary>
    Task<(bool success, string message, string? commentId)> AddCommentAsync(string postId, string userId, string username, string content);
    
    /// <summary>
    /// Delete a comment.
    /// </summary>
    Task<bool> DeleteCommentAsync(string commentId, string userId);
    
    /// <summary>
    /// Get all categories.
    /// </summary>
    Task<List<BlogCategory>> GetCategoriesAsync();
    
    /// <summary>
    /// Get posts by user.
    /// </summary>
    Task<List<BlogPost>> GetPostsByUserAsync(string userId);
}

public class BlogPost
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public ContentRating ContentRating { get; set; } = ContentRating.Everyone;
}

public class BlogComment
{
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ContentRating ContentRating { get; set; } = ContentRating.Everyone;
}

public class BlogCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PostCount { get; set; }
}
