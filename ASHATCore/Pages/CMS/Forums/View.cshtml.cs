using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Forums;

/// <summary>
/// Razor Page Model for Forum Thread List View
/// </summary>
public class ViewModel : PageModel
{
    public int ForumId { get; set; }
    public string ForumTitle { get; set; } = string.Empty;
    public string ForumDescription { get; set; } = string.Empty;
    public List<ThreadInfo> Threads { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public void OnGet(int id, int page = 1)
    {
        ForumId = id;
        CurrentPage = page;
        
        // Load forum details and threads
        LoadForumDetails(id);
        LoadThreads(id, page);
    }

    private void LoadForumDetails(int forumId)
    {
        // In production, this would load from ForumModule/API
        // For now, return sample data based on forum ID
        switch (forumId)
        {
            case 1:
                ForumTitle = "Announcements";
                ForumDescription = "Official announcements, news, and updates from the ASHAT Os team";
                break;
            case 2:
                ForumTitle = "General Chat";
                ForumDescription = "General discussion, introductions, and community chat";
                break;
            case 3:
                ForumTitle = "Feature Requests";
                ForumDescription = "Suggest new features and improvements for ASHAT Os";
                break;
            case 4:
                ForumTitle = "Installation & Setup";
                ForumDescription = "Help with installing and configuring ASHAT Os on Windows and Linux";
                break;
            case 5:
                ForumTitle = "Troubleshooting";
                ForumDescription = "Get help resolving issues and errors with your ASHAT Os installation";
                break;
            case 6:
                ForumTitle = "API & Integration";
                ForumDescription = "Questions about REST API, webhooks, and third-party integrations";
                break;
            case 7:
                ForumTitle = "Module Development";
                ForumDescription = "Discuss creating custom modules and plugins for ASHAT Os";
                break;
            case 8:
                ForumTitle = "Game Engine";
                ForumDescription = "Developing games with the Legendary Game Engine Suite";
                break;
            case 9:
                ForumTitle = "Code Snippets & Tutorials";
                ForumDescription = "Share code examples, tutorials, and development resources";
                break;
            case 10:
                ForumTitle = "Showcase";
                ForumDescription = "Show off your projects, websites, and creations built with ASHAT Os";
                break;
            case 11:
                ForumTitle = "Off-Topic";
                ForumDescription = "General off-topic discussions and casual conversation";
                break;
            default:
                ForumTitle = "Forum";
                ForumDescription = "Discussion forum";
                break;
        }
    }

    private void LoadThreads(int forumId, int page)
    {
        // In production, this would load from ForumModule/API with pagination
        // For now, return comprehensive sample data
        var allThreads = new List<ThreadInfo>
        {
            new ThreadInfo
            {
                Id = 1,
                Title = "Welcome to ASHAT Os Forums!",
                Author = "Admin",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                ReplyCount = 45,
                ViewCount = 1234,
                LastPostAuthor = "JohnDoe",
                LastPostDate = DateTime.UtcNow.AddHours(-2),
                IsSticky = true,
                Prefix = "Announcement"
            },
            new ThreadInfo
            {
                Id = 2,
                Title = "Forum Rules and Guidelines",
                Author = "Moderator",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                ReplyCount = 12,
                ViewCount = 856,
                LastPostAuthor = "Admin",
                LastPostDate = DateTime.UtcNow.AddDays(-5),
                IsSticky = true,
                IsLocked = true,
                Prefix = "Announcement"
            },
            new ThreadInfo
            {
                Id = 3,
                Title = "How to install ASHAT Os on Ubuntu 22.04?",
                Author = "NewUser123",
                CreatedDate = DateTime.UtcNow.AddHours(-12),
                ReplyCount = 8,
                ViewCount = 156,
                LastPostAuthor = "TechSupport",
                LastPostDate = DateTime.UtcNow.AddHours(-1),
                Prefix = "Question"
            },
            new ThreadInfo
            {
                Id = 4,
                Title = "LegendaryCMS API Documentation Request",
                Author = "Developer",
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                ReplyCount = 15,
                ViewCount = 342,
                LastPostAuthor = "Admin",
                LastPostDate = DateTime.UtcNow.AddHours(-8)
            },
            new ThreadInfo
            {
                Id = 5,
                Title = "Database migration issue - SOLVED",
                Author = "DBGuru",
                CreatedDate = DateTime.UtcNow.AddDays(-7),
                ReplyCount = 23,
                ViewCount = 567,
                LastPostAuthor = "Helper",
                LastPostDate = DateTime.UtcNow.AddDays(-2),
                Prefix = "Solved"
            },
            new ThreadInfo
            {
                Id = 6,
                Title = "Best practices for module development",
                Author = "ExpertDev",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ReplyCount = 34,
                ViewCount = 789,
                LastPostAuthor = "CodeMaster",
                LastPostDate = DateTime.UtcNow.AddHours(-5)
            },
            new ThreadInfo
            {
                Id = 7,
                Title = "Showcase: My first game built with ASHAT Os!",
                Author = "GameDev42",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                ReplyCount = 28,
                ViewCount = 612,
                LastPostAuthor = "Enthusiast",
                LastPostDate = DateTime.UtcNow.AddHours(-3)
            },
            new ThreadInfo
            {
                Id = 8,
                Title = "Performance optimization tips",
                Author = "PerfExpert",
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                ReplyCount = 19,
                ViewCount = 445,
                LastPostAuthor = "OptimizeKing",
                LastPostDate = DateTime.UtcNow.AddHours(-6)
            },
            new ThreadInfo
            {
                Id = 9,
                Title = "How to configure SSL certificates?",
                Author = "SecureAdmin",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                ReplyCount = 11,
                ViewCount = 234,
                LastPostAuthor = "SSLHelper",
                LastPostDate = DateTime.UtcNow.AddHours(-4),
                Prefix = "Question"
            },
            new ThreadInfo
            {
                Id = 10,
                Title = "Community project: Open source modules",
                Author = "OpenSourceFan",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                ReplyCount = 56,
                ViewCount = 1123,
                LastPostAuthor = "Contributor",
                LastPostDate = DateTime.UtcNow.AddMinutes(-30)
            }
        };

        // Simple pagination
        int perPage = 20;
        Threads = allThreads
            .OrderByDescending(t => t.IsSticky)
            .ThenByDescending(t => t.LastPostDate)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
        
        TotalPages = (int)Math.Ceiling(allThreads.Count / (double)perPage);
    }
}

public class ThreadInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int ReplyCount { get; set; }
    public int ViewCount { get; set; }
    public string LastPostAuthor { get; set; } = string.Empty;
    public DateTime LastPostDate { get; set; }
    public bool IsSticky { get; set; }
    public bool IsLocked { get; set; }
    public string Prefix { get; set; } = string.Empty;
}
