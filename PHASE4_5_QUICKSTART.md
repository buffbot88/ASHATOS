# Phase 4.5 Quick Start Guide

**Phase 4.5: Distribution System & Update Delivery**

This guide demonstrates the new Distribution, Update, and GameClient modules added in Phase 4.5.

---

## 🎯 Overview

Phase 4.5 completes the distribution infrastructure for RaCore:

- **Distribution Module**: Package and distribute authorized copies to licensed users
- **Update Module**: Deliver updates from the mainframe with version checking
- **GameClient Module**: Generate multi-platform game clients for each server

---

## 🚀 Prerequisites

1. RaCore server running on port 5000
2. Valid user account with authentication token
3. Active license key for distribution/updates

---

## 📦 Distribution System

### Create Distribution Package (Admin)

```bash
# Via Console Command
distribution create <license-key> <version>

# Example
distribution create ABC-123-XYZ 4.5.0
```

```bash
# Via API
curl -X POST http://localhost:5000/api/distribution/create \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123-XYZ",
    "version": "4.5.0"
  }'
```

**Response:**
```json
{
  "success": true,
  "package": {
    "id": "guid",
    "userId": "guid",
    "licenseKey": "ABC-123-XYZ",
    "version": "4.5.0",
    "packagePath": "/path/to/package.zip",
    "sizeBytes": 12345678,
    "checksumSHA256": "abc123...",
    "createdAt": "2025-01-08T12:00:00Z",
    "expiresAt": "2026-01-08T12:00:00Z",
    "status": "Active"
  }
}
```

### Download Distribution Package (Licensed User)

```bash
# Via Browser/wget
wget http://localhost:5000/api/distribution/download/ABC-123-XYZ

# Via curl
curl -O http://localhost:5000/api/distribution/download/ABC-123-XYZ
```

The package will be downloaded as a ZIP file containing:
- `package.json` - Metadata
- `README.md` - Installation instructions
- `LICENSE.txt` - License agreement
- Additional RaCore files (in production)

### Check Authorization

```bash
# Console Command
distribution authorize ABC-123-XYZ

# Returns: True/False
```

### View Distribution Statistics

```bash
# Console Command
distribution stats
```

**Response:**
```json
{
  "totalPackages": 10,
  "activePackages": 8,
  "expiredPackages": 1,
  "revokedPackages": 1,
  "totalDownloads": 25,
  "totalSizeBytes": 123456789
}
```

---

## 🔄 Update System

### Check for Updates (Licensed User)

```bash
# Via API
curl "http://localhost:5000/api/updates/check?version=4.3.0&license=ABC-123-XYZ"
```

**Response:**
```json
{
  "success": true,
  "update": {
    "currentVersion": "4.3.0",
    "latestVersion": "4.5.0",
    "updateAvailable": true,
    "changelog": "Phase 4.5: Added Distribution and Update modules",
    "downloadUrl": "/api/updates/download/4.5.0",
    "sizeBytes": 10485760,
    "releasedAt": "2025-01-08T12:00:00Z"
  }
}
```

### Download Update Package

```bash
# Via Browser/wget
wget "http://localhost:5000/api/updates/download/4.5.0?license=ABC-123-XYZ"

# Via curl
curl -O "http://localhost:5000/api/updates/download/4.5.0?license=ABC-123-XYZ"
```

### List All Updates (Admin)

```bash
# Via Console Command
update list

# Via API
curl http://localhost:5000/api/updates/list \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### View Update Statistics

```bash
# Console Command
update stats
```

**Response:**
```json
{
  "currentVersion": "4.5.0",
  "totalUpdates": 5,
  "activeUpdates": 3,
  "deprecatedUpdates": 2,
  "latestVersion": "4.5.0"
}
```

---

## 🎮 Game Client Generation

### Generate WebGL Client (Default)

```bash
# Via API
curl -X POST http://localhost:5000/api/gameclient/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123-XYZ",
    "platform": "WebGL",
    "configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "My RaCore Game",
      "theme": "fantasy"
    }
  }'
```

**Response:**
```json
{
  "success": true,
  "client": {
    "id": "guid",
    "userId": "guid",
    "licenseKey": "ABC-123-XYZ",
    "platform": "WebGL",
    "packagePath": "/path/to/client",
    "clientUrl": "/clients/{guid}/index.html",
    "sizeBytes": 54321,
    "createdAt": "2025-01-08T12:00:00Z"
  }
}
```

### Access Generated Client

Once generated, access your game client at:
```
http://localhost:5000/clients/{packageId}/index.html
```

The client will:
1. Connect to the RaCore WebSocket server
2. Authenticate with your license key
3. Render a game canvas with real-time updates
4. Handle keyboard/mouse input
5. Display connection status

### List Your Game Clients

```bash
# Via Console Command
gameclient list <user-id>

# Via API
curl http://localhost:5000/api/gameclient/list \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Generate Desktop Client (Windows/Linux/macOS)

```bash
curl -X POST http://localhost:5000/api/gameclient/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123-XYZ",
    "platform": "Windows",
    "configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "My RaCore Game",
      "theme": "sci-fi"
    }
  }'
```

