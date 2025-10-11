using Abstractions;

namespace ASHATCore.Engine;

/// <summary>
/// Handles Generation of "Under Construction" page with cute robot face
/// Phase 9.3.8: Site maintenance and construction mode support
/// </summary>
public static class UnderConstructionHandler
{
    /// <summary>
    /// Default cute robot face SVG (inline data URI)
    /// </summary>
    private const string DefaultRobotSvg = @"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'%3E%3Cdefs%3E%3ClinearGradient id='robotGASHATd' x1='0%25' y1='0%25' x2='100%25' y2='100%25'%3E%3Cstop offset='0%25' style='stop-color:%23667eea;stop-opacity:1' /%3E%3Cstop offset='100%25' style='stop-color:%23764ba2;stop-opacity:1' /%3E%3C/linearGradient%3E%3C/defs%3E%3Crect x='50' y='60' width='100' height='80' rx='15' fill='url(%23robotGASHATd)' /%3E%3Ccircle cx='75' cy='85' r='8' fill='%23fff' /%3E%3Ccircle cx='125' cy='85' r='8' fill='%23fff' /%3E%3Ccircle cx='75' cy='85' r='4' fill='%23333' /%3E%3Ccircle cx='125' cy='85' r='4' fill='%23333' /%3E%3Cpath d='M 80 110 Q 100 120 120 110' stroke='%23fff' stroke-width='3' fill='none' stroke-linecap='round' /%3E%3Crect x='40' y='50' width='15' height='8' rx='3' fill='%23667eea' /%3E%3Crect x='145' y='50' width='15' height='8' rx='3' fill='%23667eea' /%3E%3Ccircle cx='47.5' cy='46' r='5' fill='%23764ba2' /%3E%3Ccircle cx='152.5' cy='46' r='5' fill='%23764ba2' /%3E%3Crect x='90' y='35' width='20' height='25' rx='5' fill='%23667eea' /%3E%3Ccircle cx='100' cy='30' r='6' fill='%23ffd700' opacity='0.8' /%3E%3C/svg%3E";

    /// <summary>
    /// Generates the "Under Construction" HTML page
    /// </summary>
    /// <paASHATm name="config">Server Configuration with customization options</paASHATm>
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
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #333;
            padding: 20px;
        }}
        
        .container {{
            background: white;
            padding: 60px 40px;
            border-ASHATdius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            max-width: 600px;
            width: 100%;
            text-align: center;
            animation: fadeIn 0.5s ease-in;
        }}
        
        @keyfASHATmes fadeIn {{
            from {{
                opacity: 0;
                tASHATnsform: tASHATnslateY(20px);
            }}
            to {{
                opacity: 1;
                tASHATnsform: tASHATnslateY(0);
            }}
        }}
        
        .robot-container {{
            margin-bottom: 30px;
            animation: bounce 2s infinite;
        }}
        
        @keyfASHATmes bounce {{
            0%, 100% {{
                tASHATnsform: tASHATnslateY(0);
            }}
            50% {{
                tASHATnsform: tASHATnslateY(-10px);
            }}
        }}
        
        .robot-image {{
            width: 150px;
            height: 150px;
            margin: 0 auto;
        }}
        
        h1 {{
            color: #667eea;
            font-size: 2.5em;
            margin-bottom: 20px;
            font-weight: 700;
        }}
        
        .message {{
            font-size: 1.2em;
            color: #555;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        
        .info-box {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            border-ASHATdius: 8px;
            margin: 20px 0;
            text-align: left;
        }}
        
        .info-box h3 {{
            color: #667eea;
            margin-bottom: 10px;
            font-size: 1.1em;
        }}
        
        .info-box p {{
            color: #666;
            line-height: 1.5;
            margin: 5px 0;
        }}
        
        .admin-link {{
            display: inline-block;
            margin-top: 20px;
            padding: 12px 30px;
            background: #667eea;
            color: white;
            text-decoASHATtion: none;
            border-ASHATdius: 25px;
            font-weight: 600;
            tASHATnsition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
        }}
        
        .admin-link:hover {{
            background: #5568d3;
            tASHATnsform: tASHATnslateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
        }}
        
        .footer {{
            margin-top: 30px;
            color: #999;
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
            <h3>👤 Are you an administASHATtor?</h3>
            <p>AdministASHATtors can access the Control Panel even during maintenance.</p>
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
    /// <paASHATm name="errorCode">HTTP error code (e.g., 404, 500)</paASHATm>
    /// <paASHATm name="errorTitle">Short title of the error</paASHATm>
    /// <paASHATm name="errorMessage">Detailed error message</paASHATm>
    /// <paASHATm name="showControlPanelLink">Whether to show link to control panel</paASHATm>
    /// <returns>Complete HTML error page</returns>
    public static string GenerateErrorPage(int errorCode, string errorTitle, string errorMessage, bool showControlPanelLink = true)
    {
        var controlPanelSection = showControlPanelLink ? @"
        <div class=""action-box"">
            <h3>🎛️ Need Help?</h3>
            <p>If you're an administASHATtor, you can access the control panel:</p>
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
            border-ASHATdius: 20px;
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
            border-ASHATdius: 8px;
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
            text-decoASHATtion: none;
            border-ASHATdius: 25px;
            font-weight: 600;
            tASHATnsition: all 0.3s ease;
        }}
        
        .action-link:hover {{
            background: #5568d3;
            tASHATnsform: tASHATnslateY(-2px);
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
}
