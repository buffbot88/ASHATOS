# CMS Architecture - LegendaryCMS Integration

## Overview

SiteBuilder now integrates with **LegendaryCMS** module for all CMS functionality. PHP file generation has been removed in favor of a fully managed C# module architecture.

## Architecture

### Previous Approach (Removed)
❌ Generated PHP files in `CMS/` directory  
❌ Required external PHP runtime  
❌ Separate PHP execution process  

### New Approach (Current)
✅ LegendaryCMS runs as C# module within RaOS  
✅ No PHP files or external processes needed  
✅ All CMS functionality via REST API endpoints  

## Directory Structure

```
RaCore/
├── wwwroot/              # PUBLIC - Static HTML files
│   ├── index.html        # Calls LegendaryCMS API endpoints
│   ├── login.html
│   ├── control-panel.html
│   └── js/
│       ├── control-panel-api.js
│       └── control-panel-ui.js
│
├── bin/Debug/net9.0/
│   └── LegendaryCMS.dll  # CMS module runs here
│
└── Databases/
    └── cms_database.sqlite
```

## How It Works

1. **LegendaryCMS Module**: Runs as a C# module within RaOS process
   - Loaded at startup by RaCore ModuleManager
   - Exposes REST API endpoints (`/api/forums`, `/api/blogs`, `/api/chat`, etc.)
   - Manages all CMS data and business logic

2. **Static HTML (wwwroot)**: Public-facing site
   - Pure HTML/CSS/JavaScript files
   - Calls LegendaryCMS API endpoints via JavaScript
   - No server-side code or PHP

3. **SiteBuilder**: Orchestration
   - Generates static HTML files in `wwwroot/`
   - Checks for LegendaryCMS module availability
   - No longer generates PHP files

## API Endpoints

LegendaryCMS provides these REST API endpoints:

### Public (No Auth)
- `GET /api/health` - Health check
- `GET /api/version` - CMS version
- `GET /api/forums` - List forums
- `GET /api/blogs` - List blog posts
- `GET /api/chat/rooms` - List chat rooms

### Authenticated
- `POST /api/forums/post` - Create forum post
- `POST /api/blogs/create` - Create blog post
- `GET /api/profile` - Get user profile
- `GET /api/admin/settings` - Admin settings (Admin only)

## SiteBuilder Commands

### Generate Static Site
```bash
site spawn
# or
site spawn wwwroot
# or
site spawn static
```

Generates static HTML files in `wwwroot/`. These files call LegendaryCMS API endpoints.

### Initialize CMS
```bash
site spawn cms
# or
site spawn integrated
```

Checks if LegendaryCMS module is loaded and reports status. No files are generated - CMS runs as a module.

### Check Status
```bash
site status
```

Shows:
- Static site status (wwwroot files)
- LegendaryCMS module status
- API endpoint availability

## Migration from PHP

### What Changed
- ❌ **Removed**: CmsGenerator, ControlPanelGenerator, ForumGenerator, ProfileGenerator
- ❌ **Removed**: PhpDetector, PHP file generation
- ❌ **Removed**: Internal `CMS/` directory
- ✅ **Added**: Integration with LegendaryCMS module
- ✅ **Added**: API-based architecture

### For Users
- No action required - upgrade is automatic
- Old PHP files (if any) are no longer used
- All CMS features available through LegendaryCMS module
- Use `cms` commands for CMS features, `site` commands for static site

### For Developers
- CMS functionality: Use `LegendaryCMS` module API
- Static site: Use `WwwrootGenerator`
- No PHP runtime needed

## Benefits

✅ **Security**: No external PHP execution  
✅ **Performance**: Native C# performance  
✅ **Maintainability**: Single codebase, no PHP/C# mix  
✅ **Features**: Full plugin system, RBAC, rate limiting  
✅ **Deployment**: Single executable, no PHP dependencies  

## LegendaryCMS Module

LegendaryCMS is a production-ready CMS module (v8.0.0) that provides:

- **Full REST API** for all CMS operations
- **Plugin System** with event hooks
- **Enhanced RBAC** with granular permissions
- **Configuration Management** for different environments
- **Rate Limiting** and security features
- **Health Checks** and monitoring

See `LegendaryCMS/README.md` for full documentation.

---

**Version**: 2.0  
**Date**: 2025-10-08  
**Author**: RaOS Team  
**Migration**: PHP generation removed, LegendaryCMS integration complete

