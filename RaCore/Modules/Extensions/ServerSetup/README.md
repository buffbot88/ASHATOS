# Server Setup Module Documentation

## Overview

The ServerSetup Module manages discoverable server folders (Databases, PHP, Apache) and creates per-admin instance configurations for CMS/Forums/GameServer deployments. It enables RaCore AI to dynamically configure Apache HTTP Server and PHP for each licensed admin.

## Features

✅ **Folder Discovery** - Automatically discovers and creates required folders  
✅ **Admin Instance Management** - Per-license folder structure with isolation  
✅ **Apache Configuration** - AI-modifiable httpd.conf for each admin  
✅ **PHP Configuration** - AI-modifiable php.ini for each admin  
✅ **Database Isolation** - Separate SQLite databases per admin  
✅ **Zero Configuration** - Automatic setup on first use

## Folder Structure

### Root Level Folders
```
RaCore/
├── Databases/          # Global database storage (discoverable)
├── php/                # PHP binaries and global config (discoverable)
├── Apache/             # Apache binaries and global config (discoverable)
└── Admins/             # Per-admin instances (discoverable)
```

### Per-Admin Instance Structure
```
Admins/
└── {license}.{username}/
    ├── Databases/          # Admin's SQLite databases
    ├── wwwroot/            # Web root for Apache
    ├── documents/          # Admin documents
    ├── php.ini             # PHP config (AI-modifiable)
    ├── httpd.conf          # Apache config (AI-modifiable)
    ├── admin.json          # Instance metadata
    └── README.md           # Instance documentation
```

## Console Commands

### Discover Folders
```bash
serversetup discover
```
Discovers and creates Databases, php, Apache, and Admins folders if they don't exist.

**Output:**
```
Server Folder Discovery Results:

✓ Databases folder: /path/to/RaCore/Databases
✓ PHP folder: /path/to/RaCore/php
✓ Apache folder: /path/to/RaCore/Apache
✓ Admins folder: /path/to/RaCore/Admins

Created folders:
  - Databases
  - php
  - Apache
  - Admins
```

### Create Admin Instance
```bash
serversetup admin create license=12345 username=admin1
```
Creates a complete admin instance with folder structure and configuration files.

**Output:**
```
✅ Admin instance created: 12345.admin1
Path: /path/to/RaCore/Admins/12345.admin1
Folders: Databases, wwwroot, documents
Configs: php.ini, httpd.conf
```

### Setup Apache Configuration
```bash
serversetup apache setup license=12345 username=admin1
```
Creates or updates Apache configuration for an admin instance.

**Output:**
```
✅ Apache configuration created for 12345.admin1
Path: /path/to/RaCore/Admins/12345.admin1/httpd.conf
```

### Setup PHP Configuration
```bash
serversetup php setup license=12345 username=admin1
```
Creates or updates PHP configuration for an admin instance.

**Output:**
```
✅ PHP configuration created for 12345.admin1
Path: /path/to/RaCore/Admins/12345.admin1/php.ini
```

## API Endpoints

### Discover Server Folders
```http
GET /api/serversetup/discover
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "databasesFolderExists": true,
    "databasesFolderPath": "/path/to/Databases",
    "phpFolderExists": true,
    "phpFolderPath": "/path/to/php",
    "apacheFolderExists": true,
    "apacheFolderPath": "/path/to/Apache",
    "adminsFolderExists": true,
    "adminsFolderPath": "/path/to/Admins",
    "createdFolders": ["Databases", "php", "Apache"]
  }
}
```

### Create Admin Instance
```http
POST /api/serversetup/admin
Authorization: Bearer {token}
Content-Type: application/json

{
  "licenseNumber": "12345",
  "username": "admin1"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Admin instance created: 12345.admin1",
  "data": {
    "path": "/path/to/Admins/12345.admin1",
    "databases": "/path/to/Admins/12345.admin1/Databases",
    "wwwroot": "/path/to/Admins/12345.admin1/wwwroot",
    "documents": "/path/to/Admins/12345.admin1/documents",
    "phpIni": "/path/to/Admins/12345.admin1/php.ini",
    "httpdConf": "/path/to/Admins/12345.admin1/httpd.conf"
  }
}
```

