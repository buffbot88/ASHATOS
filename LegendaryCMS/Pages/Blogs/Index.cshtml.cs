using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LegendaryCMS.Pages.Blogs;

/// <summary>
/// Razor Page Model for Blogs Index - replaces legacy PHP blog templates
/// </summary>
public class IndexModel : PageModel
{
    public List<BlogPost> RecentPosts { get; set; } = new();

    public void OnGet()
    {
        // Load recent blog posts
        // This demonstrates the pure .NET architecture with Razor Pages
        RecentPosts = LoadRecentBlogPosts();
    }

    private List<BlogPost> LoadRecentBlogPosts()
    {
        // In production, this would call the LegendaryCMS API
        // For now, return sample data to demonstrate the page structure
        return new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "Welcome to ASHATOS",
                Author = "Admin",
                PublishedDate = DateTime.UtcNow,
                Excerpt = "Welcome to the new pure .NET CMS powered by LegendaryCMS. This platform provides a powerful, modular architecture for content management."
            },
            new BlogPost
            {
                Id = 2,
                Title = "Getting Started with Razor Pages",
                Author = "Developer",
                PublishedDate = DateTime.UtcNow.AddDays(-3),
                Excerpt = "Learn how to create dynamic web pages using Razor Pages and the LegendaryCMS framework. This guide covers the basics of page models and routing."
            },
            new BlogPost
            {
                Id = 3,
                Title = "Migrating from PHP to .NET",
                Author = "Tech Lead",
                PublishedDate = DateTime.UtcNow.AddDays(-7),
                Excerpt = "Discover the benefits of migrating from legacy PHP systems to modern .NET architecture. We cover performance improvements, security enhancements, and development workflow."
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
    public string Content { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
}
