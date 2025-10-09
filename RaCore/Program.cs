using Abstractions;
using RaCore.Endpoints;
using RaCore.Engine;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using RaCore.Models;
using RaCore.Modules.Extensions.UserProfiles;
using SQLitePCL;
using System.Text.Json;

// Ensure wwwroot directory exists (will be populated by SiteBuilder on first run)
// Use source directory for development compatibility
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"[RaCore] Created wwwroot directory: {wwwrootPath}");
}

// 1. Instantiate MemoryModule FIRST
var memoryModule = new MemoryModule();
memoryModule.Initialize(null); // Pass ModuleManager if needed

// 2. Instantiate ModuleManager and register MemoryModule as built-in
var moduleManager = new ModuleManager();
moduleManager.RegisterBuiltInModule(memoryModule);

// 3. Check for first run and auto-spawn CMS BEFORE loading other modules
// This ensures all required files and configs are created before any module initialization
var firstRunManager = new RaCore.Engine.FirstRunManager(moduleManager);

if (firstRunManager.IsFirstRun())
{
    Console.WriteLine("[RaCore] First run detected - initializing CMS homepage...");
    await firstRunManager.InitializeAsync();
}

// 4. Load other modules (plugins, etc.) AFTER CMS setup is complete
// Force load external Legendary module assemblies to ensure they're available for discovery
// This is necessary because .NET doesn't auto-load referenced DLLs unless they're explicitly used
try
{
    System.Reflection.Assembly.Load("LegendaryChat");
    System.Reflection.Assembly.Load("LegendaryLearning");
    System.Reflection.Assembly.Load("LegendaryGameServer");
    System.Reflection.Assembly.Load("LegendaryGameClient");
    // LegendaryCMS, LegendaryGameEngine, and LegendaryClientBuilder are already loaded elsewhere
}
catch (Exception ex)
{
    Console.WriteLine($"[RaCore] Warning: Could not preload some external modules: {ex.Message}");
}

moduleManager.LoadModules();

var builder = WebApplication.CreateBuilder(args);

// 5. Run boot sequence with self-healing checks and configuration verification
// This will detect the port from Nginx configuration (MUST run before building app)
var bootSequence = new RaCore.Engine.BootSequenceManager(moduleManager);
await bootSequence.ExecuteBootSequenceAsync();

// 6. Configure port - use detected port from Nginx config or fallback to default
// Nginx configuration is the source of truth for port management
var port = Environment.GetEnvironmentVariable("RACORE_DETECTED_PORT") ?? "80";
var urls = $"http://0.0.0.0:{port}";

Console.WriteLine($"[RaCore] Configuring Kestrel to listen on: {urls}");
Console.WriteLine($"[RaCore] This will bind to ALL network interfaces (0.0.0.0)");

// Add CORS support for agpstudios.online domain and dynamic port
// Check if we should use permissive CORS (for development/debugging)
var allowPermissiveCors = Environment.GetEnvironmentVariable("RACORE_PERMISSIVE_CORS")?.ToLower() == "true";

var allowedOrigins = new List<string>
{
    $"http://localhost:{port}",
    "http://localhost",
    $"http://127.0.0.1:{port}",
    "http://127.0.0.1",
    "http://agpstudios.online",
    "https://agpstudios.online"
};

