using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASHATCore.Pages;

/// <summary>
/// Page model for editing home page content with WYSIWYG editor
/// </summary>
public class EditHomeModel : PageModel
{
    [BindProperty]
    public string PageTitle { get; set; } = "AGP Studios, INC";
    
    [BindProperty]
    public string Tagline { get; set; } = "Unity meets Software - ASHAT Os";
    
    [BindProperty]
    public string WelcomeContent { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    
    public void OnGet()
    {
        // Load current home page content
        // In production, this would be retrieved from LegendaryCMS API
        LoadHomePageContent();
    }
    
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(WelcomeContent))
        {
            ErrorMessage = "Please fill in all required fields.";
            return Page();
        }
        
        // Sanitize content to prevent XSS attacks
        var sanitizedContent = SanitizeHtml(WelcomeContent);
        
        // Save home page content
        // In production, this would call the LegendaryCMS API
        var success = SaveHomePageContent(PageTitle, Tagline, sanitizedContent);
        
        if (success)
        {
            SuccessMessage = "Home page updated successfully!";
            return RedirectToPage("./Index");
        }
        else
        {
            ErrorMessage = "Failed to update home page. Please try again.";
            return Page();
        }
    }
    
    private void LoadHomePageContent()
    {
        // In production, load from LegendaryCMS API
        // For now, use default values
        PageTitle = "AGP Studios, INC";
        Tagline = "Unity meets Software - ASHAT Os";
        WelcomeContent = @"<h2>🎯 Welcome to ASHAT Os</h2>
<p>ASHAT Os is an advanced, extensible server platform that combines CMS functionality, forum systems, user profiles, game engine capabilities, and a comprehensive control panel into a unified, modular architecture.</p>
<p>Built with .NET 9.0, ASHAT Os provides a robust foundation for web applications, game servers, and community platforms. Our Legendary CMS Suite powers this platform with production-ready features including plugin architecture, REST API, enhanced RBAC, and comprehensive security.</p>";
    }
    
    private bool SaveHomePageContent(string title, string tagline, string content)
    {
        // In production, this would call the LegendaryCMS API to save content
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
