# Apache and PHP Scanning Implementation

## Summary
This update implements Apache and PHP configuration scanning to replace the deprecated Nginx scanning functionality. RaOS now scans for Apache `httpd.conf` and PHP `php.ini` from a static configuration folder at `C:\RaOS\webserver\settings`.

## Changes Made

### 1. Created ApacheManager.cs
**Location**: `RaCore/Engine/ApacheManager.cs`

**New Features**:
- `ScanForApacheConfig()` - Scans for `httpd.conf` in `C:\RaOS\webserver\settings`
- `ScanForPhpConfig()` - Scans for `php.ini` in `C:\RaOS\webserver\settings`
- `VerifyApacheConfig()` - Verifies Apache configuration has required modules
- `VerifyPhpConfig()` - Verifies PHP configuration has required settings
- `ConfigurePhpIni()` - Automatically updates PHP settings to recommended values
- `IsApacheAvailable()` - Checks if Apache is installed on the system

**Key Capabilities**:
- Scans static configuration folder: `C:\RaOS\webserver\settings`
- Validates Apache proxy modules are enabled
- Validates PHP memory limits, execution times, and other critical settings
- Creates timestamped backups before modifying configuration files
- Cross-platform support (Windows, Linux, macOS)

### 2. Updated PhpDetector.cs
**Location**: `RaCore/Modules/Extensions/SiteBuilder/PhpDetector.cs`

**Changes**:
- Removed `[Obsolete]` attribute from `FindPhpExecutable()`
- Re-implemented PHP scanning functionality
- Searches for PHP in multiple common locations:
  - Local `php` folder in server root
  - Common installation paths (Windows, Linux, macOS)
  - System PATH environment variable
- Returns detailed version information when PHP is found

### 3. Updated FirstRunManager.cs
**Location**: `RaCore/Engine/FirstRunManager.cs`

**Changes**:
- Replaced Nginx configuration with Apache and PHP scanning in `CheckSystemRequirements()`
- Added Apache and PHP configuration verification during initialization
- Removed Nginx reverse proxy configuration code
- Added Apache and PHP configuration step in initialization sequence
- Automatically configures PHP settings during first run
- Reports detailed status of Apache and PHP configurations

**New Initialization Steps**:
```
Step 1/7: Checking system requirements
  - Scans for Apache config at C:\RaOS\webserver\settings\httpd.conf
  - Scans for PHP config at C:\RaOS\webserver\settings\php.ini
  - Verifies Apache modules are enabled
  - Verifies PHP settings are correct

Step 4/7: Configuring Apache and PHP
  - Applies recommended PHP configuration
  - Creates backups before modifications
```

### 4. Updated SiteBuilderModule.cs
**Location**: `RaCore/Modules/Extensions/SiteBuilder/SiteBuilderModule.cs`

**Changes**:
- Removed `#pragma warning disable CS0618` statements for obsolete PHP methods
- PHP scanning is now fully supported and not obsolete
- All SiteBuilder commands now work with re-enabled PHP scanning

### 5. Created Comprehensive Tests
**Location**: `RaCore/Tests/ApachePhpScanningTests.cs`

**Test Coverage**:
- ApacheManager class instantiation
- Apache configuration scanning from static folder
- PHP configuration scanning from static folder
- Apache availability detection
- Configuration verification methods
- Temporary file-based testing for validation logic

## Configuration Requirements

### Static Configuration Folder
**Path**: `C:\RaOS\webserver\settings`

**Required Files**:
1. `httpd.conf` - Apache configuration file
   - Must include proxy modules
   - Should contain RaCore reverse proxy configuration
   
2. `php.ini` - PHP configuration file
   - Should contain recommended settings for RaCore
   - Will be automatically updated if settings are missing or incorrect

### Recommended Apache Configuration
```apache
LoadModule proxy_module modules/mod_proxy.so
LoadModule proxy_http_module modules/mod_proxy_http.so

# RaCore Configuration
<VirtualHost *:80>
    ServerName localhost
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/
</VirtualHost>
```

### Recommended PHP Configuration
```ini
memory_limit = 256M
max_execution_time = 60
max_input_time = 60
post_max_size = 10M
upload_max_filesize = 10M
display_errors = On
log_errors = On
date.timezone = UTC
```

