using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using SQLitePCL;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:7077");

var app = builder.Build();
app.UseWebSockets();
app.UseStaticFiles();

// 1. Instantiate MemoryModule FIRST
var memoryModule = new MemoryModule();
memoryModule.Initialize(null); // Pass ModuleManager if needed

// 2. Instantiate ModuleManager and register MemoryModule as built-in
var moduleManager = new ModuleManager();
moduleManager.RegisterBuiltInModule(memoryModule);

// 3. Load other modules (plugins, etc.)
moduleManager.LoadModules();

// 4. Check for first run and auto-spawn CMS + Apache
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

            var result = await serverSetupModule.SetupApacheConfigAsync(request.LicenseNumber, request.Username);
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
    Console.WriteLine("  POST   /api/serversetup/apache - Setup Apache config (admin only)");
    Console.WriteLine("  POST   /api/serversetup/php - Setup PHP config (admin only)");
}

// Redirect root to login page
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/login.html");
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

