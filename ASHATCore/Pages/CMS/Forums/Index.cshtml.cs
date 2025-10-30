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
        // TODO: Load from LegendaryCMS API
        // This demonstrates the pure .NET architecture
        Categories = new List<ForumCategory>
        {
            new ForumCategory
            {
                Name = "General Discussion",
                Description = "General topics and discussions",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 1,
                        Title = "Announcements",
                        Description = "Official announcements and news",
                        TopicCount = 10,
                        PostCount = 45
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
