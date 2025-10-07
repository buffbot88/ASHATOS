# Phase 8: Legendary CMS Suite - Quick Start Guide

## What is Legendary CMS Suite?

Legendary CMS Suite is a **production-ready, modular Content Management System** built as an external DLL for RaOS. It represents Phase 8 of the RaOS development roadmap and provides enterprise-grade features including:

- 🔌 **Modular DLL Architecture** - Complete isolation from mainframe
- 🧩 **Plugin System** - Event-driven extensions with dependency injection
- 🌐 **REST API** - Full API layer with 11+ endpoints
- 🔒 **Enhanced RBAC** - 25+ granular permissions across 5 roles
- ⚙️ **Configuration Management** - Environment-aware settings
- 🛡️ **Security** - Rate limiting, authentication, authorization
- 📊 **Monitoring** - Health checks and metrics
- 📚 **Documentation** - 10,000+ words of comprehensive docs

## Installation

### Prerequisites

- .NET 9.0 SDK
- RaCore (TheRaProject repository)

### Building

```bash
# Clone the repository (if not already done)
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject

# Build LegendaryCMS module
dotnet build LegendaryCMS/LegendaryCMS.csproj

# Build RaCore with LegendaryCMS
dotnet build RaCore/RaCore.csproj
```

### Verification

Run the verification script to ensure everything is set up correctly:

```bash
chmod +x verify-phase8.sh
./verify-phase8.sh
```

Expected output:
```
╔════════════════════════════════════════════════════════╗
║           Phase 8 Build Verification PASSED ✓         ║
╚════════════════════════════════════════════════════════╝

Summary:
  • LegendaryCMS module: BUILT ✓
  • RaCore integration: BUILT ✓
  • DLL output: VERIFIED ✓
  • Core files: PRESENT ✓
  • Documentation: COMPLETE ✓
```

## Quick Start

### Starting RaCore with LegendaryCMS

```bash
cd RaCore
dotnet run
```

The LegendaryCMS module is automatically loaded by RaCore's ModuleManager.

### Basic Commands

Once RaCore is running, you can use these CMS commands:

```bash
# Show module status
cms status

# Display configuration
cms config

# List API endpoints
cms api

# Show loaded plugins
cms plugins

# Display RBAC information
cms rbac

# Generate OpenAPI specification
cms openapi

# Get help
help
```

### Example Session

```
> cms status

Legendary CMS Status:
  Initialized: True
  Running: True
  Version: 8.0.0
  Uptime: 2.15 minutes
  Start Time: 2024-01-15 10:30:00 UTC

> cms api

CMS API - 11 endpoints registered:

GET:
  🌐 /api/health - Health check endpoint
  🌐 /api/version - Get CMS version information
  🌐 /api/endpoints - List all available API endpoints
  🌐 /api/forums - Get all forums
  🔒 /api/forums/post - Create a new forum post
  ...

> cms rbac

RBAC System Information:

Default Roles:
  • SuperAdmin - Full system access (all permissions)
  • Admin - Administrative access (most permissions)
  • Moderator - Content moderation
  • User - Standard user
  • Guest - Read-only access
```

## API Usage

### Health Check

```bash
curl http://localhost:8080/api/health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "8.0.0"
}
```

### List Forums

```bash
curl http://localhost:8080/api/forums
```

Response:
```json
{
  "forums": ["General", "Support", "Off-Topic"]
}
```

### Create Forum Post (Requires Authentication)

```bash
curl -X POST http://localhost:8080/api/forums/post \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "My First Post", "content": "Hello, World!"}'
```

### Get OpenAPI Specification

```bash
# From RaOS CLI
cms openapi

# Or via API
curl http://localhost:8080/api/openapi
```

## Plugin Development

### Creating a Custom Plugin

```csharp
using LegendaryCMS.Plugins;

public class MyPlugin : ICMSPlugin
{
    public Guid Id => Guid.Parse("your-guid-here");
    public string Name => "My Custom Plugin";
    public string Version => "1.0.0";
    public string Author => "Your Name";
    public string Description => "My awesome plugin";
    public List<string> Dependencies => new();
    public List<string> RequiredPermissions => new() { "custom.permission" };

    public async Task InitializeAsync(IPluginContext context)
    {
        context.Logger.LogInfo("Plugin initializing...");
        
        // Register event handlers
        context.RegisterEventHandler("cms.startup", async (data) => 
        {
            context.Logger.LogInfo("CMS started!");
        });
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

### Loading a Plugin

```csharp
// In your code
var pluginManager = serviceProvider.GetService<PluginManager>();
var result = await pluginManager.LoadPluginAsync("/path/to/MyPlugin.dll");

if (result.Success)
{
    Console.WriteLine($"Plugin loaded: {result.Metadata.Name}");
}
```

## Configuration

### Default Configuration

The CMS uses sensible defaults that work out-of-the-box:

```json
{
  "Database": {
    "Type": "SQLite",
    "ConnectionString": "Data Source=cms.db"
  },
  "Site": {
    "Name": "Legendary CMS",
    "BaseUrl": "http://localhost:8080"
  },
  "Security": {
    "SessionTimeout": 3600,
    "EnableCSRF": true,
    "EnableXSSProtection": true
  },
  "API": {
    "RateLimit": {
      "RequestsPerMinute": 60,
      "RequestsPerHour": 1000
    }
  }
}
```

### Changing Configuration

```csharp
var config = module.GetConfiguration();

// Change a value
config.SetValue("Site:Name", "My Custom CMS");

// Get a value
var siteName = config.GetValue<string>("Site:Name");

