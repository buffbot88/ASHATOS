# ASHATOS phpBB3 Authentication Integration

This document describes the phpBB3 authentication integration that replaces the previous AGP_CMS authentication system.

## What Changed

The ASHATOS AI Game Server now integrates with phpBB3 (a popular open-source forum software) for user authentication instead of the custom AGP_CMS system. This provides:

- **Battle-tested authentication**: phpBB3 has been used by millions of websites for over 20 years
- **Single sign-on**: Users have one account for both the forum and game server
- **Community features**: Built-in forum, private messaging, and community management
- **Extensibility**: Large ecosystem of phpBB extensions and themes

## Components

### 1. phpBB3 Extension: ASHATOS Authentication Bridge

Location: `/phpbb3_extension/ashatos/authbridge/`

A custom phpBB3 extension that provides REST API endpoints for authentication:
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/validate` - Session validation
- `POST /api/auth/logout` - User logout

**Features:**
- JSON-based REST API
- Session management using phpBB's native sessions
- Role-based user information (Admin/User)
- CORS support for cross-origin requests
- Secure password hashing using phpBB's password manager

**Documentation:** See `phpbb3_extension/ashatos/authbridge/README.md`

### 2. Updated Authentication Services

#### ASHATAIServer
Location: `/AGP_AI/ASHATAIServer/Services/AuthenticationService.cs`

Updated to communicate with phpBB3 instead of AGP_CMS:
- Changed configuration key from `CmsBaseUrl` to `PhpBBBaseUrl`
- Default URL changed from `http://localhost:5000` to `http://localhost/phpbb`
- Backward compatible: still supports `CmsBaseUrl` config key

#### ASHATGoddess
Location: `/AGP_AI/ASHATGoddess/Services/AuthenticationService.cs`

Updated client-side authentication service:
- Changed default URL to phpBB3
- Maintains same authentication state management
- Backward compatible configuration

### 3. Configuration Updates

#### ASHATAIServer: `appsettings.json`
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://localhost/phpbb",
    "RequireAuthentication": true
  }
}
```

#### ASHATGoddess: `appsettings.json`
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://localhost/phpbb",
    "RequireAuthentication": false,
    "AutoLogin": false
  }
}
```

## Installation & Setup

### Quick Start

1. **Install phpBB3**:
   - Download from https://www.phpbb.com/downloads/
   - Install to your web server (e.g., `/var/www/html/phpbb`)
   - Complete the installation wizard

2. **Install ASHATOS Authentication Bridge Extension**:
   ```bash
   cp -r phpbb3_extension/ashatos /path/to/phpbb/ext/
   ```
   - Enable in phpBB Admin Control Panel > Customise > Manage extensions

3. **Configure ASHATOS**:
   - Update `appsettings.json` files with your phpBB URL
   - Restart ASHATAIServer

4. **Test**:
   ```bash
   # Test registration
   curl -X POST http://localhost/phpbb/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"username":"test","email":"test@example.com","password":"pass123"}'
   
   # Test login
   curl -X POST http://localhost/phpbb/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"test","password":"pass123"}'
   ```

### Detailed Instructions

See `PHPBB3_INTEGRATION_GUIDE.md` for comprehensive installation, configuration, and troubleshooting instructions.

## API Compatibility

The phpBB3 integration maintains **full API compatibility** with the previous AGP_CMS system:
- Same endpoint paths: `/api/auth/login`, `/api/auth/register`, `/api/auth/validate`
- Same request/response formats
- Same session management approach

**No changes needed** to existing client code that uses the authentication API!

## Migration from AGP_CMS

If you're currently using AGP_CMS:

1. Install phpBB3 and the ASHATOS Authentication Bridge extension
2. Update configuration files to point to phpBB URL
3. Optionally migrate users from AGP_CMS to phpBB3 database
4. Test authentication endpoints
5. Deploy updated configuration

Users can re-register with the same credentials, or you can provide a migration script to import existing users.

## Development & Testing

### Building the Project

```bash
cd AGP_AI/ASHATAIServer
dotnet build
```

### Running ASHATAIServer

