namespace RaCore.Engine;

/// <summary>
/// Detects and verifies search engine bots and crawlers.
/// Phase 9.3.7: Bot filtering for homepage access control.
/// </summary>
public class BotDetector
{
    private static readonly HashSet<string> SearchEngineBotPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        // Google
        "Googlebot",
        "Google-InspectionTool",
        "GoogleOther",
        "Mediapartners-Google",
        "AdsBot-Google",
        "Storebot-Google",
        "Google-Read-Aloud",
        "DuplexWeb-Google",
        
        // Bing
        "bingbot",
        "BingPreview",
        "msnbot",
        "adidxbot",
        
        // Yahoo
        "Slurp",
        "yahoo",
        
        // Yandex
        "YandexBot",
        "YandexImages",
        "YandexAccessibilityBot",
        "YandexMobileBot",
        
        // Baidu
        "Baiduspider",
        "Baiduspider-image",
        "Baiduspider-video",
        
        // DuckDuckGo
        "DuckDuckBot",
        "DuckDuckGo-Favicons-Bot",
        
        // Other Major Search Engines
        "ia_archiver", // Alexa
        "Sogou", // Sogou (Chinese)
        "Exabot", // Exalead
        "facebot", // Facebook
        "facebookexternalhit", // Facebook
        "Twitterbot", // Twitter
        "LinkedInBot", // LinkedIn
        "Slackbot", // Slack
        "TelegramBot", // Telegram
        "Applebot", // Apple
        "seznambot", // Seznam (Czech)
        "AhrefsBot", // Ahrefs SEO
        "SemrushBot", // Semrush SEO
        "DotBot", // Moz/OpenSiteExplorer
        "MJ12bot", // Majestic SEO
        "PetalBot", // Huawei/Aspiegel
        "archive.org_bot", // Internet Archive
        "Screaming Frog", // SEO Spider
        "SiteAuditBot" // Various site audit tools
    };

    /// <summary>
    /// Determines if a User-Agent string represents a known search engine bot.
    /// </summary>
    /// <param name="userAgent">The User-Agent header value from the HTTP request</param>
    /// <returns>True if the user agent is a recognized search engine bot, false otherwise</returns>
    public static bool IsSearchEngineBot(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return false;
        }

        // Check if user agent contains any of the known bot patterns
        return SearchEngineBotPatterns.Any(pattern => 
            userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a detailed bot information if the user agent is a recognized bot.
    /// </summary>
    /// <param name="userAgent">The User-Agent header value</param>
    /// <returns>Bot name if detected, null otherwise</returns>
    public static string? GetBotName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        foreach (var pattern in SearchEngineBotPatterns)
        {
            if (userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return pattern;
            }
        }

        return null;
    }

    /// <summary>
    /// Generates a message for non-bot visitors explaining homepage access restrictions.
    /// </summary>
    public static string GetAccessDeniedMessage()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <title>Access Information - RaCore</title>
    <meta name=""robots"" content=""noindex, nofollow"">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 700px;
            margin: 100px auto;
            padding: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #333;
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
        }
        h1 {
            color: #667eea;
            margin-bottom: 20px;
        }
        .info {
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
            background: #d1ecf1;
            border-left: 4px solid #17a2b8;
        }
        code {
            background: #f4f4f4;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
        }
        .button {
            display: inline-block;
            padding: 12px 24px;
            margin: 10px 5px;
            background: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            transition: background 0.3s;
        }
        .button:hover {
            background: #5568d3;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🔒 Homepage Access</h1>
        <p>The homepage is currently configured for search engine indexing only.</p>
        
        <div class='info'>
            <h3>📋 For Users:</h3>
            <p>If you're looking to access RaCore services, please use the Control Panel:</p>
            <p><a href='/control-panel.html' class='button'>Open Control Panel</a></p>
        </div>
        
        <div class='info'>
            <h3>🔍 For Search Engines:</h3>
            <p>This page is accessible to verified search engine crawlers for indexing purposes.</p>
            <p>Known search engine bots (Googlebot, Bingbot, etc.) can access the homepage.</p>
        </div>
        
        <div class='info'>
            <h3>🛠️ For Administrators:</h3>
            <p>This is part of Phase 9.3.7 security implementation.</p>
            <p>To modify homepage access settings, check the <code>BotDetector</code> configuration.</p>
        </div>
        
        <p style='margin-top: 30px; text-align: center; color: #6c757d;'>
            <small>RaCore Phase 9.3.7 | Search Engine Bot Access Control</small>
        </p>
    </div>
</body>
</html>";
    }
}
