using Abstractions;
using LegendaryGameSystem.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ASHATCore.Models;
using System.Text.Json;

namespace ASHATCore.Endpoints;

/// <summary>
/// Extension methods for registering Game Engine API endpoints
/// </summary>
public static class GameEngineEndpoints
{
    /// <summary>
    /// Maps all game engine-related API endpoints
    /// </summary>
    public static WebApplication MapGameEngineEndpoints(this WebApplication app, 
        IGameEngineModule? gameEngineModule, 
        IAuthenticationModule? authModule)
    {
        if (gameEngineModule == null)
        {
            Console.WriteLine("[ASHATCore] Game Engine module not available - skipping game engine endpoints");
            return app;
        }

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

        // AI-Generate world content endpoint
        app.MapPost("/api/gameengine/scene/{sceneId}/Generate", async (HttpContext context) =>
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

        Console.WriteLine("[ASHATCore] Game Engine API endpoints registered:");
        Console.WriteLine("  POST   /api/gameengine/scene - Create scene (admin only)");
        Console.WriteLine("  GET    /api/gameengine/scenes - List scenes");
        Console.WriteLine("  GET    /api/gameengine/scene/{sceneId} - Get scene details");
        Console.WriteLine("  DELETE /api/gameengine/scene/{sceneId} - Delete scene (admin only)");
        Console.WriteLine("  POST   /api/gameengine/scene/{sceneId}/entity - Create entity (admin only)");
        Console.WriteLine("  GET    /api/gameengine/scene/{sceneId}/entities - List entities");
        Console.WriteLine("  POST   /api/gameengine/scene/{sceneId}/Generate - AI-Generate content (admin only)");
        Console.WriteLine("  GET    /api/gameengine/stats - Get engine statistics");

        // Check if we have the Legendary Game Engine with in-game chat
        var legendaryEngineModule = gameEngineModule as ILegendaryGameEngineModule;
        if (legendaryEngineModule != null)
        {
            // Create in-game chat room endpoint
            app.MapPost("/api/gameengine/scene/{sceneId}/chat/room", async (HttpContext context) =>
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

                    var sceneId = context.Request.RouteValues["sceneId"]?.ToString() ?? "";
                    var body = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
                    var roomName = body?.GetValueOrDefault("name", "General Chat") ?? "General Chat";

                    var (success, message, roomId) = await legendaryEngineModule.CreateInGameChatRoomAsync(sceneId, roomName, user?.Username ?? "unknown");
                    await context.Response.WriteAsJsonAsync(new { success, message, roomId });
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
                }
            });

            // Send in-game chat message endpoint
            app.MapPost("/api/gameengine/chat/{roomId}/message", async (HttpContext context) =>
            {
                try
                {
                    var (authorized, user, error) = await ValidateGameEngineAccess(context);
                    if (!authorized)
                    {
                        context.Response.StatusCode = error == "No token provided" || error == "Invalid or expired token" ? 401 : 403;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = error });
                        return;
                    }

                    var roomId = context.Request.RouteValues["roomId"]?.ToString() ?? "";
                    var body = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
                    var content = body?.GetValueOrDefault("content", "") ?? "";

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "Message content is required" });
                        return;
                    }

                    var (success, message, messageId) = await legendaryEngineModule.SendInGameChatMessageAsync(
                        roomId, user?.Id.ToString() ?? "unknown", user?.Username ?? "unknown", content);
                    await context.Response.WriteAsJsonAsync(new { success, message, messageId });
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
                }
            });

            // Get in-game chat messages endpoint
            app.MapGet("/api/gameengine/chat/{roomId}/messages", async (HttpContext context) =>
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

                    var roomId = context.Request.RouteValues["roomId"]?.ToString() ?? "";
                    var limitStr = context.Request.Query["limit"].ToString();
                    var limit = int.TryParse(limitStr, out var l) ? l : 50;

                    var messages = await legendaryEngineModule.GetInGameChatMessagesAsync(roomId, limit);
                    await context.Response.WriteAsJsonAsync(new { success = true, messages });
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
                }
            });

            // Get in-game chat rooms for scene endpoint
            app.MapGet("/api/gameengine/scene/{sceneId}/chat/rooms", async (HttpContext context) =>
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

                    var sceneId = context.Request.RouteValues["sceneId"]?.ToString() ?? "";
                    var rooms = await legendaryEngineModule.GetInGameChatRoomsForSceneAsync(sceneId);
                    await context.Response.WriteAsJsonAsync(new { success = true, rooms });
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
                }
            });

            Console.WriteLine("[ASHATCore] Legendary Game Engine in-game chat API endpoints registered:");
            Console.WriteLine("  POST   /api/gameengine/scene/{sceneId}/chat/room - Create in-game chat room (admin only)");
            Console.WriteLine("  POST   /api/gameengine/chat/{roomId}/message - Send message to in-game chat");
            Console.WriteLine("  GET    /api/gameengine/chat/{roomId}/messages - Get in-game chat messages");
            Console.WriteLine("  GET    /api/gameengine/scene/{sceneId}/chat/rooms - List in-game chat rooms for scene");
        }

        return app;
    }
}
