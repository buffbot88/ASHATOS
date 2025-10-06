using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
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

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure WebRootPath to ensure it's found correctly
builder.WebHost.UseWebRoot(wwwrootPath);

// 1. Instantiate MemoryModule FIRST
var memoryModule = new MemoryModule();
memoryModule.Initialize(null); // Pass ModuleManager if needed

// 2. Instantiate ModuleManager and register MemoryModule as built-in
var moduleManager = new ModuleManager();
moduleManager.RegisterBuiltInModule(memoryModule);

// 3. Load other modules (plugins, etc.)
moduleManager.LoadModules();

// 4. Run boot sequence with self-healing checks and configuration verification
// This will detect the port from Apache configuration (MUST run before building app)
var bootSequence = new RaCore.Engine.BootSequenceManager(moduleManager);
await bootSequence.ExecuteBootSequenceAsync();

// 5. Configure port - use detected port from Apache config or fallback to default
// Apache configuration is the source of truth for port management
var port = Environment.GetEnvironmentVariable("RACORE_DETECTED_PORT") ?? "5000";
var urls = $"http://*:{port}";

// Add CORS support for agpstudios.online domain and dynamic port
var allowedOrigins = new List<string>
{
    $"http://localhost:{port}",
    "http://localhost",
    "http://agpstudios.online",
    "https://agpstudios.online"
};

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure URLs with dynamic port
builder.WebHost.UseUrls(urls);

var app = builder.Build();
app.UseCors(); // Enable CORS
app.UseWebSockets();
app.UseStaticFiles();

// URL redirects for cleaner access
app.MapGet("/control-panel", async context =>
{
    await Task.CompletedTask;
    context.Response.Redirect("/control-panel.html");
});

// 5. Check for first run and auto-spawn CMS + Apache
var firstRunManager = new RaCore.Engine.FirstRunManager(moduleManager);
if (firstRunManager.IsFirstRun())
{
    Console.WriteLine("[RaCore] First run detected - initializing CMS homepage...");
    await firstRunManager.InitializeAsync();
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
if (authModule != null)
{
    // Register endpoint
    app.MapPost("/api/auth/register", async (HttpContext context) =>
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<RegisterRequest>();
            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
                return;
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "";
            var response = await authModule.RegisterAsync(request, ipAddress);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Login endpoint
    app.MapPost("/api/auth/login", async (HttpContext context) =>
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<LoginRequest>();
            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
                return;
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var response = await authModule.LoginAsync(request, ipAddress, userAgent);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Logout endpoint
    app.MapPost("/api/auth/logout", async (HttpContext context) =>
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
            
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "No token provided" });
                return;
            }

            var success = await authModule.LogoutAsync(token);
            await context.Response.WriteAsJsonAsync(new { success, message = success ? "Logged out successfully" : "Invalid token" });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Validate token endpoint
    app.MapPost("/api/auth/validate", async (HttpContext context) =>
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
            
            if (string.IsNullOrWhiteSpace(token))
            {
                await context.Response.WriteAsJsonAsync(new { valid = false, message = "No token provided" });
                return;
            }

            var session = await authModule.ValidateTokenAsync(token);
            var user = session != null ? await authModule.GetUserByTokenAsync(token) : null;
            
            await context.Response.WriteAsJsonAsync(new 
            { 
                valid = session != null, 
                user = user != null ? new { user.Username, user.Email, user.Role } : null,
                expiresAt = session?.ExpiresAtUtc
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { valid = false, message = ex.Message });
        }
    });

    // Get security events endpoint (admin only)
    app.MapGet("/api/auth/events", async (HttpContext context) =>
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
            
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "No token provided" });
                return;
            }

            var user = await authModule.GetUserByTokenAsync(token);
            if (user == null || !authModule.HasPermission(user, "Authentication", UserRole.Admin))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Insufficient permissions" });
                return;
            }

            var events = await authModule.GetSecurityEventsAsync(100);
            await context.Response.WriteAsJsonAsync(new { success = true, events });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    Console.WriteLine("[RaCore] Authentication API endpoints registered:");
    Console.WriteLine("  POST /api/auth/register");
    Console.WriteLine("  POST /api/auth/login");
    Console.WriteLine("  POST /api/auth/logout");
    Console.WriteLine("  POST /api/auth/validate");
    Console.WriteLine("  GET  /api/auth/events (admin only)");
}

// Get game engine module if present
IGameEngineModule? gameEngineModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameEngineModule>()
    .FirstOrDefault();

