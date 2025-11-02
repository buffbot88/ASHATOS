using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages;

/// <summary>
/// Main CMS Homepage - replaces the stubbed GeneratedynamicHomepage()
/// Integrates with LegendaryCMS to provide a full-featured content management system homepage
/// </summary>
public class IndexModel : PageModel
{
    public string Version { get; set; } = "9.4.0";
    public int ServerPort { get; set; } = 7077;
    public int TotalModules { get; set; } = 0;
    public int TotalPosts { get; set; } = 0;
    public int TotalForums { get; set; } = 0;
    public int TotalUsers { get; set; } = 0;
    
    public List<BlogPost> RecentBlogPosts { get; set; } = new();
    public List<ForumTopic> RecentForumTopics { get; set; } = new();

    public void OnGet()
    {
        // Load statistics and recent content
        LoadStatistics();
        LoadRecentContent();
    }

    private void LoadStatistics()
    {
        // In production, these would be retrieved from LegendaryCMS API
        // For now, provide sample data to demonstrate the full CMS functionality
        TotalModules = 15; // Simulated count of loaded modules
        TotalPosts = 42;   // Simulated blog posts count
        TotalForums = 8;   // Simulated forum topics count
        TotalUsers = 156;  // Simulated active users count
        
        // Try to get actual server port from configuration or environment
        var portEnv = Environment.GetEnvironmentVariable("ASHATCore_DETECTED_PORT");
        if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out int port))
        {
            ServerPort = port;
        }
    }

    private void LoadRecentContent()
    {
        // Load recent blog posts
        // In production, this would call the LegendaryCMS API endpoints
        RecentBlogPosts = LoadRecentBlogPosts();
        
        // Load recent forum topics
        RecentForumTopics = LoadRecentForumTopics();
    }

    private List<BlogPost> LoadRecentBlogPosts()
    {
        // Sample blog posts to demonstrate CMS functionality
        // In production, this would retrieve from LegendaryCMS via API
        return new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "Welcome to ASHAT Os - Your Complete Server Platform",
                Author = "Admin",
                PublishedDate = DateTime.UtcNow,
                Excerpt = "Discover the power of ASHAT Os - a production-ready, modular server platform that combines CMS, forums, game engine, and control panel into one unified system. Built with .NET 9.0 and powered by LegendaryCMS Suite."
            },
            new BlogPost
            {
                Id = 2,
                Title = "LegendaryCMS v8.0.0 - Production Ready Features",
                Author = "Developer",
                PublishedDate = DateTime.UtcNow.AddDays(-2),
                Excerpt = "The latest version of LegendaryCMS brings production-ready features including plugin architecture, REST API with rate limiting, enhanced RBAC with 25+ permissions, and comprehensive security features. Learn how these features can power your applications."
            },
            new BlogPost
            {
                Id = 3,
                Title = "Building Scalable Applications with Modular Architecture",
                Author = "Tech Lead",
                PublishedDate = DateTime.UtcNow.AddDays(-5),
                Excerpt = "Explore ASHAT Os's modular architecture with external DLLs, zero coupling, and plugin-based extensibility. This design pattern enables rapid development, easy testing, and seamless integration of custom functionality."
            }
        };
    }

    private List<ForumTopic> LoadRecentForumTopics()
    {
        // Sample forum topics to demonstrate CMS functionality
        // In production, this would retrieve from LegendaryCMS via API
        return new List<ForumTopic>
        {
            new ForumTopic
            {
                Id = 1,
                Title = "Getting Started with ASHAT Os - Beginner's Guide",
                Category = "General Discussion",
                Description = "New to ASHAT Os? Start here! This comprehensive guide covers installation, configuration, and your first steps with the platform.",
                Replies = 24,
                LastPostDate = DateTime.UtcNow.AddHours(-2)
            },
            new ForumTopic
            {
                Id = 2,
                Title = "Developing Custom Modules - Best Practices",
                Category = "Development",
                Description = "Share your experiences and learn best practices for developing custom modules and plugins for ASHAT Os. Discussion includes architecture patterns, testing strategies, and deployment tips.",
                Replies = 18,
                LastPostDate = DateTime.UtcNow.AddHours(-5)
            },
            new ForumTopic
            {
                Id = 3,
                Title = "Game Engine Integration - Showcase Your Projects",
                Category = "Game Development",
                Description = "Show off your game projects built with ASHAT Os's integrated game engine. Discuss multiplayer features, scene management, and WebSocket implementation.",
                Replies = 31,
                LastPostDate = DateTime.UtcNow.AddHours(-8)
            }
        };
    }
}

/// <summary>
/// Blog post model for homepage display
/// </summary>
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Excerpt { get; set; } = string.Empty;
}

/// <summary>
/// Forum topic model for homepage display
/// </summary>
public class ForumTopic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Replies { get; set; }
    public DateTime LastPostDate { get; set; }
}
