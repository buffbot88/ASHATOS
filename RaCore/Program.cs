using Abstractions;
using RaCore.Endpoints;
using RaCore.Engine;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using RaCore.Models;
using RaCore.Modules.Extensions.UserProfiles;
using SQLitePCL;
using System.Text.Json;

// Ensure wwwroot directory exists (used for config files only, not for static HTML)
// All UI is served dynamically through Window of Ra (SiteBuilder module)
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
else
{
    // Always ensure Window of Ra (SiteBuilder) is initialized on boot
    Console.WriteLine("[RaCore] Initializing Window of Ra (SiteBuilder)...");
    await firstRunManager.EnsureWwwrootAsync();
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

// URL redirects removed - all UI access goes through SiteBuilder module (Window of Ra)
// The SiteBuilder module will handle all UI routes internally

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

// Generate homepage dynamically through Window of Ra (SiteBuilder)
// No static files - all content is served dynamically via internal routing
static string GenerateDynamicHomepage()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore - AI Mainframe</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
        }
        .container {
            text-align: center;
            padding: 40px;
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
            max-width: 600px;
        }
        h1 {
            font-size: 3em;
            margin-bottom: 20px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }
        p {
            font-size: 1.2em;
            line-height: 1.6;
            margin-bottom: 30px;
        }
        .cta-button {
            display: inline-block;
            padding: 15px 40px;
            background: white;
            color: #667eea;
            text-decoration: none;
            border-radius: 50px;
            font-weight: bold;
            font-size: 1.1em;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }
        .cta-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0,0,0,0.3);
        }
        .features {
            margin-top: 30px;
            display: grid;
            grid-template-columns: 1fr;
            gap: 15px;
            text-align: left;
        }
        .feature {
            background: rgba(255, 255, 255, 0.15);
            padding: 15px;
            border-radius: 10px;
            backdrop-filter: blur(5px);
        }
        .feature h3 {
            margin-bottom: 5px;
            font-size: 1.2em;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌟 RaCore</h1>
        <p>Welcome to the Window of Ra - Your gateway to the RaOS platform</p>
        <p>Advanced AI Mainframe with CMS, Forums, Chat, and Game Engine</p>
        
        <a href='/login' class='cta-button'>Access Control Panel</a>
        
        <div class='features'>
            <div class='feature'>
                <h3>🎮 Game Engine</h3>
                <p>Full-featured 2D/3D game engine with physics and multiplayer</p>
            </div>
            <div class='feature'>
                <h3>💬 Community</h3>
                <p>Forums, blogs, and real-time chat built-in</p>
            </div>
            <div class='feature'>
                <h3>🔒 Secure</h3>
                <p>Enterprise-grade security and authentication</p>
            </div>
        </div>
        
        <p style='margin-top: 30px; font-size: 0.9em; opacity: 0.8;'>
            All features accessed through the Window of Ra (SiteBuilder module)<br>
            No external routing or static files
        </p>
    </div>
</body>
</html>";
}

// Generate Login UI dynamically
static string GenerateLoginUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Login - RaCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .login-container {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
            width: 100%;
            max-width: 400px;
        }
        h1 { color: #667eea; margin-bottom: 30px; text-align: center; }
        .form-group { margin-bottom: 20px; }
        label { display: block; margin-bottom: 5px; color: #333; font-weight: 600; }
        input {
            width: 100%;
            padding: 12px;
            border: 2px solid #e0e0e0;
            border-radius: 5px;
            font-size: 14px;
            transition: border-color 0.3s;
        }
        input:focus { outline: none; border-color: #667eea; }
        button {
            width: 100%;
            padding: 12px;
            background: #667eea;
            color: white;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.3s;
        }
        button:hover { background: #5568d3; }
        .error { color: #e74c3c; margin-top: 10px; display: none; }
        .back-link { text-align: center; margin-top: 20px; }
        .back-link a { color: #667eea; text-decoration: none; }
    </style>
</head>
<body>
    <div class=""login-container"">
        <h1>🌟 RaCore Login</h1>
        <form id=""loginForm"">
            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""username"" required>
            </div>
            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""password"" id=""password"" name=""password"" required>
            </div>
            <button type=""submit"">Login</button>
            <div class=""error"" id=""error""></div>
        </form>
        <div class=""back-link"">
            <a href=""/"">← Back to Home</a>
        </div>
    </div>
    <script>
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const error = document.getElementById('error');
            
            try {
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username, password })
                });
                
                const data = await response.json();
                if (data.success) {
                    localStorage.setItem('racore_token', data.token);
                    window.location.href = '/control-panel';
                } else {
                    error.textContent = data.message || 'Login failed';
                    error.style.display = 'block';
                }
            } catch (err) {
                error.textContent = 'Connection error. Please try again.';
                error.style.display = 'block';
            }
        });
    </script>
</body>
</html>";
}

