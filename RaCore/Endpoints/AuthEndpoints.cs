using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Models;
using System.Text.Json;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering Authentication API endpoints
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps all authentication-related API endpoints
    /// </summary>
    public static WebApplication MapAuthEndpoints(this WebApplication app, IAuthenticationModule? authModule)
    {
        if (authModule == null)
        {
            Console.WriteLine("[RaCore] Authentication module not available - skipping auth endpoints");
            return app;
        }

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

        return app;
    }
}
