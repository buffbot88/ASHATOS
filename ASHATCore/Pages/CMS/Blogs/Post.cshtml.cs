using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Blogs;

/// <summary>
/// Razor Page Model for Blog Post Detail - WordPress-like blog post display
/// </summary>
public class PostModel : PageModel
{
    public BlogPost? Post { get; set; }
    public List<BlogPost> RecentPosts { get; set; } = new();

    public void OnGet(int id)
    {
        // Load the specific blog post by ID
        Post = LoadBlogPost(id);
        
        // Load recent posts for sidebar
        RecentPosts = LoadRecentBlogPosts();
    }

    private BlogPost? LoadBlogPost(int id)
    {
        // In production, this would call the LegendaryCMS API
        // For now, return sample data based on ID
        var posts = new Dictionary<int, BlogPost>
        {
            [1] = new BlogPost
            {
                Id = 1,
                Title = "Welcome to ASHATOS",
                Author = "Admin",
                PublishedDate = DateTime.UtcNow,
                Excerpt = "Welcome to the new pure .NET CMS powered by LegendaryCMS.",
                Content = @"
                    <p>Welcome to ASHATOS (ASHAT Operating System), a revolutionary platform that combines content management, game development, and server infrastructure into one unified system.</p>
                    
                    <h2>What is ASHATOS?</h2>
                    <p>ASHATOS is built on .NET 9.0 and provides a powerful, modular architecture for content management. Unlike traditional PHP-based systems, ASHATOS leverages the performance and security of modern .NET technology with Razor Pages and Blazor components.</p>
                    
                    <h3>Key Features</h3>
                    <ul>
                        <li><strong>Pure .NET Architecture:</strong> No PHP dependencies, just clean C# code</li>
                        <li><strong>Modular Design:</strong> Extensible plugin system for custom functionality</li>
                        <li><strong>LegendaryCMS Integration:</strong> Full-featured content management system</li>
                        <li><strong>Built-in Forums:</strong> Community discussion platform included</li>
                        <li><strong>User Profiles:</strong> Rich user management and authentication</li>
                        <li><strong>Game Engine:</strong> Integrated game development capabilities</li>
                    </ul>
                    
                    <h2>Getting Started</h2>
                    <p>To get started with ASHATOS, explore our documentation and join our community forums. We're excited to have you here!</p>
                    
                    <h3>Next Steps</h3>
                    <p>Check out our tutorials on building with Razor Pages, or dive into the game engine documentation. The possibilities are endless with ASHATOS.</p>
                ",
                ViewCount = 142,
                CommentCount = 5
            },
            [2] = new BlogPost
            {
                Id = 2,
                Title = "Getting Started with Razor Pages",
                Author = "Developer",
                PublishedDate = DateTime.UtcNow.AddDays(-3),
                Excerpt = "Learn how to create dynamic web pages using Razor Pages and the LegendaryCMS framework.",
                Content = @"
                    <p>Razor Pages is a page-based programming model that makes building web UI easier and more productive. In this guide, we'll explore how to leverage Razor Pages in ASHATOS.</p>
                    
                    <h2>Why Razor Pages?</h2>
                    <p>Razor Pages offers several advantages over traditional MVC patterns:</p>
                    <ul>
                        <li>Simpler page-focused scenarios</li>
                        <li>Better organization of page-specific logic</li>
                        <li>Built-in CSRF protection</li>
                        <li>Easy to learn and implement</li>
                    </ul>
                    
                    <h2>Creating Your First Page</h2>
                    <p>Creating a Razor Page in ASHATOS is straightforward. Here's a simple example:</p>
                    
                    <pre><code>@page
@model MyPageModel
@{
    ViewData[""Title""] = ""My Page"";
}

&lt;h1&gt;@Model.Title&lt;/h1&gt;
&lt;p&gt;@Model.Content&lt;/p&gt;</code></pre>
                    
                    <h3>Page Model</h3>
                    <p>The code-behind file provides the logic for your page:</p>
                    
                    <pre><code>public class MyPageModel : PageModel
{
    public string Title { get; set; } = ""Hello World"";
    public string Content { get; set; } = ""Welcome!"";
    
    public void OnGet()
    {
        // Page initialization logic
    }
}</code></pre>
                    
                    <h2>Conclusion</h2>
                    <p>Razor Pages provides a clean, efficient way to build web pages in ASHATOS. Start exploring the framework today!</p>
                ",
                ViewCount = 98,
                CommentCount = 3
            },
            [3] = new BlogPost
            {
                Id = 3,
                Title = "Migrating from PHP to .NET",
                Author = "Tech Lead",
                PublishedDate = DateTime.UtcNow.AddDays(-7),
                Excerpt = "Discover the benefits of migrating from legacy PHP systems to modern .NET architecture.",
                Content = @"
                    <p>Many developers are making the switch from PHP to .NET, and for good reason. In this post, we'll explore the benefits and best practices for migration.</p>
                    
                    <h2>Why Migrate?</h2>
                    <p>The advantages of .NET over PHP include:</p>
                    <ul>
                        <li><strong>Performance:</strong> .NET is significantly faster than PHP in most scenarios</li>
                        <li><strong>Type Safety:</strong> C# is statically typed, catching errors at compile time</li>
                        <li><strong>Modern Tooling:</strong> Visual Studio and VS Code provide excellent development experiences</li>
                        <li><strong>Security:</strong> Built-in security features and regular updates</li>
                        <li><strong>Scalability:</strong> Better support for large-scale applications</li>
                    </ul>
                    
                    <h2>Migration Strategy</h2>
                    <p>When migrating from PHP to .NET, consider these steps:</p>
                    
                    <h3>1. Assess Your Current System</h3>
                    <p>Document all PHP components, dependencies, and functionality that needs to be migrated.</p>
                    
                    <h3>2. Plan the Architecture</h3>
                    <p>Design your .NET application architecture using modern patterns like MVC or Razor Pages.</p>
                    
                    <h3>3. Incremental Migration</h3>
                    <p>Migrate in phases, starting with the most critical components. This reduces risk and allows for testing along the way.</p>
                    
                    <h3>4. Testing</h3>
                    <p>Thoroughly test each migrated component to ensure functionality matches the original system.</p>
                    
                    <h2>ASHATOS Approach</h2>
                    <p>ASHATOS was designed with PHP migration in mind. Our LegendaryCMS framework provides familiar CMS concepts while leveraging the power of .NET.</p>
                    
                    <h2>Conclusion</h2>
                    <p>Migrating from PHP to .NET is a worthwhile investment that will pay dividends in performance, maintainability, and developer productivity. ASHATOS makes this transition smooth and efficient.</p>
                ",
                ViewCount = 156,
                CommentCount = 8
            }
        };

        return posts.ContainsKey(id) ? posts[id] : null;
    }

