using Abstractions;
using ASHATCore.Endpoints;
using ASHATCore.Engine;
using ASHATCore.Engine.Manager;
using ASHATCore.Engine.Memory;
using ASHATCore.Models;
using ASHATCore.Modules.Extensions.UserProfiles;
using SQLitePCL;
using System.Text.Json;

// Ensure wwwroot directory exists (used for config files only, not for static HTML)
// All UI is served dynamically through (SiteBuilder module)
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"[ASHATCore] Created wwwroot directory: {wwwrootPath}");
}

// 1. Instantiate MemoryModule FIRST
var memoryModule = new MemoryModule();
memoryModule.Initialize(null); // Pass ModuleManager if needed

// 2. Instantiate ModuleManager and register MemoryModule as built-in
var moduleManager = new ModuleManager();
moduleManager.RegisterBuiltInModule(memoryModule);

// 3. Check for first run and auto-spawn CMS BEFORE loading other modules
// This ensures all required files and configs are created before any module initialization
var firstRunManager = new ASHATCore.Engine.FirstRunManager(moduleManager);

if (firstRunManager.IsFirstRun())
{
    Console.WriteLine("[ASHATCore] First run detected - initializing CMS homepage...");
    await firstRunManager.InitializeAsync();
}
else
{
    // Always ensure (SiteBuilder) is initialized on boot
    Console.WriteLine("[ASHATCore] Initializing (SiteBuilder)...");
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
    Console.WriteLine($"[ASHATCore] Warning: Could not preload some external modules: {ex.Message}");
}

moduleManager.LoadModules();

var builder = WebApplication.CreateBuilder(args);

// 5. Run boot sequence with self-healing checks and Configuration verification
// This will detect the port from Nginx Configuration (MUST run before building app)
var bootSequence = new ASHATCore.Engine.BootSequenceManager(moduleManager);
await bootSequence.ExecuteBootSequenceAsync();

// 6. Configure port - use detected port from Nginx config or fallback to default
// Nginx Configuration is the source of truth for port management
// Assume 'config' is loaded from server-config.json

// 1. Try environment variable
string port = Environment.GetEnvironmentVariable("ASHATCore_DETECTED_PORT");

// 2. If not set, try config file
if (string.IsNullOrEmpty(port))
{
    var serverRoot = Directory.GetCurrentDirectory();
    var configPath = Path.Combine(serverRoot, "server-config.json");
    if (File.Exists(configPath))
    {
        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("Port", out var portProp))
        {
            port = portProp.ToString();
        }
    }
}

// 3. Default fallback
if (string.IsNullOrEmpty(port))
    port = "7077";
var urls = $"http://0.0.0.0:{port}";

Console.WriteLine($"[ASHATCore] Configuring Kestrel to listen on: {urls}");
Console.WriteLine($"[ASHATCore] This will bind to ALL network interfaces (0.0.0.0)");

// Add CORS support for agpstudios.online domain and dynamic port
// Check if we should use permissive CORS (for development/debugging)
var allowPermissiveCors = Environment.GetEnvironmentVariable("ASHATCore_PERMISSIVE_CORS")?.ToLower() == "true";

var allowedOrigins = new List<string>
{
    // Localhost with dynamic port
    $"http://localhost:{port}",
    "http://localhost",
    $"http://127.0.0.1:{port}",
    "http://127.0.0.1",
    // AGP Studios domains
    "http://agpstudios.online", //for server updates cloud-wide
    "https://agpstudios.online" //SSL must be enabled for incoming requests
};

