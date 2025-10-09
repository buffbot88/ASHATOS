# API Communication & Maintainability Improvements

## Overview
This document describes the improvements made to address API communication issues and improve codebase maintainability by refactoring the large `Program.cs` file.

## Problem Statement
1. **API Communication Issues**: Application was not properly communicating externally via APIs
2. **Maintainability**: `Program.cs` was 3354 lines long, making it difficult to debug and maintain

## Solutions Implemented

### 1. Network Configuration Fixes

#### CORS Configuration Improvements
**Problem**: CORS policy was too restrictive and didn't allow external origins properly.

**Solution**:
- Added `RACORE_PERMISSIVE_CORS` environment variable for development/debugging
- When `RACORE_PERMISSIVE_CORS=true`, CORS allows any origin (useful for debugging)
- When `false` (default), CORS uses specific allowed origins
- Added `127.0.0.1` to allowed origins list
- Added startup logging to show active CORS policy

**Usage**:
```bash
# For development/debugging - allow all origins
export RACORE_PERMISSIVE_CORS=true
dotnet run --project RaCore

# For production - use specific origins (default)
# Don't set the variable or set it to false
dotnet run --project RaCore
```

#### Kestrel Binding Configuration
**Problem**: Binding configuration was unclear and may not have been listening on all interfaces.

**Solution**:
- Changed binding from `http://*:{port}` to `http://0.0.0.0:{port}` for clarity
- Added comprehensive startup logging showing:
  - Binding address (0.0.0.0)
  - Port number
  - Available access URLs (localhost, 127.0.0.1, external IP)
  - Firewall reminder

**Output Example**:
```
[RaCore] Configuring Kestrel to listen on: http://0.0.0.0:80
[RaCore] This will bind to ALL network interfaces (0.0.0.0)
[RaCore] CORS: Allowing specific origins: http://localhost:80, http://localhost, ...
[RaCore] Kestrel webserver starting...
[RaCore] Server will be accessible at:
  - http://localhost:80
  - http://127.0.0.1:80
  - http://<your-server-ip>:80
[RaCore] Ensure firewall allows inbound connections on port 80
```

### 2. Code Refactoring - Modular Endpoint Organization

#### Before
- **Program.cs**: 3354 lines
- All API endpoints defined inline
- Difficult to navigate and maintain
- Hard to debug specific API groups

#### After
- **Program.cs**: 2830 lines (15.6% reduction)
- Endpoint groups extracted to dedicated modules
- Clear separation of concerns
- Easier to debug and extend

#### Refactored Modules

