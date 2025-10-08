# Windows 11 CMS Setup Guide

## Overview

On Windows 11, RaOS uses **Kestrel as the only supported webserver**. No external webserver (Apache, Nginx) is needed or supported.

## Architecture

### Windows 11
```
┌─────────────────────────────────────┐
│         RaOS on Windows 11          │
├─────────────────────────────────────┤
│                                     │
│  ┌──────────────────────────────┐  │
│  │   Kestrel Webserver          │  │
│  │   (ASP.NET Core)             │  │
│  │   Port 80 (default)          │  │
│  │                              │  │
│  │   • API Endpoints            │  │
│  │   • Control Panel            │  │
│  │   • CMS Serving              │  │
│  │   • Static Files             │  │
│  └──────────────────────────────┘  │
│                                     │
│  ┌──────────────────────────────┐  │
│  │   wwwroot/                   │  │
│  │   • index.html               │  │
│  │   • control-panel.html       │  │
│  │   • JS/CSS assets            │  │
│  └──────────────────────────────┘  │
│                                     │
│  FTP: Optional (external file mgmt) │
└─────────────────────────────────────┘
```

### Linux (for comparison)
```
┌─────────────────────────────────────┐
│         RaOS on Linux               │
├─────────────────────────────────────┤
│                                     │
│  ┌──────────────────────────────┐  │
│  │   Kestrel Webserver          │  │
│  │   Port 80                    │  │
│  │   • API Endpoints            │  │
│  │   • Control Panel            │  │
│  └──────────────────────────────┘  │
│                                     │
│  Optional External (for PHP exec):  │
│  ┌──────────────────────────────┐  │
│  │   Apache/PHP8 (optional)     │  │
│  │   • PHP file execution       │  │
│  │   • Admin folders            │  │
│  └──────────────────────────────┘  │
└─────────────────────────────────────┘
```

## CMS Creation on Windows 11

### Quick Start

1. **Install RaOS** on Windows 11
2. **Start RaCore.exe**
3. **Access Control Panel** at `http://localhost:80/control-panel.html`
4. **Create CMS** using SiteBuilder module

That's it! No external webserver installation needed.

### What Happens During CMS Creation

When you create a CMS on Windows 11:

1. ✅ **Kestrel** starts automatically with RaCore
2. ✅ **wwwroot** directory is created with HTML/CSS/JS files
3. ✅ **Control panel** is accessible immediately
4. ❌ **Apache/Nginx** configs are NOT generated
5. ❌ **External webserver** is NOT required

### File Structure

```
RaCore.exe location/
├── RaCore.exe
├── wwwroot/
│   ├── index.html
│   ├── login.html
│   ├── control-panel.html
│   ├── admin.html
│   ├── gameengine-dashboard.html
│   ├── clientbuilder-dashboard.html
│   └── js/
│       ├── control-panel-api.js
│       └── control-panel-ui.js
├── Admins/           (admin instances)
├── Databases/        (SQLite databases)
└── php/              (optional, not used on Windows)
```

Note: `config/` folder with Apache/Nginx configs is **NOT created on Windows**.

## Configuration

### Port Configuration

Default port is 80. To change:

1. Set environment variable: `RACORE_PORT=8080`
2. Or modify `server-config.json`

### Access URLs

- Control Panel: `http://localhost:80/control-panel.html`
- Admin Panel: `http://localhost:80/admin.html`
- Game Engine: `http://localhost:80/gameengine-dashboard.html`
- Client Builder: `http://localhost:80/clientbuilder-dashboard.html`

## FTP Configuration (Optional)

FTP is **optional** on Windows 11 for external file management only.

### When is FTP needed?

- External file transfers to/from other systems
- Multi-tenant admin folder management
- Backup/restore operations

### When is FTP NOT needed?

- CMS creation
- Control panel access
- Web serving (handled by Kestrel)
- Static file serving

### Configuring FTP (if needed)

1. Install FTP server on Windows 11:
   - IIS FTP Server
   - FileZilla Server
   - Other Windows FTP solution

