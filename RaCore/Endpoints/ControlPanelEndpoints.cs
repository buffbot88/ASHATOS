using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Engine.Manager;
using RaCore.Models;
using RaCore.Modules.Extensions.UserProfiles;
using System.Text.Json;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering Control Panel API endpoints
/// </summary>
public static class ControlPanelEndpoints
{
    /// <summary>
    /// Maps all control panel-related API endpoints
    /// </summary>
    public static WebApplication MapControlPanelEndpoints(this WebApplication app,
        ModuleManager moduleManager,
        IAuthenticationModule? authModule,
        ILicenseModule? licenseModule,
        IRaCoinModule? racoinModule,
        IGameEngineModule? gameEngineModule)
    {
        if (authModule == null)
        {
            Console.WriteLine("[RaCore] Authentication module not available - skipping Control Panel endpoints");
            return app;
        }

// Get LearningModule reference for LULModule endpoints
var learningModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ILearningModule>()
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
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Request body required" });
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
    
    var success = await forumModule.DeletePostAsync(postId, user.Id.ToString(), "Deleted by moderator");
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
    
    var success = await forumModule.BanUserAsync(userId, true, body.Reason ?? "", user.Id.ToString());
    return Results.Json(new { success, message = "User banned" });
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
    
    var result = await blogModule.CreatePostAsync(user.Id.ToString(), user.Username, body.Title, body.Content, body.Tags?.FirstOrDefault() ?? "General");
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
    if (authModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Authentication not available" });
    }
    
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule.GetUserByTokenAsync(token);
    
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
    if (authModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Authentication not available" });
    }
    
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule.GetUserByTokenAsync(token);
    
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
    if (authModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Authentication not available" });
    }
    
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule.GetUserByTokenAsync(token);
    
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
// Legendary Supermarket API Endpoints
// ============================================================================

app.MapGet("/api/market/catalog", async (HttpContext context) =>
{
    await Task.CompletedTask; // Suppress CS1998 warning - endpoint is synchronous but uses async for consistency
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var result = marketModule.Process("market catalog");
    return Results.Content(result, "application/json");
});

app.MapGet("/api/market/listings", async (HttpContext context) =>
{
    await Task.CompletedTask;
    var currencyType = context.Request.Query["currency"].ToString();
    var category = context.Request.Query["category"].ToString();
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var command = string.IsNullOrEmpty(category) 
        ? $"market filter {currencyType}"
        : $"market filter {currencyType} {category}";
    
    var result = marketModule.Process(command);
    return Results.Content(result, "application/json");
});

app.MapGet("/api/market/search", async (HttpContext context) =>
{
    await Task.CompletedTask;
    var searchTerm = context.Request.Query["q"].ToString();
    
    if (string.IsNullOrEmpty(searchTerm))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Search term required" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var result = marketModule.Process($"market search {searchTerm}");
    return Results.Content(result, "application/json");
});

app.MapGet("/api/market/seller/{sellerId}", async (HttpContext context) =>
{
    await Task.CompletedTask;
    var sellerId = context.Request.RouteValues["sellerId"]?.ToString();
    
    if (string.IsNullOrEmpty(sellerId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Seller ID required" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var result = marketModule.Process($"market seller {sellerId}");
    return Results.Content(result, "application/json");
});

app.MapPost("/api/market/list", async (HttpContext context) =>
{
    var body = await context.Request.ReadFromJsonAsync<MarketListingRequest>();
    
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Invalid request body" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var command = $"market list {body.SellerId} {body.ItemName} {body.CurrencyType} {body.Price} {body.Quantity} {body.Category} {body.Description}";
    var result = marketModule.Process(command);
    return Results.Content(result, "application/json");
});

app.MapPost("/api/market/purchase", async (HttpContext context) =>
{
    var body = await context.Request.ReadFromJsonAsync<MarketPurchaseRequest>();
    
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Invalid request body" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var command = body.IsMarketplaceListing
        ? $"market purchase {body.BuyerId} {body.ListingId} {body.Quantity}"
        : $"market buy {body.BuyerId} {body.ProductId}";
    
    var result = marketModule.Process(command);
    return Results.Content(result, "application/json");
});

app.MapPost("/api/market/review", async (HttpContext context) =>
{
    var body = await context.Request.ReadFromJsonAsync<MarketReviewRequest>();
    
    if (body == null)
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "Invalid request body" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var command = $"market review {body.ReviewerId} {body.SellerId} {body.PurchaseId} {body.Rating} {body.Comment}";
    var result = marketModule.Process(command);
    return Results.Content(result, "application/json");
});

app.MapGet("/api/market/stats", async (HttpContext context) =>
{
    await Task.CompletedTask;
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var result = marketModule.Process("market stats");
    return Results.Content(result, "application/json");
});

app.MapGet("/api/market/history/{userId}", async (HttpContext context) =>
{
    await Task.CompletedTask;
    var userId = context.Request.RouteValues["userId"]?.ToString();
    
    if (string.IsNullOrEmpty(userId))
    {
        context.Response.StatusCode = 400;
        return Results.Json(new { error = "User ID required" });
    }
    
    var marketModule = moduleManager.Modules
        .Select(m => m.Instance)
        .FirstOrDefault(m => m.GetType().Name == "LegendarySupermarketModule");
    
    if (marketModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Legendary Supermarket module not available" });
    }
    
    var result = marketModule.Process($"market history {userId}");
    return Results.Content(result, "application/json");
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

// ============================================================================
// LULModule API Endpoints
// ============================================================================

app.MapGet("/api/learning/superadmin/status", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || user.Role != UserRole.SuperAdmin)
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "SuperAdmin role required" });
    }
    
    if (learningModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Learning module not available" });
    }
    
    var hasCompleted = await learningModule.HasCompletedSuperAdminCoursesAsync(user.Id.ToString());
    var courses = await learningModule.GetCoursesAsync("SuperAdmin");
    
    return Results.Json(new 
    { 
        hasCompleted,
        totalCourses = courses.Count,
        courses = courses.Select(c => new { c.Id, c.Title, c.Description, c.LessonCount })
    });
});

app.MapPost("/api/learning/superadmin/complete", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null || user.Role != UserRole.SuperAdmin)
    {
        context.Response.StatusCode = 403;
        return Results.Json(new { error = "SuperAdmin role required" });
    }
    
