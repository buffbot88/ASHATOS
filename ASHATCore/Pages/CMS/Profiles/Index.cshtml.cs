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
        // TODO: Load from LegendaryCMS API or UserProfiles module
        // This demonstrates the pure .NET architecture
        if (!string.IsNullOrEmpty(username))
        {
            Profile = new UserProfile
            {
                Username = username,
                Bio = "ASHATOS user",
                PostCount = 42,
                JoinDate = DateTime.UtcNow.AddMonths(-6)
            };
            
            RecentActivity = new List<ActivityItem>
            {
                new ActivityItem
                {
                    Type = "Post",
                    Date = DateTime.UtcNow.AddDays(-1),
                    Description = "Posted in General Discussion"
                }
            };
        }
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
