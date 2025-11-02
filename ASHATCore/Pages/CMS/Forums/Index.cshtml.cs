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
        TotalThreads = 127;
        TotalPosts = 856;
        TotalMembers = 245;
        OnlineUsers = 18;
    }

    private List<ForumCategory> LoadForumCategories()
    {
        // In production, this would call the ForumModule and LegendaryCMS API
        // For now, return comprehensive sample data to demonstrate vBulletin-style functionality
        return new List<ForumCategory>
        {
            new ForumCategory
            {
                Name = "General Discussion",
                Description = "General topics and discussions about ASHAT Os and the community",
                Icon = "💬",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 1,
                        Title = "Announcements",
                        Description = "Official announcements, news, and updates from the ASHAT Os team",
                        Icon = "📢",
                        TopicCount = 10,
                        PostCount = 45,
                        LastPostUser = "Admin",
                        LastPostDate = DateTime.UtcNow.AddHours(-2)
                    },
                    new ForumInfo
                    {
                        Id = 2,
                        Title = "General Chat",
                        Description = "General discussion, introductions, and community chat",
                        Icon = "💭",
                        TopicCount = 45,
                        PostCount = 312,
                        LastPostUser = "JohnDoe",
                        LastPostDate = DateTime.UtcNow.AddMinutes(-15)
                    },
                    new ForumInfo
                    {
                        Id = 3,
                        Title = "Feature Requests",
                        Description = "Suggest new features and improvements for ASHAT Os",
                        Icon = "💡",
                        TopicCount = 28,
                        PostCount = 156,
                        LastPostUser = "Developer",
                        LastPostDate = DateTime.UtcNow.AddHours(-5)
                    }
                }
            },
            new ForumCategory
            {
                Name = "Technical Support",
                Description = "Get help with ASHAT Os features, troubleshooting, and technical questions",
                Icon = "🔧",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 4,
                        Title = "Installation & Setup",
                        Description = "Help with installing and configuring ASHAT Os on Windows and Linux",
                        Icon = "⚙️",
                        TopicCount = 22,
                        PostCount = 118,
                        LastPostUser = "TechSupport",
                        LastPostDate = DateTime.UtcNow.AddHours(-3)
                    },
                    new ForumInfo
                    {
                        Id = 5,
                        Title = "Troubleshooting",
                        Description = "Get help resolving issues and errors with your ASHAT Os installation",
                        Icon = "🔍",
                        TopicCount = 18,
                        PostCount = 95,
                        LastPostUser = "Helper123",
                        LastPostDate = DateTime.UtcNow.AddHours(-1)
                    },
                    new ForumInfo
                    {
                        Id = 6,
                        Title = "API & Integration",
                        Description = "Questions about REST API, webhooks, and third-party integrations",
                        Icon = "🔌",
                        TopicCount = 12,
                        PostCount = 67,
                        LastPostUser = "DevGuru",
                        LastPostDate = DateTime.UtcNow.AddHours(-8)
                    }
                }
            },
            new ForumCategory
            {
                Name = "Development",
                Description = "For developers building with and extending ASHAT Os",
                Icon = "💻",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 7,
                        Title = "Module Development",
                        Description = "Discuss creating custom modules and plugins for ASHAT Os",
                        Icon = "🧩",
                        TopicCount = 16,
                        PostCount = 89,
                        LastPostUser = "ModDev",
                        LastPostDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new ForumInfo
                    {
                        Id = 8,
                        Title = "Game Engine",
                        Description = "Developing games with the Legendary Game Engine Suite",
                        Icon = "🎮",
                        TopicCount = 24,
                        PostCount = 134,
                        LastPostUser = "GameDev42",
                        LastPostDate = DateTime.UtcNow.AddHours(-4)
                    },
                    new ForumInfo
                    {
                        Id = 9,
                        Title = "Code Snippets & Tutorials",
                        Description = "Share code examples, tutorials, and development resources",
                        Icon = "📚",
                        TopicCount = 19,
                        PostCount = 72,
                        LastPostUser = "CodeMaster",
                        LastPostDate = DateTime.UtcNow.AddDays(-2)
                    }
                }
            },
            new ForumCategory
            {
                Name = "Community",
                Description = "Community events, projects, and off-topic discussions",
                Icon = "🌟",
                Forums = new List<ForumInfo>
                {
                    new ForumInfo
                    {
                        Id = 10,
                        Title = "Showcase",
                        Description = "Show off your projects, websites, and creations built with ASHAT Os",
                        Icon = "🎨",
                        TopicCount = 31,
                        PostCount = 187,
                        LastPostUser = "Artist",
                        LastPostDate = DateTime.UtcNow.AddHours(-6)
                    },
                    new ForumInfo
                    {
                        Id = 11,
                        Title = "Off-Topic",
                        Description = "General off-topic discussions and casual conversation",
                        Icon = "🎪",
                        TopicCount = 42,
                        PostCount = 298,
                        LastPostUser = "ChattyUser",
                        LastPostDate = DateTime.UtcNow.AddMinutes(-30)
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
