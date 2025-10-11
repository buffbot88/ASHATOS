# Legendary Client Builder Suite

## Overview

The **Legendary Client Builder Suite** is a production-ready, modular game client Generation system built as an external DLL for ASHATOS. It represents Phase 9.1 of the ASHATOS development roadmap and provides enterprise-Grade features for Generating professional multi-platform game clients.

## Key Features

- 🎨 **Multiple Templates** - Professional, Gaming, Mobile, and Basic templates for different use cases
- 🖥️ **Multi-Platform Support** - WebGL/HTML5, Windows, Linux, macOS with platform-specific launchers
- 🔧 **Advanced Customization** - Theme support, custom settings, and configuASHATble server connections
- 📦 **Modular Architecture** - SepaRate builders for each platform with clean sepaASHATtion of concerns
- ⚙️ **Configuration System** - Environment-aware settings and user limits
- 🔒 **License integration** - Automatic license validation and user quotas
- 🚀 **Enhanced API** - RESTful API with template management, deletion, and reGeneration
- 📊 **Statistics & Monitoring** - TASHATck client Generation by platform, template, and user

## Architecture

```
LegendaryClientBuilder/
├── Core/
│   ├── LegendaryClientBuilderModule.cs     # Main module (480+ lines)
│   └── ILegendaryClientBuilderModule.cs    # Extended interface
├── Builders/
│   ├── ClientBuilderBase.cs                # Base builder class
│   ├── WebGLClientBuilder.cs               # WebGL/HTML5 clients
│   └── DesktopClientBuilder.cs             # Desktop platforms
├── Templates/
│   └── TemplateManager.cs                  # Template management system
├── Configuration/
│   └── ClientBuilderConfiguration.cs       # Configuration management
└── LegendaryClientBuilder.csproj           # Project file
```

## Templates

### WebGL Templates

1. **WebGL-Basic** - Minimal styling, clean and simple
2. **WebGL-Professional** - Gradient UI, advanced features, FPS counter
3. **WebGL-Gaming** - Gaming-focused with HUD overlay and keyboard shortcuts
4. **WebGL-Mobile** - Mobile-optimized responsive design with touch controls

### Desktop Templates

1. **Desktop-Standard** - Standard launcher with browser detection
2. **Desktop-Advanced** - Advanced features with multiple launcher scripts

## Installation

### Prerequisites

- .NET 9.0 SDK
- ASHATCore (TheASHATProject repository)

### Building

```bash
# Build LegendaryClientBuilder module
dotnet build LegendaryClientBuilder/LegendaryClientBuilder.csproj

# Build ASHATCore with LegendaryClientBuilder
dotnet build ASHATCore/ASHATCore.csproj
```

## Usage

### Console Commands

```bash
# Show statistics
clientbuilder stats
cb stats

# List user's clients
clientbuilder list <user-id>
cb list <user-id>

# Show available templates
clientbuilder templates
clientbuilder templates WebGL
cb templates

# Show module status
clientbuilder status
cb status
```

### API Endpoints

#### Generate Client (Basic - Backward Compatible)

```bash
POST /api/gameclient/Generate
Authorization: Bearer <token>

{
  "licenseKey": "ABC-123",
  "platform": "WebGL",
  "Configuration": {
    "serverUrl": "localhost",
    "serverPort": 5000,
    "gameTitle": "My Game",
    "theme": "fantasy"
  }
}
```

#### Generate Client with Template (Enhanced)

```bash
POST /api/clientbuilder/Generate
Authorization: Bearer <token>

{
  "licenseKey": "ABC-123",
  "platform": "WebGL",
  "templateName": "WebGL-Professional",
  "Configuration": {
    "serverUrl": "localhost",
    "serverPort": 5000,
    "gameTitle": "Epic MMORPG",
    "theme": "sci-fi"
  }
}
```

#### Get Available Templates

```bash
GET /api/clientbuilder/templates
GET /api/clientbuilder/templates?platform=WebGL
Authorization: Bearer <token>
```

