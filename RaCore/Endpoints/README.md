# RaCore Endpoints

This directory contains modular API endpoint definitions extracted from the main `Program.cs` file.

## Purpose

The endpoints are organized into logical modules to improve:
- **Maintainability**: Smaller, focused files are easier to understand and modify
- **Debuggability**: Issues with specific API groups can be isolated quickly
- **Extensibility**: New endpoint groups can be added following the same pattern

## Structure

Each endpoint module:
- Is a static class with extension methods
- Accepts necessary modules/dependencies as parameters
- Registers related API endpoints
- Provides console logging for startup diagnostics
- Returns the `WebApplication` for chaining

## Available Modules

### AuthEndpoints.cs
**Purpose**: Authentication and authorization endpoints

**Endpoints**:
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/validate` - Token validation
- `GET /api/auth/events` - Security events (admin only)

**Usage**:
```csharp
app.MapAuthEndpoints(authModule);
```

### GameEngineEndpoints.cs
**Purpose**: Game engine and world management endpoints

**Endpoints**:
- `POST /api/gameengine/scene` - Create scene (admin)
- `GET /api/gameengine/scenes` - List scenes
- `GET /api/gameengine/scene/{sceneId}` - Get scene details
- `DELETE /api/gameengine/scene/{sceneId}` - Delete scene (admin)
- `POST /api/gameengine/scene/{sceneId}/entity` - Create entity (admin)
- `GET /api/gameengine/scene/{sceneId}/entities` - List entities
- `POST /api/gameengine/scene/{sceneId}/generate` - AI-generate content (admin)
- `GET /api/gameengine/stats` - Get engine statistics

**Legendary Game Engine Extensions** (if available):
- `POST /api/gameengine/scene/{sceneId}/chat/room` - Create chat room (admin)
- `POST /api/gameengine/chat/{roomId}/message` - Send chat message
- `GET /api/gameengine/chat/{roomId}/messages` - Get chat messages
- `GET /api/gameengine/scene/{sceneId}/chat/rooms` - List chat rooms

**Usage**:
```csharp
app.MapGameEngineEndpoints(gameEngineModule, authModule);
```

## Pattern for Creating New Endpoint Modules

1. Create a new static class in this directory
2. Implement an extension method that accepts `WebApplication` and required modules
3. Register all related endpoints within the method
4. Add console logging for diagnostics
5. Return the `WebApplication` for chaining
6. Update `Program.cs` to call your extension method

**Example**:
```csharp
namespace RaCore.Endpoints;

public static class MyNewEndpoints
{
    public static WebApplication MapMyNewEndpoints(
        this WebApplication app, 
        IMyModule? myModule,
        IAuthenticationModule? authModule)
    {
        if (myModule == null)
        {
            Console.WriteLine("[RaCore] MyModule not available - skipping endpoints");
            return app;
        }

        app.MapGet("/api/mynew/endpoint", async (HttpContext context) =>
        {
            // Implementation
        });

        Console.WriteLine("[RaCore] MyNew API endpoints registered:");
        Console.WriteLine("  GET /api/mynew/endpoint");

        return app;
    }
}
```

Then in `Program.cs`:
```csharp
using RaCore.Endpoints;

// ...

app.MapMyNewEndpoints(myModule, authModule);
```

## Benefits

### Before Refactoring
- `Program.cs`: 3354 lines
- All endpoints inline, hard to navigate
- Difficult to locate specific API logic
- Challenging to debug issues

### After Refactoring
- `Program.cs`: 2830 lines (15.6% reduction)
- Clear separation by domain
- Easy to find and modify endpoint groups
- Modular architecture supports future growth

## Future Work

Additional endpoint groups that can be extracted following this pattern:
- **ServerSetup** (~250 lines): Environment discovery, admin setup
- **GameServer** (~480 lines): Game creation and deployment
- **ControlPanel** (~1600 lines): Server config, blog, chat, social, supermarket, monitoring
- **Distribution** (~150 lines): Distribution management
- **GameClient** (~300 lines): Client generation

## Related Documentation

- [API Communication & Refactoring Guide](../../docs/API_COMMUNICATION_AND_REFACTORING.md)
- [API Endpoint Registration Tests](../Tests/ApiEndpointRegistrationTests.cs)

## Conventions

1. **Naming**: `{Domain}Endpoints.cs` (e.g., `AuthEndpoints.cs`)
2. **Method naming**: `Map{Domain}Endpoints` (e.g., `MapAuthEndpoints`)
3. **Logging**: Always log registered endpoints for visibility
4. **Null handling**: Check if required modules are available before registering
5. **Return value**: Return `WebApplication` for method chaining
6. **Authorization**: Use consistent authorization patterns from existing modules
