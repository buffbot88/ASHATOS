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

// Redirect root to login page
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/login.html");
    return Task.CompletedTask;
});

app.Run();