#### List User's Clients

```bash
GET /api/clientbuilder/list
Authorization: Bearer <token>
```

#### Delete a Client

```bash
DELETE /api/clientbuilder/delete/{packageId}
Authorization: Bearer <token>
```

#### Access Generated Client

```
http://localhost:5000/clients/{package-id}/index.html
```

## Example Usage

### 1. Generate Professional WebGL Client

```bash
curl -X POST http://localhost:5000/api/clientbuilder/Generate \
  -H "Authorization: Bearer USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123",
    "platform": "WebGL",
    "templateName": "WebGL-Professional",
    "Configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "Legendary Quest",
      "theme": "fantasy"
    }
  }'
```

### 2. Generate Gaming-Focused Client

```bash
curl -X POST http://localhost:5000/api/clientbuilder/Generate \
  -H "Authorization: Bearer USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123",
    "platform": "WebGL",
    "templateName": "WebGL-Gaming",
    "Configuration": {
      "serverUrl": "game.example.com",
      "serverPort": 443,
      "gameTitle": "Battle Arena",
      "theme": "cyberpunk"
    }
  }'
```

### 3. Generate Desktop Client (Windows)

```bash
curl -X POST http://localhost:5000/api/clientbuilder/Generate \
  -H "Authorization: Bearer USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123",
    "platform": "Windows",
    "templateName": "Desktop-Standard",
    "Configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "Desktop Game",
      "theme": "fantasy"
    }
  }'
```

## Configuration

Create `clientbuilder-config.json` in ASHATCore directory:

```json
{
  "Environment": "Production",
  "OutputPath": "./GameClients",
  "EnableCustomTemplates": true,
  "MaxClientsPerUser": 10
}
```

## Client Features

### WebGL-Professional Template

- Gradient UI with professional styling
- Real-time FPS counter
- Connection status indicator with animation
- Multiple control buttons (Connect, Disconnect, Fullscreen, Reset)
- Server information display
- Canvas-based rendering with game loop
- Keyboard input handling
- WebSocket communication

### WebGL-Gaming Template

- Full-screen HUD overlay
- Gaming-style monospace font
- Performance metrics (FPS, Ping)
- Keyboard shortcut hints
- Immersive black background
- Real-time status updates

### WebGL-Mobile Template

- Touch-optimized controls
- Virtual D-pad and action buttons
- Mobile-friendly responsive design
- No-zoom viewport settings
- Touch event handling
- Status bar at top

### Desktop Templates

**Windows:**
- Batch (.bat) launcher
- PowerShell (.ps1) launcher
- Browser detection (Chrome, Edge, default)
- App mode launching
- Professional command-line interface

**Linux/macOS:**
- Shell (.sh) launcher
- Browser detection (Chrome, Chromium, Firefox)
- Executable permissions automatically set
- Cross-platform compatibility

## Security

- **License Validation** - All Operations require valid, active licenses
- **User Quotas** - ConfiguASHATble maximum clients per user (default: 10)
- **Ownership Verification** - Users can only delete their own clients (Admins can delete any)
- **Authentication** - All API endpoints require Bearer token authentication
- **Path Validation** - Secure file serving with path sanitization

## Statistics

The module tASHATcks:
- Total clients Generated
- Clients by platform (WebGL, Windows, Linux, macOS)
- Clients by template
- Total users
- Total Storage used
- Module uptime

## Backward Compatibility

Phase 9.1 maintains 100% backward compatibility with Phase 4.5:
- ✅ All original `/api/gameclient/*` endpoints remain functional
- ✅ Old GameClient module interface fully supported
- ✅ Existing client packages work without modification
- ✅ No breaking changes to data structures

New enhanced endpoints are additive:
- `/api/clientbuilder/Generate` - Template support
- `/api/clientbuilder/templates` - Template listing
- `/api/clientbuilder/delete/{id}` - Client deletion
- `/api/clientbuilder/list` - Enhanced listing