```bash
cd AGP_AI/ASHATAIServer
dotnet run
```

The server will start on `http://localhost:8088`

### Testing Authentication

With ASHATAIServer running:

```bash
# Login through game server proxy
curl -X POST http://localhost:8088/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"pass123"}'

# Register through game server proxy
curl -X POST http://localhost:8088/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"new@example.com","password":"pass123"}'
```

## Security Considerations

1. **HTTPS in Production**: Always use HTTPS for authentication endpoints
2. **Session Security**: phpBB sessions expire based on phpBB configuration (default: 1 hour)
3. **Password Policy**: Configure in phpBB ACP (minimum 6 characters by default)
4. **Rate Limiting**: Consider implementing rate limiting to prevent brute force attacks
5. **CORS**: Restrict allowed origins in production (currently allows all origins)

## Architecture Diagram

```
┌─────────────────────────────────┐
│        phpBB3 Forum             │
│  + ASHATOS Auth Bridge Ext      │
│                                 │
│  API Endpoints:                 │
│  - POST /api/auth/login         │
│  - POST /api/auth/register      │
│  - POST /api/auth/validate      │
│  - POST /api/auth/logout        │
└────────────┬────────────────────┘
             │ HTTP/JSON
             │
    ┌────────┴──────────┐
    │                   │
┌───▼──────────────┐ ┌──▼─────────────────┐
│  ASHATAIServer   │ │  ASHATGoddess      │
│  (Game Server)   │ │  (Desktop Client)  │
│                  │ │                    │
│  Port: 8088      │ │  Desktop App       │
└──────────────────┘ └────────────────────┘
```

## File Structure

```
ASHATOS/
├── phpbb3_extension/
│   └── ashatos/
│       └── authbridge/
│           ├── composer.json
│           ├── ext.php
│           ├── README.md
│           ├── config/
│           │   ├── services.yml
│           │   └── routing.yml
│           ├── controller/
│           │   └── api_controller.php
│           ├── event/
│           │   └── listener.php
│           └── language/
│               └── en/
│                   └── common.php
├── AGP_AI/
│   ├── ASHATAIServer/
│   │   ├── Services/
│   │   │   └── AuthenticationService.cs (updated)
│   │   ├── Controllers/
│   │   │   └── AuthController.cs (unchanged)
│   │   └── appsettings.json (updated)
│   └── ASHATGoddess/
│       ├── Services/
│       │   └── AuthenticationService.cs (updated)
│       └── appsettings.json (updated)
├── PHPBB3_INTEGRATION_GUIDE.md
└── PHPBB3_AUTHENTICATION.md (this file)
```

## Troubleshooting

### Common Issues

1. **Extension not appearing in ACP**
   - Verify folder structure: `phpbb/ext/ashatos/authbridge/`
   - Clear cache: `php bin/phpbbcli.php cache:purge`

2. **API returns 404**
   - Check URL rewriting is enabled
   - Try full URL: `http://domain.com/app.php/api/auth/login`

3. **Authentication fails**
   - Verify phpBB database connection
   - Check user exists and is active
   - Review phpBB error logs

4. **CORS errors**
   - Check browser console for CORS issues
   - Verify CORS headers in api_controller.php
   - May need to add OPTIONS request handler

See `PHPBB3_INTEGRATION_GUIDE.md` for detailed troubleshooting steps.

## Resources

- **phpBB Documentation**: https://www.phpbb.com/support/docs/
- **phpBB Extension Development**: https://area51.phpbb.com/docs/dev/
- **ASHATOS GitHub**: https://github.com/buffbot88/ASHATOS

## Support

For issues related to:
- **phpBB3 Extension**: Open an issue on GitHub with `phpbb3-auth` label
- **ASHATOS Integration**: Open an issue on GitHub with `authentication` label
- **phpBB General**: Visit phpBB.com community forums

## License

- **ASHATOS Authentication Bridge Extension**: GPL-2.0
- **phpBB3**: GPL-2.0
- **ASHATOS**: See main repository LICENSE

---

**Version**: 1.0.0  
**Last Updated**: 2024-11-12  
**Status**: Production Ready
