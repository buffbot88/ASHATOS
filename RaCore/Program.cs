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

// Redirect root to login page
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/login.html");
    return Task.CompletedTask;
});

app.Run();

// Request models for API endpoints
public class CreateSceneRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