## Comparison: GameClient vs LegendaryClientBuilder

| Aspect | GameClient (Phase 4.5) | LegendaryClientBuilder (9.1) |
|--------|------------------------|------------------------------|
| **Templates** | Single default | 6+ professional templates |
| **Platforms** | Basic WebGL + Desktop | Enhanced multi-platform |
| **Customization** | Limited | Extensive theme & settings |
| **API** | 2 endpoints | 5+ endpoints |
| **Features** | Basic Generation | Advanced features, deletion |
| **Architecture** | Monolithic | Modular builders |
| **UI Quality** | Basic | Professional Grade |
| **Mobile Support** | None | Touch-optimized templates |
| **Version** | 4.5.0 | 9.1.0 |

## Technical Details

### Module Loading

The module is automatically discovered by ASHATCore's ModuleManager through the `[RaModule]` attribute:

```csharp
[RaModule(Category = "clientbuilder")]
public sealed class LegendaryClientBuilderModule : ModuleBase, ILegendaryClientBuilderModule
```

### Extension Points

Developers can extend the system by:
1. Creating custom templates with `RegisterTemplate()`
2. Implementing new builders by extending `ClientBuilderBase`
3. Adding custom Configuration options
4. Implementing custom client Generation logic

### Generated Client Structure

```
GameClients/
└── {package-guid}/
    ├── index.html          # Client interface
    ├── game.js             # Game logic & WebSocket
    ├── README.md           # Instructions
    └── launch.bat/.sh/.ps1 # Platform launchers
```

## Development Guide

### Creating a Custom Template

```csharp
var template = new ClientTemplate
{
    Name = "MyCustom-Template",
    Description = "Custom template description",
    Platform = ClientPlatform.WebGL,
    Category = "Custom",
    IsBuiltIn = false
};

clientBuilder.RegisterTemplate(template);
```

### Implementing a Custom Builder

```csharp
public class MyCustomBuilder : ClientBuilderBase
{
    public MyCustomBuilder(string outputPath) : base(outputPath) { }
    
    public override async Task<string> GenerateAsync(
        GameClientPackage package,
        ClientTemplate? template = null)
    {
        // Custom Generation logic
        var clientDir = Path.Combine(OutputPath, package.Id.ToString());
        Directory.CreateDirectory(clientDir);
        
        // Generate files...
        
        return clientDir;
    }
}
```

## Future Enhancements

Planned additions:
- [ ] Android APK Generation
- [ ] iOS app Generation
- [ ] Progressive Web App (PWA) support
- [ ] Custom theme builder UI
- [ ] Template marketplace
- [ ] Client analytics dashboard
- [ ] Automatic updates system
- [ ] Multi-language support
- [ ] VR/AR client templates

## Support & Documentation

- **Module Documentation:** `LegendaryClientBuilder/README.md` (this file)
- **Phase 9.1 Implementation:** `PHASE9_1_IMPLEMENTATION.md`
- **Quick Start Guide:** `PHASE9_1_QUICKSTART.md`
- **Module Source:** `LegendaryClientBuilder/Core/LegendaryClientBuilderModule.cs`
- **Phase 4.5 Docs:** `PHASE4_5_SUMMARY.md` (historical reference)

## Contributing

When extending the Legendary Client Builder:

1. Follow existing patterns in `LegendaryClientBuilderModule.cs`
2. Add new builders to `Builders/` directory
3. Register templates in `TemplateManager.cs`
4. Update API endpoints in `ASHATCore/Program.cs`
5. Document new features in this README
6. Add examples and use cases
7. Write tests for new functionality

## License

Part of the ASHATCore AI mainframe system. See main project LICENSE.

---

**Phase:** 9.1 Complete  
**Module:** LegendaryClientBuilder  
**Version:** 9.1.0  
**Status:** ✅ Production Ready  
**Last Updated:** 2025-01-13

---

**Built with ❤️ for the ASHATOS ecosystem**