builder.Services.AddCors(options =>
{
    if (allowPermissiveCors)
    {
        Console.WriteLine("[ASHATCore] CORS: Using permissive mode (ASHATCore_PERMISSIVE_CORS=true)");
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        Console.WriteLine($"[ASHATCore] CORS: Allowing specific origins: {string.Join(", ", allowedOrigins)}");
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

Console.WriteLine($"[ASHATCore] Kestrel webserver starting...");
Console.WriteLine($"[ASHATCore] Server will be accessible at:");
Console.WriteLine($"  - http://localhost:{port}");
Console.WriteLine($"  - http://127.0.0.1:{port}");
Console.WriteLine($"  - http://<your-server-ip>:{port}");
Console.WriteLine($"[ASHATCore] Ensure firewall allows inbound connections on port {port}");

// NOTE: ASHATCore uses Kestrel webserver internally to host both the CMS and API endpoints.
// On Windows 11, Kestrel is the only supported webserver.
// External Apache/PHP8 is optional on Linux for PHP file execution only.

// URL redirects removed - all UI access goes through module (SiteBuilder)
// The module will handle all UI routes internally

// 7. Wire up FirstRunManager to ServerConfig and License modules
var serverConfigModule = moduleManager.Modules
    .Select(m => m.Instance)
    .FirstOrDefault(m => m.Name == "ServerConfig");

if (serverConfigModule != null)
{
    var setFirstRunManagerMethod = serverConfigModule.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(serverConfigModule, new object[] { firstRunManager });
    Console.WriteLine("[ASHATCore] FirstRunManager wired to ServerConfig module");
}

var licenseModuleInstance = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ILicenseModule>()
    .FirstOrDefault();

if (licenseModuleInstance != null)
{
    var setFirstRunManagerMethod = licenseModuleInstance.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(licenseModuleInstance, [firstRunManager]);
    Console.WriteLine("[ASHATCore] FirstRunManager wired to License module");
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

var wsHandler = new ASHATCoreWebSocketHandler(moduleManager, speechModule);

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

// Generate homepage dynamically through (SiteBuilder)
// No static files - all content is served dynamically via internal routing
static string GeneratedynamicHomepage()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>AGP Studios, INC - Unity meets Software</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
        }
        .container {
            text-align: center;
            padding: 50px;
            background: rgba(20, 0, 40, 0.8);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 8px 32px rgba(138, 43, 226, 0.4);
            max-width: 700px;
            width: 90%;
        }
        h1 {
            font-size: 3.5em;
            margin-bottom: 20px;
            text-shadow: 0 0 20px rgba(138, 43, 226, 0.8);
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .tagline {
            font-size: 1.3em;
            line-height: 1.6;
            margin-bottom: 15px;
            color: #e0d0ff;
        }
        .description {
            font-size: 1.1em;
            line-height: 1.6;
            margin-bottom: 35px;
            color: #c8b6ff;
        }
        .cta-button {
            display: inline-block;
            padding: 18px 45px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            text-decoration: none;
            border-radius: 50px;
            font-weight: bold;
            font-size: 1.1em;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.5);
            border: 2px solid rgba(138, 43, 226, 0.5);
        }
        .cta-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 25px rgba(138, 43, 226, 0.8);
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
        }
        .features {
            margin-top: 40px;
            display: grid;
            grid-template-columns: 1fr;
            gap: 20px;
            text-align: left;
        }
        .feature {
            background: rgba(138, 43, 226, 0.1);
            padding: 20px;
            border-radius: 12px;
            backdrop-filter: blur(5px);
            border: 1px solid rgba(138, 43, 226, 0.3);
            transition: all 0.3s ease;
        }
        .feature:hover {
            background: rgba(138, 43, 226, 0.2);
            border-color: rgba(138, 43, 226, 0.5);
            transform: translateX(5px);
        }
        .feature h3 {
            margin-bottom: 8px;
            font-size: 1.3em;
            color: #c084fc;
        }
        .feature p {
            color: #d8c8ff;
            font-size: 1em;
        }
        .footer {
            margin-top: 40px;
            font-size: 0.9em;
            opacity: 0.7;
            color: #b8a8d8;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌟 AGP Studios, INC</h1>
        <p class='tagline'>Unity meets Software</p>
        <p class='description'>We are a cloud-based, mesh network, super intelligence that is here to help build the future of interactive experiences.</p>
        
        <a href='/login' class='cta-button'>Login to Your Account</a>
        
        <div class='features'>
            <div class='feature'>
                <h3>🎮 Game Engine Suite</h3>
                <p>Full-featured 2D/3D game engine with physics, AI generation, and multiplayer capabilities</p>
            </div>
            <div class='feature'>
                <h3>💬 Community Platform</h3>
                <p>Forums, blogs, real-time chat, and user profiles - all integrated seamlessly</p>
            </div>
            <div class='feature'>
                <h3>🔒 Enterprise Security</h3>
                <p>Bank-grade security with advanced authentication and authorization systems</p>
            </div>
            <div class='feature'>
                <h3>🚀 Modular Architecture</h3>
                <p>Plugin-based system with REST APIs and real-time WebSocket support</p>
            </div>
        </div>
        
        <p class='footer'>
            Copyright © 2024 AGP Studios, INC. All rights reserved.
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
    <title>Login - AGP Studios, INC</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .login-container {
            background: rgba(20, 0, 40, 0.9);
            padding: 45px;
            border-radius: 15px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 10px 40px rgba(138, 43, 226, 0.4);
            width: 100%;
            max-width: 420px;
        }
        h1 { 
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 35px; 
            text-align: center;
            font-size: 2em;
        }
        .form-group { margin-bottom: 25px; }
        label { display: block; margin-bottom: 8px; color: #e0d0ff; font-weight: 600; }
        input {
            width: 100%;
            padding: 14px;
            background: rgba(0, 0, 0, 0.3);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 8px;
            font-size: 15px;
            color: white;
            transition: border-color 0.3s;
        }
        input:focus { 
            outline: none; 
            border-color: rgba(138, 43, 226, 0.8);
            background: rgba(0, 0, 0, 0.4);
        }
        button {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        button:hover { 
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
            transform: translateY(-2px);
        }
        .error { color: #ff6b6b; margin-top: 15px; display: none; text-align: center; }
        .back-link { text-align: center; margin-top: 25px; }
        .back-link a { color: #c084fc; text-decoration: none; transition: color 0.3s; }
        .back-link a:hover { color: #d8b4fe; }
    </style>
</head>
<body>
    <div class=""login-container"">
        <h1>🌟 Website Login</h1>
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
                    localStorage.setItem('ASHATCore_token', data.token);
                    
                    // Check if user needs to complete LULModule onboarding
                    if (data.requiresLULModule) {
                        window.location.href = '/onboarding';
                    } else {
                        window.location.href = '/control-panel';
                    }
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

// Generate Onboarding UI dynamically
static string GenerateOnboardingUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Masters Class Onboarding - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .onboarding-container {
            background: rgba(20, 0, 40, 0.9);
            border-radius: 15px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 10px 40px rgba(138, 43, 226, 0.4);
            max-width: 900px;
            width: 100%;
            padding: 40px;
        }
        h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
            font-size: 28px;
        }
        .subtitle {
            color: #c8b6ff;
            margin-bottom: 30px;
            font-size: 16px;
        }
        .progress-bar {
            width: 100%;
            height: 8px;
            background: rgba(0, 0, 0, 0.3);
            border-radius: 4px;
            margin-bottom: 30px;
            overflow: hidden;
        }
        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #8b2fc7 0%, #6a1b9a 100%);
            width: 0%;
            transition: width 0.3s ease;
        }
        .course-list {
            display: grid;
            gap: 15px;
            margin-bottom: 30px;
        }
        .course-card {
            background: rgba(138, 43, 226, 0.1);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 10px;
            padding: 20px;
            cursor: pointer;
            transition: all 0.3s;
        }
        .course-card:hover {
            border-color: rgba(138, 43, 226, 0.6);
            box-shadow: 0 4px 12px rgba(138, 43, 226, 0.3);
            transform: translateX(5px);
        }
        .course-card.completed {
            background: rgba(34, 197, 94, 0.1);
            border-color: #22c55e;
        }
        .course-card.active {
            border-color: #8b2fc7;
            background: rgba(138, 43, 226, 0.2);
        }
        .course-title {
            font-size: 18px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 8px;
        }
        .course-description {
            color: #c8b6ff;
            font-size: 14px;
            margin-bottom: 10px;
        }
        .course-meta {
            display: flex;
            gap: 20px;
            font-size: 13px;
            color: #b8a8d8;
        }
        .lesson-viewer {
            display: none;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 10px;
            padding: 30px;
            margin-bottom: 30px;
            background: rgba(138, 43, 226, 0.1);
        }
        .lesson-title {
            font-size: 22px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 15px;
        }
        .lesson-content {
            color: #d8c8ff;
            line-height: 1.8;
            margin-bottom: 25px;
            white-space: pre-wrap;
        }
        .lesson-navigation {
            display: flex;
            gap: 10px;
            justify-content: space-between;
            align-items: center;
        }
        button {
            padding: 12px 24px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        button:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        button:disabled {
            background: #444;
            border-color: #555;
            cursor: not-allowed;
            opacity: 0.5;
        }
        button.secondary {
            background: #333;
            border-color: #444;
        }
        button.secondary:hover {
            background: #444;
        }
        button.success {
            background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
            border-color: rgba(34, 197, 94, 0.5);
        }
        button.success:hover {
            background: linear-gradient(135deg, #4ade80 0%, #22c55e 100%);
        }
        .status-message {
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .status-message.success {
            background: rgba(34, 197, 94, 0.2);
            color: #4ade80;
            border: 1px solid rgba(34, 197, 94, 0.5);
        }
        .status-message.error {
            background: rgba(239, 68, 68, 0.2);
            color: #fca5a5;
            border: 1px solid rgba(239, 68, 68, 0.5);
        }
        .loading {
            text-align: center;
            padding: 40px;
            color: #c8b6ff;
        }
        .completion-badge {
            display: inline-block;
            background: #22c55e;
            color: white;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
        }
    </style>
</head>
<body>
    <div class=""onboarding-container"">
        <h1>🎓 Masters Class Onboarding</h1>
        <p class=""subtitle"">Welcome to AGP Studios! Complete these courses to access the Control Panel.</p>
        
        <div class=""progress-bar"">
            <div class=""progress-fill"" id=""progressBar""></div>
        </div>
        
        <div class=""status-message"" id=""statusMessage""></div>
        
        <div id=""loadingMessage"" class=""loading"">Loading your courses...</div>
        
        <div id=""courseList"" class=""course-list"" style=""display: none;""></div>
        
        <div id=""lessonViewer"" class=""lesson-viewer""></div>
        
        <div style=""text-align: center; margin-top: 20px;"">
            <button id=""finishButton"" style=""display: none;"" class=""success"" onclick=""completeOnboarding()"">
                🎉 Complete Onboarding & Proceed to Activation
            </button>
        </div>
    </div>
    
    <script>
        let currentCourse = null;
        let currentLesson = null;
        let courses = [];
        let lessons = [];
        let completedLessons = new Set();
        
        async function checkAuth() {
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return false;
            }
            return true;
        }
        
        async function loadCourses() {
            if (!await checkAuth()) return;
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/learning/courses/SuperAdmin', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                if (response.status === 403 || response.status === 401) {
                    window.location.href = '/login';
                    return;
                }
                
                const data = await response.json();
                if (data.success) {
                    courses = data.courses;
                    await loadProgress();
                    renderCourses();
                    document.getElementById('loadingMessage').style.display = 'none';
                    document.getElementById('courseList').style.display = 'grid';
                } else {
                    showError('Failed to load courses: ' + data.message);
                }
            } catch (error) {
                showError('Failed to load courses: ' + error.message);
            }
        }
        
        async function loadProgress() {
            const token = localStorage.getItem('ASHATCore_token');
            for (const course of courses) {
                try {
                    const response = await fetch(`/api/learning/courses/${course.id}/lessons`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    const data = await response.json();
                    if (data.success && data.lessons) {
                        course.lessons = data.lessons;
                        course.completed = data.lessons.every(l => l.completed);
                        data.lessons.forEach(l => {
                            if (l.completed) completedLessons.add(l.id);
                        });
                    }
                } catch (error) {
                    console.error('Failed to load progress for course:', course.id);
                }
            }
            updateProgress();
        }
        
        function renderCourses() {
            const container = document.getElementById('courseList');
            container.innerHTML = '';
            
            courses.forEach(course => {
                const card = document.createElement('div');
                card.className = 'course-card' + (course.completed ? ' completed' : '');
                card.innerHTML = `
                    <div class=""course-title"">
                        ${course.title}
                        ${course.completed ? '<span class=""completion-badge"">✓ Completed</span>' : ''}
                    </div>
                    <div class=""course-description"">${course.description}</div>
                    <div class=""course-meta"">
                        <span>📚 ${course.lessonCount} lessons</span>
                        <span>⏱️ ${course.estimatedMinutes} minutes</span>
                    </div>
                `;
                card.onclick = () => openCourse(course);
                container.appendChild(card);
            });
        }
        
        async function openCourse(course) {
            if (!course.lessons) {
                await loadProgress();
            }
            
            currentCourse = course;
            lessons = course.lessons || [];
            currentLesson = lessons.find(l => !l.completed) || lessons[0];
            
            if (currentLesson) {
                await loadLessonContent(currentLesson);
            }
        }
        
        async function loadLessonContent(lesson) {
            const viewer = document.getElementById('lessonViewer');
            currentLesson = lesson;
            
            viewer.innerHTML = `
                <div class=""lesson-title"">${lesson.title}</div>
                <div class=""lesson-content"">${lesson.content}</div>
                <div class=""lesson-navigation"">
                    <button class=""secondary"" onclick=""closeLessonViewer()"">← Back to Courses</button>
                    <div>
                        <button onclick=""previousLesson()"" ${lessons.indexOf(lesson) === 0 ? 'disabled' : ''}>
                            ← Previous
                        </button>
                        <button onclick=""completeLesson()"" class=""${lesson.completed ? 'success' : ''}"">
                            ${lesson.completed ? '✓ Completed' : 'Mark Complete'}
                        </button>
                        <button onclick=""nextLesson()"" ${lessons.indexOf(lesson) === lessons.length - 1 ? 'disabled' : ''}>
                            Next →
                        </button>
                    </div>
                </div>
            `;
            
            viewer.style.display = 'block';
            viewer.scrollIntoView({ behavior: 'smooth' });
        }
        
        function closeLessonViewer() {
            document.getElementById('lessonViewer').style.display = 'none';
        }
        
        function previousLesson() {
            const currentIndex = lessons.indexOf(currentLesson);
            if (currentIndex > 0) {
                loadLessonContent(lessons[currentIndex - 1]);
            }
        }
        
        function nextLesson() {
            const currentIndex = lessons.indexOf(currentLesson);
            if (currentIndex < lessons.length - 1) {
                loadLessonContent(lessons[currentIndex + 1]);
            }
        }
        
        async function completeLesson() {
            if (!currentLesson || currentLesson.completed) return;
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch(`/api/learning/lessons/${currentLesson.id}/complete`, {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                const data = await response.json();
                if (data.success) {
                    currentLesson.completed = true;
                    completedLessons.add(currentLesson.id);
                    
                    // Check if course is completed
                    if (lessons.every(l => l.completed)) {
                        currentCourse.completed = true;
                        showSuccess('🎉 Course completed!');
                    } else {
                        showSuccess('✓ Lesson completed!');
                    }
                    
                    updateProgress();
                    renderCourses();
                    
                    // Auto-advance to next lesson
                    const currentIndex = lessons.indexOf(currentLesson);
                    if (currentIndex < lessons.length - 1) {
                        setTimeout(() => loadLessonContent(lessons[currentIndex + 1]), 1000);
                    } else {
                        setTimeout(closeLessonViewer, 1500);
                    }
                } else {
                    showError('Failed to complete lesson: ' + data.message);
                }
            } catch (error) {
                showError('Failed to complete lesson: ' + error.message);
            }
        }
        
        function updateProgress() {
            const totalLessons = courses.reduce((sum, c) => sum + (c.lessons?.length || c.lessonCount), 0);
            const completed = completedLessons.size;
            const percentage = totalLessons > 0 ? (completed / totalLessons) * 100 : 0;
            
            document.getElementById('progressBar').style.width = percentage + '%';
            
            // Show finish button if all courses completed
            if (courses.every(c => c.completed)) {
                document.getElementById('finishButton').style.display = 'block';
            }
        }
        
        async function completeOnboarding() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/learning/SuperAdmin/complete', {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                const data = await response.json();
                if (data.success) {
                    showSuccess('🎉 Onboarding completed! Redirecting to server activation...');
                    setTimeout(() => {
                        window.location.href = '/activation';
                    }, 2000);
                } else {
                    showError('Failed to complete onboarding: ' + data.message);
                }
            } catch (error) {
                showError('Failed to complete onboarding: ' + error.message);
            }
        }
        
        function showSuccess(message) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message success';
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            setTimeout(() => statusMsg.style.display = 'none', 5000);
        }
        
        function showError(message) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message error';
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            document.getElementById('loadingMessage').style.display = 'none';
        }
        
        // Initialize
        loadCourses();
    </script>
</body>
</html>";
}

// Generate Server Activation UI dynamically
static string GenerateActivationUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Server Activation - ASHATOS</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .activation-container {
            background: rgba(20, 0, 40, 0.9);
            border-radius: 20px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 20px 60px rgba(138, 43, 226, 0.4);
            padding: 40px;
            max-width: 600px;
            width: 100%;
        }
        h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
            font-size: 32px;
            text-align: center;
        }
        .subtitle {
            color: #c8b6ff;
            margin-bottom: 30px;
            text-align: center;
            font-size: 16px;
        }
        .section {
            margin-bottom: 30px;
        }
        .section-title {
            font-size: 18px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 15px;
        }
        .info-box {
            background: rgba(138, 43, 226, 0.1);
            border-left: 4px solid #8b2fc7;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }
        .info-box p {
            margin: 5px 0;
            color: #d8c8ff;
            line-height: 1.6;
        }
        .form-group {
            margin-bottom: 20px;
        }
        label {
            display: block;
            margin-bottom: 8px;
            color: #e0d0ff;
            font-weight: 600;
        }
        input[type=""text""] {
            width: 100%;
            padding: 12px;
            background: rgba(0, 0, 0, 0.3);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 8px;
            font-size: 16px;
            color: white;
            transition: border-color 0.3s;
            font-family: monospace;
        }
        input[type=""text""]:focus {
            outline: none;
            border-color: rgba(138, 43, 226, 0.8);
            background: rgba(0, 0, 0, 0.4);
        }
        .button {
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            padding: 14px 28px;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            width: 100%;
            transition: all 0.3s;
        }
        .button:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        .button:disabled {
            background: #444;
            border-color: #555;
            cursor: not-allowed;
            opacity: 0.5;
        }
        .status-message {
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .status-message.success {
            background: rgba(34, 197, 94, 0.2);
            color: #4ade80;
            border: 1px solid rgba(34, 197, 94, 0.5);
        }
        .status-message.error {
            background: rgba(239, 68, 68, 0.2);
            color: #fca5a5;
            border: 1px solid rgba(239, 68, 68, 0.5);
        }
        .status-message.warning {
            background: rgba(234, 179, 8, 0.2);
            color: #fcd34d;
            border: 1px solid rgba(234, 179, 8, 0.5);
        }
        .license-types {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 10px;
            margin-top: 15px;
        }
        .license-type {
            background: rgba(138, 43, 226, 0.1);
            padding: 10px;
            border-radius: 6px;
            border: 1px solid rgba(138, 43, 226, 0.3);
            font-size: 14px;
            color: #c8b6ff;
        }
        .license-type strong {
            color: #c084fc;
        }
        .dev-mode-notice {
            background: rgba(234, 179, 8, 0.2);
            border-left: 4px solid #fcd34d;
            padding: 12px;
            margin-bottom: 20px;
            border-radius: 4px;
            font-size: 14px;
            color: #fcd34d;
        }
    </style>
</head>
<body>
    <div class=""activation-container"">
        <h1>🔑 Server Activation</h1>
        <p class=""subtitle"">You're almost there! Enter your license key to activate your ASHAT Os server.</p>
        
        <div id=""statusMessage"" class=""status-message""></div>
        <div id=""devModeNotice"" class=""dev-mode-notice"" style=""display: none;"">
            <strong>Development Mode:</strong> License validation is bypassed. Any valid format key will activate the server.
        </div>
        
        <div class=""section"">
            <div class=""info-box"">
                <p><strong>✅ Onboarding Completed</strong></p>
                <p>You've successfully completed the Masters Class Training.</p>
                <p><strong>Next Step:</strong> Activate your server with a license key to unlock all features.</p>
            </div>
        </div>
        
        <div class=""section"">
            <div class=""section-title"">License Key</div>
            <div class=""form-group"">
                <label for=""licenseKey"">Enter your RaCore license key:</label>
                <input type=""text"" id=""licenseKey"" placeholder=""RaOS-XXXX-XXXX-XXXX-XXXX"" />
            </div>
            <button class=""button"" onclick=""activateServer()"" id=""activateBtn"">
                🚀 Activate Server
            </button>
        </div>
        
        <div class=""section"">
            <div class=""section-title"">Available License Types</div>
            <div class=""license-types"">
                <div class=""license-type"">
                    <strong>Forum:</strong> Forum features
                </div>
                <div class=""license-type"">
                    <strong>CMS:</strong> Content management
                </div>
                <div class=""license-type"">
                    <strong>GameServer:</strong> Game hosting
                </div>
                <div class=""license-type"">
                    <strong>Enterprise:</strong> All features
                </div>
            </div>
        </div>
    </div>
    
    <script>
        async function checkAuth() {
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return;
            }
            
            // Check if already activated
            try {
                const response = await fetch('/api/control/activation-status', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    if (data.activated) {
                        window.location.href = '/control-panel';
                        return;
                    }
                    
                    // Show dev mode notice if applicable
                    if (data.devMode) {
                        document.getElementById('devModeNotice').style.display = 'block';
                    }
                }
            } catch (err) {
                console.log('Activation check error:', err);
            }
        }
        
        async function activateServer() {
            const licenseKey = document.getElementById('licenseKey').value.trim();
            
            if (!licenseKey) {
                showMessage('Please enter a license key', 'error');
                return;
            }
            
            const btn = document.getElementById('activateBtn');
            btn.disabled = true;
            btn.textContent = '🔄 Activating...';
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/control/activate', {
                    method: 'POST',
                    headers: {
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ licenseKey })
                });
                
                const data = await response.json();
                
                if (data.success) {
                    showMessage('✅ Server activated successfully! Redirecting to Control Panel...', 'success');
                    setTimeout(() => {
                        window.location.href = '/control-panel';
                    }, 2000);
                } else {
                    showMessage('❌ Activation failed: ' + data.message, 'error');
                    btn.disabled = false;
                    btn.textContent = '🚀 Activate Server';
                }
            } catch (error) {
                showMessage('❌ Activation error: ' + error.message, 'error');
                btn.disabled = false;
                btn.textContent = '🚀 Activate Server';
            }
        }
        
        function showMessage(message, type) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message ' + type;
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            setTimeout(() => {
                if (type !== 'success') {
                    statusMsg.style.display = 'none';
                }
            }, 5000);
        }
        
        // Allow Enter key to submit
        document.getElementById('licenseKey').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                activateServer();
            }
        });
        
        checkAuth();
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
    <title>Control Panel - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9);
            border-bottom: 2px solid rgba(138, 43, 226, 0.3);
            color: white;
            padding: 20px;
            text-align: center;
            position: relative;
        }
        .header h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .header p {
            color: #c8b6ff;
            margin-top: 10px;
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
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.3);
            transition: all 0.3s;
        }
        .stat-card:hover {
            border-color: rgba(138, 43, 226, 0.6);
            transform: translateY(-3px);
        }
        .stat-card h3 { color: #c084fc; margin-bottom: 10px; }
        .stat-card p { color: #e0d0ff; font-size: 1.2em; }
        .modules {
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.3);
        }
        .modules h2 { color: #c084fc; margin-bottom: 15px; }
        .module { 
            padding: 10px; 
            border-bottom: 1px solid rgba(138, 43, 226, 0.2);
            color: #d8c8ff;
        }
        .logout { 
            position: absolute;
            top: 20px;
            right: 20px;
            color: white;
            text-decoration: none;
            padding: 10px 20px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            transition: all 0.3s;
        }
        .logout:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎛️ AGP Studios, INC - Control Panel</h1>
        <p>SiteBuilder - Internal Module Interface</p>
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
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return;
            }
            
            // Check if SuperAdmin needs to complete onboarding
            try {
                const response = await fetch('/api/learning/SuperAdmin/status', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    if (!data.hasCompleted) {
                        // Redirect to onboarding if not completed
                        window.location.href = '/onboarding';
                        return;
                    }
                }
            } catch (err) {
                // If not SuperAdmin or learning module unavailable, continue to control panel
                console.log('Onboarding check skipped:', err);
            }
            
            // Check if server is activated
            try {
                const activationResponse = await fetch('/api/control/activation-status', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                
                if (activationResponse.ok) {
                    const activationData = await activationResponse.json();
                    if (!activationData.activated) {
                        // Redirect to activation if not activated
                        window.location.href = '/activation';
                        return;
                    }
                }
            } catch (err) {
                console.log('Activation check skipped:', err);
            }
        }
        async function loadStats() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
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
            localStorage.removeItem('ASHATCore_token');
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
    <title>Admin Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
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
        <p>SiteBuilder - Administrative Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Server Management</h2>
            <p>Advanced server Configuration and monitoring tools.</p>
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
    <title>Game Engine Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
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
        <p>SiteBuilder - Game Engine Interface</p>
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
    <title>Client Builder Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
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
        <p>SiteBuilder - Client Generation Interface</p>
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
        // All UI is served dynamically through (SiteBuilder) - no static files
        if (IsCmsAvailable(moduleManager))
        {
            Console.WriteLine("[ASHATCore] LegendaryCMS available - serving homepage via (SiteBuilder)");
            // Serve homepage dynamically through internal routing
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(GeneratedynamicHomepage());
            return;
        }
        
        // Fallback: CMS not available, use legacy bot-filtering homepage
        // Phase 9.3.7: Homepage bot filtering (legacy behavior)
        // DISABLED: Bot detection tempoASHATrily disabled for development/testing
        // TODO: Re-enable bot detection when ready for production
        var useASHATgent = context.Request.Headers["User-Agent"].ToString();
        // var isBot = BotDetector.IsSearchEngineBot(useASHATgent);
        
        // Set ContentType only when we're about to write HTML response
        context.Response.ContentType = "text/html";
        
        // Bot detection disabled - allow all visitors
        // if (!isBot)
        // {
        //     // Non-bot visitors get access denied message with control panel link
        //     await context.Response.WriteAsync(BotDetector.GetAccessDeniedMessage());
        //     return;
        // }
        
        // All visitors get the full homepage
        // var botName = BotDetector.GetBotName(useASHATgent);
        Console.WriteLine($"[ASHATCore] Visitor accessing homepage - Bot detection disabled");
    
        await context.Response.WriteAsync($@"<!DOCTYPE html>
<html>
<head>
    <title>AGP Studios, INC - Homepage</title>
    <meta name=""description"" content=""ASHATCore is a powerful, modular server platform with CMS, forums, game engine, and extensive plugin system."">
    <meta name=""keywords"" content=""ASHATCore, CMS, modular server, game engine, forum platform, control panel"">
    <meta name=""robots"" content=""index, follow"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 900px;
            margin: 50px auto;
            padding: 20px;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            color: #e0d0ff;
        }}
        .container {{
            background: rgba(20, 0, 40, 0.9);
            padding: 40px;
            border-radius: 15px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 10px 40px rgba(138, 43, 226, 0.4);
        }}
        h1 {{
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
            font-size: 2.5em;
        }}
        h3 {{
            color: #c084fc;
            margin-bottom: 15px;
        }}
        h4 {{
            color: #a855f7;
            margin-bottom: 8px;
        }}
        p {{
            color: #d8c8ff;
        }}
        .info {{
            background: rgba(138, 43, 226, 0.1);
            border-left: 4px solid #8b2fc7;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .success {{
            background: rgba(34, 197, 94, 0.1);
            border-left: 4px solid #22c55e;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        code {{
            background: rgba(0, 0, 0, 0.3);
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
            color: #c084fc;
        }}
        ul {{
            line-height: 1.8;
            color: #d8c8ff;
        }}
        .features {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }}
        .feature {{
            padding: 15px;
            background: rgba(138, 43, 226, 0.1);
            border-radius: 8px;
            border-left: 3px solid #8b2fc7;
            border: 1px solid rgba(138, 43, 226, 0.3);
            transition: all 0.3s;
        }}
        .feature:hover {{
            background: rgba(138, 43, 226, 0.2);
            border-color: rgba(138, 43, 226, 0.6);
            transform: translateY(-3px);
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌟 Welcome to AGP Studios, INC</h1>
        <p>ASHAT Os is a powerful, modular server platform running on port {port}.</p>
        
        <div class='info'>
            <h3>📋 About ASHAT Os:</h3>
            <p>ASHAT Os is an advanced, extensible server platform that combines CMS functionality, 
            forum systems, user profiles, game engine capabilities, and a comprehensive control panel 
            into a unified, modular architecture.</p>
            <p>Built with .NET 9.0, ASHAT Os provides a robust foundation for web applications, 
            game servers, and community platforms.</p>
        </div>
        
        <div class='success'>
            <h3>🎯 Core Features:</h3>
            <div class='features'>
                <div class='feature'>
                    <h4>🎛️ Control Panel</h4>
                    <p>Comprehensive admin dashboard for managing all aspects of your ASHAT Os instance.</p>
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
                    <p>Intergrated game engine with scene management, persistence, and WebSocket support.</p>
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
                <li><strong>ASP.NET Core:</strong> Web Framework with WebSocket support</li>
                <li><strong>SQLite:</strong> Embedded database for data persistence</li>
                <li><strong>PHP + Nginx:</strong> CMS frontend delivery</li>
                <li><strong>Modular Architecture:</strong> Plugin-based extensibility</li>
            </ul>
        </div>
        
        <div class='info'>
            <h3>📚 Key Components:</h3>
            <ul>
                <li><strong>Authentication Module:</strong> Secure user authentication with token-based sessions</li>
                <li><strong>SiteBuilder Module:</strong> Automated CMS and website Generation</li>
                <li><strong>RaCoin System:</strong> Intergrated virtual currency platform</li>
                <li><strong>LegendaryEngine:</strong> Game development Framework</li>
                <li><strong>Supermarket Module:</strong> E-commerce capabilities</li>
                <li><strong>Learning Module:</strong> Educational content delivery</li>
            </ul>
        </div>
        
        <p style='margin-top: 30px; text-align: center; color: #6c757d;'>
            <small>AGP Studios, INC - Copyright &copy; 2024</small>
        </p>
    </div>
</body>
</html>");
    }
    catch (Exception ex)
    {
        // Error handling to prevent server cASHATshes if header setting fails
        Console.WriteLine($"[ASHATCore] Error handling homepage request: {ex.Message}");
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
// - UI ROUTES (Dynamic, no static files)
// All UI features accessed through internal module routing
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

// Onboarding UI - served dynamically (for Masters class onboarding)
app.MapGet("/onboarding", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateOnboardingUI());
});

