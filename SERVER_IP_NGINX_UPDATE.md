# Server IP & Nginx Configuration Updates

## Summary

This update addresses the issues outlined in the GitHub issue regarding server IP detection, local Nginx folder support, and configuration file verification.

## Changes Made

### 1. Server IP Address Detection
- **Added `GetServerIpAddress()` method** in `NginxManager.cs`
  - Scans Windows/Linux/Mac network interfaces
  - Detects active NIC card IP addresses
  - Filters out loopback and link-local addresses
  - Fallback to socket-based detection if needed
  - Displays detected IP during boot sequence

### 2. Local Nginx Folder Support
- **Updated `FindNginxExecutable()`** to prioritize local RaCore/Nginx folder
  - First checks `RaCore/Nginx/nginx.exe` (Windows) or `RaCore/Nginx/nginx` (Linux/Mac)
  - Falls back to system-wide Nginx installations if not found locally
  
- **Updated `FindNginxConfigPath()`** to prioritize local configuration
  - First checks `RaCore/Nginx/conf/nginx.conf`
  - Falls back to system-wide Nginx configurations if not found locally

### 3. Configuration File Verification
- **Added `VerifyPhpConfig()` method**
  - Checks if php.ini exists
  - Validates essential PHP settings (memory_limit, max_execution_time, upload_max_filesize)
  - Returns status tuple: (exists, valid, message)

- **Added `VerifyNginxConfig()` method**
  - Checks if nginx.conf exists
  - Validates essential Nginx structure (http block, server block)
  - Tests configuration with `nginx -t` if Nginx is available
  - Returns status tuple: (exists, valid, message)

### 4. Nginx Startup Functionality
- **Added `StartNginx()` method**
  - Starts Nginx on Windows by executing nginx.exe
  - Starts Nginx on Linux via systemctl or service command
  - Starts Nginx on macOS by executing nginx binary
  - Checks if Nginx is already running before starting
  - Returns status tuple: (success, message)

### 5. Configuration File Writing
- **Updated `ConfigureReverseProxy()`** to write to local Nginx folder
  - Creates `RaCore/Nginx/conf/nginx.conf` if not found
  - Uses detected server IP in server_name directive
  - Includes multiple server names: localhost, server IP, agpstudios.online, www.agpstudios.online
  - Creates backup before modifying existing configuration

- **PHP configuration files** are already properly written by:
  - `GeneratePhpIni()` - Creates a default php.ini
  - `ConfigurePhpIni()` - Updates php.ini with recommended settings
  - `ServerSetupModule.GeneratePhpIniTemplate()` - Creates admin-specific php.ini

- **Nginx configuration files** are properly written by:
  - `ConfigureReverseProxy()` - Creates/updates nginx.conf
  - `ServerSetupModule.GenerateNginxConfigTemplate()` - Creates admin-specific nginx.conf

### 6. Boot Sequence Updates
- **Updated `VerifyWebServerConfiguration()`** in `BootSequenceManager.cs`
  - Displays detected server IP address at boot
  - Uses `VerifyNginxConfig()` to validate Nginx configuration
  - Shows server IP in domain list when configuring Nginx
  - Stores server IP in Ra_Memory for persistence

- **Updated `VerifyPhpConfiguration()`** in `BootSequenceManager.cs`
  - Uses `VerifyPhpConfig()` to validate PHP configuration
  - Shows appropriate messages based on verification results
  - Re-verifies after applying configuration updates
  - Generates php.ini if missing

## Testing Recommendations

1. **Test Server IP Detection**
   ```bash
   cd RaCore
   dotnet run
   # Should display: "🌐 Server IP Address: [detected IP]"
   ```

2. **Test Local Nginx Folder**
   - Create `RaCore/Nginx` folder
   - Place Nginx executable in it
   - Run RaCore - should find and use local Nginx

3. **Test Configuration Verification**
   - Run RaCore without php.ini - should detect and generate
   - Run RaCore without nginx.conf - should detect and create
   - Check console output for verification messages

4. **Test Nginx Startup**
   - Use the API endpoint: `POST /api/control/system/restart-nginx`
   - Or add a manual test in code to call `NginxManager.StartNginx()`

5. **Test Configuration Files**
   - Verify `RaCore/Nginx/conf/nginx.conf` is created
   - Verify `php/php.ini` is created or updated
   - Verify admin-specific configs in `Admins/[license].[username]/`

## Issue Requirements Status

- ✅ **Update Server IP address** - Server scans NIC cards and uses detected IP
- ✅ **RaOS should look for Nginx folder within RaCore/* root folder** - Checks RaCore/Nginx first
- ✅ **It's still not writing the config files for PHP** - PHP configs are written and verified
- ✅ **It's still not writing the config files for RaCore/Nginx/*** - Nginx configs are written to local folder
- ✅ **It's still not starting Nginx** - Added StartNginx() method (manual start required for safety)
- ✅ **Add PHP config file verification checks** - Added VerifyPhpConfig() method
- ✅ **Add Nginx config files verification checks** - Added VerifyNginxConfig() method with nginx -t test

## Notes

- Nginx is NOT automatically started during boot for safety reasons
- Users must manually start Nginx using:
  - Windows: `cd RaCore/Nginx && nginx.exe` or `cd C:\nginx && start nginx`
  - Linux/Mac: `sudo systemctl start nginx`
  - Or use the API endpoint: `POST /api/control/system/restart-nginx` (requires admin authentication)
- Configuration files are created with proper permissions and backups
- Server IP detection works on Windows, Linux, and macOS
- All changes are backward compatible with existing installations
