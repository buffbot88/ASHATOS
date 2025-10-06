# CMS Spawner Quick Start Guide

## Overview

The CMSSpawner module automatically generates a PHP 8+ CMS homepage with SQLite database integration, bridging .NET 10 and modern PHP technologies.

**🆕 NEW: Automatic First-Run Initialization**  
RaCore now automatically spawns the CMS homepage and starts the PHP server on first run! No manual commands needed.

## Prerequisites

- **PHP 8.0+** with SQLite extension
- **.NET 10** (RaCore)
- **Web browser** to view the CMS

### Installing PHP

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install php8.2-cli php8.2-sqlite3
```

**macOS:**
```bash
brew install php
```

**Windows:**
Download from https://windows.php.net/download/

## Quick Start

### Automatic First-Run Setup (Recommended) ⭐

**Just run RaCore - that's it!**

```bash
cd RaCore
dotnet run
```

On first run, RaCore will automatically:
1. ✅ Detect PHP 8+ on your system
2. ✅ Generate the CMS homepage with SQLite database
3. ✅ Create Apache configuration files
4. ✅ Start PHP built-in server on port 8080
5. ✅ Open the CMS at http://localhost:8080

**Console Output:**
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
```

Then just open your browser to **http://localhost:8080** and you're done! 🎉

---

### Manual Setup (Alternative)

If you prefer manual control or need to regenerate the CMS:

### 1. Start RaCore

```bash
cd RaCore
dotnet run
```

You should see:
```
[Module:CMSSpawner] INFO: CMS Spawner module initialized
```

### 2. Generate the CMS

The CMSSpawner module is automatically loaded. To use it programmatically or through the module manager, call:

```csharp
var module = new CMSSpawnerModule();
module.Initialize(null);
var result = module.Process("cms spawn");
```

Or create a test program:

```csharp
using RaCore.Modules.Extensions.CMSSpawner;

var cms = new CMSSpawnerModule();
cms.Initialize(null);

// Detect PHP
Console.WriteLine(cms.Process("cms detect php"));

// Generate CMS
Console.WriteLine(cms.Process("cms spawn"));

// Check status
Console.WriteLine(cms.Process("cms status"));
```

**Note:** If you run RaCore normally (not a test program), the CMS will be auto-spawned on first run.

### 3. Start the PHP Server (if not auto-started)

```bash
cd cms_homepage
php -S localhost:8080
```

### 4. Access the CMS

- **Homepage**: http://localhost:8080
- **Admin Panel**: http://localhost:8080/admin.php
- **Default credentials**: `admin` / `admin123`

## Available Commands

```
cms spawn         - Create PHP CMS homepage with SQLite database
cms spawn home    - Same as 'cms spawn'
cms status        - Show CMS deployment status
cms detect php    - Detect PHP runtime version
help              - Show help message
```

## Generated Structure

```
cms_homepage/
├── index.php           # Public homepage
├── admin.php           # Admin dashboard
├── config.php          # Site configuration
├── db.php              # SQLite database layer
├── styles.css          # CSS styling
├── .htaccess           # Apache configuration
├── cms_database.sqlite # SQLite database
└── apache_conf/
    └── racore.conf     # Apache VirtualHost configuration
```

## First-Run Detection

RaCore uses a `.racore_initialized` marker file to detect first runs:
- **First run**: Marker doesn't exist → Auto-spawns CMS, creates marker
- **Subsequent runs**: Marker exists → Skips initialization

To **force re-initialization**, simply delete the marker:
```bash
rm .racore_initialized
```

The marker file contains:
```json
{
  "InitializedAt": "2025-10-05T04:24:00Z",
  "Version": "1.0",
  "CmsPath": "/path/to/cms_homepage"
}
```

## Database Schema

### Pages Table
```sql
CREATE TABLE pages (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    title TEXT NOT NULL,
    content TEXT,
    slug TEXT UNIQUE NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
```

### Settings Table
```sql
CREATE TABLE settings (
    key TEXT PRIMARY KEY,
    value TEXT
);
```

### Users Table
```sql
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    email TEXT,
    created_at TEXT NOT NULL
);
```

## Features