// Generate Control Panel UI dynamically
static string GenerateControlPanelUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Control Panel - RaCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f5f5f5;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container {
            max-width: 1200px;
            margin: 20px auto;
            padding: 20px;
        }
        .stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .stat-card {
            background: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .stat-card h3 { color: #667eea; margin-bottom: 10px; }
        .modules {
            background: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .module { padding: 10px; border-bottom: 1px solid #eee; }
        .logout { 
            position: absolute;
            top: 20px;
            right: 20px;
            color: white;
            text-decoration: none;
            padding: 10px 20px;
            background: rgba(255,255,255,0.2);
            border-radius: 5px;
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎛️ RaCore Control Panel</h1>
        <p>Window of Ra - Internal Module Interface</p>
        <a href=""#"" class=""logout"" onclick=""logout()"">Logout</a>
    </div>
    <div class=""container"">
        <div class=""stats"">
            <div class=""stat-card"">
                <h3>System Status</h3>
                <p id=""status"">Loading...</p>
            </div>
            <div class=""stat-card"">
                <h3>Loaded Modules</h3>
                <p id=""modules"">Loading...</p>
            </div>
            <div class=""stat-card"">
                <h3>Active Users</h3>
                <p id=""users"">Loading...</p>
            </div>
        </div>
        <div class=""modules"">
            <h2>Available Modules</h2>
            <div id=""moduleList"">Loading modules...</div>
        </div>
    </div>
    <script>
        async function checkAuth() {
            const token = localStorage.getItem('racore_token');
            if (!token) {
                window.location.href = '/login';
                return;
            }
        }
        async function loadStats() {
            try {
                const token = localStorage.getItem('racore_token');
                const response = await fetch('/api/control/stats', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                const data = await response.json();
                if (data.success) {
                    document.getElementById('status').textContent = 'Online';
                    document.getElementById('modules').textContent = 'Loading...';
                    document.getElementById('users').textContent = data.stats?.totalUsers || '0';
                }
            } catch (err) {
                console.error('Error loading stats:', err);
            }
        }
        function logout() {
            localStorage.removeItem('racore_token');
            window.location.href = '/login';
        }
        checkAuth();
        loadStats();
    </script>
</body>
</html>";
}

// Generate Admin UI dynamically
static string GenerateAdminUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Admin Dashboard - RaCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f5f5f5;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        h2 { color: #667eea; margin-bottom: 15px; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>⚙️ Admin Dashboard</h1>
        <p>Window of Ra - Administrative Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Server Management</h2>
            <p>Advanced server configuration and monitoring tools.</p>
        </div>
        <div class=""card"">
            <h2>User Management</h2>
            <p>Manage users, roles, and permissions.</p>
        </div>
        <div class=""card"">
            <h2>Module Management</h2>
            <p>Load, unload, and configure modules.</p>
        </div>
    </div>
</body>
</html>";
}

// Generate Game Engine Dashboard UI dynamically
static string GenerateGameEngineDashboardUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Game Engine Dashboard - RaCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f5f5f5;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎮 Game Engine Dashboard</h1>
        <p>Window of Ra - Game Engine Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Active Scenes</h2>
            <p>Manage game scenes, entities, and physics.</p>
        </div>
        <div class=""card"">
            <h2>Multiplayer Sessions</h2>
            <p>Monitor active game sessions and players.</p>
        </div>
    </div>
</body>
</html>";
}

// Generate Client Builder Dashboard UI dynamically
static string GenerateClientBuilderDashboardUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Client Builder Dashboard - RaCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f5f5f5;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🛠️ Client Builder Dashboard</h1>
        <p>Window of Ra - Client Generation Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Client Templates</h2>
            <p>Select and customize client templates.</p>
        </div>
        <div class=""card"">
            <h2>Build Queue</h2>
            <p>Monitor client build progress and downloads.</p>
        </div>
    </div>
</body>
</html>";
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
        // All UI is served dynamically through SiteBuilder (Window of Ra) - no static files
        if (IsCmsAvailable(moduleManager))
        {
            Console.WriteLine("[RaCore] LegendaryCMS available - serving homepage via Window of Ra (SiteBuilder)");
            // Serve homepage dynamically through internal routing
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(GenerateDynamicHomepage());
            return;
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
// WINDOW OF RA - UI ROUTES (Dynamic, no static files)
// All UI features accessed through internal SiteBuilder module routing
// ============================================================================

// Control Panel UI - served dynamically
app.MapGet("/control-panel", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateControlPanelUI());
});

// Login UI - served dynamically
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateLoginUI());
});

// Admin UI - served dynamically
app.MapGet("/admin", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateAdminUI());
});

// Game Engine Dashboard UI - served dynamically
app.MapGet("/gameengine-dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateGameEngineDashboardUI());
});

// Client Builder Dashboard UI - served dynamically
app.MapGet("/clientbuilder-dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateClientBuilderDashboardUI());
});

Console.WriteLine("[RaCore] Window of Ra UI routes registered (dynamic, no static files):");
Console.WriteLine("  GET  /control-panel - Control Panel UI (served dynamically)");
Console.WriteLine("  GET  /login - Login UI (served dynamically)");
Console.WriteLine("  GET  /admin - Admin UI (served dynamically)");
Console.WriteLine("  GET  /gameengine-dashboard - Game Engine Dashboard UI (served dynamically)");
Console.WriteLine("  GET  /clientbuilder-dashboard - Client Builder Dashboard UI (served dynamically)");

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