2. Configure in `server-config.json`:
```json
{
  "FtpHost": "localhost",
  "FtpPort": 21,
  "FtpUsername": "your-username",
  "FtpPassword": "your-password"
}
```

### FTP Error Messages

If you see FTP errors during CMS creation:

```
Failed to connect to FTP server at localhost:21
Note: On Windows 11, FTP is optional for CMS operations.
```

**This is informational** - CMS creation will still succeed. FTP errors can be safely ignored unless you specifically need FTP for external file management.

## Troubleshooting

### Issue: Port 80 Already in Use

**Symptom**: "Address already in use" error

**Solution**:
1. Check what's using port 80: `netstat -ano | findstr :80`
2. Stop the conflicting service (often IIS or Skype)
3. Or use a different port with `RACORE_PORT` environment variable

### Issue: Can't Access Control Panel

**Symptom**: Browser shows "Can't connect" at http://localhost:80

**Checklist**:
1. ✓ Is RaCore.exe running?
2. ✓ Check console for startup messages
3. ✓ Verify port 80 is not blocked by firewall
4. ✓ Try `http://127.0.0.1:80` instead of `localhost`

### Issue: Missing Apache/Nginx Configs

**Symptom**: No `config/nginx.conf` or `config/apache.conf` files

**This is correct!** On Windows 11, these files are not generated because Kestrel handles all web serving.

### Issue: FTP Connection Errors

**Symptom**: FTP connection failures during CMS setup

**Solution**: These can be safely ignored on Windows 11. FTP is optional and not required for CMS operation. If you need FTP for external file management, install and configure an FTP server separately.

## Differences from Previous Versions

### What Changed?

In previous versions, documentation may have suggested:
- Installing Apache or Nginx
- Configuring PHP-FPM
- Setting up reverse proxies

**On Windows 11, none of this is needed!**

### Migration from Nginx/Apache

If you were previously using Nginx or Apache on Windows:

1. **Stop and uninstall** external webserver (optional)
2. **Update RaOS** to latest version
3. **Start RaCore.exe** - Kestrel handles everything
4. **Verify** control panel works at `http://localhost:80`

## Testing

To verify Windows 11 Kestrel-only configuration:

```bash
# Run from RaCore console
TestRunner windows11
```

This runs a test suite that verifies:
- Apache config generation is skipped on Windows
- PHP config is marked as optional on Windows
- FTP errors include Windows-specific guidance
- Wwwroot generation works correctly on Windows

## FAQ

### Q: Do I need to install Apache on Windows 11?
**A: No.** Kestrel is built into RaOS and handles all web serving.

### Q: Do I need to install Nginx on Windows 11?
**A: No.** Nginx is not supported or needed on Windows 11.

### Q: Do I need PHP on Windows 11?
**A: No.** PHP is not required for CMS operation on Windows 11.

### Q: Can I use IIS with RaOS on Windows 11?
**A: Not recommended.** Kestrel is the supported webserver. Using IIS may cause port conflicts.

### Q: Do I need FTP for CMS creation?
**A: No.** FTP is optional and only needed for external file management.

### Q: What if I see FTP errors during CMS creation?
**A: They can be safely ignored.** CMS will work without FTP on Windows 11.

### Q: Can I run RaOS on a different port?
**A: Yes.** Set `RACORE_PORT` environment variable or configure in `server-config.json`.

### Q: Is HTTPS supported on Windows 11?
**A: Yes.** Configure SSL certificate in Kestrel configuration.

## Additional Resources

- [NGINX_REMOVAL_NOTICE.md](NGINX_REMOVAL_NOTICE.md) - Architecture changes
- [QUICKSTART.md](QUICKSTART.md) - General RaOS setup
- [CMS_QUICKSTART.md](CMS_QUICKSTART.md) - CMS creation guide

## Support

For Windows 11-specific issues:
1. Check console logs for errors
2. Run `TestRunner windows11` to verify configuration
3. Review this guide's troubleshooting section
4. Report issues with log output and system details

---

**Last Updated**: 2025-01-08  
**RaCore Version**: 9.4.0+  
**Platform**: Windows 11  
**Webserver**: Kestrel (built-in)