    private List<BlogPost> LoadRecentBlogPosts()
    {
        // In production, this would call the LegendaryCMS API
        return new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "Welcome to ASHATOS",
                Author = "Admin",
                PublishedDate = DateTime.UtcNow,
                Excerpt = "Welcome to the new pure .NET CMS powered by LegendaryCMS."
            },
            new BlogPost
            {
                Id = 2,
                Title = "Getting Started with Razor Pages",
                Author = "Developer",
                PublishedDate = DateTime.UtcNow.AddDays(-3),
                Excerpt = "Learn how to create dynamic web pages using Razor Pages and the LegendaryCMS framework."
            },
            new BlogPost
            {
                Id = 3,
                Title = "Migrating from PHP to .NET",
                Author = "Tech Lead",
                PublishedDate = DateTime.UtcNow.AddDays(-7),
                Excerpt = "Discover the benefits of migrating from legacy PHP systems to modern .NET architecture."
            },
            new BlogPost
            {
                Id = 4,
                Title = "Advanced Game Engine Features",
                Author = "Game Dev",
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                Excerpt = "Explore the advanced features of ASHATOS's integrated game engine."
            },
            new BlogPost
            {
                Id = 5,
                Title = "Security Best Practices",
                Author = "Security Team",
                PublishedDate = DateTime.UtcNow.AddDays(-14),
                Excerpt = "Learn how to secure your ASHATOS applications with industry best practices."
            }
        };
    }
}