### Setup Apache Config
```http
POST /api/serversetup/apache
Authorization: Bearer {token}
Content-Type: application/json

{
  "licenseNumber": "12345",
  "username": "admin1"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Apache configuration created",
  "data": {
    "configPath": "/path/to/Admins/12345.admin1/httpd.conf"
  }
}
```

### Setup PHP Config
```http
POST /api/serversetup/php
Authorization: Bearer {token}
Content-Type: application/json

{
  "licenseNumber": "12345",
  "username": "admin1"
}
```

**Response:**
```json
{
  "success": true,
  "message": "PHP configuration created",
  "data": {
    "configPath": "/path/to/Admins/12345.admin1/php.ini"
  }
}
```

## Configuration Files

### php.ini Template
Generated PHP configuration includes:
- Memory limits and execution timeouts
- Error logging (per-admin log file)
- File upload settings
- SQLite3 support
- Custom RaCore AI section for modifications

### httpd.conf Template
Generated Apache configuration includes:
- Virtual host on port 8080
- ServerName: {username}.localhost
- DocumentRoot: admin's wwwroot folder
- Directory permissions
- Error and access logs (per-admin)
- PHP integration (PHPIniDir pointing to admin folder)
- Custom RaCore AI section for modifications

## Use Cases

### CMS/Forum Deployment
1. Create admin instance for licensed user
2. Deploy CMS files to `wwwroot/`
3. Create SQLite database in `Databases/`
4. AI configures Apache and PHP automatically

### Game Server Management
1. Create admin instance per game server
2. Store game databases in `Databases/`
3. Serve game assets from `wwwroot/`
4. AI tunes PHP/Apache settings for performance

### Multi-Tenant Hosting
1. Each licensed admin gets isolated instance
2. Separate configurations and databases
3. AI manages all instances dynamically
4. No manual configuration required

## RaCore AI Integration

The ServerSetup module enables RaCore AI to:

1. **Auto-Discover Folders** - Ensures required folders exist on startup
2. **Create Admin Instances** - Generate complete folder structures on-demand
3. **Configure Apache** - Write and update httpd.conf files dynamically
4. **Configure PHP** - Write and update php.ini files dynamically
5. **Manage Databases** - Create SQLite databases in admin-specific folders
6. **Deploy Web Content** - Copy files to admin wwwroot folders
7. **Monitor Instances** - Track all admin instances and their configurations

## Security

- Admin instances are isolated by license number and username
- Each admin has separate Databases, wwwroot, and documents folders
- Configuration files are per-admin (no shared configs)
- API endpoints require authentication
- Admin role required for instance creation

## Example Workflow

```bash
# 1. Discover and create required folders
serversetup discover

# 2. Create admin instance for license 12345, username admin1
serversetup admin create license=12345 username=admin1

# 3. Admin folder structure is now ready:
# Admins/12345.admin1/
#   ├── Databases/
#   ├── wwwroot/
#   ├── documents/
#   ├── php.ini
#   ├── httpd.conf
#   ├── admin.json
#   └── README.md

# 4. AI can now:
#    - Deploy CMS to wwwroot/
#    - Create databases in Databases/
#    - Modify php.ini for performance
#    - Update httpd.conf for custom domains
```

## Troubleshooting

### Folders Not Being Discovered
- Run `serversetup discover` to create missing folders
- Check file system permissions
- Verify AppContext.BaseDirectory is correct

### Admin Instance Already Exists
- Each license.username combination must be unique
- Delete existing instance or use different credentials

### Configuration Files Not Generated
- Ensure admin instance exists before running setup commands
- Check write permissions on Admins folder

## Performance

- Folder discovery: < 10ms (creates 4 folders if missing)
- Admin instance creation: < 50ms (creates 3 folders, 5 files)
- Apache config generation: < 5ms
- PHP config generation: < 5ms

## Module Information

**Name:** ServerSetup  
**Category:** Extensions  
**Version:** 1.0  
**Status:** ✅ Production Ready  
**Dependencies:** None

---

*Generated by RaCore ServerSetup Module*
