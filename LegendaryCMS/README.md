# Legendary CMS Suite v8.1.0

**Production-Ready Modular CMS System for ASHATOS**

## Overview

The Legendary CMS Suite is a fully modular, extensible, and production-ready Content Management System designed as a plug-and-play module for the ASHATOS platform. As of version 8.1.0, **all CMS functionality including Razor Pages and Components is self-contained** within the LegendaryCMS module, enabling true modularity and the ability to split off as a standalone application.

## What's New in v8.1.0

### 🎯 Complete Module Isolation
- ✅ **All Razor Pages moved**: `/cms/blogs`, `/cms/forums`, `/cms/learning`, `/cms/profiles` now live in LegendaryCMS
- ✅ **All Components included**: Razor Components like BlogPostCard and ForumPost are self-contained
- ✅ **Zero ASHATCore dependencies**: LegendaryCMS can run independently
- ✅ **Plug-and-play architecture**: Can be integrated or deployed standalone

### 📦 Self-Contained Structure
```
LegendaryCMS/
├── API/                    # REST API layer
├── Components/             # Razor Components (NEW in v8.1.0)
├── Configuration/          # CMS configuration
├── Core/                   # Module interfaces & extensions
├── Pages/                  # Razor Pages (NEW in v8.1.0)
│   ├── Blogs/
│   ├── Forums/
│   ├── Learning/
│   └── Profiles/
├── Plugins/                # Plugin system
└── Security/              # RBAC and security
```

## Key Features

### 🔌 **Modular Architecture**
- External DLL package loaded dynamically by ASHATCore
- Complete isolation from mainframe for independent updates
- Clean separation of concerns with component-based design
- Support for hot-reloading and version management

### 🧩 **Plugin System**
- Event-driven plugin architecture
- Dependency injection container
- Dynamic plugin loading/unloading
- Plugin sandboxing and security boundaries
- Comprehensive plugin metadata system

### 🌐 **REST API Layer**
- Full REST API for all CMS operations
- Rate limiting (60 req/min, 1000 req/hour)
- Authentication and authorization
- OpenAPI/Swagger documentation generation
- CORS and security headers

### 🔒 **Enhanced RBAC**
- Granular permission system
- Role-based and attribute-based access control
- Built-in roles: SuperAdmin, Admin, Moderator, User, Guest
- Permission inheritance and delegation
- Audit logging for all access attempts

### 🎨 **UI Components**
- **Razor Pages** for full-page CMS functionality
- **Razor Components** for reusable UI elements
- Modular, testable UI architecture
- Independent from ASHATCore UI

### ⚙️ **Configuration Management**
- Environment-aware configuration (Dev/Staging/Production)
- Centralized JSON/YAML configuration
- Runtime configuration updates
- Admin UI for configuration management

### 🎨 **Theming & Localization**
- Robust theming engine with template support
- Custom CSS and layout support
- Multi-language resource files
- Locale switching (en-US, es-ES, fr-FR, de-DE)
- RTL language support ready

### 📊 **Monitoring & Health Checks**
- Built-in health check endpoints
- Performance metrics collection
- Structured logging with correlation IDs
- Component status monitoring

### 🛡️ **Security Features**
- XSS, CSRF, and injection protection
- Rate limiting and DDoS mitigation
- Secure session management
- Security event auditing
- Input validation and sanitization

## Deployment Modes

### Mode 1: Integrated Module (Default)

LegendaryCMS runs as an integrated module within ASHATCore:

```csharp
// In ASHATCore Program.cs
builder.Services.AddRazorPages()
    .AddApplicationPart(typeof(LegendaryCMS.Core.LegendaryCMSModule).Assembly);

app.MapRazorPages();
```

**Benefits:**
- Single deployment
- Shared authentication and session management
- Direct in-process communication
- Shared resources and configuration

**Routes:**
- `/cms/blogs` - Blog system
- `/cms/forums` - Forum platform
- `/cms/learning` - Learning module
- `/cms/profiles` - User profiles

### Mode 2: Standalone Application

LegendaryCMS can be split off as a standalone web application. See [MODULAR_ARCHITECTURE.md](./MODULAR_ARCHITECTURE.md) for complete guide.

**Benefits:**
- Independent deployment and scaling
- Can be hosted on separate infrastructure
- Isolated failure domains
- Easier updates and maintenance

## Installation

### Building from Source

```bash
cd /path/to/ASHATOS
dotnet build LegendaryCMS/LegendaryCMS.csproj
```

### Integration with ASHATCore

