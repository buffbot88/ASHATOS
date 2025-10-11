# ASHATCore Endpoints

This directory contains modular API endpoint definitions Extracted from the main `Program.cs` file.

## Purpose

The endpoints are organized into logical modules to improve:
- **Maintainability**: Smaller, focused files are easier to understand and modify
- **Debuggability**: Issues with specific API groups can be isolated quickly
- **Extensibility**: New endpoint groups can be added following the same pattern

## Structure

Each endpoint module:
- Is a static class with extension methods
- Accepts necessary modules/dependencies as Parameters
- Registers related API endpoints
- Provides console logging for startup diagnostics
- Returns the `WebApplication` for chaining

## Available Modules

### AuthEndpoints.cs
**Purpose**: Authentication and authorization endpoints

**Endpoints**:
- `POST /api/auth/register` - User Registration
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
- `POST /api/gameengine/scene/{sceneId}/Generate` - AI-Generate content (admin)
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

### ServerSetupEndpoints.cs
**Purpose**: Server environment discovery and admin instance management

**Endpoints**:
- `GET /api/serversetup/discover` - Discover server folders (Databases, php, Admins)
- `POST /api/serversetup/admin` - Create admin instance (admin only)
- `POST /api/serversetup/php` - Setup PHP Configuration (admin only)

**Usage**:
```csharp
app.MapServerSetupEndpoints(serverSetupModule, authModule);
```

### GameServerEndpoints.cs
**Purpose**: AI-driven game creation and deployment

**Endpoints**:
- `POST /api/gameserver/create` - Create game from natural language description
- `GET /api/gameserver/games` - List user's games
- `GET /api/gameserver/game/{gameId}` - Get game project details
- `GET /api/gameserver/game/{gameId}/preview` - Get game preview
- `POST /api/gameserver/game/{gameId}/deploy` - Deploy game server (admin only)
- `PUT /api/gameserver/game/{gameId}` - Update game
- `DELETE /api/gameserver/game/{gameId}` - Delete game (admin only)
- `POST /api/gameserver/game/{gameId}/export` - Export game project
- `GET /api/gameserver/capabilities` - Get system capabilities

**Usage**:
```csharp
app.MapGameServerEndpoints(gameServerModule, authModule);
```

### ControlPanelEndpoints.cs
**Purpose**: Comprehensive admin dashboard and control panel functionality

**Endpoint Groups**:
- **Dashboard**: System statistics and overview
- **Modules**: Module management and status
- **Users**: User management and role assignment
- **Licenses**: License management and assignment
- **RaCoin**: Currency management and balances
- **Forum**: Forum moderation API
- **Blog**: Blog management API
- **Chat**: Chat system API
- **Social**: Social profiles and activity
- **Supermarket**: Digital marketplace API
- **Monitoring**: System health and performance
- **Audit**: Security audit logs

**Usage**:
```csharp
app.MapControlPanelEndpoints(moduleManager, authModule, licenseModule, RaCoinModule, gameEngineModule);
```

### DistributionEndpoints.cs
**Purpose**: Distribution package management and software updates

**Endpoints**:
- `POST /api/distribution/create` - Create distribution package (admin only)
- `GET /api/distribution/download/{licenseKey}` - Download distribution package (requires license)
- `GET /api/distribution/packages` - List all packages (admin only)
- `GET /api/updates/check` - Check for updates (requires license)
- `GET /api/updates/download/{version}` - Download update package (requires license)
- `GET /api/updates/list` - List all updates (admin only)

**Usage**:
```csharp
app.MapDistributionEndpoints(distributionModule, updateModule, authModule);
```

### GameClientEndpoints.cs
**Purpose**: Game client Generation and template management

**Endpoints**:
- `POST /api/gameclient/Generate` - Generate game client (backward compatible)
- `POST /api/clientbuilder/Generate` - Generate client with template (Phase 9.1)
- `GET /api/clientbuilder/templates` - Get available templates

**Usage**:
```csharp
app.MapGameClientEndpoints(gameClientModule, authModule);
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
namespace ASHATCore.Endpoints;

public static class MyNewEndpoints
{
    public static WebApplication MapMyNewEndpoints(
        this WebApplication app, 
        IMyModule? myModule,
        IAuthenticationModule? authModule)
    {
        if (myModule == null)
        {
            Console.WriteLine("[ASHATCore] MyModule not available - skipping endpoints");
            return app;
        }

        app.MapGet("/api/mynew/endpoint", async (HttpContext context) =>
        {
            // Implementation
        });

        Console.WriteLine("[ASHATCore] MyNew API endpoints registered:");
        Console.WriteLine("  GET /api/mynew/endpoint");

        return app;
    }
}
```

Then in `Program.cs`:
```csharp
using ASHATCore.Endpoints;

// ...

app.MapMyNewEndpoints(myModule, authModule);
```

## Benefits

### Before Refactoring (Initial State)
- `Program.cs`: 3354 lines
- All endpoints inline, hard to navigate
- Difficult to locate specific API logic
- Challenging to debug issues

### After Phase 1 (Auth & GameEngine)
- `Program.cs`: 2830 lines (15.6% reduction)
- Auth and GameEngine endpoints Extracted
- Clear sepaASHATtion by domain began

### After Phase 2 (All Endpoint Groups - Current)
- `Program.cs`: 728 lines (78.3% reduction from initial!)
- **All 7 endpoint modules Extracted**:
  - AuthEndpoints (5 endpoints)
  - GameEngineEndpoints (11 endpoints)
  - ServerSetupEndpoints (3 endpoints)
  - GameServerEndpoints (9 endpoints)
  - ControlPanelEndpoints (12 endpoint groups, ~70 endpoints)
  - DistributionEndpoints (6 endpoints)
  - GameClientEndpoints (3 endpoints)
- Clear sepaASHATtion by domain
- Easy to find and modify endpoint groups
- Modular architecture supports future growth
- Significantly improved maintainability

## Completed Work

All planned endpoint groups have been successfully Extracted:
- ✅ **ServerSetup** (~140 lines): Environment discovery, admin setup
- ✅ **GameServer** (~327 lines): Game creation and deployment
- ✅ **ControlPanel** (~1400 lines): Server config, blog, chat, social, supermarket, monitoring
- ✅ **Distribution** (~167 lines): Distribution and update management
- ✅ **GameClient** (~130 lines): Client Generation and templates

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
