# RaOS Release Build

## 📦 Release Package Contents

This directory contains the compiled Release build of RaOS with all modules including the new **LegendaryUserLearningModule (LULmodule)**.

### Build Information
- **Build Configuration**: Release (Optimized)
- **Target Framework**: .NET 9.0
- **Build Date**: December 2025
- **Version**: 9.3.2+LULmodule

### Core Assemblies

#### Main Application
- **RaCore** - Main executable (entry point)
- **RaCore.dll** - Core engine and module system
- **RaCore.deps.json** - Dependency manifest
- **RaCore.runtimeconfig.json** - Runtime configuration

#### Module Libraries
- **Abstractions.dll** - Interfaces and base types (includes ILearningModule)
- **LegendaryCMS.dll** - Content Management System module
- **LegendaryGameEngine.dll** - Game Engine module
- **LegendaryClientBuilder.dll** - Client Builder module

### Dependencies
- Microsoft.AspNetCore.* - Web server and WebSocket support
- Microsoft.Data.Sqlite.dll - Database support
- Microsoft.Extensions.* - .NET extensions
- SQLitePCLRaw.* - SQLite native libraries
- System.IO.Pipelines.dll - High-performance I/O

### Native Libraries
- **runtimes/** - Platform-specific native binaries
  - win-x64/native/e_sqlite3.dll
  - win-arm64/native/e_sqlite3.dll
  - win-x86/native/e_sqlite3.dll
  - linux-x64/native/libe_sqlite3.so
  - osx-x64/native/libe_sqlite3.dylib

## 🚀 Running RaOS

### Windows
```cmd
RaCore.exe
```

### Linux/Mac
```bash
chmod +x RaCore
./RaCore
```

Or using dotnet:
```bash
dotnet RaCore.dll
```

## 🎓 New Feature: LegendaryUserLearningModule

This release includes the **LegendaryUserLearningModule (LULmodule)** with:

- **8 Courses** across 3 permission levels (User, Admin, SuperAdmin)
- **43 Lessons** with comprehensive RaOS training
- **Trophy System** with 5 tiers (Bronze → Diamond)
- **Achievement System** for tracking progress
- **Real-time Updates** capability for new features
- **AI Agent Integration** for code assistance

### Using LULmodule

After starting RaCore, use these commands:
```
Learn RaOS courses User        # View beginner courses
Learn RaOS courses Admin       # View advanced courses
Learn RaOS courses SuperAdmin  # View master courses
Learn RaOS help                # See all commands
```

## 📁 Directory Structure

```
Release/
├── RaCore                    # Main executable (Linux/Mac)
├── RaCore.exe               # Main executable (Windows, if built on Windows)
├── RaCore.dll               # Core library
├── RaCore.deps.json         # Dependencies
├── RaCore.runtimeconfig.json # Runtime config
├── Abstractions.dll         # Core abstractions
├── LegendaryCMS.dll         # CMS module
├── LegendaryGameEngine.dll  # Game engine
├── LegendaryClientBuilder.dll # Client builder
├── Microsoft.*.dll          # Microsoft libraries
├── SQLitePCLRaw.*.dll      # SQLite libraries
├── System.*.dll            # System libraries
├── runtimes/               # Platform-specific binaries
│   ├── linux-x64/
│   ├── osx-x64/
│   └── win-x64/
└── README.md               # This file
```

## ⚙️ Configuration

### Port Configuration
Default port: **5000**

To change the port, set the `RACORE_PORT` environment variable:
```bash
# Linux/Mac
export RACORE_PORT=8080

# Windows
set RACORE_PORT=8080
```

### URLs
- **Server**: http://localhost:5000
- **Control Panel**: http://localhost:5000/control-panel.html
- **WebSocket**: ws://localhost:5000/ws

### Nginx Integration
RaOS auto-configures Nginx as a reverse proxy. To customize the domain:
```bash
export RACORE_PROXY_DOMAIN=yourdomain.com
```

## 📊 Modules Included

### Core Modules
- Module Manager
- Memory Module
- Subconscious Module
- Speech Module

### Extension Modules
- **LegendaryUserLearningModule (NEW!)** - Self-paced learning system
- Authentication Module
- Blog Module
- Chat Module
- Forum Module
- AI Language Module
- AI Content Generation Module
- Code Generation Module
- Game Engine Module
- Game Server Module
- Game Client Module
- Site Builder Module
- Server Setup Module
- RaCoin Module
- License Module
- Market Monitor Module
- Module Spawner
- Support Chat Module
- Supermarket Module
- Update Module
- Feature Explorer Module
- Distribution Module
- Test Runner Module

### CMS Modules
- Legendary CMS Suite
- Plugin Manager
- Theme Manager
- API Manager
- RBAC Security Manager

## 🔧 System Requirements

### Minimum Requirements
- **OS**: Windows 10+, Ubuntu 20.04+, macOS 11+
- **.NET Runtime**: .NET 9.0
- **RAM**: 2 GB
- **Disk**: 500 MB free space

### Recommended Requirements
- **OS**: Windows 11, Ubuntu 22.04+, macOS 13+
- **.NET Runtime**: .NET 9.0
- **RAM**: 4 GB+
- **Disk**: 2 GB free space
- **Optional**: Nginx (auto-configured)

## 📝 Notes

### First Run
On first run, RaOS will:
1. Initialize the module system
2. Create necessary directories (Databases, php, Nginx, Admins)
3. Seed example data for all modules
4. Load all 8 LULmodule courses with 43 lessons
5. Configure web server on port 5000

### File Structure Created
RaOS will create the following directories on first run:
- `Databases/` - Database files
- `php/` - PHP configuration
- `Nginx/` - Nginx configuration
- `Admins/` - Admin instance data
- `wwwroot/` - Web assets
- `GameProjects/` - Game server projects

### Documentation
- See `LULMODULE_SUMMARY.md` in the repository for complete LULmodule details
- See `DOCS_ORGANIZATION.md` for documentation structure
- See `DOCUMENTATION_INDEX.md` for all available documentation

## 🐛 Troubleshooting

### Module Not Loading
Check console output for initialization messages. All modules should show:
```
[ModuleName] Module initialized
```

### Port Already in Use
Change the port using the `RACORE_PORT` environment variable or stop the service using the port.

### Permission Errors
Ensure the application has write permissions to create directories and configuration files.

### LULmodule Not Loading
Verify the console shows:
```
[Learn RaOS] Initializing Legendary User Learning Module (LULmodule)...
[Learn RaOS] Seeded 8 courses with 43 lessons
[Learn RaOS] Learning Module initialized with 8 courses
```

## 📞 Support

For issues or questions:
1. Complete the LULmodule "Getting Help" lesson (User course)
2. Check the documentation in the repository
3. Review boot sequence logs
4. Contact system administrator

## 📜 License

See LICENSE file in the repository root.

---

**RaOS v9.3.2+LULmodule**  
*Release Build - Optimized for Production*  
*Now with LegendaryUserLearningModule! 🎓*