Desktop clients include:
- WebGL client files (index.html, game.js)
- Platform-specific launcher script
- README with instructions

### Client Platforms Supported

- **WebGL** (HTML5) - Works in all modern browsers
- **Windows** - Generates .bat launcher
- **Linux** - Generates .sh launcher
- **macOS** - Generates .sh launcher
- **Android** (placeholder - future support)
- **iOS** (placeholder - future support)

### View Client Statistics

```bash
# Console Command
gameclient stats
```

**Response:**
```json
{
  "totalClients": 15,
  "webGLClients": 10,
  "windowsClients": 3,
  "linuxClients": 2,
  "macOSClients": 0,
  "totalUsers": 8
}
```

---

## 🔒 Security & Authorization

### License Validation

All Phase 4.5 features require valid, active licenses:

- **Distribution downloads**: License must be Active and not expired
- **Update checks**: License validated before showing update info
- **Client generation**: License required for all platforms

### Access Control

- **Distribution creation**: Admin role required
- **Package listing**: Admin role required
- **Update creation**: Admin role required (manual process)
- **Client generation**: Any authenticated user with valid license
- **Client access**: Public once generated (served as static files)

### Expiration

- Distribution packages expire after 1 year
- Expired packages cannot be downloaded
- Updates check license expiration before delivery

---

## 📂 File Structure

Phase 4.5 creates the following directories:

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
        ├── index.html
        ├── game.js
        ├── README.md
        └── launch.bat (or .sh)
```

---

## 🎯 Sample Workflow: Complete Distribution Cycle

### 1. Admin Creates Distribution Package

```bash
# Create package for licensed user
distribution create ABC-123-XYZ 4.5.0
```

### 2. User Downloads Their Copy

```bash
# User downloads with their license key
wget http://localhost:5000/api/distribution/download/ABC-123-XYZ
```

### 3. User Checks for Updates

```bash
# User checks if updates are available
curl "http://localhost:5000/api/updates/check?version=4.5.0&license=ABC-123-XYZ"
```

### 4. User Generates Game Client

```bash
# User creates a WebGL client for their game server
curl -X POST http://localhost:5000/api/gameclient/generate \
  -H "Authorization: Bearer USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "ABC-123-XYZ",
    "platform": "WebGL",
    "configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "My Fantasy MMORPG",
      "theme": "fantasy"
    }
  }'
```

### 5. User Accesses Their Game

```
http://localhost:5000/clients/{package-id}/index.html
```

---

## 🛠 Integration with Existing Systems

### License Module Integration

All Phase 4.5 modules integrate with the existing License module:

```csharp
// Check license validity before operations
var license = _licenseModule.GetAllLicenses()
    .FirstOrDefault(l => l.LicenseKey == licenseKey);

if (license == null || license.Status != LicenseStatus.Active)
{
    throw new InvalidOperationException("Invalid or inactive license");
}
```

### RaCoin Integration (Future)

In future updates, distribution and client generation may cost RaCoins:

- Distribution package creation: 1000 RaCoins
- Game client generation: 500 RaCoins per platform
- Update downloads: Free for active licenses

### GameEngine Integration

Generated game clients connect to the GameEngine module via WebSocket:

```javascript
// Client connects to game engine
const ws = new WebSocket('ws://localhost:5000/ws');
ws.send(JSON.stringify({
    type: 'auth',
    licenseKey: 'ABC-123-XYZ'
}));
```

---

## 📊 API Reference Summary

### Distribution Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/distribution/create` | Admin | Create distribution package |
| GET | `/api/distribution/download/{key}` | Public | Download package (license check) |
| GET | `/api/distribution/packages` | Admin | List all packages |

### Update Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/updates/check` | Public | Check for updates (license required) |
| GET | `/api/updates/download/{version}` | Public | Download update (license required) |
| GET | `/api/updates/list` | Admin | List all updates |

### GameClient Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/gameclient/generate` | User | Generate game client |
| GET | `/api/gameclient/list` | User | List user's clients |
| GET | `/clients/{id}/{file}` | Public | Serve client files |

---

## 🎉 What's New in Phase 4.5

1. **Authorized Copy Distribution** - Package and distribute RaCore with license validation
2. **Mainframe Updates** - Automatic update checking and delivery system
3. **Multi-Platform Clients** - Generate WebGL and desktop game clients
4. **Package Management** - ZIP compression with SHA256 checksums
5. **Version Control** - Semantic versioning with intelligent comparison
6. **Client Templates** - Professional HTML5 game client with WebSocket support
7. **Console Commands** - Full CLI interface for all operations
8. **REST APIs** - Complete HTTP endpoints for external integration

---

## 🔮 Future Enhancements (Phase 5+)

- Automatic update installation
- P2P distribution network
- Multi-tenant mainframe access
- Client customization UI
- Mobile platform support (Android/iOS)
- Steam/Epic Games Store integration
- Digital rights management (DRM)
- Telemetry and usage analytics

---

**Document Version:** 1.0  
**Phase:** 4.5  
**Status:** Complete  
**Last Updated:** 2025-01-08
