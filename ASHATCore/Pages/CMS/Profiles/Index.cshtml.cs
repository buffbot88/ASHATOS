using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Profiles;

/// <summary>
/// Razor Page Model for User Profiles - replaces legacy PHP profile templates
/// </summary>
public class IndexModel : PageModel
{
    public UserProfile? Profile { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();

    public void OnGet(string? username)
    {
        // Load user profile data
        // This demonstrates the pure .NET architecture with Razor Pages
        if (!string.IsNullOrEmpty(username))
        {
            Profile = LoadUserProfile(username);
            RecentActivity = LoadRecentActivity(username);
        }
    }

    private UserProfile? LoadUserProfile(string username)
    {
        // In production, this would call the LegendaryCMS API or UserProfiles module
        // For now, return sample data to demonstrate the page structure
        return new UserProfile
        {
            Username = username,
            Bio = "ASHATOS user - Welcome to the Legendary CMS community",
            PostCount = 42,
            JoinDate = DateTime.UtcNow.AddMonths(-6)
        };
    }

    private List<ActivityItem> LoadRecentActivity(string username)
    {
        // In production, this would fetch from the database via LegendaryCMS API
        return new List<ActivityItem>
        {
            new ActivityItem
            {
                Type = "Post",
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Posted in General Discussion"
            },
            new ActivityItem
            {
                Type = "Comment",
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Commented on 'Welcome to ASHATOS'"
            }
        };
    }
}

public class UserProfile
{
    public string Username { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public DateTime JoinDate { get; set; }
}

public class ActivityItem
{
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}
