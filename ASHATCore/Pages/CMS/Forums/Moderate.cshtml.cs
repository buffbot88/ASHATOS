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
        TotalThreads = 0;
        TotalPosts = 0;
        DeletedPostsCount = 0;
        LockedThreadsCount = 0;
        BannedUsers = 0;
        ActiveWarnings = 0;
    }

    private void LoadFlaggedPosts()
    {
        // In production, this would load from ForumModule/API
        FlaggedPosts = new List<FlaggedPost>();
    }

    private void LoadLockedThreads()
    {
        // In production, this would load from ForumModule/API
        LockedThreadsList = new List<LockedThread>();
    }

    private void LoadBannedUsers()
    {
        // In production, this would load from ForumModule/API
        BannedUsersList = new List<BannedUser>();
    }

    private void LoadDeletedPosts()
    {
        // In production, this would load from ForumModule/API
        DeletedPostsList = new List<DeletedPost>();
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
