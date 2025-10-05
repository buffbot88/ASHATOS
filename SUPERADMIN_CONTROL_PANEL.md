# RaCore SuperAdmin Control Panel

## Overview

The RaCore SuperAdmin Control Panel is a secure, permission-gated management interface that serves as the central hub for all CMS/server operations. It is automatically generated during first-run initialization and is accessible only to users with SuperAdmin privileges.

---

## Key Features

### 🔐 Security & Access Control

- **SuperAdmin-Only Access**: Restricted to users with SuperAdmin role (role = 2)
- **Token-Based Authentication**: Integrates with RaCore Authentication API
- **Session Management**: Secure sessions with automatic timeout
- **Audit Logging**: Comprehensive logging of all administrative actions
- **IP Tracking**: All authentication events logged with IP addresses

### 📊 System Dashboard

The Control Panel provides a comprehensive overview of your RaCore instance:

- **User Statistics**: Total users and role distribution
- **License Information**: Active licenses and subscription status
- **System Status**: Real-time health monitoring
- **Security Events**: Recent authentication and access attempts

### 👥 User Management

- View all registered users
- Monitor user roles and permissions
- Track last login times
- View user creation dates
- Integration with RaCore Authentication Module

### 🔑 License Management

The Control Panel includes a subscription-based licensing system:

- **License Key Generation**: Unique keys for each instance
- **Instance Tracking**: Monitor multiple deployed instances
- **Status Management**: Active/inactive license states
- **User Limits**: Configure maximum users per license
- **Expiration Tracking**: Monitor license expiration dates

### 🏥 Server Health & Diagnostics

Monitor the health and status of your RaCore deployment:

- RaCore version information
- PHP version and configuration
- Database status and location
- Authentication API connectivity
- System timestamps (UTC)
- Multi-tenant status

### 📝 Audit Log

Complete audit trail of all system activities:

- Timestamp of each event
- User who performed the action
- Action type and details
- IP address of the request
- Success/failure status

### 🚀 Future Server Spawning

Placeholder features for upcoming functionality:

- Spawn additional RaCore instances from Control Panel
- Centralized instance management
- Remote server monitoring
- Update distribution from mainframe
- Multi-tenant orchestration

---

## Technical Architecture

### Database Schema

The Control Panel uses SQLite with the following tables:

#### users
```sql
CREATE TABLE users (
    id TEXT PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    role TEXT NOT NULL,
    created_at TEXT NOT NULL,
    last_login TEXT
);
```

#### modules
```sql
CREATE TABLE modules (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT UNIQUE NOT NULL,
    category TEXT,
    status TEXT NOT NULL,
    created_at TEXT NOT NULL
);
```

#### permissions
```sql
CREATE TABLE permissions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id TEXT NOT NULL,
    module_name TEXT NOT NULL,
    can_access INTEGER DEFAULT 0,
    can_configure INTEGER DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id)
);
```

#### server_health
```sql
CREATE TABLE server_health (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    cpu_usage REAL,
    memory_usage REAL,
    status TEXT
);
```

#### licenses
```sql
CREATE TABLE licenses (
    id TEXT PRIMARY KEY,
    license_key TEXT UNIQUE NOT NULL,
    instance_name TEXT,
    status TEXT NOT NULL,
    created_at TEXT NOT NULL,
    expires_at TEXT,
    max_users INTEGER DEFAULT 1
);
```

#### audit_log
```sql
CREATE TABLE audit_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    user_id TEXT,
    action TEXT NOT NULL,
    details TEXT,
    ip_address TEXT
);
```

### Authentication Flow

1. **User Access**: User navigates to Control Panel URL
2. **Login Form**: Control Panel presents login interface
3. **Credential Submission**: User submits username/password
4. **API Authentication**: Control Panel calls RaCore Auth API
5. **Role Verification**: API validates credentials and returns user role
6. **Access Grant/Deny**: 
   - If SuperAdmin (role = 2): Access granted, token stored in session
   - Otherwise: Access denied with error message
7. **Token Validation**: Each subsequent request validates token via API
8. **Session Management**: Automatic logout after timeout

### API Integration

The Control Panel integrates with these RaCore APIs:

- **POST /api/auth/login**: Authenticate user credentials
- **POST /api/auth/validate**: Validate session tokens
- **POST /api/auth/logout**: End user session
- **GET /api/auth/events**: Retrieve security events (future)

---

## Installation & Setup

### Automatic Installation (First Run)

The Control Panel is automatically generated on first run:

```bash
cd RaCore
dotnet run
```

Expected output:
```
========================================
   RaCore First-Run Initialization
   SuperAdmin Control Panel Setup
========================================

[FirstRunManager] Step 1/3: Spawning SuperAdmin Control Panel...
✅ SuperAdmin Control Panel generated successfully!
```

### Manual Installation

To manually spawn the Control Panel:

```bash
# From RaCore directory
dotnet run

# In the RaCore console, use the CMSSpawner module:
cms spawn control
```

---

## Usage Guide

### Accessing the Control Panel

1. **Start RaCore**: Ensure RaCore is running on port 7077
2. **Start PHP Server**: 
   ```bash
   cd superadmin_control_panel
   php -S localhost:8080
   ```
3. **Open Browser**: Navigate to `http://localhost:8080`
4. **Login**: Use SuperAdmin credentials (default: admin / admin123)

### Default Credentials

**⚠️ IMPORTANT: Change these credentials immediately after first login!**

- **Username**: `admin`
- **Password**: `admin123`
- **Role**: SuperAdmin

### Changing Default Password

1. Access RaCore Authentication API directly:
   ```bash
   curl -X POST http://localhost:7077/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"Username":"admin","Password":"admin123"}'
   ```
