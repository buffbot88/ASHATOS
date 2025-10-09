using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Models;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering ServerSetup API endpoints
/// </summary>
public static class ServerSetupEndpoints
{
    /// <summary>
    /// Maps all server setup-related API endpoints
    /// </summary>
    public static WebApplication MapServerSetupEndpoints(this WebApplication app,
        IServerSetupModule? serverSetupModule,
        IAuthenticationModule? authModule)
    {
        if (serverSetupModule == null || authModule == null)
        {
            Console.WriteLine("[RaCore] ServerSetup module or Authentication not available - skipping ServerSetup endpoints");
            return app;
        }

        // Discover server folders (Databases, php, Admins)
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

        Console.WriteLine("[RaCore] ServerSetup API endpoints registered:");
        Console.WriteLine("  GET  /api/serversetup/discover - Discover server folders");
        Console.WriteLine("  POST /api/serversetup/admin - Create admin instance (admin only)");
        Console.WriteLine("  POST /api/serversetup/php - Setup PHP config (admin only)");

        return app;
    }
}