✅ **🆕 Auto-Spawn on First Run** - Zero configuration needed!  
✅ **Automatic PHP Detection** - Finds PHP 8+ on your system  
✅ **SQLite Database** - Lightweight, file-based database  
✅ **Admin Dashboard** - Content management interface  
✅ **Modern Design** - Beautiful purple gradient theme  
✅ **Secure** - Session-based authentication  
✅ **Apache Configuration** - Auto-generated Apache config files  
✅ **Extensible** - Easy to add new features  

## Customization

### Change Site Title

Edit the database:
```sql
UPDATE settings SET value = 'My Custom Title' WHERE key = 'site_title';
```

Or through PHP:
```php
$db = Database::getInstance();
$db->setSetting('site_title', 'My Custom Title');
```

### Add New Pages

```php
$db = Database::getInstance();
$db->createPage('About', '<h2>About Us</h2><p>Content</p>', 'about');
```

### Customize Styles

Edit `cms_homepage/styles.css` to change colors, fonts, and layout.

## Production Deployment

⚠️ **Before going to production:**

1. **Change default password** (admin/admin123)
2. **Enable HTTPS** with valid certificates
3. **Set proper file permissions** (644 for files, 755 for directories)
4. **Disable error display** in `config.php`
5. **Implement proper password hashing** (replace simple comparison)
6. **Add rate limiting** for login attempts
7. **Regular backups** of the SQLite database

### Apache Configuration

```apache
<VirtualHost *:80>
    ServerName yourdomain.com
    DocumentRoot /path/to/cms_homepage
    
    <Directory /path/to/cms_homepage>
        AllowOverride All
        Require all granted
    </Directory>
    
    # Force HTTPS
    RewriteEngine On
    RewriteCond %{HTTPS} off
    RewriteRule ^(.*)$ https://%{HTTP_HOST}%{REQUEST_URI} [L,R=301]
</VirtualHost>

<VirtualHost *:443>
    ServerName yourdomain.com
    DocumentRoot /path/to/cms_homepage
    
    SSLEngine on
    SSLCertificateFile /path/to/cert.pem
    SSLCertificateKeyFile /path/to/key.pem
    
    <Directory /path/to/cms_homepage>
        AllowOverride All
        Require all granted
    </Directory>
</VirtualHost>
```

### Nginx Configuration

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    root /path/to/cms_homepage;
    index index.php;
    
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    
    location / {
        try_files $uri $uri/ /index.php?$query_string;
    }
    
    location ~ \.php$ {
        fastcgi_pass unix:/var/run/php/php8.2-fpm.sock;
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
    }
    
    location ~ /\.ht {
        deny all;
    }
}
```

## Troubleshooting

### PHP Not Found

```bash
# Install PHP
sudo apt install php8.2-cli php8.2-sqlite3

# Verify installation
php --version
```

### Permission Denied

```bash
# Fix permissions
chmod 755 cms_homepage
chmod 644 cms_homepage/*.php
chmod 666 cms_homepage/cms_database.sqlite
```

### Port Already in Use

```bash
# Use a different port
php -S localhost:8081
```

### Database Locked

- Close all connections to the SQLite database
- Ensure only one PHP process is accessing it
- Check file permissions

## AI Integration

The CMS is designed for AI-powered content generation:

```csharp
// Example: AI-generated content
var cms = new CMSSpawnerModule();
cms.Initialize(moduleManager);

// Generate content via AI (future enhancement)
var content = await llm.GenerateAsync("Write a welcome message for RaCore CMS");

// Insert into database programmatically
using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();
var cmd = conn.CreateCommand();
cmd.CommandText = "UPDATE pages SET content = $content WHERE slug = 'home'";
cmd.Parameters.AddWithValue("$content", content);
cmd.ExecuteNonQuery();
```

## Future Enhancements

Planned features:
- Blog module with posts and comments
- Theme system with multiple templates
- RESTful API for external integrations
- Markdown editor
- Media library
- User roles and permissions
- SEO optimization tools

## Resources

- **Module Documentation**: `RaCore/Modules/Extensions/CMSSpawner/README.md`
- **RaCore Docs**: `README.md`
- **PHP Manual**: https://www.php.net/manual/
- **SQLite Docs**: https://www.sqlite.org/docs.html

## Support

For issues or questions:
1. Check the module README
2. Review PHP error logs
3. Verify PHP and SQLite are installed
4. Check file permissions
5. Review RaCore console output

---

**Generated by RaCore CMSSpawner Module**  
**Version**: v4.8.9  
**Tech Stack**: .NET 10 + PHP 8+ + SQLite 3
