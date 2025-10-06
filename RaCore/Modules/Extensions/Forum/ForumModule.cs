using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Forum;

[RaModule(Category = "extensions")]
public sealed class ForumModule : ModuleBase, IForumModule
{
    public override string Name => "Forum";
    
    private readonly ConcurrentDictionary<string, ForumPost> _posts = new();
    private readonly ConcurrentDictionary<string, List<ForumWarning>> _userWarnings = new();
    private readonly ConcurrentDictionary<string, BanRecord> _bannedUsers = new();
    private ModuleManager? _manager;
    private IContentModerationModule? _moderationModule;
    
    public override void Initialize(object? manager)
    {
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _moderationModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IContentModerationModule>()
                .FirstOrDefault();
        }
        
        Console.WriteLine($"[{Name}] Initializing Forum Module...");
        Console.WriteLine($"[{Name}] Content moderation: {(_moderationModule != null ? "enabled" : "disabled")}");
        
        // Seed some example data
        SeedExampleData();
        
        Console.WriteLine($"[{Name}] Forum Module initialized with {_posts.Count} posts");
    }
    
    public override string Process(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return GetHelp();
        
        return parts[0].ToLower() switch
        {
            "help" => GetHelp(),
            "stats" => GetStatsSync(),
            "posts" => ListPosts(),
            "delete" => parts.Length > 1 ? DeletePost(parts[1]) : "Usage: forum delete <postId>",
            "lock" => parts.Length > 1 ? LockPost(parts[1]) : "Usage: forum lock <postId>",
            "unlock" => parts.Length > 1 ? UnlockPost(parts[1]) : "Usage: forum unlock <postId>",
            "warn" => parts.Length > 2 ? WarnUser(parts[1], string.Join(" ", parts.Skip(2))) : "Usage: forum warn <userId> <reason>",
            "ban" => parts.Length > 2 ? BanUser(parts[1], string.Join(" ", parts.Skip(2))) : "Usage: forum ban <userId> <reason>",
            "unban" => parts.Length > 1 ? UnbanUser(parts[1]) : "Usage: forum unban <userId>",
            _ => $"Unknown command: {parts[0]}\n{GetHelp()}"
        };
    }
    
    private string GetHelp()
    {
        return @"Forum Module Commands:
  forum stats              - Show forum statistics
  forum posts              - List all posts
  forum delete <postId>    - Delete a post
  forum lock <postId>      - Lock a thread
  forum unlock <postId>    - Unlock a thread
  forum warn <userId> <reason> - Issue warning to user
  forum ban <userId> <reason>  - Ban a user
  forum unban <userId>     - Unban a user
  forum help               - Show this help";
    }
    
    public async Task<List<ForumPost>> GetPostsAsync(int page = 1, int perPage = 20, bool includeDeleted = false)
    {
        var query = _posts.Values.AsEnumerable();
        
        if (!includeDeleted)
            query = query.Where(p => !p.IsDeleted);
        
        return await Task.FromResult(query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList());
    }
    
    public async Task<(bool success, string message, string? postId)> CreatePostAsync(string userId, string username, string title, string content)
    {
        // Check if user is suspended
        if (_moderationModule != null)
        {
            var isSuspended = await _moderationModule.IsUserSuspendedAsync(userId);
            if (isSuspended)
            {
                var suspension = await _moderationModule.GetActiveSuspensionAsync(userId);
                return (false, $"Cannot post: User is suspended until {suspension?.ExpiresAt?.ToString() ?? "indefinitely"}", null);
            }
        }
        
        // Check if user is banned
        if (_bannedUsers.ContainsKey(userId))
        {
            return (false, "Cannot post: User is banned from the forum", null);
        }
        
        // Moderate content before accepting
        if (_moderationModule != null)
        {
            var postId = Guid.NewGuid().ToString();
            var moderationResult = await _moderationModule.ScanTextAsync($"{title}\n{content}", userId, Name, postId);
            
            if (moderationResult.Action == ModerationAction.Blocked || moderationResult.Action == ModerationAction.AutoSuspended)
            {
                var violations = string.Join(", ", moderationResult.Violations.Select(v => v.Type));
                return (false, $"Post blocked due to content violations: {violations}", null);
            }
            
            if (moderationResult.Action == ModerationAction.Flagged)
            {
                Console.WriteLine($"[{Name}] Post {postId} flagged for review");
            }
            
            // Create the post
            var post = new ForumPost
            {
                Id = postId,
                ThreadId = postId, // New thread
                UserId = userId,
                Username = username,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ReplyCount = 0,
                ViewCount = 0
            };
            
            _posts[postId] = post;
            Console.WriteLine($"[{Name}] Post created by {username}: {title}");
            
            return (true, "Post created successfully", postId);
        }
        else
        {
            // No moderation module - create post without scanning
            var postId = Guid.NewGuid().ToString();
            var post = new ForumPost
            {
                Id = postId,
                ThreadId = postId,
                UserId = userId,
                Username = username,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ReplyCount = 0,
                ViewCount = 0
            };
            
            _posts[postId] = post;
            Console.WriteLine($"[{Name}] Post created by {username}: {title} (no moderation)");
            
            return (true, "Post created successfully", postId);
        }
    }
    
    public async Task<bool> DeletePostAsync(string postId, string moderatorId, string reason)
    {
        if (_posts.TryGetValue(postId, out var post))
        {
            post.IsDeleted = true;
            post.DeletedBy = moderatorId;
            post.DeleteReason = reason;
            Console.WriteLine($"[{Name}] Post {postId} deleted by {moderatorId}: {reason}");
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }
    
    public async Task<bool> LockThreadAsync(string postId, bool locked, string moderatorId)
    {
        if (_posts.TryGetValue(postId, out var post))
        {
            post.IsLocked = locked;
            Console.WriteLine($"[{Name}] Thread {postId} {(locked ? "locked" : "unlocked")} by {moderatorId}");
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }
    
    public async Task<List<ForumWarning>> GetUserWarningsAsync(string userId)
    {
        if (_userWarnings.TryGetValue(userId, out var warnings))
            return await Task.FromResult(warnings.Where(w => w.IsActive).ToList());
        
        return await Task.FromResult(new List<ForumWarning>());
    }
    
    public async Task<bool> IssueWarningAsync(string userId, string reason, string moderatorId)
    {
        var warning = new ForumWarning
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Reason = reason,
            IssuedBy = moderatorId,
            IssuedAt = DateTime.UtcNow
        };
        
        _userWarnings.AddOrUpdate(userId, 
            new List<ForumWarning> { warning },
            (key, list) => { list.Add(warning); return list; });
        
        Console.WriteLine($"[{Name}] Warning issued to user {userId} by {moderatorId}: {reason}");
        
        // Auto-ban after 3 warnings
        var activeWarnings = _userWarnings[userId].Count(w => w.IsActive);
        if (activeWarnings >= 3)
        {
            await BanUserAsync(userId, true, "Automatic ban: 3 warnings received", "System");
        }
        
        return await Task.FromResult(true);
    }
    
    public async Task<bool> BanUserAsync(string userId, bool banned, string reason, string moderatorId)
    {
        if (banned)
        {
            var ban = new BanRecord
            {
                UserId = userId,
                Reason = reason,
                BannedBy = moderatorId,
                BannedAt = DateTime.UtcNow
            };
            _bannedUsers[userId] = ban;
            Console.WriteLine($"[{Name}] User {userId} banned by {moderatorId}: {reason}");
        }
        else
        {
            _bannedUsers.TryRemove(userId, out _);
            Console.WriteLine($"[{Name}] User {userId} unbanned by {moderatorId}");
        }
        
        return await Task.FromResult(true);
    }
    
    public async Task<bool> IsUserBannedAsync(string userId)
    {
        return await Task.FromResult(_bannedUsers.ContainsKey(userId));
    }
    
    public async Task<ForumStats> GetStatsAsync()
    {
        var stats = new ForumStats
        {
            TotalPosts = _posts.Count,
            ActiveThreads = _posts.Values.Count(p => !p.IsDeleted && !p.IsLocked),
            DeletedPosts = _posts.Values.Count(p => p.IsDeleted),
            LockedThreads = _posts.Values.Count(p => p.IsLocked),
            BannedUsers = _bannedUsers.Count,
            TotalWarnings = _userWarnings.Values.Sum(w => w.Count(x => x.IsActive))
        };
        
        return await Task.FromResult(stats);
    }
    
    private string GetStatsSync()
    {
        var stats = GetStatsAsync().Result;
        return $@"Forum Statistics:
  Total Posts: {stats.TotalPosts}
  Active Threads: {stats.ActiveThreads}
  Deleted Posts: {stats.DeletedPosts}
  Locked Threads: {stats.LockedThreads}
  Banned Users: {stats.BannedUsers}
  Active Warnings: {stats.TotalWarnings}";
    }
    
    private string ListPosts()
    {
        var posts = GetPostsAsync(1, 10).Result;
        if (posts.Count == 0) return "No posts found.";
        
        var result = "Recent Posts:\n";
        foreach (var post in posts)
        {
            var status = post.IsDeleted ? " [DELETED]" : (post.IsLocked ? " [LOCKED]" : "");
            result += $"  [{post.Id}] {post.Title} by {post.Username} ({post.ReplyCount} replies){status}\n";
        }
        return result;
    }
    
    private string DeletePost(string postId)
    {
        var result = DeletePostAsync(postId, "console", "Deleted via console").Result;
        return result ? $"Post {postId} deleted." : $"Post {postId} not found.";
    }
    
    private string LockPost(string postId)
    {
        var result = LockThreadAsync(postId, true, "console").Result;
        return result ? $"Thread {postId} locked." : $"Post {postId} not found.";
    }
    
    private string UnlockPost(string postId)
    {
        var result = LockThreadAsync(postId, false, "console").Result;
        return result ? $"Thread {postId} unlocked." : $"Post {postId} not found.";
    }
    
    private string WarnUser(string userId, string reason)
    {
        IssueWarningAsync(userId, reason, "console").Wait();
        var warnings = GetUserWarningsAsync(userId).Result;
        return $"Warning issued to user {userId}. Total warnings: {warnings.Count}";
    }
    
    private string BanUser(string userId, string reason)
    {
        BanUserAsync(userId, true, reason, "console").Wait();
        return $"User {userId} has been banned.";
    }
    
    private string UnbanUser(string userId)
    {
        BanUserAsync(userId, false, "", "console").Wait();
        return $"User {userId} has been unbanned.";
    }
    
    private void SeedExampleData()
    {
        // Create some example posts
        var posts = new[]
        {
            new ForumPost
            {
                Id = "post_1",
                ThreadId = "thread_1",
                UserId = "user_1",
                Username = "JohnDoe",
                Title = "Welcome to RaCore Forums!",
                Content = "This is the first post on our new forum system.",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ReplyCount = 15,
                ViewCount = 234
            },
            new ForumPost
            {
                Id = "post_2",
                ThreadId = "thread_2",
                UserId = "user_2",
                Username = "JaneSmith",
                Title = "How to use AI Content Generation",
                Content = "Tutorial on using the AI content generation system.",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ReplyCount = 8,
                ViewCount = 156
            },
            new ForumPost
            {
                Id = "post_3",
                ThreadId = "thread_3",
                UserId = "user_3",
                Username = "GameMaster",
                Title = "Server Maintenance Schedule",
                Content = "Scheduled maintenance for this weekend.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ReplyCount = 3,
                ViewCount = 89,
                IsLocked = true
            }
        };
        
        foreach (var post in posts)
        {
            _posts[post.Id] = post;
        }
    }
    
    private class BanRecord
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string BannedBy { get; set; } = string.Empty;
        public DateTime BannedAt { get; set; }
    }
}
