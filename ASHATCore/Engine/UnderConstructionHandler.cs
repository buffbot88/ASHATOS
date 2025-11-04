using Abstractions;

namespace ASHATCore.Engine;

/// <summary>
/// Handles Generation of "Under Construction" page with cute robot face
/// Phase 9.3.8: Site maintenance and construction mode support
/// </summary>
public static class UnderConstructionHandler
{
    /// <summary>
    /// Default cute robot face SVG (inline data URI) - Updated for purple theme
    /// </summary>
    private const string DefaultRobotSvg = @"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'%3E%3Cdefs%3E%3ClinearGradient id='robotGrad' x1='0%25' y1='0%25' x2='100%25' y2='100%25'%3E%3Cstop offset='0%25' style='stop-color:%238b2fc7;stop-opacity:1' /%3E%3Cstop offset='100%25' style='stop-color:%236a1b9a;stop-opacity:1' /%3E%3C/linearGradient%3E%3C/defs%3E%3Crect x='50' y='60' width='100' height='80' rx='15' fill='url(%23robotGrad)' /%3E%3Ccircle cx='75' cy='85' r='8' fill='%23fff' /%3E%3Ccircle cx='125' cy='85' r='8' fill='%23fff' /%3E%3Ccircle cx='75' cy='85' r='4' fill='%23333' /%3E%3Ccircle cx='125' cy='85' r='4' fill='%23333' /%3E%3Cpath d='M 80 110 Q 100 120 120 110' stroke='%23fff' stroke-width='3' fill='none' stroke-linecap='round' /%3E%3Crect x='40' y='50' width='15' height='8' rx='3' fill='%238b2fc7' /%3E%3Crect x='145' y='50' width='15' height='8' rx='3' fill='%238b2fc7' /%3E%3Ccircle cx='47.5' cy='46' r='5' fill='%236a1b9a' /%3E%3Ccircle cx='152.5' cy='46' r='5' fill='%236a1b9a' /%3E%3Crect x='90' y='35' width='20' height='25' rx='5' fill='%238b2fc7' /%3E%3Ccircle cx='100' cy='30' r='6' fill='%23ffd700' opacity='0.8' /%3E%3C/svg%3E";