    if (learningModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Learning module not available" });
    }
    
    var success = await learningModule.MarkSuperAdminCoursesCompletedAsync(user.Id.ToString());
    
    return Results.Json(new 
    { 
        success,
        message = success ? "All SuperAdmin courses marked as completed" : "Failed to mark courses as completed"
    });
});

app.MapGet("/api/learning/courses/{level}", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    if (learningModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Learning module not available" });
    }
    
    var level = context.Request.RouteValues["level"]?.ToString() ?? "User";
    var courses = await learningModule.GetCoursesAsync(level);
    
    return Results.Json(new { courses });
});

app.MapGet("/api/learning/courses/{courseId}/lessons", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    if (learningModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Learning module not available" });
    }
    
    var courseId = context.Request.RouteValues["courseId"]?.ToString() ?? "";
    var lessons = await learningModule.GetLessonsAsync(courseId);
    
    return Results.Json(new { lessons });
});

app.MapPost("/api/learning/lessons/{lessonId}/complete", async (HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await authModule?.GetUserByTokenAsync(token)!;
    
    if (user == null)
    {
        context.Response.StatusCode = 401;
        return Results.Json(new { error = "Authentication required" });
    }
    
    if (learningModule == null)
    {
        context.Response.StatusCode = 503;
        return Results.Json(new { error = "Learning module not available" });
    }
    
    var lessonId = context.Request.RouteValues["lessonId"]?.ToString() ?? "";
    var success = await learningModule.CompleteLessonAsync(user.Id.ToString(), lessonId);
    
    return Results.Json(new 
    { 
        success,
        message = success ? "Lesson marked as completed" : "Failed to mark lesson as completed"
    });
});

        Console.WriteLine("[RaCore] Control Panel API endpoints registered");

        return app;
    }
}
