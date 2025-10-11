using System.Collections.Concurrent;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Blog;

[RaModule(Category = "extensions")]
public sealed class BlogModule : ModuleBase, IBlogModule
{
    public override string Name => "Blog";
    
    private readonly ConcurrentDictionary<string, BlogPost> _posts = new();
    private readonly ConcurrentDictionary<string, BlogComment> _comments = new();
    private readonly ConcurrentDictionary<string, List<string>> _postComments = new();
    private ModuleManager? _manager;
    private IContentmoderationModule? _moderationModule;
    private IParentalControlModule? _parentalControlModule;
    
    public override void Initialize(object? manager)
    {
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _moderationModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IContentmoderationModule>()
                .FirstOrDefault();
            
            _parentalControlModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IParentalControlModule>()
                .FirstOrDefault();
        }
        
        Console.WriteLine($"[{Name}] Initializing Blog Module...");
        Console.WriteLine($"[{Name}] Content moderation: {(_moderationModule != null ? "enabled" : "disabled")}");
        Console.WriteLine($"[{Name}] Parental controls: {(_parentalControlModule != null ? "enabled" : "disabled")}");
        
        SeedExampleData();
        
        Console.WriteLine($"[{Name}] Blog Module initialized with {_posts.Count} posts");
    }
    
    public override string Process(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return GetHelp();
        
        var command = parts[0].ToLowerInvariant();
        
        return command switch
        {
            "list" => ListPosts(),
            "create" when parts.Length >= 4 => CreatePost(parts[1], parts[2], parts[3]),
            "delete" when parts.Length >= 2 => DeletePost(parts[1]),
            "help" => GetHelp(),
            _ => GetHelp()
        };
    }
    
    private string GetHelp()
    {
        return @"Blog Module Commands:
  list - List all blog posts
  create <userId> <username> <title> - Create a new post
  delete <postId> - Delete a post
  help - Show this help message";
    }
    
    private string ListPosts()
    {
        var task = GetPostsAsync();
        task.Wait();
        var posts = task.Result;
        
        if (posts.Count == 0)
        {
            return "No blog posts found.";
        }
        
        var lines = new List<string> { $"Found {posts.Count} blog posts:" };
        foreach (var post in posts.OrderByDescending(p => p.CreatedAt).Take(10))
        {
            lines.Add($"  [{post.Id}] {post.Title} by {post.Username} ({post.Category ?? "Uncategorized"}) - {post.CommentCount} comments");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string CreatePost(string userId, string username, string title)
    {
        var task = CreatePostAsync(userId, username, title, "Sample content", null);
        task.Wait();
        var result = task.Result;
        
        return result.success 
            ? $"Blog post created: {result.postId}" 
            : $"Failed to create post: {result.message}";
    }
    
    private string DeletePost(string postId)
    {
        var task = DeletePostAsync(postId, "admin");
        task.Wait();
        
        return task.Result 
            ? $"Blog post {postId} deleted." 
            : $"Failed to delete post {postId}.";
    }
    
    public async Task<List<BlogPost>> GetPostsAsync(int page = 1, int perPage = 20, string? category = null)
    {
        await Task.CompletedTask;
        
        var query = _posts.Values.Where(p => p.IsPublished);
        
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }
        
        return query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }
    
    public async Task<BlogPost?> GetPostByIdAsync(string postId)
    {
        await Task.CompletedTask;
        
        if (_posts.TryGetValue(postId, out var post))
        {
            post.ViewCount++;
            return post;
        }
        
        return null;
    }
    
    public async Task<(bool success, string message, string? postId)> CreatePostAsync(
        string userId, string username, string title, string content, string? category = null)
    {
        await Task.CompletedTask;
        
        // Content moderation check
        if (_moderationModule != null)
        {
            var titleResult = await _moderationModule.ScanTextAsync(title, userId, Name, "blog-title");
            if (titleResult.Action == moderationAction.Blocked || titleResult.Action == moderationAction.RequiresReview)
            {
                var reason = titleResult.Violations.Any() ? titleResult.Violations[0].Description : "Content policy violation";
                return (false, $"Title rejected: {reason}", null);
            }
            
            var contentResult = await _moderationModule.ScanTextAsync(content, userId, Name, "blog-content");
            if (contentResult.Action == moderationAction.Blocked || contentResult.Action == moderationAction.RequiresReview)
            {
                var reason = contentResult.Violations.Any() ? contentResult.Violations[0].Description : "Content policy violation";
                return (false, $"Content rejected: {reason}", null);
            }
        }
        
        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost
        {
            Id = postId,
            UserId = userId,
            Username = username,
            Title = title,
            Content = content,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            IsPublished = true,
            ViewCount = 0,
            CommentCount = 0
        };
        
        _posts[postId] = post;
        _postComments[postId] = new List<string>();
        
        Console.WriteLine($"[{Name}] Created blog post: {title} by {username}");
        return (true, "Post created successfully", postId);
    }
    
    public async Task<bool> UpdatePostAsync(string postId, string userId, string title, string content, string? category = null)
    {
        await Task.CompletedTask;
        
        if (!_posts.TryGetValue(postId, out var post))
        {
            return false;
        }
        
        if (post.UserId != userId)
        {
            return false; // Only the author can update
        }
        
        // Content moderation check
        if (_moderationModule != null)
        {
            var titleResult = await _moderationModule.ScanTextAsync(title, userId, Name, "blog-update-title");
            if (titleResult.Action == moderationAction.Blocked || titleResult.Action == moderationAction.RequiresReview)
            {
                return false;
            }
            
            var contentResult = await _moderationModule.ScanTextAsync(content, userId, Name, "blog-update-content");
            if (contentResult.Action == moderationAction.Blocked || contentResult.Action == moderationAction.RequiresReview)
            {
                return false;
            }
        }
        
        post.Title = title;
        post.Content = content;
        post.Category = category;
        post.UpdatedAt = DateTime.UtcNow;
        
        Console.WriteLine($"[{Name}] Updated blog post: {postId}");
        return true;
    }
    
    public async Task<bool> DeletePostAsync(string postId, string userId)
    {
        await Task.CompletedTask;
        
        if (!_posts.TryGetValue(postId, out var post))
        {
            return false;
        }
        
        if (post.UserId != userId)
        {
            return false; // Only the author can delete
        }
        
        _posts.TryRemove(postId, out _);
        
        // Delete associated comments
        if (_postComments.TryRemove(postId, out var commentIds))
        {
            foreach (var commentId in commentIds)
            {
                _comments.TryRemove(commentId, out _);
            }
        }
        
        Console.WriteLine($"[{Name}] Deleted blog post: {postId}");
        return true;
    }
    
    public async Task<List<BlogComment>> GetCommentsAsync(string postId)
    {
        await Task.CompletedTask;
        
        if (!_postComments.TryGetValue(postId, out var commentIds))
        {
            return new List<BlogComment>();
        }
        
        return commentIds
            .Select(id => _comments.TryGetValue(id, out var comment) ? comment : null)
            .Where(c => c != null)
            .Cast<BlogComment>()
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }
    
    public async Task<(bool success, string message, string? commentId)> AddCommentAsync(
        string postId, string userId, string username, string content)
    {
        await Task.CompletedTask;
        
        if (!_posts.TryGetValue(postId, out var post))
        {
            return (false, "Post not found", null);
        }
        
        // Content moderation check
        if (_moderationModule != null)
        {
            var result = await _moderationModule.ScanTextAsync(content, userId, Name, "blog-comment");
            if (result.Action == moderationAction.Blocked || result.Action == moderationAction.RequiresReview)
            {
                var reason = result.Violations.Any() ? result.Violations[0].Description : "Content policy violation";
                return (false, $"Comment rejected: {reason}", null);
            }
        }
        
        var commentId = Guid.NewGuid().ToString();
        var comment = new BlogComment
        {
            Id = commentId,
            PostId = postId,
            UserId = userId,
            Username = username,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        
        _comments[commentId] = comment;
        
        if (!_postComments.ContainsKey(postId))
        {
            _postComments[postId] = new List<string>();
        }
        _postComments[postId].Add(commentId);
        
        post.CommentCount++;
        
        Console.WriteLine($"[{Name}] Added comment to post {postId} by {username}");
        return (true, "Comment added successfully", commentId);
    }
    
    public async Task<bool> DeleteCommentAsync(string commentId, string userId)
    {
        await Task.CompletedTask;
        
        if (!_comments.TryGetValue(commentId, out var comment))
        {
            return false;
        }
        
        if (comment.UserId != userId)
        {
            return false; // Only the author can delete
        }
        
        _comments.TryRemove(commentId, out _);
        
        // Remove from post's comment list
        if (_postComments.TryGetValue(comment.PostId, out var commentIds))
        {
            commentIds.Remove(commentId);
            
            if (_posts.TryGetValue(comment.PostId, out var post))
            {
                post.CommentCount--;
            }
        }
        
        Console.WriteLine($"[{Name}] Deleted comment: {commentId}");
        return true;
    }
    
    public async Task<List<BlogCategory>> GetCategoriesAsync()
    {
        await Task.CompletedTask;
        
        var categories = _posts.Values
            .Where(p => !string.IsNullOrEmpty(p.Category))
            .GroupBy(p => p.Category)
            .Select(g => new BlogCategory
            {
                Name = g.Key!,
                Description = $"{g.Key} posts",
                PostCount = g.Count()
            })
            .OrderBy(c => c.Name)
            .ToList();
        
        return categories;
    }
    
    public async Task<List<BlogPost>> GetPostsByUserAsync(string userId)
    {
        await Task.CompletedTask;
        
        return _posts.Values
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }
    
    private void SeedExampleData()
    {
        var post1Id = Guid.NewGuid().ToString();
        var post1 = new BlogPost
        {
            Id = post1Id,
            UserId = "user1",
            Username = "ASHATAI",
            Title = "Welcome to ASHATCore Blogs",
            Content = "This is the first blog post on ASHATCore. Share your thoughts, stories, and ideas with the community!",
            Category = "Announcements",
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            IsPublished = true,
            ViewCount = 42,
            CommentCount = 3
        };
        _posts[post1Id] = post1;
        _postComments[post1Id] = new List<string>();
        
        var post2Id = Guid.NewGuid().ToString();
        var post2 = new BlogPost
        {
            Id = post2Id,
            UserId = "user2",
            Username = "TechUser",
            Title = "Getting Started with ASHATCore",
            Content = "A comprehensive guide to using ASHATCore's blogging features.",
            Category = "Tutorials",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            IsPublished = true,
            ViewCount = 28,
            CommentCount = 1
        };
        _posts[post2Id] = post2;
        _postComments[post2Id] = new List<string>();
        
        Console.WriteLine($"[{Name}] Seeded {_posts.Count} example blog posts");
    }
}
