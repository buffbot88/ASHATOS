using Microsoft.AspNetCore.Mvc.RazorPages;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Extensions.UserProfiles;
using Abstractions;

namespace ASHATCore.Pages.CMS.Profiles;

/// <summary>
/// Razor Page Model for User Profiles - MySpace-style social profiles
/// </summary>
public class IndexModel : PageModel
{
    public Abstractions.UserProfile? Profile { get; set; }
    public List<Abstractions.Activity> RecentActivity { get; set; } = new();
    public List<SocialPost> Posts { get; set; } = new();
    public List<string> Friends { get; set; } = new();

    public IndexModel()
    {
        // Try to get ModuleManager from HttpContext if available
        // In a production scenario, this would be injected via DI
    }

    public async Task OnGet(string? username)
    {
        // Default to Admin if no username provided
        if (string.IsNullOrEmpty(username))
        {
            username = "admin";
        }
        
        // Try to get the UserProfile module from ModuleManager
        var userProfileModule = GetUserProfileModule();
        
        if (userProfileModule != null)
        {
            Profile = await userProfileModule.GetProfileAsync(username);
            
            if (Profile != null)
            {
                RecentActivity = await userProfileModule.GetActivityFeedAsync(username, 10);
                Posts = await userProfileModule.GetSocialPostsAsync(username);
                Friends = await userProfileModule.GetFriendsAsync(username);
            }
        }
        
        // Fallback to sample data if profile not found
        if (Profile == null)
        {
            Profile = new Abstractions.UserProfile
            {
                UserId = username,
                DisplayName = username,
                Bio = "ASHATOS user - Welcome to the Legendary CMS community",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                LastActiveAt = DateTime.UtcNow,
                Role = "User"
            };
        }
    }

    private UserProfileModule? GetUserProfileModule()
    {
        try
        {
            // Try to get from HttpContext if available
            if (HttpContext?.RequestServices != null)
            {
                var manager = HttpContext.RequestServices.GetService(typeof(ModuleManager)) as ModuleManager;
                if (manager != null)
                {
                    return manager.Modules
                        .Select(m => m.Instance)
                        .OfType<UserProfileModule>()
                        .FirstOrDefault();
                }
            }
        }
        catch
        {
            // Ignore errors, will use fallback data
        }
        
        return null;
    }
}
