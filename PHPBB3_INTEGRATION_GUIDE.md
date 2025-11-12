# phpBB3 Authentication Integration Guide

This guide explains how to integrate the ASHATOS AI Game Server with phpBB3 for user authentication using the ASHATOS Authentication Bridge extension.

## Overview

The integration replaces the previous AGP_CMS authentication system with phpBB3, providing a robust, battle-tested forum platform for user management and authentication. This allows users to have a single account across both the forum and the game server.

## Architecture

```
┌─────────────────────────────────────┐
│         phpBB3 Forum                │
│  (with ASHATOS Auth Bridge)         │
│                                     │
│  Extension Endpoints:               │
│  - POST /api/auth/login             │
│  - POST /api/auth/register          │
│  - POST /api/auth/validate          │
│  - POST /api/auth/logout            │
│                                     │
│  Database:                          │
│  - phpbb_users                      │
│  - phpbb_sessions                   │
│  - phpbb_groups                     │
└────────────┬────────────────────────┘
             │
             │ HTTP/JSON
             │
    ┌────────┴──────────┐
    │                   │
┌───▼──────────────┐ ┌──▼─────────────────┐
│  ASHATAIServer   │ │  ASHATGoddess      │
│  (GameServer)    │ │  (Desktop Client)  │
│                  │ │                    │
│  Services:       │ │  Services:         │
│  - AuthService   │ │  - AuthService     │
│                  │ │                    │
│  Controllers:    │ │  State:            │
│  - AuthController│ │  - IsAuthenticated │
│                  │ │  - CurrentUser     │
└──────────────────┘ └────────────────────┘
```

## Prerequisites

1. **phpBB3 Installation**
   - phpBB 3.2.0 or higher
   - PHP 7.4 or higher
   - MySQL/MariaDB or PostgreSQL database
   - Web server (Apache/Nginx)

2. **ASHATOS Components**
   - ASHATAIServer (Game Server)
   - ASHATGoddess (Client) - Optional

## Installation Steps

### Step 1: Install phpBB3

If you don't have phpBB3 installed:

1. Download phpBB3 from https://www.phpbb.com/downloads/
2. Extract to your web server directory (e.g., `/var/www/html/phpbb`)
3. Create a MySQL database for phpBB
4. Run the phpBB installer by visiting `http://your-domain.com/phpbb/install/`
5. Complete the installation wizard

### Step 2: Install ASHATOS Authentication Bridge Extension

1. Copy the extension to phpBB:
   ```bash
   cp -r phpbb3_extension/ashatos /path/to/phpbb/ext/
   ```

2. Set proper permissions:
   ```bash
   chown -R www-data:www-data /path/to/phpbb/ext/ashatos
   chmod -R 755 /path/to/phpbb/ext/ashatos
   ```

3. Enable the extension:
   - Login to phpBB Admin Control Panel (ACP)
   - Navigate to "Customise" > "Manage extensions"
   - Find "ASHATOS Authentication Bridge" and click "Enable"

4. Clear phpBB cache:
   ```bash
   cd /path/to/phpbb
   php bin/phpbbcli.php cache:purge
   ```

### Step 3: Configure ASHATAIServer

Update `AGP_AI/ASHATAIServer/appsettings.json`:

```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-domain.com/phpbb",
    "RequireAuthentication": true
  }
}
```

For development/testing:
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://localhost/phpbb",
    "RequireAuthentication": true
  }
}
```

### Step 4: Configure ASHATGoddess (Optional)

Update `AGP_AI/ASHATGoddess/appsettings.json`:

```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-domain.com/phpbb",
    "RequireAuthentication": false,
    "AutoLogin": false
  }
}
```

## Testing the Integration

### Test 1: User Registration

```bash
curl -X POST http://localhost/phpbb/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "securepass123"
  }'
```

Expected Response:
```json
{
  "success": true,
  "message": "Registration successful",
  "sessionId": "a1b2c3d4...",
  "user": {
    "id": 2,
    "username": "testuser",
    "email": "test@example.com",
    "role": "User"
  }
}
```

### Test 2: User Login

```bash
curl -X POST http://localhost/phpbb/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "securepass123"
  }'
```

Expected Response:
```json
{
  "success": true,
  "message": "Login successful",
  "sessionId": "xyz789...",
  "user": {
    "id": 2,
    "username": "testuser",
    "email": "test@example.com",
    "role": "User"
  }
}
```

### Test 3: Session Validation

```bash
curl -X POST http://localhost/phpbb/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "xyz789..."
  }'
```

Expected Response:
```json
{
  "success": true,
  "user": {
    "id": 2,
    "username": "testuser",
    "email": "test@example.com",
    "role": "User"
  }
}
```

### Test 4: Integration Test with ASHATAIServer

Start the ASHATAIServer:
```bash
cd AGP_AI/ASHATAIServer
dotnet run
```

Then test the authentication endpoint:
```bash
curl -X POST http://localhost:8088/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "securepass123"
  }'
