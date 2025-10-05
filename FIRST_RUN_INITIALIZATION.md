# RaCore First-Run Auto-Initialization

## Overview

RaCore now features automatic first-run initialization that spawns a fully functional PHP 8+ CMS homepage with Apache configuration, requiring zero manual setup from users.

## What Happens on First Run?

When you run RaCore for the first time:

```
========================================
   RaCore First-Run Initialization
========================================

[FirstRunManager] Step 1/3: Spawning CMS Homepage...
✅ CMS Homepage generated successfully!

[FirstRunManager] Step 2/3: Configuring Apache...
[ApacheManager] Apache configuration files created

[FirstRunManager] Step 3/3: Starting web server...
[ApacheManager] PHP server started successfully on http://localhost:8080

✅ CMS Homepage is now running!
   Access it at: http://localhost:8080
   Admin panel: http://localhost:8080/admin.php
   Default login: admin / admin123

========================================
   First-Run Initialization Complete
========================================
```

## Architecture

### Components

#### 1. FirstRunManager (`RaCore/Engine/FirstRunManager.cs`)
- **Purpose**: Manages first-run detection and orchestrates initialization
- **Marker File**: `.racore_initialized` (JSON format)
- **Key Methods**:
  - `IsFirstRun()`: Checks for marker file existence
  - `InitializeAsync()`: Orchestrates the 3-step initialization process
  - `MarkAsInitialized()`: Creates marker file with metadata

#### 2. ApacheManager (`RaCore/Engine/ApacheManager.cs`)
- **Purpose**: Manages Apache configuration and PHP server lifecycle
- **Key Methods**:
  - `IsApacheAvailable()`: Detects Apache installation
  - `CreateApacheConfig()`: Generates VirtualHost configuration
  - `CreateHtaccess()`: Creates .htaccess for security and routing
  - `StartPhpServer()`: Launches PHP built-in server as fallback
  - `ConfigureApache()`: Orchestrates Apache setup

#### 3. Program.cs Integration
- **Line Added**: First-run check after module loading
- **Behavior**: Async initialization before server startup
- **Non-blocking**: Server continues startup after initialization

### Initialization Flow

```
RaCore Startup
    ↓
Load Modules
    ↓
FirstRunManager.IsFirstRun() ?
    ↓ Yes
    ├─→ Step 1: CMSSpawner.Process("cms spawn")
    │   ├─→ Detect PHP
    │   ├─→ Create cms_homepage/
    │   ├─→ Initialize SQLite database
    │   └─→ Generate PHP files
    ↓
    ├─→ Step 2: ApacheManager.ConfigureApache()
    │   ├─→ Create apache_conf/racore.conf
    │   ├─→ Create .htaccess
    │   └─→ Display Apache instructions
    ↓
    ├─→ Step 3: ApacheManager.StartPhpServer()
    │   ├─→ Launch PHP server on port 8080
    │   └─→ Log server output
    ↓
    └─→ MarkAsInitialized()
        └─→ Create .racore_initialized marker
    ↓
Continue Normal Startup
```

## Marker File Format

`.racore_initialized`:
```json
{
  "InitializedAt": "2025-10-05T04:24:00Z",
  "Version": "1.0",
  "CmsPath": "/path/to/cms_homepage"
}
```

## Generated Structure

```
<RaCore Output Directory>/
├── .racore_initialized          # Marker file (JSON)
└── cms_homepage/                # CMS root directory
    ├── apache_conf/
    │   └── racore.conf          # Apache VirtualHost config
    ├── .htaccess                # Apache directives
    ├── index.php                # Public homepage
    ├── admin.php                # Admin panel
    ├── config.php               # PHP configuration
    ├── db.php                   # Database layer
    ├── styles.css               # CSS styling
    └── cms_database.sqlite      # SQLite database
```

## Configuration

### Port Configuration
Default: `8080`

To change:
```csharp
var apacheManager = new ApacheManager(_cmsPath, 8081); // Custom port
```

### CMS Path Configuration
Default: `<AppContext.BaseDirectory>/cms_homepage`

To change in FirstRunManager:
```csharp
_cmsPath = Path.Combine(AppContext.BaseDirectory, "my_custom_path");
```

## Force Re-initialization

To force re-initialization on next run:

```bash
# Delete the marker file
rm .racore_initialized

# Optionally delete the CMS directory
rm -rf cms_homepage

# Run RaCore again
dotnet run
```

## Fallback Behavior

### No PHP Found
- Skips PHP server startup
- Displays installation instructions
- Continues with RaCore startup

