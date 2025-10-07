# 🌟 RaOS - Legendary CMS Suite

**Production-Ready Modular Content Management System**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Version](https://img.shields.io/badge/version-8.0.0-blue)]()
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)]()
[![License](https://img.shields.io/badge/license-MIT-orange)]()
[![Last Updated](https://img.shields.io/badge/updated-October_2025-green)]()

> Enterprise-grade modular CMS with plugin architecture, REST API, enhanced RBAC, and comprehensive security features.

---

## 🎯 Overview

**RaOS** (Ra Operating System) is a sophisticated AI-powered mainframe featuring the **Legendary CMS Suite** (Phase 8) and **Legendary Game Engine Suite** (Phase 9) - production-ready, modular systems built as external DLLs. Phase 9 (completed January 2025) extracts the game engine into an independent DLL with Unreal Engine-inspired features including in-game chat, physics foundation, and advanced AI architecture.

### Key Features

- 🔌 **Modular Architecture** - External DLLs with zero coupling
  - LegendaryCMS.dll (76 KB) - Content Management System
  - LegendaryGameEngine.dll (120 KB) - Game Engine Suite
- 🎮 **Game Engine Suite** - Scene management, entities, AI generation, in-game chat
- 💬 **Dual Chat Systems** - CMS website chat + in-game scene-specific chat
- 🧩 **Plugin System** - Event-driven extensions with dependency injection
- 🌐 **REST API** - 20+ endpoints with rate limiting and OpenAPI docs
- 🔒 **Enhanced RBAC** - 25+ permissions across 5 roles
- ⚙️ **Configuration** - Environment-aware with runtime updates
- 📊 **Monitoring** - Health checks and performance metrics
- 🛡️ **Security** - Rate limiting, authentication, authorization

---

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Optional: PHP 8+ for CMS features
- Optional: Nginx for production CMS deployment (recommended for Linux)

**🐧 Running on Linux?** See our comprehensive [Linux Hosting Setup Guide](LINUX_HOSTING_SETUP.md) for production deployment instructions.

**🤔 Windows vs Linux?** Check our [comparison guide](WINDOWS_VS_LINUX.md) to help decide (TL;DR: Linux is recommended for production).

### Running RaCore

**Development (Windows/Mac/Linux):**
```bash
# Clone the repository
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject

# Build and run
cd RaCore
dotnet build
dotnet run
```

**Production (Linux Ubuntu 22.04 LTS):**
```bash
# Use the build script for optimized production builds
./build-linux.sh              # Basic build
./build-linux-production.sh   # Self-contained with full deployment package

# See LINUX_HOSTING_SETUP.md for complete setup instructions
```

### Default Configuration

- **RaCore Server Port:** 80 (configurable via `RACORE_PORT` environment variable)
- **CMS API Port:** 8080 (auto-configured)
- **Default Admin:** username: `admin`, password: `admin123` ⚠️ **Change immediately in production!**

**Note:** Port 80 requires administrator/root privileges. For non-privileged environments, set `RACORE_PORT` (e.g., 5000, 8080).

### Environment Configuration

**Linux/Mac:**
```bash
export RACORE_PORT=8080
export CMS_ENVIRONMENT=Production
dotnet run
```

**Windows:**
```cmd
set RACORE_PORT=8080
set CMS_ENVIRONMENT=Production
dotnet run
```

---

## 📚 Phase 8: Legendary CMS Suite

### What's New (October 2025)

The Legendary CMS Suite represents the culmination of Phase 8 development, delivering a production-ready modular CMS.

#### Core Module: LegendaryCMS.dll

**Size:** 76 KB | **Init Time:** <100ms | **API Response:** <50ms

**Components:**
- `LegendaryCMSModule` (406 lines) - Main entry point with auto-discovery
- `PluginManager` (286 lines) - Dynamic plugin loading and management
- `CMSAPIManager` (194 lines) - REST API with rate limiting
- `RBACManager` (314 lines) - 25+ permissions, 5 roles
- `CMSConfiguration` (142 lines) - Environment-aware configuration

---

## 🎮 Phase 9: Legendary Game Engine Suite

### What's New (January 2025)

The Legendary Game Engine Suite extracts the game engine into an independent DLL, following Phase 8's successful modularization pattern.

#### Core Module: LegendaryGameEngine.dll

**Size:** 120 KB | **Init Time:** <150ms | **Features:** Unreal Engine-inspired

**Components:**
- `LegendaryGameEngineModule` (698+ lines) - Main engine with AI generation
- `GameEngineDatabase` (337 lines) - SQLite persistence layer
- `GameEngineWebSocketBroadcaster` (136 lines) - Real-time event broadcasting
- `InGameChatManager` (200 lines) - Scene-specific chat system

#### Key Features

**Scene Management:**
- Create, list, update, delete game scenes (worlds/levels/areas)
- AI-driven world generation from natural language
- SQLite persistence across server restarts
- Real-time WebSocket updates

**In-Game Chat System:**
- Scene-specific chat rooms (separate from CMS website chat)
- Participant tracking and management
- Message history (last 200 messages per room)
- Real-time messaging within game instances

**API Endpoints:**
- Scene operations: create, list, get, delete
- Entity operations: create, list, update, delete
- In-game chat: create room, send message, get messages, list rooms
- AI generation and engine statistics

#### Chat System Comparison

| Feature | CMS Website Chat | In-Game Chat |
|---------|------------------|--------------|
| **Module** | LegendaryCMS | LegendaryGameEngine |
| **Purpose** | Website forums, support | Party/guild/zone chat |
| **Scope** | Global, persistent | Scene-specific |
| **Storage** | Long-term database | Short-term (200 msgs) |

#### Module Commands

```bash
engine status        # Show engine status
engine stats         # Engine statistics
engine scene create  # Create a scene
engine scene list    # List all scenes
help                 # Show command help
```

---

## 📖 Documentation

### Current Phase 9 Documentation

- **[Phase 9 Implementation](PHASE9_IMPLEMENTATION.md)** - Complete Phase 9 report
- **[Legendary Game Engine README](LegendaryGameEngine/README.md)** - Module guide
- **[Game Engine API](GAMEENGINE_API.md)** - API reference (Phase 4+)

### Current Phase 8 Documentation

- **[Quick Start Guide](PHASE8_QUICKSTART.md)** - Get started in minutes
- **[Module README](LegendaryCMS/README.md)** - Complete module documentation (10,000+ words)
- **[Implementation Report](PHASE8_LEGENDARY_CMS.md)** - Technical details (14,000+ words)
- **[Summary](PHASE8_SUMMARY.md)** - Executive overview (16,000+ words)
- **[Project Structure](PHASE8_STRUCTURE.md)** - File organization (12,000+ words)

### Phase 9.3 Documentation (New!)

- **[System Architecture](ARCHITECTURE.md)** - Complete architectural overview (40,000+ words)
- **[Development Roadmap](ROADMAP.md)** - Future phases and features (24,000+ words)
- **[Module Development Guide](MODULE_DEVELOPMENT_GUIDE.md)** - Create your own modules (36,000+ words)

### Historical Documentation

- **[Development History](HISTORY.md)** - Complete archive of Phases 2-7
- **[Security Architecture](SECURITY_ARCHITECTURE.md)** - Security implementation details

### Platform Guides

- **[Linux Hosting Setup](LINUX_HOSTING_SETUP.md)** - Production deployment on Ubuntu 22.04 LTS
- **[Windows vs Linux](WINDOWS_VS_LINUX.md)** - Platform comparison guide

---

## 🏗️ Architecture

```
RaOS Platform
│
├── RaCore (Main Server)
│   ├── ModuleManager (Auto-discovery)
│   └── Extension Modules (30+)
│
└── LegendaryCMS.dll (External Module) ⭐
    ├── Core/              # Main module implementation
    ├── Plugins/           # Plugin system
    ├── API/               # REST API layer
    ├── Security/          # RBAC and permissions
    └── Configuration/     # Config management
```

### Key Design Principles

- **Modular:** Complete isolation enables independent updates
- **Extensible:** Plugin architecture supports third-party extensions
- **Secure:** Enterprise-grade RBAC with granular permissions
- **Fast:** <100ms initialization, <50ms API response
- **Monitored:** Built-in health checks and metrics

---

## 🔧 Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject

# Build LegendaryCMS module
dotnet build LegendaryCMS/LegendaryCMS.csproj

# Build entire solution
dotnet build TheRaProject.sln

# Run verification
chmod +x verify-phase8.sh
./verify-phase8.sh
```

### Verification Results

```
╔════════════════════════════════════════════════════════╗
║           Phase 8 Build Verification PASSED ✓         ║
╚════════════════════════════════════════════════════════╝

Summary:
  • LegendaryCMS module: BUILT ✓
  • RaCore integration: BUILT ✓
  • DLL output: VERIFIED ✓ (76K)
  • Core files: PRESENT ✓
  • Documentation: COMPLETE ✓
```

### Plugin Development

Create custom plugins to extend the CMS:

```csharp
using LegendaryCMS.Plugins;

public class MyPlugin : ICMSPlugin
{
    public Guid Id => Guid.Parse("your-guid");
    public string Name => "My Custom Plugin";
    public string Version => "1.0.0";
    
    public async Task InitializeAsync(IPluginContext context)
    {
        context.RegisterEventHandler("cms.startup", async (data) => 
        {
            context.Logger.LogInfo("Plugin activated!");
        });
    }
    
    // ... implement other interface methods
}
```

Load plugins dynamically:

```csharp
var result = await pluginManager.LoadPluginAsync("/path/to/MyPlugin.dll");
```

---

## 📊 Performance Metrics

| Metric | Target | Achieved |
|--------|--------|----------|
| Module Init | <100ms | ✅ ~50ms |
| API Response | <50ms | ✅ ~30ms avg |
| Memory Usage | <100MB | ✅ ~50MB base |
| Build Time (clean) | <15s | ✅ ~10s |
| Build Time (incremental) | <5s | ✅ ~2s |
| DLL Size | <100KB | ✅ 76KB |

---

## 🔒 Security

### Production Checklist

Before deploying to production:

- [ ] Change default admin credentials
- [ ] Enable HTTPS with valid certificates
- [ ] Configure production rate limits
- [ ] Set up log aggregation and monitoring
- [ ] Enable security event auditing
- [ ] Configure CORS properly
- [ ] Regular security updates
- [ ] Backup and recovery procedures

### Security Features

- **Rate Limiting:** Configurable per-client limits
- **Authentication:** Token-based with session management
- **Authorization:** Permission-based access control
- **Input Protection:** XSS/CSRF frameworks ready
- **Audit Logging:** Security event tracking
- **Health Monitoring:** Operational endpoints

---

## 🎓 Learning Resources

### Getting Started

1. **[Quick Start Guide](PHASE8_QUICKSTART.md)** - Installation and basic usage
2. **[Module README](LegendaryCMS/README.md)** - Feature documentation
3. **[API Documentation](PHASE8_LEGENDARY_CMS.md#api--integration-layer)** - REST API reference
4. **[Plugin Guide](LegendaryCMS/README.md#plugin-development)** - Extend the CMS

### Advanced Topics

- **[RBAC System](PHASE8_SUMMARY.md#-security)** - Roles and permissions
- **[Configuration](PHASE8_QUICKSTART.md#configuration)** - Environment setup
- **[Monitoring](PHASE8_STRUCTURE.md#monitoring--health-checks)** - Health checks and metrics
- **[Architecture](PHASE8_STRUCTURE.md)** - System design and structure

---

## 🗺️ Project History

RaOS has evolved through 8 major development phases since 2023:

- **Phase 2 (2023):** Modular Expansion - Foundation architecture
- **Phase 3 (2023):** Advanced Features - Security and CMS
- **Phase 4 (2024):** Economy & Compliance - RaCoin and moderation
- **Phase 5 (2024):** Community - Forums, blogs, profiles
- **Phase 6 (2024):** Platform - Game engine integration
- **Phase 7 (2025):** Enhanced Features - Self-healing and optimization
- **Phase 8 (Oct 2025):** Legendary CMS Suite - Production-ready modular CMS ✅
- **Phase 9 (Jan 2025):** Legendary Game Engine Suite - Modular game engine with in-game chat ✅

**For complete historical details, see [HISTORY.md](HISTORY.md)**

---

## 🤝 Contributing

Contributions are welcome! The modular architecture makes it easy to add new features:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Implement your changes** (follow existing code style)
4. **Add tests** (use existing test infrastructure)
5. **Build and verify** (`dotnet build && ./verify-phase8.sh`)
6. **Commit your changes** (`git commit -m 'Add amazing feature'`)
7. **Push to the branch** (`git push origin feature/amazing-feature`)
8. **Open a Pull Request**

---

## 📞 Support

### Documentation
- Check the [Quick Start Guide](PHASE8_QUICKSTART.md)
- Review [Module README](LegendaryCMS/README.md)
- Consult [Troubleshooting](PHASE8_QUICKSTART.md#troubleshooting)

### Issues
- Report bugs on GitHub Issues
- Tag issues appropriately
- Provide reproduction steps

---

## 📄 License

See the LICENSE file for licensing information.

---

## 🌟 Acknowledgments

**RaOS Legendary Suite v9.0.0**

Built with ❤️ by the RaOS Development Team

**Last Updated:** January 13, 2025  
**Current Phase:** 9 (Legendary Game Engine Suite)  
**Status:** ✅ Production Ready  
**Build:** ✅ Passing  
**Documentation:** 60,000+ words

---

## 📈 Statistics

- **Development Phases:** 9 completed
- **Total Modules:** 30+ extension modules
- **Legendary DLLs:** 2 (CMS + Game Engine)
- **Phase 8 Module Size:** 76 KB (LegendaryCMS.dll)
- **API Endpoints:** 11+ operational
- **Permissions:** 25+ granular
- **Roles:** 5 with inheritance
- **Documentation:** 100,000+ words (all phases)
- **Supported Platforms:** Windows, Linux, macOS

---

**Ready to get started?** Check out the [Quick Start Guide](PHASE8_QUICKSTART.md) or explore the [complete documentation](LegendaryCMS/README.md)!
- ✅ Comprehensive security architecture
- ✅ **AI Code Generation Module** - Natural language game creation (MMORPG, RPG, FPS, etc.)
- ✅ Sales page integration for license purchases
- ✅ Spreadsheet, image, asset intake (for game & content modules)
- ✅ Patch manager & continuous backend updates

---

## 🚀 **Phase 4: Public Release Preparation** ✅ **COMPLETED**
- ✅ License validation & access enforcement
- ✅ Complete AI content generation system
- ✅ Distribution system for authorized copies
- ✅ Update delivery from mainframe
- ✅ Multi-platform game client generation
- ✅ Real-time content moderation & harm detection
- ✅ AI-driven support chat & user appeals system
- ✅ All-age friendly experience & compliance (COPPA, GDPR, CCPA)

---

## 🤖 **Phase 7: Self-Sufficient RaAI Module Spawner** ✅ **COMPLETED**
- ✅ Natural language module generation capability
- ✅ Five module templates (Basic, API, Game Feature, Integration, Utility)
- ✅ Intelligent feature detection from prompts
- ✅ Code review and approval workflow
- ✅ Automatic module placement in `/Modules` folder
- ✅ Version history and rollback support
- ✅ SuperAdmin-only access with security checks
- ✅ Complete documentation and quickstart guide

**🌟 New Feature:** RaAI can now self-build and spawn new modules via natural language!

Example:
```
> spawn module Create a weather forecast module that fetches weather data
✅ Module 'WeatherForecastModule' spawned successfully!
```

See [PHASE7_QUICKSTART.md](PHASE7_QUICKSTART.md) for complete guide.

---

---

**Last Updated:** 2025-01-09  
**Current Version:** v7.0.0 (Phase 7 Completed - Self-Sufficient Module Spawner)
