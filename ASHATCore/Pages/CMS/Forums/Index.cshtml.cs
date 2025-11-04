using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Forums;

/// <summary>
/// Razor Page Model for Forums Index - Professional vBulletin-style forum interface
/// </summary>
public class IndexModel : PageModel
{
    public List<ForumCategory> Categories { get; set; } = new();
    public int TotalThreads { get; set; }
    public int TotalPosts { get; set; }
    public int TotalMembers { get; set; }
    public int OnlineUsers { get; set; }

    public void OnGet()
    {
        // Load forum statistics
        LoadStatistics();
        
        // Load forum categories and forums
        // This demonstrates the pure .NET architecture with Razor Pages
        Categories = LoadForumCategories();
    }

    private void LoadStatistics()
    {
        // In production, these would be retrieved from ForumModule via API
        TotalThreads = 0;
        TotalPosts = 0;
        TotalMembers = 0;
        OnlineUsers = 0;
    }

    private List<ForumCategory> LoadForumCategories()
    {
        // In production, this would call the ForumModule and LegendaryCMS API
        // Return empty list - data should be loaded via API calls in the UI
        return new List<ForumCategory>();
    }
}

public class ForumCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public List<ForumInfo> Forums { get; set; } = new();
}

public class ForumInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "💬";
    public int TopicCount { get; set; }
    public int PostCount { get; set; }
    public string LastPostUser { get; set; } = string.Empty;
    public DateTime LastPostDate { get; set; }
}