builder.Services.AddCors(options =>
{
    if (allowPermissiveCors)
    {
        Console.WriteLine("[RaCore] CORS: Using permissive mode (RACORE_PERMISSIVE_CORS=true)");
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        Console.WriteLine($"[RaCore] CORS: Allowing specific origins: {string.Join(", ", allowedOrigins)}");
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins.ToArray())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// Configure URLs with dynamic port
builder.WebHost.UseUrls(urls);

var app = builder.Build();
app.UseCors(); // Enable CORS
app.UseWebSockets();

Console.WriteLine($"[RaCore] Kestrel webserver starting...");
Console.WriteLine($"[RaCore] Server will be accessible at:");
Console.WriteLine($"  - http://localhost:{port}");
Console.WriteLine($"  - http://127.0.0.1:{port}");
Console.WriteLine($"  - http://<your-server-ip>:{port}");
Console.WriteLine($"[RaCore] Ensure firewall allows inbound connections on port {port}");

// NOTE: RaCore uses Kestrel webserver internally to host both the CMS and API endpoints.
// On Windows 11, Kestrel is the only supported webserver.
// External Apache/PHP8 is optional on Linux for PHP file execution only.

// URL redirects for cleaner API access
app.MapGet("/control-panel", async context =>
{
    await Task.CompletedTask;
    context.Response.Redirect("/control-panel.html");
});

// 7. Wire up FirstRunManager to ServerConfig and License modules
var serverConfigModule = moduleManager.Modules
    .Select(m => m.Instance)
    .FirstOrDefault(m => m.Name == "ServerConfig");

if (serverConfigModule != null)
{
    var setFirstRunManagerMethod = serverConfigModule.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(serverConfigModule, new object[] { firstRunManager });
    Console.WriteLine("[RaCore] FirstRunManager wired to ServerConfig module");
}

var licenseModuleInstance = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<Abstractions.ILicenseModule>()
    .FirstOrDefault();

if (licenseModuleInstance != null)
{
    var setFirstRunManagerMethod = licenseModuleInstance.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(licenseModuleInstance, new object[] { firstRunManager });
    Console.WriteLine("[RaCore] FirstRunManager wired to License module");
}

// Optionally pick up a SpeechModule if present
ISpeechModule? speechModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ISpeechModule>()
    .FirstOrDefault();


// Get authentication module if present
IAuthenticationModule? authModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IAuthenticationModule>()
    .FirstOrDefault();

var wsHandler = new RaCoreWebSocketHandler(moduleManager, speechModule);

// Map WebSocket endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await wsHandler.HandleAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Authentication API endpoints
app.MapAuthEndpoints(authModule);



// Get game engine module if present
IGameEngineModule? gameEngineModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameEngineModule>()
    .FirstOrDefault();

// Game Engine API endpoints
app.MapGameEngineEndpoints(gameEngineModule, authModule);


// ServerSetup API endpoints
IServerSetupModule? serverSetupModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IServerSetupModule>()
    .FirstOrDefault();

app.MapServerSetupEndpoints(serverSetupModule, authModule);

// GameServer API endpoints
IGameServerModule? gameServerModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameServerModule>()
    .FirstOrDefault();

app.MapGameServerEndpoints(gameServerModule, authModule);

// Helper method to check if CMS is available
// LegendaryCMS runs as a module, no PHP files needed
static bool IsCmsAvailable(ModuleManager moduleManager)
{
    // Check if LegendaryCMS module is loaded and initialized
    var cmsModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<LegendaryCMS.Core.ILegendaryCMSModule>()
        .FirstOrDefault();
    
    if (cmsModule != null)
    {
        var status = cmsModule.GetStatus();
        return status.IsInitialized && status.IsRunning;
    }
    
    return false;
}

// Root endpoint - always register to handle Under Construction and CMS routing
// Phase 9.3.9: Unified homepage handler for all scenarios
app.MapGet("/", async (HttpContext context) =>
{
    try
    {
        // Phase 9.3.8: Check for Under Construction mode FIRST
        // This check must happen before any response headers or body are written
        var serverConfig = firstRunManager.GetServerConfiguration();
        if (serverConfig.UnderConstruction)
        {
            // Check if user is an admin - admins can still access during construction
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
            
            User? user = null;
            if (!string.IsNullOrWhiteSpace(token) && authModule != null)
            {
                user = await authModule.GetUserByTokenAsync(token);
            }
            
            // Non-admins see Under Construction page
            if (user == null || user.Role < UserRole.Admin)
            {
                // Set ContentType only when we're about to write the response
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(UnderConstructionHandler.GenerateUnderConstructionPage(serverConfig));
                return;
            }
            
            // Admins can access normally - continue to CMS or fallback
        }
        
        // Phase 9.3.9: Check if LegendaryCMS is available
        // LegendaryCMS runs as C# module, serves content via API endpoints
        // Static HTML from wwwroot calls these API endpoints
        if (IsCmsAvailable(moduleManager))
        {
            Console.WriteLine("[RaCore] LegendaryCMS available - serving static site (wwwroot/index.html)");
            // Serve static HTML which will call LegendaryCMS API endpoints
            var indexHtmlPath = Path.Combine(wwwrootPath, "index.html");
            if (File.Exists(indexHtmlPath))
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(indexHtmlPath);
                return;
            }
        }
        
        // Fallback: CMS not available, use legacy bot-filtering homepage
        // Phase 9.3.7: Homepage bot filtering (legacy behavior)
        // Only allow search engine bots to access homepage for SEO indexing
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var isBot = BotDetector.IsSearchEngineBot(userAgent);
        
        // Set ContentType only when we're about to write HTML response
        context.Response.ContentType = "text/html";
        
        if (!isBot)
        {
            // Non-bot visitors get access denied message with control panel link
            await context.Response.WriteAsync(BotDetector.GetAccessDeniedMessage());
            return;
        }
        
        // Search engine bots get the full homepage for indexing
        var botName = BotDetector.GetBotName(userAgent);
        Console.WriteLine($"[RaCore] Search engine bot detected: {botName} - Allowing homepage access");
    
        await context.Response.WriteAsync($@"<!DOCTYPE html>
<html>
<head>
    <title>RaCore - Advanced Modular Server Platform</title>
    <meta name=""description"" content=""RaCore is a powerful, modular server platform with CMS, forums, game engine, and extensive plugin system."">
    <meta name=""keywords"" content=""RaCore, CMS, modular server, game engine, forum platform, control panel"">
    <meta name=""robots"" content=""index, follow"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 800px;
            margin: 50px auto;
            padding: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #333;
        }}
        .container {{
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
        }}
        h1 {{
            color: #667eea;
            margin-bottom: 10px;
        }}
        .info {{
            background: #d1ecf1;
            border-left: 4px solid #17a2b8;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .success {{
            background: #d4edda;
            border-left: 4px solid #28a745;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        code {{
            background: #f4f4f4;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
        }}
        ul {{
            line-height: 1.8;
        }}
        .features {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }}
        .feature {{
            padding: 15px;
            background: #f8f9fa;
            border-radius: 5px;
            border-left: 3px solid #667eea;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌟 Welcome to RaCore</h1>
        <p>RaCore is a powerful, modular server platform running on port {port}.</p>
        
        <div class='info'>
            <h3>📋 About RaCore:</h3>
            <p>RaCore is an advanced, extensible server platform that combines CMS functionality, 
            forum systems, user profiles, game engine capabilities, and a comprehensive control panel 
            into a unified, modular architecture.</p>
            <p>Built with .NET 9.0, RaCore provides a robust foundation for web applications, 
            game servers, and community platforms.</p>
        </div>
        
        <div class='success'>
            <h3>🎯 Core Features:</h3>
            <div class='features'>
                <div class='feature'>
                    <h4>🎛️ Control Panel</h4>
                    <p>Comprehensive admin dashboard for managing all aspects of your RaCore instance.</p>
                </div>
                <div class='feature'>
                    <h4>📝 CMS System</h4>
                    <p>Full-featured content management system with SQLite database backend.</p>
                </div>
                <div class='feature'>
                    <h4>💬 Forum Platform</h4>
                    <p>Built-in forum system with categories, threads, and user moderation.</p>
                </div>
                <div class='feature'>
                    <h4>👤 User Profiles</h4>
                    <p>Rich user profile system with authentication and role-based permissions.</p>
                </div>
                <div class='feature'>
                    <h4>🎮 Game Engine</h4>
                    <p>Integrated game engine with scene management, persistence, and WebSocket support.</p>
                </div>
                <div class='feature'>
                    <h4>🔌 Plugin System</h4>
                    <p>Extensible module architecture for adding custom functionality.</p>
                </div>
            </div>
        </div>
        
        <div class='info'>
            <h3>🚀 Technology Stack:</h3>
            <ul>
                <li><strong>.NET 9.0:</strong> Modern, high-performance runtime</li>
                <li><strong>ASP.NET Core:</strong> Web framework with WebSocket support</li>
                <li><strong>SQLite:</strong> Embedded database for data persistence</li>
                <li><strong>PHP + Nginx:</strong> CMS frontend delivery</li>
                <li><strong>Modular Architecture:</strong> Plugin-based extensibility</li>
            </ul>
        </div>
        
        <div class='info'>
            <h3>📚 Key Components:</h3>
            <ul>
                <li><strong>Authentication Module:</strong> Secure user authentication with token-based sessions</li>
                <li><strong>SiteBuilder Module:</strong> Automated CMS and website generation</li>
                <li><strong>RaCoin System:</strong> Integrated virtual currency platform</li>
                <li><strong>LegendaryEngine:</strong> Game development framework</li>
                <li><strong>Supermarket Module:</strong> E-commerce capabilities</li>
                <li><strong>Learning Module:</strong> Educational content delivery</li>
            </ul>
        </div>
        
        <p style='margin-top: 30px; text-align: center; color: #6c757d;'>
            <small>RaCore v1.0 - Phase 9.4.0 | 
            <a href='https://github.com/buffbot88/TheRaProject' target='_blank'>View on GitHub</a> | 
            Bot Access Control Enabled</small>
        </p>
    </div>
</body>
</html>");
    }
    catch (Exception ex)
    {
        // Error handling to prevent server crashes if header setting fails
        Console.WriteLine($"[RaCore] Error handling homepage request: {ex.Message}");
        // Only try to set status code if response hasn't started
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body><h1>500 Internal Server Error</h1><p>An error occurred while loading the homepage.</p></body></html>");
        }
    }
});

