using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LegendaryCMS.Pages.Forums;

/// <summary>
/// Page model for creating forum threads with WYSIWYG editor
/// </summary>
public class CreateThreadModel : PageModel
{
    [BindProperty]
    public string Title { get; set; } = string.Empty;
    
    [BindProperty]
    public string PostContent { get; set; } = string.Empty;
    
    [BindProperty]
    public int ForumId { get; set; }
    
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public void OnGet(int? forumId)
    {
        if (forumId.HasValue)
        {
            ForumId = forumId.Value;
        }
    }
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(PostContent) || ForumId <= 0)
        {
            ErrorMessage = "Please fill in all required fields and select a forum.";
            return Page();
        }
        
        // Sanitize content to prevent XSS attacks
        var sanitizedContent = SanitizeHtml(PostContent);
        
        // Create new thread
        // In production, this would call the LegendaryCMS API
        var threadId = CreateThread(ForumId, Title, sanitizedContent);
        
        if (threadId > 0)
        {
            SuccessMessage = "Thread created successfully!";
            return RedirectToPage("./Thread", new { id = threadId });
        }
        else
        {
            ErrorMessage = "Failed to create thread. Please try again.";
            return Page();
        }
    }
    
    private int CreateThread(int forumId, string title, string content)
    {
        // In production, this would call the LegendaryCMS API to create a new thread
        // For now, return a simulated thread ID
        return new Random().Next(100, 999);
    }
    
    private string SanitizeHtml(string html)
    {
        // Basic sanitization - in production, use HtmlSanitizer NuGet package
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        
        var dangerous = new[] { "<script", "<iframe", "javascript:", "onerror=", "onload=" };
        var result = html;
        
        foreach (var danger in dangerous)
        {
            result = result.Replace(danger, string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        
        return result;
    }
}
