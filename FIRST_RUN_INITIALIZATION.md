# RaCore First-Run Auto-Initialization - SuperAdmin Control Panel

## Overview

RaCore now features automatic first-run initialization that spawns a fully functional SuperAdmin Control Panel with PHP 8+ and SQLite database, requiring zero manual setup from users. This Control Panel is restricted to SuperAdmin accounts only and serves as the central management hub for all CMS/server operations.

## What Happens on First Run?

When you run RaCore for the first time:

```
========================================
   RaCore First-Run Initialization
   SuperAdmin Control Panel Setup
========================================

[FirstRunManager] Step 1/3: Spawning SuperAdmin Control Panel...
✅ SuperAdmin Control Panel generated successfully!

[FirstRunManager] Step 2/3: Configuring Apache...
[ApacheManager] Apache configuration files created

[FirstRunManager] Step 3/3: Starting web server...
[ApacheManager] PHP server started successfully on http://localhost:8080

✅ SuperAdmin Control Panel is now running!
   Access it at: http://localhost:8080
   Login: admin / admin123 (SuperAdmin only)

⚠️  SECURITY: Only SuperAdmin accounts can access the Control Panel
   Change the default password immediately!

========================================
   First-Run Initialization Complete
========================================
```

## SuperAdmin Control Panel Features

### Access Control
- **SuperAdmin Role Required**: Only users with SuperAdmin role (role = 2) can access
- **Secure Authentication**: Integrates with RaCore Authentication API
- **Token-Based Sessions**: Secure session management with token validation
- **Audit Logging**: All actions are logged for security compliance

### Management Features
1. **System Overview Dashboard**
   - Total users count
   - Active licenses
   - System status monitoring
   - Recent security events

2. **License Management**
   - Subscription-based licensing system
   - License key generation
   - Instance tracking
   - Multi-tenant support

3. **User Management**
   - View all users and roles
   - Track last login times
   - Role-based access control

4. **Server Health & Diagnostics**
   - RaCore version information
   - PHP version and configuration
   - Database status
   - Authentication API connectivity

5. **Future Server Spawning**
   - Placeholder for spawning additional instances
   - Centralized instance management
   - Update distribution from mainframe

## Architecture

### Components

#### 1. FirstRunManager (`RaCore/Engine/FirstRunManager.cs`)
- **Purpose**: Manages first-run detection and orchestrates initialization
- **Marker File**: `.racore_initialized` (JSON format)
- **Control Panel Path**: `superadmin_control_panel/`
- **Key Methods**:
  - `IsFirstRun()`: Checks for marker file existence
  - `InitializeAsync()`: Orchestrates the 3-step initialization process
  - `MarkAsInitialized()`: Creates marker file with metadata

#### 2. CMSSpawnerModule (`RaCore/Modules/Extensions/CMSSpawner/CMSSpawnerModule.cs`)
- **Purpose**: Generates SuperAdmin Control Panel and optional CMS homepages
- **Commands**:
  - `cms spawn control`: Creates SuperAdmin Control Panel (used by first-run)
  - `cms spawn`: Creates general CMS homepage (manual command)
- **Key Methods**:
  - `SpawnControlPanel()`: Generates SuperAdmin Control Panel
  - `InitializeControlPanelDatabase()`: Creates control panel database schema
  - Database tables: users, modules, permissions, server_health, licenses, audit_log

#### 3. ApacheManager (`RaCore/Engine/ApacheManager.cs`)
- **Purpose**: Manages Apache configuration and PHP server lifecycle
- **Key Methods**:
  - `IsApacheAvailable()`: Detects Apache installation
  - `CreateApacheConfig()`: Generates VirtualHost configuration
  - `CreateHtaccess()`: Creates .htaccess for security and routing
  - `StartPhpServer()`: Launches PHP built-in server as fallback
  - `ConfigureApache()`: Orchestrates Apache setup

#### 4. AuthenticationModule Integration
- **Token Validation**: Control Panel validates tokens via `/api/auth/validate`
- **Role Checking**: Only SuperAdmin (role = 2) can access Control Panel
- **Session Management**: Secure token-based authentication
- **Security Events**: All login attempts logged

#### 5. Program.cs Integration
- **Line Added**: First-run check after module loading
- **Behavior**: Async initialization before server startup
- **Non-blocking**: Server continues startup after initialization

### Initialization Flow