// ============================================================================
// CONTROL PANEL API ENDPOINTS
// ============================================================================

// Get RaCoin module if present
IRaCoinModule? racoinModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IRaCoinModule>()
    .FirstOrDefault();

// Get License module if present
ILicenseModule? licenseModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ILicenseModule>()
    .FirstOrDefault();

// Server Configuration API Endpoints
app.MapGet("/api/control/server/config", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Insufficient permissions" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        await context.Response.WriteAsJsonAsync(new { success = true, config });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapPost("/api/control/server/mode", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.SuperAdmin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "SuperAdmin role required" });
            return;
        }

        var request = await context.Request.ReadFromJsonAsync<ServerModeChangeRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
            return;
        }

        if (!Enum.TryParse<ServerMode>(request.Mode, true, out var mode))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid server mode" });
            return;
        }

        firstRunManager.SetServerMode(mode);
        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true, 
            message = $"Server mode changed to {mode}",
            mode = mode.ToString(),
            requiresRestart = true
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapGet("/api/control/server/modes", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Insufficient permissions" });
            return;
        }

        var modes = Enum.GetValues<ServerMode>().Select(m => new
        {
            value = m.ToString(),
            name = m.ToString(),
            description = m switch
            {
                ServerMode.Alpha => "Early development and testing with full logging",
                ServerMode.Beta => "Pre-release testing with selected users",
                ServerMode.Omega => "Main server configuration (US-Omega)",
                ServerMode.Demo => "Demonstration instance with limited features",
                ServerMode.Production => "Full production deployment",
                _ => ""
            }
        });

        await context.Response.WriteAsJsonAsync(new { success = true, modes });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Under Construction Mode API endpoints (Phase 9.3.8)