```

## Configuration Options

### phpBB Session Settings

Adjust session lifetime in phpBB ACP:
- Navigate to "General" > "Server configuration" > "Cookie settings"
- Modify "Session length" (default: 3600 seconds)

### Security Hardening

1. **Enable HTTPS**:
   ```json
   {
     "Authentication": {
       "PhpBBBaseUrl": "https://your-domain.com/phpbb",
       "RequireAuthentication": true
     }
   }
   ```

2. **Restrict CORS** (edit phpBB extension):
   In `api_controller.php`, modify `json_response()`:
   ```php
   'Access-Control-Allow-Origin' => 'https://your-ashat-server.com',
   ```

3. **Enable Rate Limiting**:
   Consider using nginx rate limiting or phpBB extensions for DDoS protection.

4. **Strong Password Policy**:
   In phpBB ACP: "General" > "User registration settings" > "Password length"

## Migration from AGP_CMS

If you're migrating from the previous AGP_CMS system:

1. **Export Users from AGP_CMS** (if needed):
   - Create a script to export user data from AGP_CMS database
   - Import into phpBB using phpBB's user import tools or custom script

2. **Update Configuration**:
   - Change `CmsBaseUrl` to `PhpBBBaseUrl` in all config files
   - Update URL from `http://localhost:5000` to your phpBB URL

3. **Notify Users**:
   - Inform users about the authentication system change
   - Provide password reset instructions if needed

## Troubleshooting

### Extension Not Working

1. **Check Extension Status**:
   - Verify extension is enabled in ACP
   - Check `ext/ashatos/authbridge/` folder exists
   - Verify file permissions (755 for directories, 644 for files)

2. **Clear Cache**:
   ```bash
   cd /path/to/phpbb
   rm -rf cache/*
   php bin/phpbbcli.php cache:purge
   ```

3. **Check Logs**:
   - phpBB error log: `store/errors.log`
   - Web server error log: `/var/log/apache2/error.log` or `/var/log/nginx/error.log`

### API Returns 404

1. **Check URL Rewriting**:
   - Ensure mod_rewrite is enabled (Apache)
   - Verify nginx rewrite rules are configured

2. **Verify Routing**:
   - Check `config/routing.yml` in extension
   - Test with full URL: `http://domain.com/app.php/api/auth/login`

### Authentication Fails

1. **Verify Database Connection**:
   - Check phpBB database credentials
   - Ensure phpBB tables exist

2. **Test User Exists**:
   - Try logging in through phpBB forum directly
   - Check user is active (not banned/inactive)

3. **Check Password**:
   - Passwords created directly in phpBB should work
   - Ensure password meets minimum requirements

### CORS Issues

1. **Browser Console**:
   - Check for CORS errors in browser developer console
   - Look for preflight (OPTIONS) request failures

2. **Add OPTIONS Handler** (if needed):
   Edit `routing.yml` to add:
   ```yml
   ashatos_authbridge_api_options:
       path: /api/auth/{endpoint}
       defaults: { _controller: ashatos.authbridge.controller.api:options }
       methods: [OPTIONS]
   ```

## Production Deployment

### Checklist

- [ ] phpBB installed on production server
- [ ] ASHATOS Auth Bridge extension installed and enabled
- [ ] HTTPS configured with valid SSL certificate
- [ ] phpBB database backed up regularly
- [ ] Session timeout configured appropriately
- [ ] Rate limiting enabled
- [ ] CORS restricted to known domains
- [ ] Monitoring and logging configured
- [ ] Password policy enforced
- [ ] Regular security updates scheduled

### Recommended Infrastructure

```
[Load Balancer/Reverse Proxy]
         |
         ├─> [phpBB Web Server 1] ─┐
         ├─> [phpBB Web Server 2] ─┼─> [MySQL Master-Slave]
         └─> [phpBB Web Server 3] ─┘
```

### Monitoring

Monitor these metrics:
- API response times
- Authentication success/failure rates
- Active sessions count
- Database connection pool
- Error rates

## Additional Resources

- [phpBB Documentation](https://www.phpbb.com/support/docs/en/3.3/ug/)
- [phpBB Extension Development](https://area51.phpbb.com/docs/dev/)
- [ASHATOS GitHub Repository](https://github.com/buffbot88/ASHATOS)

## Support

For issues specific to the ASHATOS Authentication Bridge:
- GitHub Issues: https://github.com/buffbot88/ASHATOS/issues
- Include error logs, phpBB version, and reproduction steps

For phpBB general support:
- phpBB Community: https://www.phpbb.com/community/

## License

This integration uses:
- phpBB: GNU General Public License v2.0
- ASHATOS Authentication Bridge: GNU General Public License v2.0

See individual LICENSE files for details.