### Apache Not Available
- Falls back to PHP built-in server
- Displays instructions for manual Apache setup
- Continues with RaCore startup

### CMSSpawner Module Missing
- Logs warning
- Skips CMS initialization
- Marks as initialized to prevent retries
- Continues with RaCore startup

## Security Considerations

### Generated Files
- `.htaccess` protects sensitive files (`.sqlite`, `.db`, `.log`)
- Session security headers configured
- Default credentials (admin/admin123) - **MUST BE CHANGED**

### Production Recommendations
1. Change default admin password immediately
2. Enable HTTPS with valid certificates
3. Set proper file permissions (644 for files, 755 for directories)
4. Disable error display in production
5. Implement rate limiting for login attempts
6. Regular database backups

## Testing

### Manual Testing Scenarios

1. **Fresh Install**
   ```bash
   dotnet run
   # Should see first-run initialization
   ```

2. **Subsequent Run**
   ```bash
   dotnet run
   # Should skip initialization
   ```

3. **Force Reinitialize**
   ```bash
   rm .racore_initialized
   dotnet run
   # Should reinitialize
   ```

4. **Missing PHP**
   ```bash
   # Temporarily remove PHP from PATH
   export PATH=$(echo $PATH | sed 's|:/usr/bin||')
   dotnet run
   # Should show PHP not found message
   ```

### Integration Tests

See `RaCore/Tests/` (if test infrastructure exists) for:
- FirstRunManager unit tests
- ApacheManager unit tests
- Integration tests for full flow

## Performance Impact

- **First Run**: ~2-3 seconds (includes PHP detection, file generation, server startup)
- **Subsequent Runs**: ~5ms (single file existence check)
- **Async Initialization**: Non-blocking, runs in background during startup

## Troubleshooting

### Issue: CMS Not Generated
**Symptoms**: No `cms_homepage` directory created  
**Solution**: 
1. Check PHP installation: `php --version`
2. Check RaCore logs for errors
3. Verify write permissions

### Issue: PHP Server Not Starting
**Symptoms**: Server shows as started but not accessible  
**Solution**:
1. Check if port 8080 is available: `netstat -tuln | grep 8080`
2. Try different port: Edit `FirstRunManager.cs`
3. Check PHP error logs

### Issue: Repeated Initialization
**Symptoms**: First-run runs on every startup  
**Solution**:
1. Check if `.racore_initialized` is being created
2. Verify write permissions in output directory
3. Check logs for marker creation errors

### Issue: Apache Config Not Working
**Symptoms**: Apache config files created but not functional  
**Solution**:
1. Manually copy config: `sudo cp apache_conf/racore.conf /etc/apache2/sites-available/`
2. Enable site: `sudo a2ensite racore.conf`
3. Reload Apache: `sudo systemctl reload apache2`
4. Check Apache error logs: `sudo tail -f /var/log/apache2/error.log`

## Future Enhancements

Potential improvements:
- [ ] Configuration file for customization (port, path, etc.)
- [ ] Interactive setup wizard
- [ ] Database migration system
- [ ] Multi-site support
- [ ] Custom theme selection during first run
- [ ] Automated Apache site enablement (with sudo)
- [ ] Health check endpoint for PHP server
- [ ] Automatic SSL certificate generation (Let's Encrypt)
- [ ] Docker container support
- [ ] Cloud deployment options (Azure, AWS)

## API Reference

### FirstRunManager

```csharp
public class FirstRunManager
{
    public FirstRunManager(ModuleManager moduleManager);
    public bool IsFirstRun();
    public Task<bool> InitializeAsync();
    public void MarkAsInitialized();
}
```

### ApacheManager

```csharp
public class ApacheManager
{
    public ApacheManager(string cmsPath, int port = 8080);
    public static bool IsApacheAvailable();
    public void CreateApacheConfig();
    public void CreateHtaccess();
    public bool StartPhpServer(string phpPath);
    public bool ConfigureApache();
    public void Stop();
}
```

## Contributing

When modifying first-run behavior:

1. **Test thoroughly** with fresh installations
2. **Preserve backward compatibility** with existing installations
3. **Update documentation** (this file, CMS_QUICKSTART.md)
4. **Consider edge cases** (missing PHP, permission issues, etc.)
5. **Log appropriately** for debugging

## License

Part of the RaCore project. See main repository for license information.

---

**Generated by RaCore Development Team**  
**Version**: 1.0  
**Last Updated**: 2025-10-05