// Game Engine API endpoints
if (gameEngineModule != null)
{
    // Helper function to validate auth and permissions
    async Task<(bool authorized, User? user, string? error)> ValidateGameEngineAccess(HttpContext context, UserRole requiredRole = UserRole.User)
    {
        if (authModule == null)
            return (false, null, "Authentication not available");

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, "No token provided");

        var user = await authModule.GetUserByTokenAsync(token);
        if (user == null)
            return (false, null, "Invalid or expired token");

        if (!authModule.HasPermission(user, "GameEngine", requiredRole))
            return (false, user, "Insufficient permissions");

        return (true, user, null);
    }

    // Create scene endpoint
    app.MapPost("/api/gameengine/scene", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameEngineAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<CreateSceneRequest>();
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
                return;
            }

            var response = await gameEngineModule.CreateSceneAsync(request.Name, user!.Username);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // List scenes endpoint
    app.MapGet("/api/gameengine/scenes", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameEngineAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var scenes = await gameEngineModule.ListScenesAsync();
            await context.Response.WriteAsJsonAsync(new { success = true, scenes });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Get scene endpoint
    app.MapGet("/api/gameengine/scene/{sceneId}", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameEngineAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var sceneId = context.Request.RouteValues["sceneId"]?.ToString();
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene ID required" });
                return;
            }

            var scene = await gameEngineModule.GetSceneAsync(sceneId);
            if (scene == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene not found" });
                return;
            }

            await context.Response.WriteAsJsonAsync(new { success = true, scene });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Delete scene endpoint
    app.MapDelete("/api/gameengine/scene/{sceneId}", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameEngineAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var sceneId = context.Request.RouteValues["sceneId"]?.ToString();
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene ID required" });
                return;
            }

            var response = await gameEngineModule.DeleteSceneAsync(sceneId, user!.Username);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Create entity endpoint
    app.MapPost("/api/gameengine/scene/{sceneId}/entity", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameEngineAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var sceneId = context.Request.RouteValues["sceneId"]?.ToString();
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene ID required" });
                return;
            }

            var entity = await context.Request.ReadFromJsonAsync<GameEntity>();
            if (entity == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid entity data" });
                return;
            }

            var response = await gameEngineModule.CreateEntityAsync(sceneId, entity, user!.Username);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // List entities endpoint
    app.MapGet("/api/gameengine/scene/{sceneId}/entities", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameEngineAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var sceneId = context.Request.RouteValues["sceneId"]?.ToString();
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene ID required" });
                return;
            }

            var entities = await gameEngineModule.ListEntitiesAsync(sceneId);
            await context.Response.WriteAsJsonAsync(new { success = true, entities });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // AI-generate world content endpoint
    app.MapPost("/api/gameengine/scene/{sceneId}/generate", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameEngineAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var sceneId = context.Request.RouteValues["sceneId"]?.ToString();
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Scene ID required" });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<WorldGenerationRequest>();
            if (request == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
                return;
            }

            var response = await gameEngineModule.GenerateWorldContentAsync(sceneId, request, user!.Username);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Get engine stats endpoint
    app.MapGet("/api/gameengine/stats", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameEngineAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var stats = await gameEngineModule.GetStatsAsync();
            await context.Response.WriteAsJsonAsync(new { success = true, stats });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    Console.WriteLine("[RaCore] Game Engine API endpoints registered:");
    Console.WriteLine("  POST   /api/gameengine/scene - Create scene (admin only)");
    Console.WriteLine("  GET    /api/gameengine/scenes - List scenes");
    Console.WriteLine("  GET    /api/gameengine/scene/{sceneId} - Get scene details");
    Console.WriteLine("  DELETE /api/gameengine/scene/{sceneId} - Delete scene (admin only)");
    Console.WriteLine("  POST   /api/gameengine/scene/{sceneId}/entity - Create entity (admin only)");
    Console.WriteLine("  GET    /api/gameengine/scene/{sceneId}/entities - List entities");
    Console.WriteLine("  POST   /api/gameengine/scene/{sceneId}/generate - AI-generate content (admin only)");
    Console.WriteLine("  GET    /api/gameengine/stats - Get engine statistics");
}

// ServerSetup API endpoints - Folder discovery and admin instance management
IServerSetupModule? serverSetupModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IServerSetupModule>()
    .FirstOrDefault();

if (serverSetupModule != null && authModule != null)
{
    // Discover server folders (Databases, php, Apache, Admins)
    app.MapGet("/api/serversetup/discover", async (HttpContext context) =>
    {
        try
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
                return;
            }

            var user = await authModule.GetUserByTokenAsync(token);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid token" });
                return;
            }

            var result = await serverSetupModule.DiscoverServerFoldersAsync();
            await context.Response.WriteAsJsonAsync(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Create admin instance
    app.MapPost("/api/serversetup/admin", async (HttpContext context) =>
    {
        try
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
                return;
            }

            var user = await authModule.GetUserByTokenAsync(token);
            if (user == null || !authModule.HasPermission(user, "ServerSetup", UserRole.Admin))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<CreateAdminInstanceRequest>();
            if (request == null || string.IsNullOrEmpty(request.LicenseNumber) || string.IsNullOrEmpty(request.Username))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request: licenseNumber and username required" });
                return;
            }

            var result = await serverSetupModule.CreateAdminFolderStructureAsync(request.LicenseNumber, request.Username);
            if (result.Success)
            {
                await context.Response.WriteAsJsonAsync(new { success = true, message = result.Message, data = result.Details });
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Setup Apache configuration
    app.MapPost("/api/serversetup/apache", async (HttpContext context) =>
    {
        try
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
                return;
            }

            var user = await authModule.GetUserByTokenAsync(token);
            if (user == null || !authModule.HasPermission(user, "ServerSetup", UserRole.Admin))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<SetupConfigRequest>();
            if (request == null || string.IsNullOrEmpty(request.LicenseNumber) || string.IsNullOrEmpty(request.Username))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request: licenseNumber and username required" });
                return;
            }

            var result = await serverSetupModule.SetupNginxConfigAsync(request.LicenseNumber, request.Username);
            if (result.Success)
            {
                await context.Response.WriteAsJsonAsync(new { success = true, message = result.Message, data = result.Details });
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Setup PHP configuration
    app.MapPost("/api/serversetup/php", async (HttpContext context) =>
    {
        try
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
                return;
            }

            var user = await authModule.GetUserByTokenAsync(token);
            if (user == null || !authModule.HasPermission(user, "ServerSetup", UserRole.Admin))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<SetupConfigRequest>();
            if (request == null || string.IsNullOrEmpty(request.LicenseNumber) || string.IsNullOrEmpty(request.Username))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request: licenseNumber and username required" });
                return;
            }

            var result = await serverSetupModule.SetupPhpConfigAsync(request.LicenseNumber, request.Username);
            if (result.Success)
            {
                await context.Response.WriteAsJsonAsync(new { success = true, message = result.Message, data = result.Details });
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    Console.WriteLine("ServerSetup API endpoints registered:");
    Console.WriteLine("  GET    /api/serversetup/discover - Discover server folders");
    Console.WriteLine("  POST   /api/serversetup/admin - Create admin instance (admin only)");
    Console.WriteLine("  POST   /api/serversetup/nginx - Setup Nginx config (admin only)");
    Console.WriteLine("  POST   /api/serversetup/php - Setup PHP config (admin only)");
}

// GameServer API endpoints - AI-driven game creation and deployment
IGameServerModule? gameServerModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameServerModule>()
    .FirstOrDefault();

if (gameServerModule != null && authModule != null)
{
    // Helper function to validate auth and permissions
    async Task<(bool authorized, User? user, string? error)> ValidateGameServerAccess(HttpContext context, UserRole requiredRole = UserRole.User)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, "No token provided");

        var user = await authModule.GetUserByTokenAsync(token);
        if (user == null)
            return (false, null, "Invalid or expired token");

        if (!authModule.HasPermission(user, "GameServer", requiredRole))
            return (false, user, "Insufficient permissions");

        return (true, user, null);
    }

    // Create game from natural language description
    app.MapPost("/api/gameserver/create", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context, UserRole.User);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<GameCreationRequest>();
            if (request == null || string.IsNullOrWhiteSpace(request.Description))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request: description required" });
                return;
            }

            request.UserId = user!.Id;
            var response = await gameServerModule.CreateGameFromDescriptionAsync(request);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // List user's games
    app.MapGet("/api/gameserver/games", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var games = await gameServerModule.ListUserGamesAsync(user!.Id);
            await context.Response.WriteAsJsonAsync(new { success = true, games });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Get game project details
    app.MapGet("/api/gameserver/game/{gameId}", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameServerAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var project = await gameServerModule.GetGameProjectAsync(gameId);
            if (project == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game not found" });
                return;
            }

            await context.Response.WriteAsJsonAsync(new { success = true, project });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Get game preview
    app.MapGet("/api/gameserver/game/{gameId}/preview", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameServerAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var preview = await gameServerModule.GetGamePreviewAsync(gameId);
            await context.Response.WriteAsJsonAsync(new { success = true, preview });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Deploy game server
    app.MapPost("/api/gameserver/game/{gameId}/deploy", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var options = await context.Request.ReadFromJsonAsync<DeploymentOptions>();
            if (options == null)
            {
                options = new DeploymentOptions();
            }

            var response = await gameServerModule.DeployGameServerAsync(gameId, options);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Update game
    app.MapPut("/api/gameserver/game/{gameId}", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context, UserRole.User);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var updateRequest = await context.Request.ReadFromJsonAsync<GameUpdateRequest>();
            if (updateRequest == null || string.IsNullOrWhiteSpace(updateRequest.Description))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Update description required" });
                return;
            }

            var response = await gameServerModule.UpdateGameAsync(gameId, updateRequest.Description);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Delete game
    app.MapDelete("/api/gameserver/game/{gameId}", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context, UserRole.Admin);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var response = await gameServerModule.DeleteGameProjectAsync(gameId);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Export game
    app.MapPost("/api/gameserver/game/{gameId}/export", async (HttpContext context) =>
    {
        try
        {
            var (authorized, user, error) = await ValidateGameServerAccess(context, UserRole.User);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var gameId = context.Request.RouteValues["gameId"]?.ToString();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Game ID required" });
                return;
            }

            var exportRequest = await context.Request.ReadFromJsonAsync<GameExportRequest>();
            var format = exportRequest?.Format ?? ExportFormat.Complete;

            var response = await gameServerModule.ExportGameProjectAsync(gameId, format);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    // Get system capabilities
    app.MapGet("/api/gameserver/capabilities", async (HttpContext context) =>
    {
        try
        {
            var (authorized, _, error) = await ValidateGameServerAccess(context);
            if (!authorized)
            {
                context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                return;
            }

            var capabilities = await gameServerModule.GetCapabilitiesAsync();
            await context.Response.WriteAsJsonAsync(new { success = true, capabilities });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
        }
    });

    Console.WriteLine("GameServer API endpoints registered:");
    Console.WriteLine("  POST   /api/gameserver/create - Create game from description");
    Console.WriteLine("  GET    /api/gameserver/games - List user's games");
    Console.WriteLine("  GET    /api/gameserver/game/{gameId} - Get game details");
    Console.WriteLine("  GET    /api/gameserver/game/{gameId}/preview - Get game preview");
    Console.WriteLine("  POST   /api/gameserver/game/{gameId}/deploy - Deploy game (admin only)");
    Console.WriteLine("  PUT    /api/gameserver/game/{gameId} - Update game");
    Console.WriteLine("  DELETE /api/gameserver/game/{gameId} - Delete game (admin only)");
    Console.WriteLine("  POST   /api/gameserver/game/{gameId}/export - Export game");
    Console.WriteLine("  GET    /api/gameserver/capabilities - Get system capabilities");
}

// Redirect root to CMS homepage
app.MapGet("/", (HttpContext context) =>
{
    // Get CMS port from environment or use default 8080
    var cmsPort = Environment.GetEnvironmentVariable("RACORE_CMS_PORT") ?? "8080";
    context.Response.Redirect($"http://localhost:{cmsPort}");
    return Task.CompletedTask;
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

// Control Panel: Get Dashboard Stats
app.MapGet("/api/control/stats", async (HttpContext context) =>
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

        // Get stats from various modules
        var stats = new
        {
            totalUsers = authModule.GetAllUsers().Count(),
            activeLicenses = licenseModule?.GetAllLicenses().Count(l => l.Status == LicenseStatus.Active) ?? 0,
            totalRaCoins = racoinModule?.GetTotalSystemRaCoins() ?? 0,
            activeServers = gameEngineModule?.GetAllScenes().Count() ?? 0,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(new { success = true, stats });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Get Available Modules (Admin+)
app.MapGet("/api/control/modules", async (HttpContext context) =>
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

        // Get available modules from moduleManager
        var modules = moduleManager.Modules
            .Where(m => m.Instance != null)
            .Select(m => new
            {
                name = m.Instance!.Name,
                description = $"{m.Instance.Name} module",
                category = m.Category
            })
            .ToList();

        await context.Response.WriteAsJsonAsync(new { success = true, modules });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Get All Users (Admin+)
app.MapGet("/api/control/users", async (HttpContext context) =>
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

        var users = authModule.GetAllUsers().Select(u => new
        {
            u.Id,
            u.Username,
            Role = u.Role.ToString(),
            u.CreatedAtUtc
        });

        await context.Response.WriteAsJsonAsync(new { success = true, users });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Create User (Admin+)
app.MapPost("/api/control/users", async (HttpContext context) =>
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

        var request = await context.Request.ReadFromJsonAsync<RegisterRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "";
        var response = await authModule.RegisterAsync(request, ipAddress);
        await context.Response.WriteAsJsonAsync(response);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Update User Role (SuperAdmin only)
app.MapPut("/api/control/users/{userId}/role", async (HttpContext context) =>
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
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Only SuperAdmin can change roles" });
            return;
        }

        var userId = Guid.Parse(context.Request.RouteValues["userId"]?.ToString() ?? "");
        var request = await context.Request.ReadFromJsonAsync<JsonElement>();
        var newRole = (UserRole)request.GetProperty("role").GetInt32();

        var result = authModule.UpdateUserRole(userId, newRole);
        await context.Response.WriteAsJsonAsync(new { success = result, message = result ? "Role updated" : "Update failed" });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Get All Licenses (Admin+)
app.MapGet("/api/control/licenses", async (HttpContext context) =>
{
    try
    {
        if (authModule == null || licenseModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Required modules not available" });
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

        var licenses = licenseModule.GetAllLicenses().Select(l => new
        {
            l.Id,
            l.LicenseKey,
            l.InstanceName,
            Status = l.Status.ToString(),
            Type = l.Type.ToString(),
            l.CreatedAtUtc,
            l.ExpiresAtUtc
        });

        await context.Response.WriteAsJsonAsync(new { success = true, licenses });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Assign License (Admin+)
app.MapPost("/api/control/licenses", async (HttpContext context) =>
{
    try
    {
        if (authModule == null || licenseModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Required modules not available" });
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

        var request = await context.Request.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(request.GetProperty("userId").GetString() ?? "");
        var instanceName = request.GetProperty("instanceName").GetString() ?? "";
        var licenseType = (LicenseType)request.GetProperty("licenseType").GetInt32();
        var duration = request.GetProperty("durationYears").GetInt32();

        var license = licenseModule.CreateLicense(userId, instanceName, licenseType, duration);
        await context.Response.WriteAsJsonAsync(new { success = true, license });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: RaCoin Top Up (Admin+)
app.MapPost("/api/control/racoin/topup", async (HttpContext context) =>
{
    try
    {
        if (authModule == null || racoinModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Required modules not available" });
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

        var request = await context.Request.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(request.GetProperty("userId").GetString() ?? "");
        var amount = request.GetProperty("amount").GetDecimal();
        var reason = request.GetProperty("reason").GetString() ?? "Admin top-up";

        var result = await racoinModule.AddAsync(userId, amount, reason);
        await context.Response.WriteAsJsonAsync(result);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Get RaCoin Balances (Admin+)
app.MapGet("/api/control/racoin/balances", async (HttpContext context) =>
{
    try
    {
        if (authModule == null || racoinModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Required modules not available" });
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

        var users = authModule.GetAllUsers();
        var balances = users.Select(u => new
        {
            userId = u.Id,
            username = u.Username,
            balance = racoinModule.GetBalanceAsync(u.Id).Result
        });

        await context.Response.WriteAsJsonAsync(new { success = true, balances });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Control Panel: Get Game Server Stats (GameMaster/GameMonitor+)
app.MapGet("/api/control/game/stats", async (HttpContext context) =>
{
    try
    {
        if (authModule == null || gameEngineModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Required modules not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || (user.Role != UserRole.GameMaster && user.Role != UserRole.GameMonitor && user.Role < UserRole.Admin))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Requires GameMaster, GameMonitor, or Admin role" });
            return;
        }

        var stats = await gameEngineModule.GetStatsAsync();
        await context.Response.WriteAsJsonAsync(new { success = true, stats });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// ============================================================================
// Forum Moderation API Endpoints
// ============================================================================

app.MapGet("/api/control/forum/posts", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions. ForumModerator role required." });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var posts = await forumModule.GetPostsAsync();
    return Results.Json(new { posts });
});

app.MapDelete("/api/control/forum/posts/{postId}", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var postId = context.Request.RouteValues["postId"]?.ToString();
    if (string.IsNullOrEmpty(postId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Post ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<ForumPostActionRequest>();
    if (body == null || string.IsNullOrEmpty(body.Reason))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Reason required" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var success = await forumModule.DeletePostAsync(postId, user.Id.ToString(), body.Reason);
    return Results.Json(new { success, message = success ? "Post deleted" : "Post not found" });
});

app.MapPut("/api/control/forum/posts/{postId}/lock", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var postId = context.Request.RouteValues["postId"]?.ToString();
    if (string.IsNullOrEmpty(postId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Post ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<ForumLockRequest>();
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Lock status required" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var success = await forumModule.LockThreadAsync(postId, body.Locked, user.Id.ToString());
    return Results.Json(new { success, message = success ? (body.Locked ? "Thread locked" : "Thread unlocked") : "Post not found" });
});

app.MapGet("/api/control/forum/users/{userId}/warnings", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var warnings = await forumModule.GetUserWarningsAsync(userId);
    return Results.Json(new { warnings });
});

app.MapPost("/api/control/forum/users/{userId}/warnings", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<ForumWarningRequest>();
    if (body == null || string.IsNullOrEmpty(body.Reason))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Reason required" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var success = await forumModule.IssueWarningAsync(userId, body.Reason, user.Id.ToString());
    return Results.Json(new { success, message = "Warning issued" });
});

app.MapPut("/api/control/forum/users/{userId}/ban", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.ForumModerator))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<ForumBanRequest>();
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Ban request required" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var success = await forumModule.BanUserAsync(userId, body.Banned, body.Reason ?? "", user.Id.ToString());
    return Results.Json(new { success, message = body.Banned ? "User banned" : "User unbanned" });
});

app.MapGet("/api/control/forum/stats", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Forum", UserRole.Admin))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var forumModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IForumModule>()
        .FirstOrDefault();
    
    if (forumModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Forum module not available" });
    }
    
    var stats = await forumModule.GetStatsAsync();
    return Results.Json(new { stats });
});

// ============================================================================
// Blog API Endpoints
// ============================================================================

app.MapGet("/api/blog/posts", async (HttpContext context) =>
{
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var pageStr = context.Request.Query["page"].ToString();
    var page = int.TryParse(pageStr, out var p) ? p : 1;
    var category = context.Request.Query["category"].ToString();
    
    var posts = await blogModule.GetPostsAsync(page, 20, string.IsNullOrEmpty(category) ? null : category);
    return Results.Json(new { posts });
});

app.MapGet("/api/blog/posts/{postId}", async (HttpContext context) =>
{
    var postId = context.Request.RouteValues["postId"]?.ToString();
    if (string.IsNullOrEmpty(postId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Post ID required" });
    }
    
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var post = await blogModule.GetPostByIdAsync(postId);
    if (post == null)
    {
        context.Response.StatusCode = 404;
        return Results.Json(new { error = "Post not found" });
    }
    
    return Results.Json(new { post });
});

app.MapPost("/api/blog/posts", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<CreateBlogPostRequest>();
    if (body == null || string.IsNullOrEmpty(body.Title) || string.IsNullOrEmpty(body.Content))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Title and content required" });
    }
    
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var result = await blogModule.CreatePostAsync(user.Id.ToString(), user.Username, body.Title, body.Content, body.Category);
    return Results.Json(new { success = result.success, message = result.message, postId = result.postId });
});

app.MapGet("/api/blog/posts/{postId}/comments", async (HttpContext context) =>
{
    var postId = context.Request.RouteValues["postId"]?.ToString();
    if (string.IsNullOrEmpty(postId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Post ID required" });
    }
    
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var comments = await blogModule.GetCommentsAsync(postId);
    return Results.Json(new { comments });
});

app.MapPost("/api/blog/posts/{postId}/comments", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var postId = context.Request.RouteValues["postId"]?.ToString();
    if (string.IsNullOrEmpty(postId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Post ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<AddCommentRequest>();
    if (body == null || string.IsNullOrEmpty(body.Content))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Content required" });
    }
    
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var result = await blogModule.AddCommentAsync(postId, user.Id.ToString(), user.Username, body.Content);
    return Results.Json(new { success = result.success, message = result.message, commentId = result.commentId });
});

app.MapGet("/api/blog/categories", async (HttpContext context) =>
{
    var blogModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IBlogModule>()
        .FirstOrDefault();
    
    if (blogModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Blog module not available" });
    }
    
    var categories = await blogModule.GetCategoriesAsync();
    return Results.Json(new { categories });
});

// ============================================================================
// Chat API Endpoints
// ============================================================================

app.MapGet("/api/chat/rooms", async (HttpContext context) =>
{
    var chatModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IChatModule>()
        .FirstOrDefault();
    
    if (chatModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Chat module not available" });
    }
    
    var rooms = await chatModule.GetRoomsAsync();
    return Results.Json(new { rooms });
});

app.MapPost("/api/chat/rooms", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<CreateChatRoomRequest>();
    if (body == null || string.IsNullOrEmpty(body.Name))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Room name required" });
    }
    
    var chatModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IChatModule>()
        .FirstOrDefault();
    
    if (chatModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Chat module not available" });
    }
    
    var result = await chatModule.CreateRoomAsync(body.Name, user.Id.ToString(), body.IsPrivate);
    return Results.Json(new { success = result.success, message = result.message, roomId = result.roomId });
});

app.MapGet("/api/chat/rooms/{roomId}/messages", async (HttpContext context) =>
{
    var roomId = context.Request.RouteValues["roomId"]?.ToString();
    if (string.IsNullOrEmpty(roomId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Room ID required" });
    }
    
    var chatModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IChatModule>()
        .FirstOrDefault();
    
    if (chatModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Chat module not available" });
    }
    
    var messages = await chatModule.GetMessagesAsync(roomId);
    return Results.Json(new { messages });
});

app.MapPost("/api/chat/rooms/{roomId}/messages", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var roomId = context.Request.RouteValues["roomId"]?.ToString();
    if (string.IsNullOrEmpty(roomId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Room ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<SendMessageRequest>();
    if (body == null || string.IsNullOrEmpty(body.Content))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Content required" });
    }
    
    var chatModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IChatModule>()
        .FirstOrDefault();
    
    if (chatModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Chat module not available" });
    }
    
    var result = await chatModule.SendMessageAsync(roomId, user.Id.ToString(), user.Username, body.Content);
    return Results.Json(new { success = result.success, message = result.message, messageId = result.messageId });
});

app.MapPost("/api/chat/rooms/{roomId}/join", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var roomId = context.Request.RouteValues["roomId"]?.ToString();
    if (string.IsNullOrEmpty(roomId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Room ID required" });
    }
    
    var chatModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<IChatModule>()
        .FirstOrDefault();
    
    if (chatModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Chat module not available" });
    }
    
    var success = await chatModule.JoinRoomAsync(roomId, user.Id.ToString(), user.Username);
    return Results.Json(new { success });
});

// ============================================================================
// Social Profile API Endpoints
// ============================================================================

app.MapGet("/api/social/profile/{userId}", async (HttpContext context) =>
{
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var profileModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<UserProfileModule>()
        .FirstOrDefault();
    
    if (profileModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Profile module not available" });
    }
    
    var profile = await profileModule.GetProfileAsync(userId);
    if (profile == null)
    {
        context.Response.StatusCode = 404;
        return Results.Json(new { error = "Profile not found" });
    }
    
    return Results.Json(new { profile });
});

app.MapPost("/api/social/profile/{userId}/friends", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<AddFriendRequest>();
    if (body == null || string.IsNullOrEmpty(body.FriendId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Friend ID required" });
    }
    
    var profileModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<UserProfileModule>()
        .FirstOrDefault();
    
    if (profileModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Profile module not available" });
    }
    
    var success = await profileModule.AddFriendAsync(userId, body.FriendId);
    return Results.Json(new { success });
});

app.MapGet("/api/social/profile/{userId}/posts", async (HttpContext context) =>
{
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var profileModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<UserProfileModule>()
        .FirstOrDefault();
    
    if (profileModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Profile module not available" });
    }
    
    var posts = await profileModule.GetSocialPostsAsync(userId);
    return Results.Json(new { posts });
});

app.MapPost("/api/social/profile/{userId}/posts", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var body = await context.Request.ReadFromJsonAsync<CreateSocialPostRequest>();
    if (body == null || string.IsNullOrEmpty(body.Content))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Content required" });
    }
    
    var profileModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<UserProfileModule>()
        .FirstOrDefault();
    
    if (profileModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Profile module not available" });
    }
    
    var result = await profileModule.CreateSocialPostAsync(userId, body.Content);
    return Results.Json(new { success = result.success, postId = result.postId });
});

app.MapGet("/api/social/profile/{userId}/activity", async (HttpContext context) =>
{
    var userId = context.Request.RouteValues["userId"]?.ToString();
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var profileModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<UserProfileModule>()
        .FirstOrDefault();
    
    if (profileModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Profile module not available" });
    }
    
    var activity = await profileModule.GetActivityFeedAsync(userId);
    return Results.Json(new { activity });
});

// ============================================================================
// System Health & Monitoring API Endpoints
// ============================================================================

app.MapGet("/api/control/system/health", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "System", UserRole.Admin))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions" });
    }
    
    var process = System.Diagnostics.Process.GetCurrentProcess();
    var health = new
    {
        status = "healthy",
        uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
        memory_mb = process.WorkingSet64 / 1024 / 1024,
        cpu_time_seconds = process.TotalProcessorTime.TotalSeconds,
        threads = process.Threads.Count,
        modules_loaded = moduleManager.Modules.Count,
        timestamp = DateTime.UtcNow
    };
    
    return await Task.FromResult(Results.Json(new { health }));
});

// Restart Apache server (Admin only)
app.MapPost("/api/control/system/restart-apache", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "System", UserRole.Admin))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { success = false, error = "Insufficient permissions. Admin role required." });
    }
    
    Console.WriteLine($"[API] Nginx restart requested by user: {user.Username}");
    
    var (success, message) = RaCore.Engine.NginxManager.RestartNginx();
    
    if (success)
    {
        Console.WriteLine($"[API] ✅ Nginx restarted successfully by {user.Username}");
        return Results.Json(new { success = true, message });
    }
    else
    {
        Console.WriteLine($"[API] ⚠️  Nginx restart failed: {message}");
        context.Response.StatusCode = 500;
        return Results.Json(new { success = false, error = message });
    }
});

// ============================================================================
// Audit Logs API Endpoints
// ============================================================================

app.MapGet("/api/control/audit/logs", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || !authModule!.HasPermission(user, "Audit", UserRole.SuperAdmin))
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "Insufficient permissions. SuperAdmin role required." });
    }
    
    var events = await authModule!.GetSecurityEventsAsync(limit: 100);
    return Results.Json(new { logs = events });
});

// ============================================================================
// Distribution API Endpoints (Phase 4.5)
// ============================================================================

var distributionModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IDistributionModule>()
    .FirstOrDefault();

if (distributionModule != null)
{
    // Create distribution package (Admin only)
    app.MapPost("/api/distribution/create", async (HttpContext context) =>
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await authModule?.GetUserByTokenAsync(token)!;
        
        if (user == null || !authModule!.HasPermission(user, "Distribution", UserRole.Admin))
        {
            context.Response.StatusCode = 403;
            return Results.Json(new { error = "Insufficient permissions" });
        }
        
        var request = await context.Request.ReadFromJsonAsync<CreateDistributionRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "Invalid request" });
        }
        
        try
        {
            var package = await distributionModule.CreatePackageAsync(user.Id, request.LicenseKey, request.Version);
            return Results.Json(new { success = true, package });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = ex.Message });
        }
    });

    // Download distribution package (requires valid license)
    app.MapGet("/api/distribution/download/{licenseKey}", async (HttpContext context) =>
    {
        await Task.CompletedTask;
        var licenseKey = context.Request.RouteValues["licenseKey"]?.ToString();
        if (string.IsNullOrEmpty(licenseKey))
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "License key required" });
        }
        
        if (!distributionModule.IsAuthorizedForDownload(licenseKey))
        {
            context.Response.StatusCode = 403;
            return Results.Json(new { error = "Unauthorized - invalid or expired license" });
        }
        
        var packages = distributionModule.GetAllPackages()
            .Where(p => p.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();
        
        if (packages == null || !File.Exists(packages.PackagePath))
        {
            context.Response.StatusCode = 404;
            return Results.Json(new { error = "Package not found" });
        }
        
        return Results.File(packages.PackagePath, "application/zip", Path.GetFileName(packages.PackagePath));
    });

    // List all distribution packages (Admin only)
    app.MapGet("/api/distribution/packages", async (HttpContext context) =>
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await authModule?.GetUserByTokenAsync(token)!;
        
        if (user == null || !authModule!.HasPermission(user, "Distribution", UserRole.Admin))
        {
            context.Response.StatusCode = 403;
            return Results.Json(new { error = "Insufficient permissions" });
        }
        
        var packages = distributionModule.GetAllPackages();
        return Results.Json(new { packages });
    });

    Console.WriteLine("[RaCore] Distribution API endpoints registered");
}

// ============================================================================
// Update API Endpoints (Phase 4.5)
// ============================================================================

var updateModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IUpdateModule>()
    .FirstOrDefault();

if (updateModule != null)
{
    // Check for updates (requires valid license)
    app.MapGet("/api/updates/check", async (HttpContext context) =>
    {
        var currentVersion = context.Request.Query["version"].ToString();
        var licenseKey = context.Request.Query["license"].ToString();
        
        if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(licenseKey))
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "Version and license key required" });
        }
        
        try
        {
            var updateInfo = await updateModule.CheckForUpdatesAsync(currentVersion, licenseKey);
            return Results.Json(new { success = true, update = updateInfo });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = ex.Message });
        }
    });

    // Download update package (requires valid license)
    app.MapGet("/api/updates/download/{version}", async (HttpContext context) =>
    {
        await Task.CompletedTask;
        var version = context.Request.RouteValues["version"]?.ToString();
        var licenseKey = context.Request.Query["license"].ToString();
        
        if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(licenseKey))
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "Version and license key required" });
        }
        
        var package = updateModule.GetUpdatePackage(version);
        if (package == null || !File.Exists(package.PackagePath))
        {
            context.Response.StatusCode = 404;
            return Results.Json(new { error = "Update package not found" });
        }
        
        return Results.File(package.PackagePath, "application/zip", Path.GetFileName(package.PackagePath));
    });

    // List all updates (Admin only)
    app.MapGet("/api/updates/list", async (HttpContext context) =>
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await authModule?.GetUserByTokenAsync(token)!;
        
        if (user == null || !authModule!.HasPermission(user, "Updates", UserRole.Admin))
        {
            context.Response.StatusCode = 403;
            return Results.Json(new { error = "Insufficient permissions" });
        }
        
        var updates = updateModule.GetAllUpdates();
        return Results.Json(new { updates });
    });

    Console.WriteLine("[RaCore] Update API endpoints registered");
}