##### `RaCore/Endpoints/AuthEndpoints.cs`
Contains all authentication-related API endpoints:
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/validate` - Token validation
- `GET /api/auth/events` - Security events (admin only)

**Usage in Program.cs**:
```csharp
app.MapAuthEndpoints(authModule);
```

##### `RaCore/Endpoints/GameEngineEndpoints.cs`
Contains all game engine-related API endpoints:
- Scene management (create, list, get, delete)
- Entity management (create, list)
- World generation
- Engine statistics
- In-game chat (for Legendary Game Engine)

**Usage in Program.cs**:
```csharp
app.MapGameEngineEndpoints(gameEngineModule, authModule);
```

### 3. Troubleshooting Guide

#### Cannot Access API from External Network

**Symptoms**:
- API works from localhost but not from external IP
- Connection timeout or refused errors
- CORS errors in browser console

**Diagnostic Steps**:

1. **Check if Kestrel is listening on all interfaces**
   ```bash
   # Look for this in startup logs:
   [RaCore] Configuring Kestrel to listen on: http://0.0.0.0:80
   ```

2. **Check firewall settings**
   ```bash
   # On Linux
   sudo ufw status
   sudo ufw allow 80/tcp
   
   # On Windows
   netsh advfirewall firewall show rule name=all | findstr 80
   ```

3. **Test from within server**
   ```bash
   curl http://localhost:80/api/auth/validate
   curl http://127.0.0.1:80/api/auth/validate
   ```

4. **Test from external network**
   ```bash
   curl http://<server-ip>:80/api/auth/validate
   ```

5. **Enable permissive CORS for debugging**
   ```bash
   export RACORE_PERMISSIVE_CORS=true
   # Restart application
   ```

#### CORS Errors

**Symptoms**:
- Browser console shows "CORS policy blocked" errors
- API works with curl but not from browser

**Solutions**:

1. **Temporary (Debug)**: Enable permissive CORS
   ```bash
   export RACORE_PERMISSIVE_CORS=true
   ```

2. **Permanent (Production)**: Add your origin to allowed origins in `Program.cs`
   ```csharp
   var allowedOrigins = new List<string>
   {
       $"http://localhost:{port}",
       "http://localhost",
       $"http://127.0.0.1:{port}",
       "http://127.0.0.1",
       "http://agpstudios.online",
       "https://agpstudios.online",
       "http://your-domain.com",  // Add your domain
       "https://your-domain.com"
   };
   ```

#### Port Already in Use

**Symptoms**:
- "Address already in use" error on startup
- Application fails to start

**Solutions**:

1. **Find what's using the port**
   ```bash
   # Linux
   sudo lsof -i :80
   sudo netstat -tulpn | grep :80
   
   # Windows
   netstat -ano | findstr :80
   ```

2. **Change RaCore port**
   ```bash
   export RACORE_PORT=8080
   # Or
   export RACORE_DETECTED_PORT=8080
   ```

3. **Stop conflicting service**
   ```bash
   # Common culprits: IIS, Apache, Nginx, other web servers
   sudo systemctl stop apache2
   sudo systemctl stop nginx
   ```

#### Endpoints Not Responding

**Symptoms**:
- Specific API endpoints return 404
- Some endpoints work, others don't

**Diagnostic Steps**:

1. **Check module registration**
   ```bash
   # Look for these in startup logs:
   [RaCore] Authentication API endpoints registered:
   [RaCore] Game Engine API endpoints registered:
   ```

2. **Verify module is loaded**
   - Ensure required modules are in the Modules directory
   - Check for module loading errors in startup logs

3. **Check authentication**
   - Many endpoints require authentication
   - Include `Authorization: Bearer <token>` header
   - Use `/api/auth/login` to get a token first

4. **Check permissions**
   - Some endpoints require specific roles (Admin, GameMaster, etc.)
   - Check user role with `/api/auth/validate`

### 4. Architecture Benefits

#### Easier Debugging
- Isolated endpoint groups make it easy to find specific API logic
- Each module can be tested independently
- Clear separation of concerns

#### Better Maintainability
- Smaller files are easier to understand
- Related functionality grouped together
- Extension method pattern allows for clean registration

#### Easier Extension
- Add new endpoint groups by creating new endpoint modules
- Follow the pattern established by existing modules
- No need to modify Program.cs for new endpoint groups

### 5. Future Work

The following sections remain in Program.cs and could be further refactored:

1. **ServerSetup Endpoints** (~250 lines)
   - Environment discovery
   - Admin setup
   - PHP configuration

2. **GameServer Endpoints** (~480 lines)
   - Game creation and deployment
   - Game server management

3. **Control Panel Endpoints** (~1600 lines)
   - Server configuration
   - Under construction mode
   - Forum moderation
   - Blog API
   - Chat API
   - Social profiles
   - Supermarket
   - System health monitoring
   - Audit logs

4. **Distribution & Update Endpoints** (~150 lines)
   - Distribution management
   - Update management

5. **GameClient Endpoints** (~300 lines)
   - Client generation
   - Template management

**Recommended Next Steps**:
- Extract ServerSetup, GameServer, and Distribution endpoints
- Split large Control Panel section into sub-modules (Blog, Chat, Social, etc.)
- Create integration tests for all endpoint modules
- Document API endpoints with OpenAPI/Swagger

## Testing

Run the endpoint registration tests:
```bash
dotnet run --project RaCore/Tests
# Or run specific test
dotnet run --project RaCore -- --test ApiEndpointRegistrationTests
```

## Conclusion

These changes address the core issues:
1. ✅ **API Communication**: Fixed CORS and binding configuration with better logging
2. ✅ **Maintainability**: Reduced Program.cs by 15.6% with modular endpoint organization
3. ✅ **Debuggability**: Added comprehensive logging and isolated endpoint groups
4. ✅ **No Features Lost**: All endpoints preserved and tested

The refactored codebase is now easier to debug, maintain, and extend while providing better visibility into network configuration issues.
