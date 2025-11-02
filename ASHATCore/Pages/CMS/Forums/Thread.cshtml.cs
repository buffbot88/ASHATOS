using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Forums;

/// <summary>
/// Razor Page Model for Thread View with Posts
/// </summary>
public class ThreadModel : PageModel
{
    public int ThreadId { get; set; }
    public int ForumId { get; set; }
    public string ForumName { get; set; } = string.Empty;
    public string ThreadTitle { get; set; } = string.Empty;
    public string ThreadAuthor { get; set; } = string.Empty;
    public DateTime ThreadCreatedDate { get; set; }
    public int ViewCount { get; set; }
    public int ReplyCount { get; set; }
    public List<PostInfo> Posts { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public void OnGet(int id, int page = 1)
    {
        ThreadId = id;
        CurrentPage = page;
        
        // Load thread details and posts
        LoadThreadDetails(id);
        LoadPosts(id, page);
    }

    private void LoadThreadDetails(int threadId)
    {
        // In production, this would load from ForumModule/API
        // For now, return sample data based on thread ID
        ThreadTitle = "Welcome to ASHAT Os Forums!";
        ThreadAuthor = "Admin";
        ThreadCreatedDate = DateTime.UtcNow.AddDays(-30);
        ViewCount = 1234;
        ReplyCount = 45;
        ForumId = 1;
        ForumName = "Announcements";
    }

    private void LoadPosts(int threadId, int page)
    {
        // In production, this would load from ForumModule/API with pagination
        // For now, return comprehensive sample data
        var allPosts = new List<PostInfo>
        {
            new PostInfo
            {
                Id = 1,
                Author = "Admin",
                PostDate = DateTime.UtcNow.AddDays(-30),
                Content = "Welcome to the official ASHAT Os community forums!\n\nWe're excited to have you here. This forum system features:\n\n• Professional vBulletin-inspired design\n• Advanced moderation tools\n• Content filtering and parental controls\n• User reputation system\n• Thread subscriptions\n• And much more!\n\nFeel free to explore, ask questions, share your projects, and connect with other ASHAT Os users. Our community is here to help!\n\nPlease read our forum rules and guidelines before posting. Enjoy your stay!",
                UserTitle = "Administrator",
                PostCount = 2847,
                JoinDate = DateTime.UtcNow.AddYears(-2),
                Location = "Worldwide",
                Signature = "AGP Studios, INC - Building the future of web platforms",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 2,
                Author = "JohnDoe",
                PostDate = DateTime.UtcNow.AddDays(-29),
                Content = "Thank you for the warm welcome! I'm excited to be part of this community.\n\nI've been exploring ASHAT Os for a few weeks now and I'm impressed by its capabilities. The modular architecture makes it so easy to extend and customize.\n\nLooking forward to learning from everyone here!",
                UserTitle = "Member",
                PostCount = 156,
                JoinDate = DateTime.UtcNow.AddMonths(-3),
                Location = "United States",
                Signature = "Happy coding! 💻",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 3,
                Author = "TechEnthusiast",
                PostDate = DateTime.UtcNow.AddDays(-28),
                Content = "Great to see such an active community forming around ASHAT Os!\n\nI have a question about module development - is there a guide or tutorial available? I'd like to create a custom authentication module for my project.",
                UserTitle = "Active Member",
                PostCount = 342,
                JoinDate = DateTime.UtcNow.AddMonths(-6),
                Location = "Canada",
                Signature = "",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 4,
                Author = "Developer",
                PostDate = DateTime.UtcNow.AddDays(-27),
                Content = "@TechEnthusiast - Check out the Module Development forum! There are several tutorials and code examples there.\n\nThe ASHAT Os documentation also has a comprehensive guide on creating custom modules. The plugin architecture is really well designed and makes it straightforward to add new functionality.\n\nIf you need help, feel free to create a thread in the Development section!",
                UserTitle = "Senior Developer",
                PostCount = 891,
                JoinDate = DateTime.UtcNow.AddYears(-1),
                Location = "United Kingdom",
                Signature = "Open source advocate | Full-stack developer",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 5,
                Author = "GameDev42",
                PostDate = DateTime.UtcNow.AddDays(-25),
                Content = "I'm particularly interested in the Game Engine integration. Has anyone here built a game using the Legendary Game Engine Suite?\n\nI'd love to see some examples and learn about best practices for multiplayer game development with ASHAT Os.",
                UserTitle = "Game Developer",
                PostCount = 234,
                JoinDate = DateTime.UtcNow.AddMonths(-4),
                Location = "Australia",
                Signature = "Creating worlds, one line of code at a time 🎮",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 6,
                Author = "ModMaster",
                PostDate = DateTime.UtcNow.AddDays(-24),
                Content = "@GameDev42 - Absolutely! I've been working on a multiplayer RPG using the Game Engine Suite.\n\nThe WebSocket integration makes real-time gameplay incredibly smooth. The scene management and entity system are also very intuitive.\n\nI'll be posting a detailed tutorial in the Game Engine forum soon. Stay tuned!",
                UserTitle = "Expert Developer",
                PostCount = 1245,
                JoinDate = DateTime.UtcNow.AddYears(-1).AddMonths(-3),
                Location = "Germany",
                Signature = "Modding enthusiast | Open source contributor",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 7,
                Author = "Newbie123",
                PostDate = DateTime.UtcNow.AddDays(-20),
                Content = "Hi everyone! I'm new to ASHAT Os and web development in general.\n\nThis community seems very welcoming. I have a lot to learn, but I'm eager to get started!\n\nAny tips for absolute beginners?",
                UserTitle = "Newbie",
                PostCount = 12,
                JoinDate = DateTime.UtcNow.AddDays(-21),
                Location = "USA",
                Signature = "",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 8,
                Author = "Helper",
                PostDate = DateTime.UtcNow.AddDays(-19),
                Content = "@Newbie123 - Welcome! The best way to start is by following the Quick Start guide in the documentation.\n\nAlso check out the Installation & Setup forum for step-by-step tutorials. Don't hesitate to ask questions - we're all here to help!\n\nThe community is very friendly and supportive of newcomers.",
                UserTitle = "Helpful Member",
                PostCount = 567,
                JoinDate = DateTime.UtcNow.AddMonths(-8),
                Location = "France",
                Signature = "Always happy to help! 😊",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 9,
                Author = "CodeNinja",
                PostDate = DateTime.UtcNow.AddDays(-15),
                Content = "The forum system itself is a great example of what you can build with ASHAT Os!\n\nI love the purple gradient theme and the vBulletin-style layout. Very professional looking.\n\nKudos to the development team! 👏",
                UserTitle = "Advanced Member",
                PostCount = 678,
                JoinDate = DateTime.UtcNow.AddMonths(-9),
                Location = "Japan",
                Signature = "Code is poetry 💜",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 10,
                Author = "ForumFan",
                PostDate = DateTime.UtcNow.AddDays(-10),
                Content = "I agree! The forum features are really impressive:\n\n✓ Sticky threads\n✓ Thread prefixes\n✓ User avatars and signatures\n✓ Post counts and user titles\n✓ Pagination\n✓ Moderation tools\n✓ Content filtering\n\nIt's got everything you'd expect from a professional forum system.",
                UserTitle = "Forum Regular",
                PostCount = 423,
                JoinDate = DateTime.UtcNow.AddMonths(-5),
                Location = "Netherlands",
                Signature = "Discussion is the best form of learning",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 11,
                Author = "SecurityPro",
                PostDate = DateTime.UtcNow.AddDays(-5),
                Content = "I'm particularly impressed by the security features.\n\nThe integration with content moderation and parental controls is excellent. It's clear that a lot of thought went into making this a safe platform for all users.\n\nGreat work on the security architecture!",
                UserTitle = "Security Expert",
                PostCount = 389,
                JoinDate = DateTime.UtcNow.AddMonths(-7),
                Location = "Switzerland",
                Signature = "Security first, always! 🔒",
                IsEditable = false
            },
            new PostInfo
            {
                Id = 12,
                Author = "Admin",
                PostDate = DateTime.UtcNow.AddHours(-2),
                Content = "Thank you all for the wonderful feedback!\n\nWe're constantly working on improvements and new features. Your input helps us make ASHAT Os better for everyone.\n\nKeep the discussions going, and don't forget to check out the other forum categories. There's always something interesting happening in the community! 🌟",
                UserTitle = "Administrator",
                PostCount = 2852,
                JoinDate = DateTime.UtcNow.AddYears(-2),
                Location = "Worldwide",
                Signature = "AGP Studios, INC - Building the future of web platforms",
                IsEditable = false
            }
        };

        // Simple pagination
        int perPage = 10;
        Posts = allPosts
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
        
        TotalPages = (int)Math.Ceiling(allPosts.Count / (double)perPage);
    }
}

public class PostInfo
{
    public int Id { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime PostDate { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserTitle { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public DateTime JoinDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
}
