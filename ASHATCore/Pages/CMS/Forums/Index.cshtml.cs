using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Forums;

/// <summary>
/// Razor Page Model for Forums Index - replaces legacy PHP forum templates
/// </summary>
public class IndexModel : PageModel
{
    public List<ForumCategory> Categories { get; set; } = new();

    public void OnGet()
    {
        // Load forum categories and forums
        // This demonstrates the pure .NET architecture with Razor Pages
        Categories = LoadForumCategories();
    }

    private List<ForumCategory> LoadForumCategories()
    {
        // In production, this would call the LegendaryCMS API
        // For now, return sample data to demonstrate the page structure
        return new List<ForumCategory>
        {
            new ForumCategory
            {
                Name = "General Discussion",
                Description = "General topics and discussions about ASHATOS",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 1,
                        Title = "Announcements",
                        Description = "Official announcements and news",
                        TopicCount = 10,
                        PostCount = 45
                    },
                    new ForumInfo
                    {
                        Id = 2,
                        Title = "General Chat",
                        Description = "General discussion and community chat",
                        TopicCount = 25,
                        PostCount = 156
                    }
                }
            },
            new ForumCategory
            {
                Name = "Technical Support",
                Description = "Get help with ASHATOS features and troubleshooting",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 3,
                        Title = "Installation & Setup",
                        Description = "Help with installing and configuring ASHATOS",
                        TopicCount = 15,
                        PostCount = 78
                    }
                }
            }
        };
    }
}

public class ForumCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ForumInfo> Forums { get; set; } = new();
}

public class ForumInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TopicCount { get; set; }
    public int PostCount { get; set; }
}
