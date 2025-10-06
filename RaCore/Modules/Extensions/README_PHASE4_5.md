# Phase 4.5 Modules - Distribution, Updates & Game Client

This directory contains the Phase 4.5 modules that implement the distribution and update infrastructure for RaCore.

---

## Overview

Phase 4.5 adds three critical modules for RaCore's public release:

1. **Distribution Module** - Package and distribute authorized RaCore copies
2. **Update Module** - Deliver updates from mainframe with version checking
3. **GameClient Module** - Generate multi-platform game clients

---

## Modules

### Distribution Module
**Location:** `RaCore/Modules/Extensions/Distribution/`

Manages authorized copy distribution with license validation.

**Features:**
- ZIP package creation with metadata
- License-based download authorization
- SHA256 checksum verification
- Package expiration tracking (1 year)
- Download statistics and revocation

**Console Commands:**
```bash
distribution stats
distribution create <license-key> <version>
distribution authorize <license-key>
```

**API Endpoints:**
- `POST /api/distribution/create` - Create package (Admin)
- `GET /api/distribution/download/{licenseKey}` - Download package
- `GET /api/distribution/packages` - List packages (Admin)

### Update Module
**Location:** `RaCore/Modules/Extensions/Updates/`

Delivers updates from mainframe with intelligent version management.

**Features:**
- Semantic version comparison
- License-validated update access
- Changelog management
- Mandatory update support
- Version deprecation

**Console Commands:**
```bash
update stats
update check <current-version> <license-key>
update list
```

**API Endpoints:**
- `GET /api/updates/check` - Check for updates
- `GET /api/updates/download/{version}` - Download update
- `GET /api/updates/list` - List updates (Admin)

### GameClient Module
**Location:** `RaCore/Modules/Extensions/GameClient/`

Generates multi-platform game clients for RaCore servers.

**Features:**
- HTML5/WebGL client generation
- Desktop launcher scripts (Windows/Linux/macOS)
- WebSocket server connection
- Canvas-based rendering
- License-based authentication
- Configurable themes and settings

**Console Commands:**
```bash
gameclient stats
gameclient list <user-id>
```

**API Endpoints:**
- `POST /api/gameclient/generate` - Generate client (User+)
- `GET /api/gameclient/list` - List user's clients
- `GET /clients/{packageId}/{*file}` - Serve client files

**Supported Platforms:**
- WebGL (HTML5) - Browser-based
- Windows - Desktop with .bat launcher
- Linux - Desktop with .sh launcher
- macOS - Desktop with .sh launcher
- Android (future)
- iOS (future)

---

## Integration

All three modules integrate with:
- **License Module** - Validates active licenses before operations
- **Authentication Module** - Enforces role-based access control
- **WebSocket Handler** - Game clients connect via WebSocket
- **GameEngine Module** - Clients interact with game scenes

---

## Quick Start

### 1. Create Distribution Package (Admin)
```bash
curl -X POST http://localhost:5000/api/distribution/create \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -d '{"licenseKey": "ABC-123", "version": "4.5.0"}'
```

### 2. Download Package (Licensed User)
```bash
wget http://localhost:5000/api/distribution/download/ABC-123
```

### 3. Check for Updates
```bash
curl "http://localhost:5000/api/updates/check?version=4.3.0&license=ABC-123"
```

### 4. Generate Game Client
```bash
curl -X POST http://localhost:5000/api/gameclient/generate \
  -H "Authorization: Bearer USER_TOKEN" \
  -d '{
    "licenseKey": "ABC-123",
    "platform": "WebGL",
    "configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "My Game",
      "theme": "fantasy"
    }
  }'
```

### 5. Access Generated Client
```
http://localhost:5000/clients/{package-id}/index.html
```

---

## File Structure

```
RaCore/
├── Distribution/
│   └── Packages/
│       └── RaCore_{license}_{version}_{guid}.zip
├── Updates/
│   └── Packages/
│       └── RaCore_Update_{version}_{guid}.zip
└── GameClients/
    └── {package-guid}/
        ├── index.html          # Client interface
        ├── game.js             # Game logic
        ├── README.md           # Instructions
        └── launch.bat/.sh      # Desktop launcher
```

---

## Security

- **License Validation** - All operations require valid, active licenses
- **Expiration Checking** - Packages expire after 1 year
- **Checksum Verification** - SHA256 checksums for all packages
- **Admin-Only Operations** - Package/update creation requires Admin role
- **User Isolation** - Each user's clients are isolated

---

## Documentation

For detailed information, see:
- `PHASE4_5_QUICKSTART.md` - Complete usage guide with examples
- `PHASE4_5_SUMMARY.md` - Implementation details and architecture
- `PHASES.md` - Updated roadmap showing Phase 4.5 completion

---

## Testing

Build and run:
```bash
cd RaCore
dotnet build
dotnet run
```

All modules will initialize and register their endpoints:
```
[Module:Distribution] INFO: Distribution module initialized
[Module:Update] INFO: Update module initialized
[Module:GameClient] INFO: GameClient module initialized
[RaCore] Distribution API endpoints registered
[RaCore] Update API endpoints registered
[RaCore] GameClient API endpoints registered
```

---

## Version

**Phase:** 4.5  
**Status:** Complete  
**Date:** 2025-01-08  
**Modules:** 3 (Distribution, Update, GameClient)  
**API Endpoints:** 9 new endpoints  
**Lines of Code:** 2,502 lines added

---

**RaCore AI Mainframe - Phase 4.5**  
Distribution & Update Infrastructure
