using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Models;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering Distribution and Update API endpoints
/// </summary>
public static class DistributionEndpoints
{
    /// <summary>
    /// Maps all distribution and update-related API endpoints
    /// </summary>
    public static WebApplication MapDistributionEndpoints(this WebApplication app,
        IDistributionModule? distributionModule,
        IUpdateModule? updateModule,
        IAuthenticationModule? authModule)
    {
        // Distribution endpoints
        if (distributionModule != null && authModule != null)
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

        // Update endpoints
        if (updateModule != null && authModule != null)
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

        return app;
    }
}