The module is automatically loaded by ASHATCore on startup. The DLL is located at:
```
LegendaryCMS/bin/Release/net9.0/LegendaryCMS.dll
```

## Module Commands

```bash
# Show CMS status
cms status

# Display configuration
cms config

# List API endpoints
cms api

# Show loaded plugins
cms plugins

# Display RBAC information
cms rbac

# Generate OpenAPI spec
cms openapi
```

## CMS Pages

### Blogs (`/cms/blogs`)
- **Index**: Browse all blog posts
- **Post**: View individual blog post
- **Create**: Create/edit blog posts (authenticated)

### Forums (`/cms/forums`)
- **Index**: Browse forum categories
- **View**: View forum threads
- **Thread**: View individual thread and replies
- **CreateThread**: Create new forum thread (authenticated)
- **Reply**: Reply to forum thread (authenticated)
- **Moderate**: Forum moderation panel (moderator+)

### Learning (`/cms/learning`)
- **Index**: Browse courses and lessons
- **Progress**: Track learning progress
- **Achievements**: View earned achievements

### Profiles (`/cms/profiles`)
- **Index**: View user profiles (MySpace-style social profiles)

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
  "version": "8.1.0"
}
```

### List Forums (No Auth Required)

```bash
curl http://localhost:8080/api/forums
```

Response:
```json
{
  "forums": ["General", "Support", "Off-Topic"]
}
```

### Create Forum Post (Auth Required)

```bash
curl -X POST http://localhost:8080/api/forums/post \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "My Post", "content": "Hello World"}'
```

## Splitting Off the Project

See [MODULAR_ARCHITECTURE.md](./MODULAR_ARCHITECTURE.md) for a complete step-by-step guide on:
- Creating a standalone deployment
- Configuring mainframe API connectivity
- Setting up authentication flow
- Managing content synchronization

## Configuration

### Default Configuration

```json
{
  "LegendaryCMS": {
    "Mode": "Integrated",
    "Database": {
      "Type": "SQLite",
      "ConnectionString": "Data Source=cms.db",
      "AutoMigrate": true
    },
    "Site": {
      "Name": "Legendary CMS",
      "BaseUrl": "http://localhost:8080",
      "AdminEmail": "admin@legendarycms.local"
    },
    "Security": {
      "SessionTimeout": 3600,
      "EnableCSRF": true,
      "EnableXSSProtection": true,
      "MaxLoginAttempts": 5
    },
    "API": {
      "Enabled": true,
      "RateLimit": {
        "RequestsPerMinute": 60,
        "RequestsPerHour": 1000
      }
    },
    "Theme": {
      "Default": "classic",
      "AllowCustomThemes": true
    },
    "Features": {
      "Blogs": true,
      "Forums": true,
      "Learning": true,
      "Profiles": true
    }
  }
}
```

## Performance

- **API Response Time:** < 50ms average
- **Concurrent Requests:** Up to 100 simultaneous
- **Memory Usage:** ~50MB base + plugins
- **Rate Limits:** Configurable per endpoint

## Security Considerations

1. **Always change default credentials** in production
2. **Enable HTTPS** for all production deployments
3. **Configure rate limits** based on expected traffic
4. **Regular security audits** of plugins
5. **Keep dependencies updated**
6. **Monitor security events** for anomalies

## Roadmap

### Phase 8.1 Completed ✅
- [x] Modular DLL architecture
- [x] Plugin system with event hooks
- [x] REST API with rate limiting
- [x] Enhanced RBAC system
- [x] Configuration management
- [x] API documentation
- [x] **Complete UI module isolation**
- [x] **Razor Pages integration**
- [x] **Razor Components**
- [x] **Standalone deployment ready**

### Future Enhancements 🚀
- [ ] GraphQL API support
- [ ] Real-time WebSocket support for forums/chat
- [ ] Advanced theming UI
- [ ] Complete localization system
- [ ] Database migration tools
- [ ] Backup/restore utilities
- [ ] Comprehensive test suite
- [ ] PWA support
- [ ] WCAG 2.1 compliance audit

## Contributing

Contributions are welcome! Please follow the existing code style and add tests for new features.

## License

See the main repository LICENSE file for licensing information.

## Support

For issues and questions:
- Open an issue on GitHub
- Consult the API documentation
- Check the troubleshooting guide
- Review [MODULAR_ARCHITECTURE.md](./MODULAR_ARCHITECTURE.md) for deployment questions

---

**Legendary CMS Suite v8.1.0** - Built with ❤️ for ASHATOS  
**Now with Complete Module Isolation!**