2. Use the returned token to update your password
3. Implement password change UI in future Control Panel updates

---

## Security Best Practices

### Production Deployment

Before deploying to production:

1. **Change Default Password**
   - Update admin password immediately
   - Use strong, unique passwords (12+ characters)
   - Consider password rotation policies

2. **Enable HTTPS**
   - Obtain valid SSL certificate
   - Configure Apache/nginx for HTTPS
   - Redirect HTTP to HTTPS

3. **Firewall Configuration**
   - Restrict access to port 8080
   - Only allow trusted IP addresses
   - Use VPN for remote access

4. **Regular Audits**
   - Review audit logs regularly
   - Monitor for suspicious activity
   - Set up alerting for failed login attempts

5. **Database Security**
   - Regular backups of control_panel.sqlite
   - Secure file permissions (600 for database)
   - Encrypt database at rest (future enhancement)

6. **Session Security**
   - Configure short session timeouts
   - Use secure, HTTP-only cookies
   - Implement CSRF protection

### Access Control

- Only grant SuperAdmin role to trusted administrators
- Implement IP whitelisting for production
- Consider two-factor authentication (future)
- Audit all role changes

### Monitoring

- Monitor audit log for:
  - Failed login attempts
  - Unusual access patterns
  - Configuration changes
  - License modifications
- Set up automated alerts for security events

---

## Troubleshooting

### Cannot Access Control Panel

**Problem**: Browser shows "Connection refused" or "Page not found"

**Solutions**:
1. Verify PHP server is running: `ps aux | grep php`
2. Check port 8080 is not in use: `netstat -tuln | grep 8080`
3. Restart PHP server: 
   ```bash
   cd superadmin_control_panel
   php -S localhost:8080
   ```

### Access Denied Error

**Problem**: "Access denied. SuperAdmin role required."

**Solutions**:
1. Verify user has SuperAdmin role in RaCore Auth system
2. Check RaCore server is running on port 7077
3. Test Auth API directly:
   ```bash
   curl -X POST http://localhost:7077/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"Username":"admin","Password":"admin123"}'
   ```
4. Verify response includes `"Role": 2`

### Token Validation Fails

**Problem**: Logged out immediately after login

**Solutions**:
1. Check RaCore Auth API is accessible
2. Verify session configuration in config.php
3. Clear browser cookies and try again
4. Check PHP session directory permissions

### Database Errors

**Problem**: "Database connection failed" or "Database query error"

**Solutions**:
1. Check database file exists: `ls -l control_panel.sqlite`
2. Verify file permissions: `chmod 644 control_panel.sqlite`
3. Check SQLite extension is enabled: `php -m | grep sqlite`
4. Regenerate database:
   ```bash
   rm control_panel.sqlite
   rm ../.racore_initialized
   cd ..
   dotnet run
   ```

---

## Customization

### Changing Theme Colors

Edit `styles.css` to customize the appearance:

```css
/* Primary colors */
.stat-card {
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
}

/* Change to your brand colors */
.stat-card {
    background: linear-gradient(135deg, #your-color-1 0%, #your-color-2 100%);
}
```

### Adding Custom Sections

Add new sections to `index.php`:

```php
<div class="section">
    <h2>🔧 Custom Section</h2>
    <div class="info-box">
        <p>Your custom content here</p>
    </div>
</div>
```

### Extending Database Schema

To add new tables:

1. Edit `CMSSpawnerModule.cs`
2. Update `InitializeControlPanelDatabase()` method
3. Add new table creation SQL
4. Update `db.php` with new query methods
5. Regenerate Control Panel

---

## API Reference

### Configuration (config.php)

```php
define('DB_PATH', __DIR__ . '/control_panel.sqlite');
define('RACORE_AUTH_API', 'http://localhost:7077/api/auth');
define('SESSION_LIFETIME', 3600); // 1 hour
```

### Database Class (db.php)

```php
Database::getInstance()              // Get database instance
$db->getUsers()                      // Get all users
$db->getModules()                    // Get all modules
$db->getLicenses()                   // Get all licenses
$db->getServerHealth()               // Get health metrics
$db->getAuditLog($limit = 50)        // Get audit log entries
$db->logAudit($userId, $action, $details, $ipAddress)  // Log audit event
```

### Authentication Functions

```php
checkSuperAdmin()                    // Validate current session has SuperAdmin role
```

---

## Development Roadmap

### Implemented Features
- [x] SuperAdmin-only access control
- [x] Token-based authentication
- [x] User management viewing
- [x] License management system
- [x] System health dashboard
- [x] Audit logging
- [x] Multi-tenant database schema

### Planned Features
- [ ] Real-time server monitoring
- [ ] Instance spawning interface
- [ ] Remote server management
- [ ] Update distribution system
- [ ] Two-factor authentication
- [ ] Advanced permission UI
- [ ] Backup/restore functionality
- [ ] Email notifications
- [ ] API key management
- [ ] Module marketplace integration

---

## Contributing

When adding features to the Control Panel:

1. **Security First**: Always validate SuperAdmin role
2. **Audit Everything**: Log all administrative actions
3. **Test Thoroughly**: Test with multiple user roles
4. **Document Changes**: Update this README
5. **Follow Patterns**: Use existing code patterns
6. **Database Migrations**: Plan for schema changes

---

## Support

For issues or questions:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review [FIRST_RUN_INITIALIZATION.md](FIRST_RUN_INITIALIZATION.md)
3. Check [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md)
4. Submit issues to the RaCore repository

---

## License

Part of the RaCore project. See main repository for license information.

---

**RaCore SuperAdmin Control Panel**  
**Version**: 1.0  
**Generated by**: RaCore CMSSpawner Module  
**Last Updated**: 2025-10-05
