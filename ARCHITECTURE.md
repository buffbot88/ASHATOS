# 🏗️ RaOS Architecture Documentation

## Overview

This document provides a comprehensive overview of the RaOS (Ra Operating System) architecture, including system design, component relationships, data flow, and key architectural decisions.

**Version:** 9.0.0  
**Last Updated:** January 13, 2025  
**Status:** Production Ready

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architectural Patterns](#architectural-patterns)
3. [Core Components](#core-components)
4. [Module System](#module-system)
5. [Data Architecture](#data-architecture)
6. [API Architecture](#api-architecture)
7. [Security Architecture](#security-architecture)
8. [Deployment Architecture](#deployment-architecture)
9. [Extension Points](#extension-points)
10. [Design Decisions](#design-decisions)

---

## System Overview

RaOS is a modular AI-powered platform that combines content management, game engine capabilities, and extensible module architecture.

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         RaOS Platform                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              RaCore (Main Server)                     │  │
│  │  - Module Manager                                     │  │
│  │  - ASP.NET Core Web API                              │  │
│  │  - Authentication & Authorization                     │  │
│  │  - WebSocket Hub                                      │  │
│  │  - Configuration Management                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                 │
│           ┌────────────────┼────────────────┐               │
│           ▼                ▼                ▼               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Legendary   │  │ Legendary   │  │  Extension  │        │
│  │ CMS.dll     │  │ GameEngine  │  │  Modules    │        │
│  │ (76 KB)     │  │ .dll (120KB)│  │  (30+)      │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
│                                                               │
├─────────────────────────────────────────────────────────────┤
│                    Data Layer                                │
│  - SQLite (Default)                                          │
│  - PostgreSQL/MySQL (Optional)                               │
│  - Redis Cache (Optional)                                    │
└─────────────────────────────────────────────────────────────┘
```

### Key Characteristics

- **Modular Design:** Components are loosely coupled and independently deployable
- **DLL-Based Extensions:** Core features in external DLLs for hot-swapping
- **API-First:** All functionality exposed through RESTful APIs
- **Event-Driven:** Components communicate through events and messages
- **Scalable:** Designed to scale horizontally and vertically

---

## Architectural Patterns

### 1. Plugin Architecture

RaOS uses a plugin-based architecture where functionality is organized into modules that can be loaded, unloaded, and replaced at runtime.

```csharp
// Module Discovery Pattern
[RaModule(Category = "extensions")]
public sealed class MyModule : ModuleBase
{
    public override string Name => "MyModule";
    
    public override void Initialize(object? manager)
    {
        // Module initialization
    }
    
    public override string Process(string input)
    {
        // Command processing
    }
}
```

**Benefits:**
- Easy to add new features
- Modules can be developed independently
- Hot-reload support (without restart)
- Clean separation of concerns

### 2. Event-Driven Architecture

Components communicate through events rather than direct coupling.

```csharp
// Event Publishing
context.PublishEvent("cms.user.created", userData);

// Event Subscription
context.RegisterEventHandler("cms.user.created", async (data) => 
{
    // Handle user creation
});
```

**Benefits:**
- Loose coupling between components
- Asynchronous processing
- Easy to add new event handlers
- Better testability

### 3. Repository Pattern

Data access is abstracted through repositories.

```csharp
public interface IGameEngineRepository
{
    Task<Scene> GetSceneAsync(int id);
    Task<List<Scene>> GetAllScenesAsync();
    Task<int> CreateSceneAsync(Scene scene);
    Task<bool> UpdateSceneAsync(Scene scene);
    Task<bool> DeleteSceneAsync(int id);
}
```

**Benefits:**
- Database agnostic
- Easy to test with mocks
- Centralized data access logic
- Better maintainability

### 4. Strategy Pattern

Algorithms are encapsulated and made interchangeable.

```csharp
public interface IContentGenerator
{
    Task<string> GenerateAsync(ContentRequest request);
}

public class AIContentGenerator : IContentGenerator { }
public class TemplateContentGenerator : IContentGenerator { }
```

**Benefits:**
- Flexible algorithm selection
- Easy to add new strategies
- Better code organization
- Runtime algorithm switching

---

## Core Components

### 1. RaCore (Main Server)

**Location:** `/RaCore`  
**Technology:** ASP.NET Core 9.0  
**Port:** 80 (default, configurable)

**Responsibilities:**
- HTTP/WebSocket server
- Module discovery and loading
- Request routing
- Authentication/authorization
- Configuration management
- Logging and monitoring

**Key Files:**
- `Program.cs` - Application entry point and configuration
- `Engine/Manager/ModuleManager.cs` - Module lifecycle management
- `Modules/` - Extension modules directory

### 2. LegendaryCMS Module

**Location:** `/LegendaryCMS`  
**Type:** External DLL  
**Size:** 76 KB  
**Version:** 8.0.0

**Responsibilities:**
- Content management system
- Plugin system
- REST API (11+ endpoints)
- RBAC permissions (25+)
- Configuration management

**Key Components:**
```
LegendaryCMS/
├── Core/
│   ├── LegendaryCMSModule.cs       # Main module
│   └── PluginManager.cs            # Plugin system
├── Plugins/
│   └── ICMSPlugin.cs               # Plugin interface
├── API/
│   ├── CMSAPIManager.cs            # API management
│   └── CMSAPIModels.cs             # API models
├── Security/
│   └── RBACManager.cs              # Permissions
└── Configuration/
    └── CMSConfiguration.cs         # Config management
```

### 3. LegendaryGameEngine Module

**Location:** `/LegendaryGameEngine`  
**Type:** External DLL  
**Size:** 120 KB  
**Version:** 9.0.0

**Responsibilities:**
- Scene management
- Entity system
- AI-driven world generation
- In-game chat system
- WebSocket broadcasting
- SQLite persistence

**Key Components:**
```
LegendaryGameEngine/
├── Core/
│   ├── LegendaryGameEngineModule.cs    # Main module
│   ├── GameEngineDatabase.cs           # Persistence
│   └── InGameChatManager.cs            # Chat system
├── Physics/ (planned)
│   └── PhysicsEngine.cs
└── AI/ (planned)
    └── BehaviorTreeSystem.cs
```

### 4. Extension Modules (30+)

**Location:** `/RaCore/Modules/Extensions`  
**Type:** C# Classes  
**Category:** Various

**Categories:**
- **Security:** Authentication, License, Safety
- **Content:** Blog, Forum, Chat, UserProfiles
- **Commerce:** SuperMarket, RaCoin, MarketMonitor
- **Development:** ModuleSpawner, CodeGeneration, TestRunner
- **Infrastructure:** ServerSetup, Updates, SiteBuilder
- **AI:** Language, Knowledge, Planning, Execution

**Common Pattern:**
```csharp
[RaModule(Category = "extensions")]
public sealed class ExampleModule : ModuleBase
{
    public override string Name => "Example";
    
    public override void Initialize(object? manager)
    {
        // Setup
    }
    
    public override string Process(string input)
    {
        // Handle commands
        return "Response";
    }
}
```

---

## Module System

### Module Lifecycle

```
┌─────────────┐
│  Discovery  │  ModuleManager scans directories
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Loading   │  Load DLL and create instance
└──────┬──────┘
       │
       ▼
┌─────────────┐
│Initialize() │  Module setup and registration
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Running   │  Process() handles commands
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Unloading  │  Cleanup and dispose
└─────────────┘
```

### Module Discovery

**Auto-Discovery Process:**

1. **Scan Directories:** ModuleManager recursively scans:
   - `/RaCore/Modules/Extensions/`
   - `/LegendaryCMS/`
   - `/LegendaryGameEngine/`
   - Custom paths from configuration

2. **Identify Modules:** Look for classes with `[RaModule]` attribute:
   ```csharp
   [RaModule(Category = "extensions")]
   public sealed class MyModule : ModuleBase { }
   ```

3. **Load & Initialize:** Create instance and call `Initialize()`

4. **Register:** Add to active modules list

### Module Communication

**Direct Invocation:**
```csharp
var result = moduleManager.InvokeModule("MyModule", "command arg1 arg2");
```

**Event-Based:**
```csharp
// Publisher
moduleManager.PublishEvent("event.name", eventData);

// Subscriber
moduleManager.SubscribeEvent("event.name", handler);
```

**API Endpoints:**
```csharp
app.MapPost("/api/mymodule/action", async (HttpContext ctx) => 
{
    var result = moduleManager.InvokeModule("MyModule", "action");
    return Results.Ok(result);
});
```

---

## Data Architecture

### Database Strategy

RaOS uses a multi-database strategy:

```
┌─────────────────────────────────────────────────────┐
│                   Application                        │
└──────────────────┬──────────────────────────────────┘
                   │
        ┌──────────┼──────────┐
        ▼          ▼          ▼
   ┌────────┐ ┌────────┐ ┌────────┐
   │SQLite  │ │SQLite  │ │SQLite  │
   │CMS     │ │Game    │ │Module  │
   │Data    │ │Engine  │ │Data    │
   └────────┘ └────────┘ └────────┘
```

**SQLite Databases:**
- `cms.db` - CMS content, users, posts
- `gameengine.db` - Scenes, entities, chat
- `modules.db` - Module-specific data

**Optional Integration:**
- PostgreSQL for production CMS
- MySQL for legacy compatibility
- Redis for caching and sessions

### Data Models

**Entity Framework Core:**
```csharp
public class Scene
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<Entity> Entities { get; set; }
}
```

**Migrations:**
```bash
# Add migration
dotnet ef migrations add MigrationName --project LegendaryGameEngine

# Apply migration
dotnet ef database update --project LegendaryGameEngine
```

---

## API Architecture

### RESTful API Design

**Base URL:** `http://localhost:80/api`

**Endpoint Structure:**
```
/api/{module}/{resource}/{action}
```

**Examples:**
- `GET /api/cms/health` - Health check
- `POST /api/gameengine/scene` - Create scene
- `GET /api/gameengine/scenes` - List scenes
- `DELETE /api/gameengine/scene/{id}` - Delete scene

### Authentication Flow

```
┌────────┐                 ┌────────┐                ┌────────┐
│ Client │                 │ RaCore │                │ Module │
└───┬────┘                 └───┬────┘                └───┬────┘
    │                          │                         │
    │  POST /api/auth/login    │                         │
    ├─────────────────────────>│                         │
    │                          │                         │
    │  { token: "..." }        │                         │
    │<─────────────────────────┤                         │
    │                          │                         │
    │  GET /api/resource       │                         │
    │  Authorization: Bearer   │                         │
    ├─────────────────────────>│                         │
    │                          │  Validate Token         │
    │                          │                         │
    │                          │  Invoke Module          │
    │                          ├────────────────────────>│
    │                          │                         │
    │                          │  Result                 │
    │                          │<────────────────────────┤
    │  { data: "..." }         │                         │
    │<─────────────────────────┤                         │
```

### WebSocket Architecture

**Real-time Communication:**

```javascript
// Client connection
const ws = new WebSocket('ws://localhost:80/ws');

ws.onopen = () => {
    console.log('Connected');
};

ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    console.log('Received:', data);
};
```

**Server Broadcasting:**
```csharp
public async Task BroadcastEvent(string eventType, object data)
{
    var message = new { type = eventType, data = data };
    await _broadcaster.BroadcastAsync(
        "GameEngineEvents", 
        JsonSerializer.Serialize(message)
    );
}
```

---

## Security Architecture

### Authentication & Authorization

**PBKDF2 Password Hashing:**
```csharp
public static string HashPassword(string password)
{
    using var pbkdf2 = new Rfc2898DeriveBytes(
        password, 
        saltSize: 16, 
        iterations: 10000,
        HashAlgorithmName.SHA256
    );
    // ... implementation
}
```

**RBAC System:**
```
Roles:
  SuperAdmin (4) - Full access
  Admin (3) - Manage users and content
  Moderator (2) - Moderate content
  User (1) - Basic access
  Guest (0) - Read-only

Permissions:
  cms.* - All CMS operations
  forum.post - Create forum posts
  gameengine.scene.create - Create game scenes
  admin.users - Manage users
```

**Permission Check:**
```csharp
public async Task<bool> HasPermission(
    string userId, 
    string permission)
{
    var user = await GetUserAsync(userId);
    var role = await GetRoleAsync(user.RoleId);
    return role.Permissions.Contains(permission);
}
```

### Rate Limiting

**Per-Client Limits:**
```csharp
// 60 requests per minute
// 1000 requests per hour
var rateLimiter = new RateLimiter(
    requestsPerMinute: 60,
    requestsPerHour: 1000
);
```

### Content Security

**XSS Protection:**
```csharp
public string SanitizeInput(string input)
{
    return HttpUtility.HtmlEncode(input);
}
```

**Content Moderation:**
- Real-time scanning
- Violation detection
- Automated actions
- Appeal system

---

## Deployment Architecture

### Development Environment

```
Developer Machine
├── .NET 9.0 SDK
├── Visual Studio / VS Code
├── SQLite (embedded)
└── Port 80 (or custom via env var)
```

**Run:**
```bash
cd RaCore
dotnet run
```

### Production Environment (Linux)

```
Ubuntu 22.04 LTS Server
├── .NET 9.0 Runtime
├── Nginx (reverse proxy)
├── PostgreSQL (optional)
├── Redis (optional)
├── SystemD (service management)
└── UFW (firewall)
```

**Deployment:**
```bash
# Build for production
./build-linux-production.sh

# Deploy
sudo systemctl start racore
sudo systemctl enable racore
```

### Docker Deployment (Future)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "RaCore.dll"]
```

---

## Extension Points

### 1. Creating Custom Modules

**Step 1: Create Module Class**
```csharp
using Abstractions;

namespace MyExtensions;

[RaModule(Category = "extensions")]
public sealed class CustomModule : ModuleBase
{
    public override string Name => "Custom";
    
    public override void Initialize(object? manager)
    {
        Log("Custom module initialized");
    }
    
    public override string Process(string input)
    {
        if (input.StartsWith("custom "))
        {
            return HandleCustomCommand(input);
        }
        return string.Empty;
    }
}
```

**Step 2: Place in Modules Directory**
```
/RaCore/Modules/Extensions/Custom/
└── CustomModule.cs
```

**Step 3: Restart RaCore**
Module will be auto-discovered and loaded.

### 2. Creating CMS Plugins

**Step 1: Implement Plugin Interface**
```csharp
using LegendaryCMS.Plugins;

public class MyPlugin : ICMSPlugin
{
    public Guid Id => Guid.Parse("...");
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    
    public async Task InitializeAsync(IPluginContext context)
    {
        context.RegisterEventHandler(
            "cms.startup", 
            OnStartup
        );
    }
    
    private async Task OnStartup(object? data)
    {
        // Plugin logic
    }
}
```

**Step 2: Load Plugin**
```csharp
await pluginManager.LoadPluginAsync("/path/to/plugin.dll");
```

### 3. Adding API Endpoints

**Register in Program.cs:**
```csharp
app.MapPost("/api/custom/action", async (HttpContext ctx) =>
{
    // Authentication
    if (!await IsAuthenticated(ctx))
        return Results.Unauthorized();
    
    // Get module
    var module = moduleManager.GetModule("Custom");
    if (module == null)
        return Results.NotFound();
    
    // Invoke
    var result = module.Process("action");
    return Results.Ok(new { result });
});
```

### 4. Extending Game Engine

**Add Components to Entities:**
```csharp
public class CustomComponent : IEntityComponent
{
    public void Update(float deltaTime)
    {
        // Custom logic
    }
}

// Attach to entity
entity.AddComponent(new CustomComponent());
```

---

## Design Decisions

### Why External DLLs?

**Decision:** Extract CMS and Game Engine into separate DLLs

**Reasoning:**
- ✅ Hot-swappable without restarting RaCore
- ✅ Independent versioning
- ✅ Reduced coupling
- ✅ Easier testing
- ✅ Better organization

### Why SQLite?

**Decision:** Use SQLite as default database

**Reasoning:**
- ✅ Zero configuration
- ✅ Embedded database
- ✅ Fast for small to medium workloads
- ✅ Cross-platform
- ✅ Easy backup (single file)

**Trade-offs:**
- ❌ Limited concurrency
- ❌ Not ideal for large-scale deployments
- ✅ Can migrate to PostgreSQL/MySQL for production

### Why ASP.NET Core?

**Decision:** Use ASP.NET Core for web server

**Reasoning:**
- ✅ High performance
- ✅ Cross-platform
- ✅ Built-in DI container
- ✅ Excellent WebSocket support
- ✅ Modern async/await patterns
- ✅ Great tooling

### Why Module Pattern?

**Decision:** Plugin/module architecture for extensions

**Reasoning:**
- ✅ Easy to extend
- ✅ Clean separation
- ✅ Third-party contributions
- ✅ Hot-reload support
- ✅ Better testability

### Why Separate Chat Systems?

**Decision:** Separate CMS chat from in-game chat

**Reasoning:**
- ✅ Different use cases (forums vs. game)
- ✅ Different persistence requirements
- ✅ Different moderation needs
- ✅ Better performance isolation
- ✅ Clearer architecture

---

## Performance Considerations

### Optimization Strategies

1. **Lazy Loading:** Modules loaded on-demand
2. **Caching:** Frequently accessed data cached
3. **Connection Pooling:** Database connection reuse
4. **Async Operations:** Non-blocking I/O
5. **Compression:** WebSocket message compression

### Monitoring

**Metrics to Track:**
- Request latency (p50, p95, p99)
- Module initialization time
- Database query time
- Memory usage
- CPU usage
- WebSocket connection count

**Tools:**
- Built-in health checks
- Custom performance profiler (planned)
- Application Insights (optional)

---

## Future Architecture

### Planned Enhancements

1. **Microservices:** Break into smaller services
2. **Event Sourcing:** Audit trail and replay
3. **CQRS:** Separate read/write models
4. **Service Mesh:** Istio/Linkerd integration
5. **Kubernetes:** Container orchestration

---

## Resources

### Documentation
- [Module Structure Guide](MODULE_STRUCTURE_GUIDE.md)
- [Security Architecture](SECURITY_ARCHITECTURE.md)
- [API Documentation](GAMEENGINE_API.md)

### Code Examples
- [Extension Modules](/RaCore/Modules/Extensions/)
- [LegendaryCMS](/LegendaryCMS/)
- [LegendaryGameEngine](/LegendaryGameEngine/)

---

**Last Updated:** January 13, 2025  
**Version:** 1.0  
**Maintainer:** RaOS Architecture Team