// ============================================================================
// GameClient API Endpoints (Phase 4.5)
// ============================================================================

var gameClientModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameClientModule>()
    .FirstOrDefault();

if (gameClientModule != null)
{
    // Generate game client (User+)
    app.MapPost("/api/gameclient/generate", async (HttpContext context) =>
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await authModule?.GetUserByTokenAsync(token)!;
        
        if (user == null)
        {
            context.Response.StatusCode = 401;
            return Results.Json(new { error = "Authentication required" });
        }
        
        var request = await context.Request.ReadFromJsonAsync<GenerateClientRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "Invalid request" });
        }
        
        try
        {
            var clientPackage = await gameClientModule.GenerateClientAsync(
                user.Id, 
                request.LicenseKey, 
                request.Platform, 
                request.Configuration);
            return Results.Json(new { success = true, client = clientPackage });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = ex.Message });
        }
    });

    // Get user's game clients
    app.MapGet("/api/gameclient/list", async (HttpContext context) =>
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await authModule?.GetUserByTokenAsync(token)!;
        
        if (user == null)
        {
            context.Response.StatusCode = 401;
            return Results.Json(new { error = "Authentication required" });
        }
        
        var clients = gameClientModule.GetUserClientPackages(user.Id);
        return Results.Json(new { clients });
    });

    // Serve game client files
    app.MapGet("/clients/{packageId}/{*file}", async (HttpContext context) =>
    {
        await Task.CompletedTask;
        var packageId = context.Request.RouteValues["packageId"]?.ToString();
        var file = context.Request.RouteValues["file"]?.ToString() ?? "index.html";
        
        if (!Guid.TryParse(packageId, out var id))
        {
            context.Response.StatusCode = 400;
            return Results.Json(new { error = "Invalid package ID" });
        }
        
        var clientPackage = gameClientModule.GetClientPackage(id);
        if (clientPackage == null)
        {
            context.Response.StatusCode = 404;
            return Results.Json(new { error = "Client not found" });
        }
        
        var filePath = Path.Combine(clientPackage.PackagePath, file);
        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = 404;
            return Results.Json(new { error = "File not found" });
        }
        
        var contentType = file.EndsWith(".html") ? "text/html" :
                         file.EndsWith(".js") ? "application/javascript" :
                         file.EndsWith(".css") ? "text/css" :
                         "application/octet-stream";
        
        return Results.File(filePath, contentType);
    });

    Console.WriteLine("[RaCore] GameClient API endpoints registered");
}

