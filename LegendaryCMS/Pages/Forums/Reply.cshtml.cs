using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LegendaryCMS.Pages.Forums;

/// <summary>
/// Page model for replying to forum threads with WYSIWYG editor
/// </summary>
public class ReplyModel : PageModel
{
    [BindProperty]
    public string PostContent { get; set; } = string.Empty;
    
    [BindProperty]
    public int ThreadId { get; set; }
    
    [BindProperty]
    public int? QuotePostId { get; set; }
    
    public string ThreadTitle { get; set; } = string.Empty;
    public string ForumName { get; set; } = string.Empty;
    public string QuoteAuthor { get; set; } = string.Empty;
    public string QuoteContent { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public void OnGet(int threadId, int? quotePostId)
    {
        ThreadId = threadId;
        QuotePostId = quotePostId;
        
        // Load thread information
        LoadThreadInfo(threadId);
        
        // If quoting a post, load the quoted content
        if (quotePostId.HasValue)
        {
            LoadQuotedPost(quotePostId.Value);
        }
    }
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(PostContent) || ThreadId <= 0)
        {
            ErrorMessage = "Please enter your reply content.";
            LoadThreadInfo(ThreadId);
            if (QuotePostId.HasValue)
            {
                LoadQuotedPost(QuotePostId.Value);
            }
            return Page();
        }
        
        // Sanitize content to prevent XSS attacks
        var sanitizedContent = SanitizeHtml(PostContent);
        
        // If this is a quote, prepend the quoted content
        if (QuotePostId.HasValue)
        {
            sanitizedContent = $"<blockquote><strong>{QuoteAuthor} wrote:</strong><br/>{QuoteContent}</blockquote><br/>{sanitizedContent}";
        }
        
        // Create reply
        // In production, this would call the LegendaryCMS API
        var success = CreateReply(ThreadId, sanitizedContent);
        
        if (success)
        {
            SuccessMessage = "Reply posted successfully!";
            return RedirectToPage("./Thread", new { id = ThreadId });
        }
        else
        {
            ErrorMessage = "Failed to post reply. Please try again.";
            LoadThreadInfo(ThreadId);
            if (QuotePostId.HasValue)
            {
                LoadQuotedPost(QuotePostId.Value);
            }
            return Page();
        }
    }
    
    private void LoadThreadInfo(int threadId)
    {
        // In production, this would load from LegendaryCMS API
        // For now, use sample data
        ThreadTitle = "Sample Thread Title";
        ForumName = "General Discussion";
    }
    
    private void LoadQuotedPost(int postId)
    {
        // In production, this would load from LegendaryCMS API
        // For now, use sample data
        QuoteAuthor = "Sample User";
        QuoteContent = "This is the quoted post content that the user wants to reply to.";
    }
    
    private bool CreateReply(int threadId, string content)
    {
        // In production, this would call the LegendaryCMS API to create a reply
        // For now, simulate success
        return true;
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
