using Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RaCore.Models;

namespace RaCore.Endpoints;

/// <summary>
/// Extension methods for registering GameServer API endpoints
/// </summary>
public static class GameServerEndpoints
{
    /// <summary>
    /// Maps all game server-related API endpoints
    /// </summary>
    public static WebApplication MapGameServerEndpoints(this WebApplication app,
        IGameServerModule? gameServerModule,
        IAuthenticationModule? authModule)
    {
        if (gameServerModule == null || authModule == null)
        {
            Console.WriteLine("[RaCore] GameServer module or Authentication not available - skipping GameServer endpoints");
            return app;
        }

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
                var formatString = exportRequest?.Format ?? "Complete";
                var format = Enum.TryParse<ExportFormat>(formatString, true, out var parsedFormat) ? parsedFormat : ExportFormat.Complete;

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

        Console.WriteLine("[RaCore] GameServer API endpoints registered:");
        Console.WriteLine("  POST   /api/gameserver/create - Create game from description");
        Console.WriteLine("  GET    /api/gameserver/games - List user's games");
        Console.WriteLine("  GET    /api/gameserver/game/{gameId} - Get game details");
        Console.WriteLine("  GET    /api/gameserver/game/{gameId}/preview - Get game preview");
        Console.WriteLine("  POST   /api/gameserver/game/{gameId}/deploy - Deploy game (admin only)");
        Console.WriteLine("  PUT    /api/gameserver/game/{gameId} - Update game");
        Console.WriteLine("  DELETE /api/gameserver/game/{gameId} - Delete game (admin only)");
        Console.WriteLine("  POST   /api/gameserver/game/{gameId}/export - Export game");
        Console.WriteLine("  GET    /api/gameserver/capabilities - Get system capabilities");

        return app;
    }
}
