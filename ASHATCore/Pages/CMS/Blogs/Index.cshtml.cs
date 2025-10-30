using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Blogs;

/// <summary>
/// Razor Page Model for Blogs Index - replaces legacy PHP blog templates
/// </summary>
public class IndexModel : PageModel
{
    public List<BlogPost> RecentPosts { get; set; } = new();

    public void OnGet()
    {
        // TODO: Load from LegendaryCMS API
        // This demonstrates the pure .NET architecture
        RecentPosts = new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "Welcome to ASHATOS",
                Author = "Admin",
                PublishedDate = DateTime.UtcNow,
                Excerpt = "Welcome to the new pure .NET CMS powered by LegendaryCMS..."
            }
        };
    }
}

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Excerpt { get; set; } = string.Empty;
}
