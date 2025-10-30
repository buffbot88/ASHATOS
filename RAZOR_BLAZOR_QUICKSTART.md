# Razor Pages and Blazor Components in ASHATOS

## Overview

ASHATOS/LegendaryCMS now uses **pure .NET architecture** with Razor Pages and Blazor components, eliminating the need for PHP.

## Quick Start

### Accessing CMS Features

All CMS features are available via Razor Pages:
- **Forums**: `http://localhost:7077/cms/forums`
- **Blogs**: `http://localhost:7077/cms/blogs`
- **Profiles**: `http://localhost:7077/cms/profiles/{username}`

### Using Blazor Components

Blazor components can be embedded in any Razor Page:

```razor
@page "/forums"
@using ASHATCore.Components.CMS

<h1>Forums</h1>

@foreach (var post in Model.Posts)
{
    <ForumPost 
        Author="@post.Author"
        PostDate="@post.Date"
        Content="@post.Content"
        AllowEdit="@Model.CanEdit(post)" />
}
```

## Directory Structure

```
ASHATCore/
├── Pages/
│   └── CMS/                      # Razor Pages for CMS
│       ├── Forums/
│       │   ├── Index.cshtml     # Forum list view
│       │   └── Index.cshtml.cs  # Forum page model
│       ├── Blogs/
│       │   ├── Index.cshtml
│       │   └── Index.cshtml.cs
│       ├── Profiles/
│       │   ├── Index.cshtml
│       │   └── Index.cshtml.cs
│       └── README.md
└── Components/
    └── CMS/                      # Blazor Components
        ├── ForumPost.razor
        ├── BlogPostCard.razor
        └── (more components...)
```

## Creating New Razor Pages

### 1. Create the Razor Page

Create `Pages/CMS/YourFeature/Index.cshtml`:

```razor
@page
@model ASHATCore.Pages.CMS.YourFeature.IndexModel
@{
    ViewData["Title"] = "Your Feature";
}

<div class="container">
    <h1>@ViewData["Title"]</h1>
    <!-- Your content here -->
</div>
```

### 2. Create the Page Model

Create `Pages/CMS/YourFeature/Index.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.YourFeature;

public class IndexModel : PageModel
{
    public List<Item> Items { get; set; } = new();

    public void OnGet()
    {
        // Load data from LegendaryCMS API
        Items = LoadFromAPI();
    }
}
```

### 3. Register Route (if needed)

Routes are automatically discovered. Custom routes can be added in `Program.cs`:

```csharp
app.MapRazorPages();
```

## Creating Blazor Components

### 1. Create the Component

Create `Components/CMS/YourComponent.razor`:

```razor
@* Your component description *@

<div class="@CssClass">
    <h3>@Title</h3>
    <p>@Content</p>
</div>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
    
    [Parameter]
    public string Content { get; set; } = string.Empty;
    
    [Parameter]
    public string CssClass { get; set; } = string.Empty;
}
```

### 2. Use in Razor Pages

```razor
@using ASHATCore.Components.CMS

<YourComponent 
    Title="@item.Title"
    Content="@item.Content"
    CssClass="custom-class" />
```

## Integration with LegendaryCMS

### Using the API

Razor Pages integrate with LegendaryCMS module API:

```csharp
public class IndexModel : PageModel
{
    private readonly ILegendaryCMSModule _cms;
    
    public IndexModel(ILegendaryCMSModule cms)
    {
        _cms = cms;
    }
    
    public async Task OnGetAsync()
    {
        var forums = await _cms.GetForumsAsync();
        // Populate model
    }
}
```

### API Endpoints

LegendaryCMS provides these API endpoints:
- `/api/forums` - Forum data
- `/api/blogs` - Blog posts
- `/api/profiles` - User profiles
- `/api/chat` - Chat messages

## Benefits Over PHP

### Performance
- ✅ **No PHP-FPM** - Everything runs in Kestrel
- ✅ **Compiled code** - Faster execution
- ✅ **Async/await** - Better resource utilization

### Security
- ✅ **Auto HTML encoding** - XSS protection by default
- ✅ **CSRF tokens** - Built-in validation
- ✅ **Type safety** - Compile-time checking

### Developer Experience
- ✅ **IntelliSense** - Full IDE support
- ✅ **Debugging** - Step through C# code
- ✅ **Single language** - C# everywhere
- ✅ **Modern features** - LINQ, async, pattern matching

## Migration from PHP

See [PHP_TO_RAZOR_MIGRATION.md](../PHP_TO_RAZOR_MIGRATION.md) for complete migration guide.

### Quick Conversion

**PHP Template:**
```php
<?php foreach($posts as $post): ?>
    <h2><?= htmlspecialchars($post['title']) ?></h2>
<?php endforeach; ?>
```

**Razor Page:**
```razor
@foreach (var post in Model.Posts)
{
    <h2>@post.Title</h2>
}
```

## Examples

### Forum Post Display

```razor
@page "/forums/{forumId:int}"
@model ForumModel

<h1>@Model.Forum.Title</h1>

@foreach (var post in Model.Posts)
{
    <ForumPost 
        Author="@post.Author"
        PostDate="@post.Date"
        Content="@post.Content" />
}
```

### Blog List

```razor
@page "/blogs"
@model BlogsModel

<div class="blog-grid">
    @foreach (var post in Model.RecentPosts)
    {
        <BlogPostCard
            PostId="@post.Id"
            Title="@post.Title"
            Author="@post.Author"
            PublishedDate="@post.Date"
            Excerpt="@post.Excerpt" />
    }
</div>
```

## Testing

Unit test Razor Pages:

```csharp
[Fact]
public void ForumIndexPage_LoadsCategories()
{
    // Arrange
    var pageModel = new ForumIndexModel();
    
    // Act
    pageModel.OnGet();
    
    // Assert
    Assert.NotEmpty(pageModel.Categories);
}
```

## Deployment

### Windows
No changes needed - already uses Kestrel.

### Linux
Update Nginx/Apache to proxy to Kestrel:

**Nginx:**
```nginx
location / {
    proxy_pass http://localhost:7077;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
}
```

**Apache:**
```apache
ProxyPreserveHost On
ProxyPass / http://localhost:7077/
ProxyPassReverse / http://localhost:7077/
```

## Resources

- [ASP.NET Core Razor Pages](https://docs.microsoft.com/aspnet/core/razor-pages)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [LegendaryCMS Module](../LegendaryCMS/README.md)
- [Migration Guide](../PHP_TO_RAZOR_MIGRATION.md)

## Status

✅ **Production Ready** - Pure .NET architecture fully implemented  
⚠️ **PHP Deprecated** - Legacy PHP support will be removed  
🚀 **Active Development** - More Blazor components coming

---

**Last Updated**: October 30, 2025  
**Version**: 1.0