app.MapPost("/api/control/server/underconstruction", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
            return;
        }

        var request = await context.Request.ReadFromJsonAsync<UnderConstructionRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        config.UnderConstruction = request.Enabled;
        
        if (request.Message != null)
        {
            config.UnderConstructionMessage = request.Message;
        }

        // Save the configuration
        firstRunManager.SaveConfiguration();

        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true, 
            message = request.Enabled ? "Under Construction mode enabled" : "Under Construction mode disabled",
            underConstruction = config.UnderConstruction,
            customMessage = config.UnderConstructionMessage,
            customRobotImage = config.UnderConstructionRobotImage
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapGet("/api/control/server/underconstruction", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true,
            underConstruction = config.UnderConstruction,
            message = config.UnderConstructionMessage,
            robotImage = config.UnderConstructionRobotImage
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

Console.WriteLine("[RaCore] Server Configuration API endpoints registered:");
Console.WriteLine("  GET  /api/control/server/config - Get server configuration (Admin+)");
Console.WriteLine("  POST /api/control/server/mode - Change server mode (SuperAdmin only)");
Console.WriteLine("  GET  /api/control/server/modes - List available server modes (Admin+)");
Console.WriteLine("  POST /api/control/server/underconstruction - Toggle Under Construction mode (Admin+)");
Console.WriteLine("  GET  /api/control/server/underconstruction - Get Under Construction status (Admin+)");



// Control Panel API endpoints
app.MapControlPanelEndpoints(moduleManager, authModule, licenseModule, racoinModule, gameEngineModule);

// ============================================================================
// Distribution & Update API Endpoints
// ============================================================================

var distributionModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IDistributionModule>()
    .FirstOrDefault();

var updateModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IUpdateModule>()
    .FirstOrDefault();

app.MapDistributionEndpoints(distributionModule, updateModule, authModule);

// ============================================================================
// GameClient API Endpoints
// ============================================================================

var gameClientModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameClientModule>()
    .FirstOrDefault();

app.MapGameClientEndpoints(gameClientModule, authModule);

app.Run();