// Save configuration (to be implemented)
await config.SaveAsync();
```

### Environment Variables

Set the CMS environment:

```bash
export CMS_ENVIRONMENT=Production
dotnet run
```

Supported environments:
- `Development` (default)
- `Staging`
- `Production`

## Permissions & Roles

### Default Roles

| Role | Description | Example Use Case |
|------|-------------|------------------|
| **SuperAdmin** | Full access | System administrators |
| **Admin** | Most permissions | Site administrators |
| **Moderator** | Content moderation | Community moderators |
| **User** | Standard access | Regular users |
| **Guest** | Read-only | Anonymous visitors |

### Key Permissions

**Forum:**
- `forum.view` - View forums
- `forum.post` - Create posts
- `forum.moderate` - Moderate content

**Blog:**
- `blog.view` - View blogs
- `blog.create` - Create posts
- `blog.publish` - Publish posts

**Admin:**
- `admin.access` - Access admin panel
- `admin.users` - Manage users
- `admin.settings` - Manage settings

### Checking Permissions

```csharp
var rbacManager = serviceProvider.GetService<IRBACManager>();

// Check if user has permission
bool canPost = rbacManager.HasPermission(userId, CMSPermissions.ForumPost);

// Assign role
await rbacManager.AssignRoleAsync(userId, CMSRoles.Moderator);

// Get user permissions
var permissions = await rbacManager.GetUserPermissionsAsync(userId);
```

## Architecture Overview

```
LegendaryCMS.dll
├── Core/
│   ├── LegendaryCMSModule      # Main module entry point
│   └── ICMSComponent           # Component interface
├── Plugins/
│   └── PluginManager           # Plugin loader
├── API/
│   └── CMSAPIManager           # REST API handler
├── Security/
│   └── RBACManager             # Permission management
└── Configuration/
    └── CMSConfiguration        # Config system
```

## Monitoring & Health Checks

### Check Module Health

```bash
# From CLI
cms status

# Via API
curl http://localhost:8080/api/health
```

### Component Status

The status check shows:
- Module initialization state
- Uptime
- Component health (Configuration, API, Plugins, RBAC)
- Overall system health

## Common Tasks

### Task 1: Add a New API Endpoint

```csharp
apiManager.RegisterEndpoint(new CMSAPIEndpoint
{
    Path = "/api/custom",
    Method = "GET",
    Description = "My custom endpoint",
    RequiresAuthentication = true,
    RequiredPermissions = new() { "custom.access" },
    Handler = async (request) =>
    {
        return CMSAPIResponse.Success(new { message = "Hello!" });
    }
});
```

### Task 2: Create a Custom Role

```csharp
// Create custom role
var rbac = new RBACManager();

// Assign permissions to a user
await rbac.AssignPermissionAsync(userId, "custom.permission");

// Or assign a role
await rbac.AssignRoleAsync(userId, CMSRoles.Moderator);
```

### Task 3: Listen for Events

```csharp
pluginContext.RegisterEventHandler("cms.user.created", async (data) =>
{
    var userData = data as UserData;
    pluginContext.Logger.LogInfo($"New user: {userData.Username}");
});
```

## Troubleshooting

### Module Not Loading

**Problem:** LegendaryCMS module doesn't appear in loaded modules.

**Solution:**
1. Ensure the DLL is built: `dotnet build LegendaryCMS/LegendaryCMS.csproj`
2. Check RaCore has reference: `dotnet list RaCore/RaCore.csproj reference`
3. Verify the `[RaModule(Category = "cms")]` attribute is present

### API Endpoints Not Working

**Problem:** API requests return 404.

**Solution:**
1. Check module is initialized: `cms status`
2. List endpoints: `cms api`
3. Verify the API manager is initialized in module logs

### Permission Denied

**Problem:** User can't access a feature.

**Solution:**
1. Check user permissions: `cms rbac`
2. Assign necessary role: `rbacManager.AssignRoleAsync(userId, role)`
3. Or grant specific permission: `rbacManager.AssignPermissionAsync(userId, permission)`

## Next Steps

### For Developers

1. **Explore the Source Code**
   - `LegendaryCMS/Core/LegendaryCMSModule.cs` - Main module
   - `LegendaryCMS/Plugins/PluginManager.cs` - Plugin system
   - `LegendaryCMS/API/CMSAPIManager.cs` - API layer

2. **Read the Full Documentation**
   - `LegendaryCMS/README.md` - Complete feature documentation
   - `PHASE8_LEGENDARY_CMS.md` - Implementation report

3. **Create Your First Plugin**
   - Use the plugin template above
   - Implement your custom logic
   - Load it with PluginManager

### For System Administrators

1. **Configure for Production**
   - Set `CMS_ENVIRONMENT=Production`
   - Change default credentials
   - Enable HTTPS
   - Configure rate limits

2. **Monitor the System**
   - Use `cms status` regularly
   - Check API health endpoint
   - Review security events

3. **Backup and Maintenance**
   - Implement backup strategy
   - Plan upgrade path
   - Document custom configurations

## Resources

- **Full Documentation:** `LegendaryCMS/README.md`
- **Implementation Report:** `PHASE8_LEGENDARY_CMS.md`
- **Verification Script:** `verify-phase8.sh`
- **Source Code:** `LegendaryCMS/` directory

## Support

For issues, questions, or contributions:
- Open an issue on GitHub
- Consult the documentation
- Review the example code

---

**Legendary CMS Suite v8.0.0** - Production-Ready Modular CMS for RaOS

Last Updated: 2024-01-15