    /// <summary>
    /// Generates the "Under Construction" HTML page
    /// </summary>
    /// <param name="config">Server Configuration with customization options</param>
    /// <returns>Complete HTML page as string</returns>
    public static string GenerateUnderConstructionPage(ServerConfiguration config)
    {
        var message = config.UnderConstructionMessage ?? "We're working on something awesome! Please check back soon.";
        var robotImage = config.UnderConstructionRobotImage ?? DefaultRobotSvg;
        
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Under Construction - ASHATCore</title>
    <meta name=""robots"" content=""noindex, nofollow"">
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            color: white;
            padding: 20px;
        }}
        
        .container {{
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 60px 40px;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(138, 43, 226, 0.4);
            max-width: 600px;
            width: 100%;
            text-align: center;
            animation: fadeIn 0.5s ease-in;
        }}
        
        @keyframes fadeIn {{
            from {{
                opacity: 0;
                transform: translateY(20px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}
        
        .robot-container {{
            margin-bottom: 30px;
            animation: bounce 2s infinite;
        }}
        
        @keyframes bounce {{
            0%, 100% {{
                transform: translateY(0);
            }}
            50% {{
                transform: translateY(-10px);
            }}
        }}
        
        .robot-image {{
            width: 150px;
            height: 150px;
            margin: 0 auto;
        }}
        
        h1 {{
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-size: 2.5em;
            margin-bottom: 20px;
            font-weight: 700;
        }}
        
        .message {{
            font-size: 1.2em;
            color: #d8c8ff;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        
        .info-box {{
            background: rgba(138, 43, 226, 0.1);
            border-left: 4px solid #8b2fc7;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: left;
        }}
        
        .info-box h3 {{
            color: #c084fc;
            margin-bottom: 10px;
            font-size: 1.1em;
        }}
        
        .info-box p {{
            color: #c8b6ff;
            line-height: 1.5;
            margin: 5px 0;
        }}
        
        .admin-link {{
            display: inline-block;
            margin-top: 20px;
            padding: 12px 30px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            text-decoration: none;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 25px;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.4);
        }}
        
        .admin-link:hover {{
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(138, 43, 226, 0.6);
        }}
        
        .footer {{
            margin-top: 30px;
            color: #b8a8d8;
            font-size: 0.9em;
        }}
        
        @media (max-width: 600px) {{
            .container {{
                padding: 40px 20px;
            }}
            
            h1 {{
                font-size: 2em;
            }}
            
            .message {{
                font-size: 1em;
            }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""robot-container"">
            <img src=""{robotImage}"" alt=""Robot"" class=""robot-image"">
        </div>
        
        <h1>🚧 Under Construction</h1>
        
        <div class=""message"">
            {message}
        </div>
        
        <div class=""info-box"">
            <h3>🔧 What's Happening?</h3>
            <p>Our team is hard at work improving your experience. We're adding new features, 
            fixing bugs, and making everything even better!</p>
        </div>
        
        <div class=""info-box"">
            <h3>👤 Are you an administrator?</h3>
            <p>Administrators can access the Control Panel even during maintenance.</p>
            <a href=""/control-panel"" class=""admin-link"">🎛️ Admin Control Panel</a>
        </div>
        
        <div class=""footer"">
            ASHATCore - Under Construction Mode | Server Status: Maintenance
        </div>
    </div>
</body>
</html>";
    }
    
    /// <summary>
    /// Generates an enhanced HTML error page with consistent styling
    /// </summary>
    /// <param name="errorCode">HTTP error code (e.g., 404, 500)</param>
    /// <param name="errorTitle">Short title of the error</param>
    /// <param name="errorMessage">Detailed error message</param>
    /// <param name="showControlPanelLink">Whether to show link to control panel</param>
    /// <returns>Complete HTML error page</returns>
    public static string GenerateErrorPage(int errorCode, string errorTitle, string errorMessage, bool showControlPanelLink = true)
    {
        var controlPanelSection = showControlPanelLink ? @"
        <div class=""action-box"">
            <h3>🎛️ Need Help?</h3>
            <p>If you're an administrator, you can access the control panel:</p>
            <a href=""/control-panel"" class=""action-link"">Open Control Panel</a>
        </div>" : "";
        
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{errorCode} - {errorTitle}</title>
    <meta name=""robots"" content=""noindex, nofollow"">
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #333;
            padding: 20px;
        }}
        
        .container {{
            background: white;
            padding: 50px 40px;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            max-width: 600px;
            width: 100%;
            text-align: center;
        }}
        
        .error-code {{
            font-size: 6em;
            font-weight: 700;
            color: #667eea;
            line-height: 1;
            margin-bottom: 10px;
        }}
        
        h1 {{
            color: #333;
            font-size: 2em;
            margin-bottom: 20px;
        }}
        
        .message {{
            font-size: 1.1em;
            color: #555;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        
        .action-box {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
        }}
        
        .action-box h3 {{
            color: #667eea;
            margin-bottom: 10px;
        }}
        
        .action-link {{
            display: inline-block;
            margin-top: 15px;
            padding: 12px 30px;
            background: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            transition: all 0.3s ease;
        }}
        
        .action-link:hover {{
            background: #5568d3;
            transform: translateY(-2px);
        }}
        
        .footer {{
            margin-top: 30px;
            color: #999;
            font-size: 0.9em;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""error-code"">{errorCode}</div>
        <h1>{errorTitle}</h1>
        <div class=""message"">{errorMessage}</div>
        {controlPanelSection}
        <div class=""footer"">
            ASHATCore Error Handler | If this problem persists, please contact support
        </div>
    </div>
</body>
</html>";
    }
    
    /// <summary>
    /// Generates HTML for the server activation warning banner
    /// Shows a white banner with red text when server is not activated
    /// </summary>
    /// <param name="daysRemaining">Number of days remaining to activate</param>
    /// <returns>HTML banner string</returns>
    public static string GenerateActivationWarningBanner(int daysRemaining)
    {
        var urgencyClass = daysRemaining <= 7 ? "urgent" : "";
        var message = daysRemaining > 0 
            ? $"⚠️ SERVER NOT ACTIVATED - {daysRemaining} days remaining to activate this server. <a href=\"/activation\">Activate Now</a>"
            : "⚠️ SERVER ACTIVATION EXPIRED - This server must be activated immediately. <a href=\"/activation\">Activate Now</a>";
            
        return $@"
<div class=""activation-warning-banner {urgencyClass}"" style=""
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    background: white;
    color: #dc2626;
    padding: 12px 20px;
    text-align: center;
    font-weight: 600;
    font-size: 14px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    z-index: 9999;
    border-bottom: 3px solid #dc2626;
"">
    {message}
</div>
<div style=""height: 50px;""></div>
<style>
.activation-warning-banner a {{
    color: #dc2626;
    text-decoration: underline;
    font-weight: 700;
}}
.activation-warning-banner a:hover {{
    color: #991b1b;
}}
.activation-warning-banner.urgent {{
    background: #fef2f2;
    animation: pulse-red 2s ease-in-out infinite;
}}
@keyframes pulse-red {{
    0%, 100% {{ background: white; }}
    50% {{ background: #fee2e2; }}
}}
</style>
";
    }
}