// Activation UI - served dynamically (for license activation)
app.MapGet("/activation", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateActivationUI());
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

Console.WriteLine("[ASHATCore] UI routes registered (dynamic, no static files):");
Console.WriteLine("  GET  /control-panel - Control Panel UI (served dynamically)");
Console.WriteLine("  GET  /login - Login UI (served dynamically)");
Console.WriteLine("  GET  /admin - Admin UI (served dynamically)");
Console.WriteLine("  GET  /gameengine-dashboard - Game Engine Dashboard UI (served dynamically)");
Console.WriteLine("  GET  /clientbuilder-dashboard - Client Builder Dashboard UI (served dynamically)");

// ============================================================================
// CONTROL PANEL API ENDPOINTS
// ============================================================================

// Get RaCoin module if present
IRaCoinModule? RaCoinModule = moduleManager.Modules
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
                ServerMode.Omega => "Main server Configuration (us-omega)",
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

        // Save the Configuration
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

Console.WriteLine("[ASHATCore] Server Configuration API endpoints registered:");
Console.WriteLine("  GET  /api/control/server/config - Get server Configuration (Admin+)");
Console.WriteLine("  POST /api/control/server/mode - Change server mode (SuperAdmin only)");
Console.WriteLine("  GET  /api/control/server/modes - List available server modes (Admin+)");
Console.WriteLine("  POST /api/control/server/underconstruction - Toggle Under Construction mode (Admin+)");
Console.WriteLine("  GET  /api/control/server/underconstruction - Get Under Construction status (Admin+)");



// Control Panel API endpoints
app.MapControlPanelEndpoints(moduleManager, authModule, licenseModule, RaCoinModule, gameEngineModule, firstRunManager);

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