// Display startup information
Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("   RaCore Server Starting");
Console.WriteLine("========================================");
Console.WriteLine($"Server URL: http://localhost:{port}");
Console.WriteLine($"Control Panel: http://localhost:{port}/control-panel.html");
Console.WriteLine($"WebSocket: ws://localhost:{port}/ws");
Console.WriteLine();
Console.WriteLine("To use a different port, set the RACORE_PORT environment variable:");
Console.WriteLine("  Example: export RACORE_PORT=8080 (Linux/Mac)");
Console.WriteLine("  Example: set RACORE_PORT=8080 (Windows)");
Console.WriteLine();
Console.WriteLine("Apache Reverse Proxy Configuration:");
Console.WriteLine("  ✨ Apache is automatically configured during boot sequence!");
Console.WriteLine("  To customize domain: set RACORE_PROXY_DOMAIN=yourdomain.com");
Console.WriteLine("  After configuration, restart Apache to access via http://localhost");
Console.WriteLine();
Console.WriteLine("Apache Management API:");
Console.WriteLine("  POST /api/control/system/restart-apache - Restart Apache without restarting RaOS");
Console.WriteLine("  (Admin authentication required)");
Console.WriteLine("========================================");
Console.WriteLine();

app.Run();

// Request models for API endpoints
public class CreateSceneRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ============================================================================
// Request Models for Forum/Control Panel
// ============================================================================

public class ForumPostActionRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ForumLockRequest
{
    public bool Locked { get; set; }
}

public class ForumWarningRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ForumBanRequest
{
    public bool Banned { get; set; }
    public string? Reason { get; set; }
}

// ============================================================================
// Request Models for Blog, Chat, and Social
// ============================================================================

public class CreateBlogPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class CreateChatRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class AddFriendRequest
{
    public string FriendId { get; set; } = string.Empty;
}

public class CreateSocialPostRequest
{
    public string Content { get; set; } = string.Empty;
}

public class CreateAdminInstanceRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class SetupConfigRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

// ============================================================================
// Request Models for Phase 4.5 (Distribution, Updates, GameClient)
// ============================================================================

public class CreateDistributionRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class GenerateClientRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public ClientPlatform Platform { get; set; } = ClientPlatform.WebGL;
    public ClientConfiguration Configuration { get; set; } = new();
}

// ============================================================================
// Request Models for GameServer
// ============================================================================

public class GameUpdateRequest
{
    public string Description { get; set; } = string.Empty;
}

public class GameExportRequest
{
    public ExportFormat Format { get; set; } = ExportFormat.Complete;
}
