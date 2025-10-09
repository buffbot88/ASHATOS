using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Models;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering GameClient API endpoints
/// </summary>
public static class GameClientEndpoints
{
    /// <summary>
    /// Maps all game client-related API endpoints
    /// </summary>
    public static WebApplication MapGameClientEndpoints(this WebApplication app,
        IGameClientModule? gameClientModule,
        IAuthenticationModule? authModule)
    {
        if (gameClientModule == null || authModule == null)
        {
            Console.WriteLine("[RaCore] GameClient module or Authentication not available - skipping GameClient endpoints");
            return app;
        }

        // Generate game client (User+) - backward compatible endpoint
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

        // Enhanced: Generate client with template (Phase 9.1)
        app.MapPost("/api/clientbuilder/generate", async (HttpContext context) =>
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await authModule?.GetUserByTokenAsync(token)!;
            
            if (user == null)
            {
                context.Response.StatusCode = 401;
                return Results.Json(new { error = "Authentication required" });
            }
            
            var request = await context.Request.ReadFromJsonAsync<GenerateClientWithTemplateRequest>();
            if (request == null)
            {
                context.Response.StatusCode = 400;
                return Results.Json(new { error = "Invalid request" });
            }
            
            try
            {
                // Check if module supports templates
                var legendaryCB = gameClientModule as dynamic;
                if (legendaryCB?.GetType()?.Name == "LegendaryClientBuilderModule")
                {
                    var clientPackage = await legendaryCB!.GenerateClientFromTemplateAsync(
                        user.Id, 
                        request.LicenseKey ?? "", 
                        request.Platform,
                        request.TemplateName ?? "",
                        request.Configuration);
                    return Results.Json(new { success = true, client = clientPackage });
                }
                else
                {
                    // Fall back to basic generation
                    var clientPackage = await gameClientModule.GenerateClientAsync(
                        user.Id, 
                        request.LicenseKey, 
                        request.Platform, 
                        request.Configuration);
                    return Results.Json(new { success = true, client = clientPackage });
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                return Results.Json(new { error = ex.Message });
            }
        });

        // Get available templates (Phase 9.1)
        app.MapGet("/api/clientbuilder/templates", async (HttpContext context) =>
        {
            await Task.CompletedTask;
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = await authModule?.GetUserByTokenAsync(token)!;
            
            if (user == null)
            {
                context.Response.StatusCode = 401;
                return Results.Json(new { error = "Authentication required" });
            }
            
            try
            {
                var legendaryCB = gameClientModule as dynamic;
                if (legendaryCB?.GetType()?.Name == "LegendaryClientBuilderModule")
                {
                    var platformParam = context.Request.Query["platform"].ToString();
                    ClientPlatform? platform = null;
                    if (!string.IsNullOrEmpty(platformParam) && Enum.TryParse<ClientPlatform>(platformParam, true, out var p))
                    {
                        platform = p;
                    }
                    
                    var templates = legendaryCB!.GetAvailableTemplates(platform);
                    return Results.Json(new { success = true, templates });
                }
                else
                {
                    context.Response.StatusCode = 400;
                    return Results.Json(new { error = "Module does not support templates" });
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                return Results.Json(new { error = ex.Message });
            }
        });

        Console.WriteLine("[RaCore] GameClient API endpoints registered:");
        Console.WriteLine("  POST /api/gameclient/generate - Generate game client");
        Console.WriteLine("  POST /api/clientbuilder/generate - Generate client with template");
        Console.WriteLine("  GET  /api/clientbuilder/templates - Get available templates");

        return app;
    }
}
