using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Forums;

/// <summary>
/// Razor Page Model for Forum Moderation Panel
/// </summary>
public class ModerateModel : PageModel
{
    public int TotalThreads { get; set; }
    public int TotalPosts { get; set; }
    public int DeletedPostsCount { get; set; }
    public int LockedThreadsCount { get; set; }
    public int BannedUsers { get; set; }
    public int ActiveWarnings { get; set; }
    
    public List<FlaggedPost> FlaggedPosts { get; set; } = new();
    public List<LockedThread> LockedThreadsList { get; set; } = new();
    public List<BannedUser> BannedUsersList { get; set; } = new();
    public List<DeletedPost> DeletedPostsList { get; set; } = new();

    public void OnGet()
    {
        // Load moderation statistics
        LoadStatistics();
        
        // Load moderation queues
        LoadFlaggedPosts();
        LoadLockedThreads();
        LoadBannedUsers();
        LoadDeletedPosts();
    }

    private void LoadStatistics()
    {
        // In production, these would be retrieved from ForumModule/API
        TotalThreads = 127;
        TotalPosts = 856;
        DeletedPostsCount = 23;
        LockedThreadsCount = 8;
        BannedUsers = 5;
        ActiveWarnings = 12;
    }

    private void LoadFlaggedPosts()
    {
        // In production, this would load from ForumModule/API
        FlaggedPosts = new List<FlaggedPost>
        {
            new FlaggedPost
            {
                Id = 1,
                ThreadId = 5,
                Title = "Inappropriate content in thread",
                Author = "TrollUser",
                Date = DateTime.UtcNow.AddHours(-2),
                FlagReason = "Spam / Commercial advertising"
            },
            new FlaggedPost
            {
                Id = 2,
                ThreadId = 12,
                Title = "Request for help with installation",
                Author = "NewUser123",
                Date = DateTime.UtcNow.AddHours(-5),
                FlagReason = "Potentially harmful content"
            },
            new FlaggedPost
            {
                Id = 3,
                ThreadId = 8,
                Title = "Feature suggestion for next version",
                Author = "Enthusiast",
                Date = DateTime.UtcNow.AddHours(-8),
                FlagReason = "Off-topic / Wrong category"
            }
        };
    }

    private void LoadLockedThreads()
    {
        // In production, this would load from ForumModule/API
        LockedThreadsList = new List<LockedThread>
        {
            new LockedThread
            {
                Id = 1,
                Title = "Heated debate about programming languages",
                Author = "Developer",
                LockedBy = "Moderator",
                LockReason = "Thread became too heated and off-topic"
            },
            new LockedThread
            {
                Id = 2,
                Title = "Forum Rules and Guidelines",
                Author = "Admin",
                LockedBy = "Admin",
                LockReason = "Official announcement - no replies needed"
            },
            new LockedThread
            {
                Id = 3,
                Title = "Resolved: Database migration issue",
                Author = "DBGuru",
                LockedBy = "Moderator",
                LockReason = "Issue resolved, preventing necroposting"
            }
        };
    }

    private void LoadBannedUsers()
    {
        // In production, this would load from ForumModule/API
        BannedUsersList = new List<BannedUser>
        {
            new BannedUser
            {
                UserId = "user_123",
                Username = "SpamBot",
                BannedBy = "Admin",
                BanDate = DateTime.UtcNow.AddDays(-1),
                Reason = "Repeated spam violations"
            },
            new BannedUser
            {
                UserId = "user_456",
                Username = "ToxicUser",
                BannedBy = "Moderator",
                BanDate = DateTime.UtcNow.AddDays(-3),
                Reason = "Harassment and abusive behavior"
            },
            new BannedUser
            {
                UserId = "user_789",
                Username = "RuleBreaker",
                BannedBy = "Moderator",
                BanDate = DateTime.UtcNow.AddDays(-7),
                Reason = "Multiple forum rule violations"
            }
        };
    }

    private void LoadDeletedPosts()
    {
        // In production, this would load from ForumModule/API
        DeletedPostsList = new List<DeletedPost>
        {
            new DeletedPost
            {
                Id = 1,
                ThreadId = 5,
                Title = "Off-topic spam post",
                Author = "SpamUser",
                DeletedBy = "Moderator",
                DeleteReason = "Spam / Commercial advertising"
            },
            new DeletedPost
            {
                Id = 2,
                ThreadId = 8,
                Title = "Inappropriate language",
                Author = "AngryUser",
                DeletedBy = "Admin",
                DeleteReason = "Violation of community guidelines"
            },
            new DeletedPost
            {
                Id = 3,
                ThreadId = 15,
                Title = "Duplicate post",
                Author = "NewUser",
                DeletedBy = "Moderator",
                DeleteReason = "Duplicate content"
            }
        };
    }
}

public class FlaggedPost
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string FlagReason { get; set; } = string.Empty;
}

public class LockedThread
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string LockedBy { get; set; } = string.Empty;
    public string LockReason { get; set; } = string.Empty;
}

public class BannedUser
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string BannedBy { get; set; } = string.Empty;
    public DateTime BanDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DeletedPost
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DeletedBy { get; set; } = string.Empty;
    public string DeleteReason { get; set; } = string.Empty;
}