## Nginx Status

### Nginx Methods Status
Nginx scanning methods in `NginxManager.cs` remain marked as `[Obsolete]`:
- `IsNginxAvailable()` - Obsolete
- `FindNginxExecutable()` - Obsolete
- `FindNginxConfigPath()` - Obsolete
- `StartNginx()` - Obsolete
- `ConfigureReverseProxy()` - Obsolete

### Preserved Functionality
- `CreateNginxConfig()` - Kept for generating Nginx config templates
- Configuration file generation in `WwwrootGenerator` still includes Nginx templates

## CMS Suite Spawning

### Full CMS Suite Components
The integrated CMS spawning creates:

1. **Core CMS Files** (via CmsGenerator):
   - `config.php` - Database and site configuration
   - `database.php` - Database connection handler
   - `index.php` - CMS homepage with navigation
   - `admin.php` - Admin interface
   - `styles.css` - Site styling
   - `blogs.php` - Blog system
   - `forums.php` - Forum system
   - `chat.php` - Chat system
   - `profile.php` - User profiles

2. **Control Panel** (via ControlPanelGenerator):
   - Control panel directory structure
   - Administrative interface files

3. **Community Features** (via ForumGenerator):
   - Community directory structure
   - Forum system files

4. **Profile System** (via ProfileGenerator):
   - User profile functionality

5. **wwwroot Files** (via WwwrootGenerator):
   - `index.html` - Landing page
   - `login.html` - Authentication
   - `control-panel.html` - Control panel interface
   - `admin.html` - Admin dashboard
   - `gameengine-dashboard.html` - Game engine controls
   - `clientbuilder-dashboard.html` - Client builder tools
   - JavaScript files for API and UI
   - Configuration templates (Apache, Nginx, PHP)

## Testing

### Test Results
All tests pass successfully:
- ✅ ApacheManager instantiation
- ✅ Apache configuration scanning
- ✅ PHP configuration scanning
- ✅ Apache availability detection
- ✅ Configuration verification methods

### Build Status
- **Build**: ✅ Success (0 warnings, 0 errors)
- **Platform**: .NET 9.0
- **Architecture**: Cross-platform (Windows, Linux, macOS)

## Migration from Nginx

### For Users Previously Using Nginx
If you were using Nginx with RaOS:

1. **Install Apache** web server
2. **Create** the static configuration folder: `C:\RaOS\webserver\settings`
3. **Copy** your Nginx configuration and convert it to Apache format
4. **Place** `httpd.conf` and `php.ini` in the settings folder
5. **Restart** RaOS

### Backward Compatibility
- Nginx configuration generation is preserved for reference
- No breaking changes to existing module APIs
- All CMS spawning functionality remains intact

## Benefits

1. **Simplified Configuration**: Static configuration folder eliminates scanning complexity
2. **Apache Support**: Full support for Apache web server
3. **PHP Scanning Restored**: Automatic PHP detection and configuration
4. **Better Validation**: Configuration verification ensures correct setup
5. **Automatic Updates**: PHP settings are automatically configured to recommended values
6. **Detailed Logging**: Clear console output for debugging and status tracking
7. **Cross-Platform**: Works on Windows, Linux, and macOS

## Issue Resolution

This implementation resolves all issues described in the bug report:

✅ **Apache Scanning**: Implemented and working
✅ **PHP Scanning**: Re-enabled and fully functional  
✅ **Static Config Folder**: Uses `C:\RaOS\webserver\settings`
✅ **Nginx Removal**: Nginx scanning methods marked as obsolete
✅ **CMS Suite Spawning**: Full CMS suite generation verified

## Next Steps

For users deploying RaOS with Apache:

1. Create the configuration folder structure:
   ```
   C:\RaOS\webserver\settings\
   ├── httpd.conf
   └── php.ini
   ```

2. Run RaOS first-run initialization
3. Verify Apache and PHP are detected during startup
4. Check that PHP configuration is automatically updated
5. Access the CMS at `http://localhost`

## Documentation Updated
- [x] Implementation summary created
- [ ] User guide update needed (for setup instructions)
- [ ] Migration guide update needed (for Nginx to Apache transition)
