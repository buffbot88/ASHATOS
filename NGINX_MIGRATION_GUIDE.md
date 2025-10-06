# Nginx Migration Guide - RaOS 5.0

## Overview
RaOS has migrated from Apache to Nginx as its default web server. This document explains the changes and provides migration instructions for existing users.

## Why Nginx?
- **Modern Architecture**: Nginx is designed for modern web workloads with better performance
- **Lower Resource Usage**: Nginx uses less memory and handles concurrent connections more efficiently
- **Better WebSocket Support**: Native support for WebSockets without additional modules
- **Simpler Configuration**: More straightforward configuration syntax
- **Industry Standard**: Nginx is now the most widely used web server

## What Changed

### Code Changes
1. **New NginxManager.cs**: Replaces ApacheManager.cs with equivalent Nginx functionality
2. **BootSequenceManager**: Now detects and configures Nginx instead of Apache
3. **ServerSetupModule**: Generates Nginx configuration files instead of Apache configs
4. **FirstRunManager**: Uses Nginx for first-run setup
5. **Folder Structure**: "Apache" folder renamed to "Nginx"

### API Changes
- `SetupApacheConfigAsync()` → `SetupNginxConfigAsync()`
- `/api/serversetup/apache` → `/api/serversetup/nginx`
- `RestartApache()` → `RestartNginx()`

### Configuration Files
- `httpd.conf` → `nginx.conf`
- Apache VirtualHost syntax → Nginx server block syntax

## PHP Configuration - Still Supported!
**Important**: PHP configuration handling is fully preserved and unchanged:
- `FindPhpExecutable()` - Still works
- `FindPhpIniPath()` - Still works
- `GeneratePhpIni()` - Still works
- `ConfigurePhpIni()` - Still works

PHP configuration is now managed through NginxManager but functions identically.

## Migration Steps for Existing Users

### If You Were Using Apache

#### 1. Install Nginx
**Windows:**
```powershell
# Download from nginx.org and extract to C:\nginx
# Or use Chocolatey:
choco install nginx
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install nginx
```

**Linux (CentOS/RHEL):**
```bash
sudo yum install nginx
```

**macOS:**
```bash
brew install nginx
```

#### 2. Stop Apache (Optional)
If you want to completely replace Apache:

**Windows:**
```powershell
# Stop Apache service
net stop Apache2.4
# Or disable it from Services
```

**Linux:**
```bash
sudo systemctl stop apache2
sudo systemctl disable apache2
```

#### 3. Run RaCore
RaCore will automatically:
- Detect Nginx installation
- Configure reverse proxy settings
- Set up server blocks for your domains
- Configure WebSocket support

#### 4. Verify Configuration
After RaCore starts, check:
```bash
# Linux
sudo nginx -t

# Windows (from Nginx directory)
nginx -t
```

#### 5. Access RaCore
- http://localhost
- http://agpstudios.online
- http://www.agpstudios.online

### If You Want to Keep Apache
You can continue using Apache, but RaCore will no longer automatically configure it. You'll need to:
1. Manually configure Apache reverse proxy
2. Use the generated Nginx configs as a reference
3. Convert Nginx syntax to Apache syntax

## Nginx Configuration Reference

### Basic Server Block (Equivalent to Apache VirtualHost)
```nginx
server {
    listen 80;
    server_name example.com www.example.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

### PHP-FPM Configuration
```nginx
server {
    listen 8080;
    server_name localhost;
    root "/path/to/wwwroot";
    index index.php index.html;
    
    location ~ \.php$ {
        fastcgi_pass 127.0.0.1:9000;
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
    }
}
```

## Comparison: Apache vs Nginx Syntax

### Reverse Proxy
**Apache:**
```apache
<VirtualHost *:80>
    ServerName example.com
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/
</VirtualHost>
```

**Nginx:**
```nginx
server {
    listen 80;
    server_name example.com;
    location / {
        proxy_pass http://localhost:5000;
    }
}
```

### Document Root
**Apache:**
```apache
DocumentRoot "/var/www/html"
<Directory "/var/www/html">
    Options Indexes FollowSymLinks
    AllowOverride All
    Require all granted
</Directory>
```

**Nginx:**
```nginx
root /var/www/html;
location / {
    try_files $uri $uri/ =404;
}
```

## Troubleshooting

### Nginx Not Found
**Problem**: RaCore reports "Nginx not found"
**Solution**: 
1. Verify Nginx is installed: `nginx -v`
2. Add Nginx to system PATH
3. Restart RaCore

### Port 80 Already in Use
**Problem**: Nginx fails to start because port 80 is in use
**Solution**:
1. Check if Apache is still running: `sudo systemctl status apache2`
2. Stop Apache: `sudo systemctl stop apache2`
3. Or configure Nginx to use a different port

### Permission Denied
**Problem**: Cannot modify nginx.conf
**Solution**:
- **Windows**: Run RaCore as Administrator
- **Linux**: Run with sudo or adjust file permissions

### PHP Not Working
**Problem**: PHP files download instead of executing
**Solution**:
1. Ensure PHP-FPM is installed and running
2. Check fastcgi_pass configuration in nginx.conf
3. Verify PHP-FPM is listening on 127.0.0.1:9000

## Platform-Specific Notes

### Windows
- Nginx config: `C:\nginx\conf\nginx.conf`
- Service management: Use nginx.exe with -s flag
- Reload: `nginx -s reload`
- Stop: `nginx -s stop`

### Linux
- Nginx config: `/etc/nginx/nginx.conf`
- Service management: `systemctl`
- Reload: `sudo systemctl reload nginx`
- Stop: `sudo systemctl stop nginx`

### macOS  
- Nginx config: `/usr/local/etc/nginx/nginx.conf`
- Service management: `brew services`
- Reload: `brew services reload nginx`
- Stop: `brew services stop nginx`

## Support

If you encounter issues during migration:
1. Check Nginx error logs
2. Verify configuration with `nginx -t`
3. Ensure PHP-FPM is running (if using PHP)
4. Check RaCore logs for detailed error messages

## Benefits of the Migration

1. **Better Performance**: Nginx handles concurrent connections more efficiently
2. **Lower Memory Usage**: Nginx uses significantly less RAM than Apache
3. **Modern Standards**: Nginx is better suited for modern web applications
4. **WebSocket Support**: Native WebSocket support without additional modules
5. **Easier Configuration**: More intuitive configuration syntax
6. **Active Development**: Nginx has more active development and community support

## Backwards Compatibility

### What's Preserved
- PHP configuration (php.ini) handling - fully functional
- Admin instance folder structure
- Database management
- All RaCore features and modules
- API endpoints (with updated names)

### What Changed
- Web server from Apache to Nginx
- Configuration file format
- Some API method names
- Folder names (Apache → Nginx)

## Future Plans
- Deprecate ApacheManager.cs (kept temporarily for reference)
- Additional Nginx optimization templates
- Automated Apache-to-Nginx configuration converter
- Load balancing configurations for multi-instance setups
