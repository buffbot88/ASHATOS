# CMS Homepage Routing - Phase 9.3.9

## Overview

Phase 9.3.9 restructures the homepage routing pipeline to make the CMS the default static homepage for all users (guests, members, and bots). This unifies the entrypoint, simplifies maintenance, and allows dynamic homepage content management through the CMS.

## Changes Made

### 1. Homepage Route Modification

**File:** `RaCore/Program.cs`

The "/" route now follows this decision flow:

```
1. Check Under Construction Mode
   ├─ If enabled AND user is not admin → Show Under Construction Page
   └─ If disabled OR user is admin → Continue to step 2

2. Check if CMS is Available (index.php exists)
   ├─ If CMS available → Redirect to /index.php (CMS homepage)
   └─ If CMS not available → Continue to step 3

3. Legacy Bot Filtering (Fallback)
   ├─ If user is a bot → Show static SEO homepage
   └─ If user is not a bot → Show access denied message
```

### 2. CMS Availability Check

A new helper method was added:

```csharp
static bool IsCmsAvailable(string wwwrootPath)
{
    var indexPhpPath = Path.Combine(wwwrootPath, "index.php");
    return File.Exists(indexPhpPath);
}
```

This checks if the CMS homepage file (`wwwroot/index.php`) exists before attempting to redirect.

## Behavior Changes

### Before Phase 9.3.9

| Scenario | Result |
|----------|--------|
| Under Construction + Non-Admin | Under Construction page |
| Under Construction + Admin | Static HTML homepage (for bots) or Access Denied (for users) |
| Normal Mode + Bot | Static HTML homepage |
| Normal Mode + Non-Bot | Access Denied message |

### After Phase 9.3.9

| Scenario | Result |
|----------|--------|
| Under Construction + Non-Admin | Under Construction page |
| Under Construction + Admin + CMS Available | Redirect to CMS homepage |
| Under Construction + Admin + CMS Not Available | Fallback to legacy behavior |
| Normal Mode + CMS Available | Redirect to CMS homepage (all users) |
| Normal Mode + CMS Not Available + Bot | Static HTML homepage |
| Normal Mode + CMS Not Available + Non-Bot | Access Denied message |

## Key Features

### ✅ Backward Compatibility

The system gracefully falls back to the legacy bot-filtering behavior when CMS is not installed. This ensures the server remains functional during first-run setup or if CMS generation fails.

### ✅ Under Construction Mode Preserved

The Under Construction mode check happens **first**, before any CMS routing. This ensures:
- Non-admin users always see the maintenance page when enabled
- Admins can still access the CMS during maintenance
- Security and access control are maintained

### ✅ SEO and Bot Access Maintained

When CMS is available, all visitors (including bots) are redirected to the CMS homepage. The CMS can then implement its own SEO optimizations and bot handling logic.

When CMS is not available, the legacy bot detection logic ensures search engines can still index the site.

### ✅ Unified User Experience

All users (guests, members, bots) now access the same CMS homepage, providing a consistent experience and centralized content management.

## Admin Management

Administrators can manage the homepage content through the CMS interface located at `/admin.php` (when authenticated). The CMS provides:

- Content editing
- Page customization
- Blog management
- Forum integration
- User profile systems
- Chat functionality

## Testing

Unit tests have been added in `RaCore/Tests/CmsHomepageRoutingTests.cs` to verify:

- CMS availability detection with various scenarios
- File existence checking logic
- Correct handling of index.html vs index.php
- Empty directory behavior

To run the tests:

```bash
dotnet run --project RaCore/RaCore.csproj cmshomepage
```

## Manual Verification

To manually verify the changes:

1. **Test without CMS (Legacy Behavior)**
   ```bash
   # Ensure wwwroot/index.php does not exist
   rm -f wwwroot/index.php
   # Start server and visit http://localhost:5000/
   # Expected: Access denied for non-bots, static HTML for bots
   ```

2. **Test with CMS**
   ```bash
   # Generate CMS using SiteBuilder module
   # Or manually create wwwroot/index.php
   echo "<?php echo 'CMS Homepage'; ?>" > wwwroot/index.php
   # Start server and visit http://localhost:5000/
   # Expected: Redirect to /index.php
   ```

3. **Test Under Construction Mode**
   ```bash
   # Enable Under Construction via Control Panel or API
   # POST /api/control/server/underconstruction
   # {"enabled": true}
   # Visit http://localhost:5000/ without authentication
   # Expected: Under Construction page
   ```

## API Integration

The change does not affect existing API endpoints. All API routes remain unchanged:

- `/api/auth/*` - Authentication
- `/api/control/*` - Control panel
- `/api/gameengine/*` - Game engine
- `/api/cms/*` - CMS API (when LegendaryCMS is loaded)

## Production Deployment

In production with Nginx:

1. Nginx serves PHP files from wwwroot
2. RaCore redirects "/" to "/index.php"
3. Nginx executes PHP and returns the CMS homepage
4. All dynamic content is handled by the CMS

Example Nginx configuration:

```nginx
location / {
    proxy_pass http://localhost:5000;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
}

location ~ \.php$ {
    include snippets/fastcgi-php.conf;
    fastcgi_pass unix:/var/run/php/php8.3-fpm.sock;
    fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
}
```

## Benefits

### For Users
- Consistent homepage experience
- Dynamic content managed through CMS
- No more "access denied" messages
- Seamless integration with forums, blogs, and chat

### For Administrators
- Centralized homepage management via CMS interface
- Easy content updates without code changes
- Full control over homepage design and content
- Integration with all CMS features

### For Developers
- Simplified routing logic
- Backward compatible fallback
- Clear separation of concerns
- Easier to maintain and extend

## Acceptance Criteria Status

- [x] Homepage ("/") routes to the CMS for all users (except when under construction is enabled)
- [x] Static homepage logic is removed or replaced (now fallback only)
- [x] Bot detection and SEO logic is preserved within the CMS
- [x] Admins can manage homepage from within CMS (via /admin.php)

## Related Documentation

- [Under Construction Mode](UNDER_CONSTRUCTION_MODE.md) - Phase 9.3.8
- [CMS Quick Start](CMS_QUICKSTART.md) - Setting up the CMS
- [SiteBuilder Module](RaCore/Modules/Extensions/SiteBuilder/README.md) - CMS generation
- [Nginx Management](NGINX_MANAGEMENT_UBUNTU.md) - Production setup

## Migration Guide

Existing installations will automatically benefit from this change with no action required:

1. If CMS is already installed (index.php exists), users will be redirected to CMS
2. If CMS is not installed, the system falls back to legacy behavior
3. To enable CMS routing, simply run the SiteBuilder module to generate the CMS

No breaking changes or configuration updates are necessary.