```
RaCore Startup
    ↓
Load Modules (including AuthenticationModule)
    ↓
FirstRunManager.IsFirstRun() ?
    ↓ Yes
    ├─→ Step 1: CMSSpawner.Process("cms spawn control")
    │   ├─→ Detect PHP
    │   ├─→ Create superadmin_control_panel/
    │   ├─→ Initialize SQLite database with:
    │   │   ├─→ users table
    │   │   ├─→ modules table
    │   │   ├─→ permissions table
    │   │   ├─→ server_health table
    │   │   ├─→ licenses table (with default license)
    │   │   └─→ audit_log table
    │   └─→ Generate PHP files:
    │       ├─→ config.php (with Auth API integration)
    │       ├─→ db.php (database layer)
    │       ├─→ index.php (Control Panel UI)
    │       └─→ styles.css (professional styling)
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
    ↓
Register Authentication API Endpoints
    ↓
Start RaCore Web Server (port 7077)
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
└── superadmin_control_panel/    # SuperAdmin Control Panel root
    ├── apache_conf/
    │   └── racore.conf          # Apache VirtualHost config
    ├── .htaccess                # Apache directives
    ├── index.php                # Control Panel UI (SuperAdmin only)
    ├── config.php               # PHP configuration with Auth API
    ├── db.php                   # Database layer
    ├── styles.css               # Professional dark theme styling
    └── control_panel.sqlite     # SQLite database with:
        ├── users table          # User tracking
        ├── modules table        # Module management
        ├── permissions table    # Permission controls
        ├── server_health table  # Health monitoring
        ├── licenses table       # License management
        └── audit_log table      # Security audit log
```

## Database Schema

### users table
- `id` (TEXT PRIMARY KEY): User GUID
- `username` (TEXT): Username
- `role` (TEXT): User role (SuperAdmin, Admin, User)
- `created_at` (TEXT): Creation timestamp
- `last_login` (TEXT): Last login timestamp

### modules table
- `id` (INTEGER PRIMARY KEY): Auto-increment ID
- `name` (TEXT): Module name
- `category` (TEXT): Module category
- `status` (TEXT): Module status
- `created_at` (TEXT): Creation timestamp

### permissions table
- `id` (INTEGER PRIMARY KEY): Auto-increment ID
- `user_id` (TEXT): Foreign key to users
- `module_name` (TEXT): Module name
- `can_access` (INTEGER): Access permission flag
- `can_configure` (INTEGER): Configuration permission flag

### server_health table
- `id` (INTEGER PRIMARY KEY): Auto-increment ID
- `timestamp` (TEXT): Measurement timestamp
- `cpu_usage` (REAL): CPU usage percentage
- `memory_usage` (REAL): Memory usage percentage
- `status` (TEXT): Server status

### licenses table
- `id` (TEXT PRIMARY KEY): License GUID
- `license_key` (TEXT): Unique license key
- `instance_name` (TEXT): Instance name
- `status` (TEXT): License status (active, inactive)
- `created_at` (TEXT): Creation timestamp
- `expires_at` (TEXT): Expiration timestamp
- `max_users` (INTEGER): Maximum users allowed

### audit_log table
- `id` (INTEGER PRIMARY KEY): Auto-increment ID
- `timestamp` (TEXT): Event timestamp
- `user_id` (TEXT): User GUID
- `action` (TEXT): Action performed
- `details` (TEXT): Action details
- `ip_address` (TEXT): Client IP address

### Configuration

### RaCore Server Port Configuration
**Default:** `5000` (non-privileged port)

The RaCore server port is configurable via the `RACORE_PORT` environment variable. This allows the application to run without administrator/root privileges.

To change the port:

**Linux/Mac:**
```bash
export RACORE_PORT=8080
dotnet run
```

**Windows:**
```cmd
set RACORE_PORT=8080
dotnet run
```

**Docker/Compose:**
```yaml
environment:
  - RACORE_PORT=8080
```

When the server starts, it displays:
```
========================================
   RaCore Server Starting
========================================
Server URL: http://localhost:5000
Control Panel: http://localhost:5000/control-panel.html
WebSocket: ws://localhost:5000/ws
========================================
```

**Note:** The default port 80 has been removed to ensure plug-and-play operation without requiring administrator privileges. Port 5000 is used by default as it's non-privileged and commonly available.

### CMS PHP Server Port Configuration
Default: `8080`

To change:
```csharp
var apacheManager = new ApacheManager(_cmsPath, 8081); // Custom port
```

### Control Panel Path Configuration
Default: `<AppContext.BaseDirectory>/superadmin_control_panel`

To change in FirstRunManager:
```csharp
_cmsPath = Path.Combine(AppContext.BaseDirectory, "my_custom_path");
```

### Authentication API Configuration
Default: `http://localhost:7077/api/auth`

To change in generated config.php:
```php
define('RACORE_AUTH_API', 'http://your-server:port/api/auth');
```

## Force Re-initialization

To force re-initialization on next run:

```bash
# Delete the marker file
rm .racore_initialized

# Optionally delete the Control Panel directory
rm -rf superadmin_control_panel

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
- Skips Control Panel initialization
- Marks as initialized to prevent retries
- Continues with RaCore startup

### Non-SuperAdmin Access Attempt
- Control Panel denies access
- Returns error message: "Access denied. SuperAdmin role required."
- Logs failed authentication attempt
- User must authenticate with SuperAdmin credentials

## Security Considerations

### Generated Files
- `.htaccess` protects sensitive files (`.sqlite`, `.db`, `.log`)
- Session security headers configured
- Default credentials (admin/admin123) - **MUST BE CHANGED**
- SuperAdmin role enforcement via RaCore Auth API

### Production Recommendations
1. **Change default admin password immediately**
2. Enable HTTPS with valid certificates
3. Set proper file permissions (644 for files, 755 for directories)
4. Disable error display in production (`error_reporting` off)
5. Implement rate limiting for login attempts
6. Regular database backups
7. Monitor audit log for suspicious activity
8. Rotate license keys periodically
9. Review and update permissions regularly
10. Keep RaCore and dependencies updated

### SuperAdmin Access Control
- Only SuperAdmin accounts (role = 2) can access Control Panel
- Token validation performed on every request
- Session expiry after 1 hour (configurable)
- Failed login attempts logged in audit log
- IP address tracking for all authentication events

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

### Issue: Control Panel Not Generated
**Symptoms**: No `superadmin_control_panel` directory created  
**Solution**: 
1. Check PHP installation: `php --version`
2. Check RaCore logs for errors
3. Verify write permissions
4. Ensure CMSSpawner module is loaded

### Issue: PHP Server Not Starting
**Symptoms**: Server shows as started but not accessible  
**Solution**:
1. Check if port 8080 is available: `netstat -tuln | grep 8080`
2. Try different port: Edit `FirstRunManager.cs`
3. Check PHP error logs
4. Verify firewall settings

### Issue: Repeated Initialization
**Symptoms**: First-run runs on every startup  
**Solution**:
1. Check if `.racore_initialized` is being created
2. Verify write permissions in output directory
3. Check logs for marker creation errors

### Issue: Access Denied to Control Panel
**Symptoms**: "Access denied. SuperAdmin role required" error  
**Solution**:
1. Verify user has SuperAdmin role (role = 2)
2. Check Authentication API is running on port 7077
3. Verify token is valid: Test `/api/auth/validate` endpoint
4. Check if session token is being sent correctly
5. Review audit log for authentication failures

### Issue: Apache Config Not Working
**Symptoms**: Apache config files created but not functional  
**Solution**:
1. Manually copy config: `sudo cp apache_conf/racore.conf /etc/apache2/sites-available/`
2. Enable site: `sudo a2ensite racore.conf`
3. Reload Apache: `sudo systemctl reload apache2`
4. Check Apache error logs: `sudo tail -f /var/log/apache2/error.log`

### Issue: Authentication API Not Responding
**Symptoms**: Control Panel can't validate tokens  
**Solution**:
1. Ensure RaCore main server is running on port 7077
2. Check RaCore logs for Auth API initialization
3. Test API directly: `curl http://localhost:7077/api/auth/validate`
4. Verify no firewall blocking port 7077

## Future Enhancements

Potential improvements:
- [ ] Configuration file for customization (port, path, etc.)
- [ ] Interactive setup wizard for initial configuration
- [ ] Database migration system for schema updates
- [ ] Multi-site spawning and management from Control Panel
- [ ] Custom theme selection during first run
- [ ] Automated Apache site enablement (with sudo)
- [ ] Health check endpoint for PHP server
- [ ] Automatic SSL certificate generation (Let's Encrypt)
- [ ] Docker container support for easy deployment
- [ ] Cloud deployment options (Azure, AWS, GCP)
- [x] SuperAdmin-only Control Panel
- [x] License management system
- [x] Multi-tenant architecture support
- [x] Audit logging for security compliance
- [ ] Real-time server monitoring dashboard
- [ ] Automated instance spawning from Control Panel
- [ ] Remote server management capabilities
- [ ] Update distribution system from mainframe
- [ ] Two-factor authentication for SuperAdmin
- [ ] Advanced permission management UI

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

## Authentication Flow

### SuperAdmin Login Process
1. User visits Control Panel at `http://localhost:8080`
2. Control Panel presents login form
3. User enters credentials (default: admin/admin123)
4. Control Panel sends POST request to RaCore Auth API at `http://localhost:7077/api/auth/login`
5. Auth API validates credentials and checks user role
6. If role is SuperAdmin (role = 2):
   - Auth API returns success with token
   - Control Panel stores token in session
   - User is granted access to Control Panel
7. If role is not SuperAdmin:
   - Auth API returns success but Control Panel denies access
   - Error: "Access denied. SuperAdmin role required."

### Token Validation on Each Request
1. User makes request to Control Panel
2. Control Panel checks for session token
3. If token exists:
   - Control Panel calls `checkSuperAdmin()` function
   - Function sends POST to `/api/auth/validate` with token
   - Auth API validates token and returns user info
   - Control Panel verifies `user.Role === 2` (SuperAdmin)
4. If validation fails:
   - User is redirected to login page
   - Session is destroyed

### Security Features
- **Token-Based Authentication**: Secure, stateless authentication
- **Role Verification**: Double-check on both API and Control Panel
- **Session Management**: Automatic timeout after 1 hour
- **HTTPS Ready**: Configure certificates for production
- **Audit Logging**: All authentication events logged

---

**Generated by RaCore Development Team**  
**Version**: 2.0  
**Last Updated**: 2025-10-05
