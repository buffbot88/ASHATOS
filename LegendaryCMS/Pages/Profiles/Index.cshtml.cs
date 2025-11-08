using Microsoft.AspNetCore.Mvc.RazorPages;
using Abstractions;

namespace LegendaryCMS.Pages.Profiles;

/// <summary>
/// Razor Page Model for User Profiles - MySpace-style social profiles
/// TODO: Integrate with mainframe API for profile data in standalone mode
/// </summary>
public class IndexModel : PageModel
{
    private const string DefaultUsername = "admin";
    private const string DefaultBio = "ASHATOS user - Welcome to the Legendary CMS community";
    private const string DefaultRole = "User";
    private const int DefaultJoinMonthsAgo = 6;
    
    public Abstractions.UserProfile? Profile { get; set; }
    public List<Abstractions.Activity> RecentActivity { get; set; } = new();
    public List<SocialPost> Posts { get; set; } = new();
    public List<string> Friends { get; set; } = new();

    public IndexModel()
    {
        // Constructor - dependencies can be injected here in standalone mode
    }

    public async Task OnGet(string? username)
    {
        // Default to Admin if no username provided
        if (string.IsNullOrEmpty(username))
        {
            username = DefaultUsername;
        }
        
        // TODO: In standalone mode, fetch profile from mainframe API
        // For now, use sample data
        Profile = new Abstractions.UserProfile
        {
            UserId = username,
            DisplayName = username,
            Bio = DefaultBio,
            CreatedAt = DateTime.UtcNow.AddMonths(-DefaultJoinMonthsAgo),
            LastActiveAt = DateTime.UtcNow,
            Role = DefaultRole
        };
        
        // TODO: Fetch activity, posts, and friends from mainframe API
        RecentActivity = new List<Abstractions.Activity>();
        Posts = new List<SocialPost>();
        Friends = new List<string>();
        
        await Task.CompletedTask;
    }
}
