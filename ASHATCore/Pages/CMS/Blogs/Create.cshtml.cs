using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages.CMS.Blogs;

/// <summary>
/// Page model for creating and editing blog posts with WYSIWYG editor
/// </summary>
public class CreateModel : PageModel
{
    [BindProperty]
    public string Title { get; set; } = string.Empty;
    
    [BindProperty]
    public new string? Content { get; set; } = string.Empty;
    
    [BindProperty]
    public string Category { get; set; } = string.Empty;
    
    public bool IsEdit { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public void OnGet(int? id)
    {
        if (id.HasValue)
        {
            IsEdit = true;
            LoadPost(id.Value);
        }
    }
    
    public IActionResult OnPost(int? id, string? action)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
        {
            ErrorMessage = "Please fill in all required fields.";
            IsEdit = id.HasValue;
            return Page();
        }
        
        // Sanitize content to prevent XSS attacks
        // In production, use a proper HTML sanitizer library like HtmlSanitizer
        var sanitizedContent = SanitizeHtml(Content);
        
        if (IsEdit || id.HasValue)
        {
            // Update existing post
            // In production, this would call the LegendaryCMS API
            var success = UpdatePost(id ?? 0, Title, sanitizedContent, Category);
            
            if (success)
            {
                SuccessMessage = "Blog post updated successfully!";
                return RedirectToPage("./Post", new { id = id ?? 0 });
            }
            else
            {
                ErrorMessage = "Failed to update blog post. Please try again.";
                IsEdit = true;
                return Page();
            }
        }
        else
        {
            // Create new post
            // In production, this would call the LegendaryCMS API
            var isDraft = action == "draft";
            var postId = CreatePost(Title, sanitizedContent, Category, isDraft);
            
            if (postId > 0)
            {
                SuccessMessage = isDraft ? "Blog post saved as draft!" : "Blog post published successfully!";
                return RedirectToPage("./Post", new { id = postId });
            }
            else
            {
                ErrorMessage = "Failed to create blog post. Please try again.";
                return Page();
            }
        }
    }
    
    private void LoadPost(int id)
    {
        // In production, this would load from LegendaryCMS API
        // For now, load sample data
        if (id == 1)
        {
            Title = "Welcome to ASHATOS";
            Category = "General";
            Content = "<p>Welcome to ASHATOS (ASHAT Operating System), a revolutionary platform...</p>";
        }
    }
    
    private bool UpdatePost(int id, string title, string content, string category)
    {
        // In production, this would call the LegendaryCMS API to update the post
        // For now, simulate success
        return true;
    }
    
    private int CreatePost(string title, string content, string category, bool isDraft)
    {
        // In production, this would call the LegendaryCMS API to create a new post
        // For now, return a simulated post ID
        return new Random().Next(100, 999);
    }
    
    private string SanitizeHtml(string html)
    {
        // Basic sanitization - in production, use HtmlSanitizer NuGet package
        // This is a minimal implementation for demonstration
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        
        // Remove potentially dangerous tags and attributes
        var dangerous = new[] { "<script", "<iframe", "javascript:", "onerror=", "onload=" };
        var result = html;
        
        foreach (var danger in dangerous)
        {
            result = result.Replace(danger, string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        
        return result;
    }
}
