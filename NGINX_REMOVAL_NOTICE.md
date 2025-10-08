# Nginx to Apache Migration Notice

## Summary

As of this release, **Nginx support has been completely removed** from RaOS/RaCore. The system now focuses exclusively on **Apache HTTP Server** for web serving capabilities.

## What Changed

### Removed Components
- `RaCore/Engine/NginxManager.cs` - Nginx configuration manager
- `RaCore/Tests/NginxManagerPhpPathTests.cs` - Nginx-related tests
- `RaCore/Nginx/` - Nginx configuration directory
- `RaCore/nginx_conf/` - Nginx configuration templates
- `setup/setup-nginx-permissions.sh` - Nginx permissions setup script
- `setup/ubuntu-nginx-sudoers` - Nginx sudoers configuration
- All Nginx-related API endpoints in Program.cs
- `IServerSetupModule.SetupNginxConfigAsync()` method

### Added/Enhanced Components
- **Enhanced Apache Detection**: Added process detection and status checking
  - `ApacheManager.IsApacheRunning()` - Check if Apache process is running
  - `ApacheManager.GetApacheStatus()` - Get detailed Apache status, version, and running state
  
- **Enhanced PHP Detection**: Added process detection and status checking
  - `PhpDetector.IsPhpRunning()` - Check if PHP/PHP-FPM process is running
  - `PhpDetector.GetPhpStatus()` - Get detailed PHP status, version, and running state

- **FTP Integration**: Added FTP client support for local FTP server communication
  - `FtpHelper` class for file operations using configured credentials
  - ServerConfiguration now includes FTP credentials (FtpUsername, FtpPassword, FtpHost, FtpPort)

## Migration Guide

### For Developers

If you have code that references Nginx components:

1. **Remove Nginx References**: Delete any code that calls `NginxManager` or `SetupNginxConfigAsync`
2. **Use Apache Instead**: Replace with `ApacheManager` equivalents where applicable
3. **Update Configuration**: Remove Nginx configuration from your setup

### For System Administrators

1. **Uninstall Nginx** (optional): If you were using Nginx solely for RaOS, you can remove it
2. **Install Apache**: Ensure Apache HTTP Server is installed and configured
3. **Configure PHP**: Set up PHP with Apache (mod_php or PHP-FPM)
4. **Update FTP Settings**: Add FTP credentials to your `server-config.json`:
   ```json
   {
     "FtpUsername": "your-ftp-username",
     "FtpPassword": "your-ftp-password",
     "FtpHost": "localhost",
     "FtpPort": 21
   }
   ```

### Apache Configuration

Place Apache configuration files in: `C:\RaOS\webserver\settings\httpd.conf` (Windows) or the equivalent path on Linux.

### PHP Configuration

Place PHP configuration files in: `C:\RaOS\webserver\settings\php.ini` (Windows) or the equivalent path on Linux.

## Why This Change?

- **Simplified Architecture**: Focusing on a single web server (Apache) reduces complexity
- **Better Integration**: Apache has mature PHP integration options
- **Community Support**: Apache remains widely used and well-documented
- **Process Management**: Enhanced detection allows RaOS to better understand the running state

## Need Help?

If you encounter issues during migration:

1. Check that Apache is installed: `apache2 -v` or `httpd -v`
2. Verify PHP is installed: `php -v`
3. Ensure configuration files exist in `C:\RaOS\webserver\settings\`
4. Check the RaOS logs for any Apache/PHP detection warnings

## Documentation Updates

The following documentation files contain historical Nginx references but are kept for archival purposes:

- `docs/archive/summaries/BOOT_SEQUENCE.md`
- `docs/archive/summaries/NGINX_MIGRATION_SUMMARY.md`
- `docs/archive/migrations/NGINX_MIGRATION_GUIDE.md`
- Various files in `docs/archive/`

Current documentation has been updated to reflect Apache as the primary web server.

---

**Last Updated**: 2025-01-08  
**RaCore Version**: 1.0+  
**Migration Status**: Complete
