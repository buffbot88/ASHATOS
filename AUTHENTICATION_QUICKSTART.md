# RaCore Authentication - Quick Start Guide

## Overview

This guide will help you quickly integrate and use the RaCore authentication system.

---

## Installation & Setup

### 1. Run RaCore

```bash
cd RaCore
dotnet run
```

The server will start on `http://localhost:7077`

### 2. Access the Web Interface

- **Login Page**: http://localhost:7077/login.html
- **Admin Dashboard**: http://localhost:7077/admin.html
- **Root URL**: http://localhost:7077 (redirects to login)

### 3. Default Credentials

```
Username: admin
Password: admin123
Role: SuperAdmin
```

**⚠️ IMPORTANT: Change this password immediately after first login!**

---

## Using the Authentication Module

### From Code (C#)

```csharp
// Get the authentication module
IAuthenticationModule? authModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IAuthenticationModule>()
    .FirstOrDefault();

// Register a new user
var registerRequest = new RegisterRequest
{
    Username = "newuser",
    Email = "user@example.com",
    Password = "SecurePassword123"
};

var registerResponse = await authModule.RegisterAsync(registerRequest, "127.0.0.1");
if (registerResponse.Success)
{
    Console.WriteLine($"User registered: {registerResponse.User.Username}");
}

// Login
var loginRequest = new LoginRequest
{
    Username = "newuser",
    Password = "SecurePassword123"
};

var loginResponse = await authModule.LoginAsync(loginRequest, "127.0.0.1", "UserAgent");
if (loginResponse.Success)
{
    string token = loginResponse.Token;
    Console.WriteLine($"Login successful! Token: {token}");
}

// Validate token
var session = await authModule.ValidateTokenAsync(token);
if (session != null)
{
    var user = await authModule.GetUserByTokenAsync(token);
    Console.WriteLine($"Valid session for: {user.Username}");
}

// Check permissions
bool hasAccess = authModule.HasPermission(user, "SensitiveModule", UserRole.Admin);
if (hasAccess)
{
    // Allow access
}

// Logout
bool loggedOut = await authModule.LogoutAsync(token);
```

### From Module Commands

The Authentication module responds to these commands:

```
help   - Show help information
stats  - Show authentication statistics
events - Show recent security events
```

Example via WebSocket or direct module call:
```csharp
var response = await authModule.ProcessAsync("stats");
Console.WriteLine(response.Text); // JSON with user/session counts
```

---

## API Integration

### Register User

```bash
curl -X POST http://localhost:7077/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123"
  }'
```

Response:
```json
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "id": "guid",
    "username": "testuser",
    "email": "test@example.com",
    "role": 0
  }
}
```

### Login

```bash
curl -X POST http://localhost:7077/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "SecurePass123"
  }'
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "token": "base64-token-here",
  "user": {...},
  "tokenExpiresAt": "2025-10-06T03:00:00Z"
}
```

### Validate Token

```bash
curl -X POST http://localhost:7077/api/auth/validate \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

Response:
```json
{
  "valid": true,
  "user": {
    "username": "testuser",
    "email": "test@example.com",
    "role": 0
  },
  "expiresAt": "2025-10-06T03:00:00Z"
}
```

### Logout

```bash
curl -X POST http://localhost:7077/api/auth/logout \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

Response:
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

### Get Security Events (Admin Only)

```bash
curl -X GET http://localhost:7077/api/auth/events \
  -H "Authorization: Bearer ADMIN_TOKEN_HERE"
```

Response:
```json
{
  "success": true,
  "events": [
    {
      "id": "guid",
      "timestampUtc": "2025-10-05T03:00:00Z",
      "type": 1,
      "username": "admin",
      "userId": "guid",
      "ipAddress": "127.0.0.1",
      "details": "Login successful",
      "success": true
    }
  ]
}
```

---

## JavaScript Integration

### Login from Web Page

```javascript
async function login(username, password) {
    const response = await fetch('http://localhost:7077/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ username, password })
    });
    
    const data = await response.json();
    
    if (data.success) {
        localStorage.setItem('authToken', data.token);
        console.log('Login successful!');
        return data.token;
    } else {
        console.error('Login failed:', data.message);
        return null;
    }
}

// Usage
const token = await login('admin', 'admin123');
```

### Make Authenticated Requests

```javascript
async function makeAuthenticatedRequest(endpoint) {
    const token = localStorage.getItem('authToken');
    
    const response = await fetch(`http://localhost:7077${endpoint}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    
    return await response.json();
}

// Usage
const validationResult = await makeAuthenticatedRequest('/api/auth/validate');
if (validationResult.valid) {
    console.log('Token is valid!');
}
```

---

## Protecting Module Access

### In Your Module

```csharp
public async Task<ModuleResponse> ProcessAsync(string input)
{
    // Extract token from input (you'll need to design your token passing)
    var token = ExtractTokenFromInput(input);
    
    // Get auth module
    var authModule = Manager.Modules
        .Select(m => m.Instance)
        .OfType<IAuthenticationModule>()
        .FirstOrDefault();
    
    if (authModule == null)
    {
        return new ModuleResponse
        {
            Text = "Authentication not available",
            Status = "error"
        };
    }
    
    // Validate token
    var user = await authModule.GetUserByTokenAsync(token);
    if (user == null)
    {
        return new ModuleResponse
        {
            Text = "Unauthorized: Invalid or expired token",
            Status = "error"
        };
    }
    
    // Check permissions (require Admin role for this module)
    if (!authModule.HasPermission(user, Name, UserRole.Admin))
    {
        return new ModuleResponse
        {
            Text = "Forbidden: Insufficient permissions",
            Status = "error"
        };
    }
    
    // User is authenticated and authorized - process request
    return ProcessAuthorizedRequest(input, user);
}
```

---

## Security Events

### Event Types

| Type | Description |
|------|-------------|
| `LoginAttempt` | User attempted to log in |
| `LoginSuccess` | Successful login |
| `LoginFailure` | Failed login attempt |
| `Logout` | User logged out |
| `RegistrationAttempt` | User attempted to register |
| `RegistrationSuccess` | Successful registration |
| `RegistrationFailure` | Failed registration |
| `PasswordChange` | Password was changed |
| `SessionExpired` | Session expired |
| `SessionInvalidated` | Session manually invalidated |
| `UnauthorizedAccess` | Unauthorized access attempt |
| `PermissionDenied` | Permission check failed |

### Monitoring Events

```csharp
var events = await authModule.GetSecurityEventsAsync(100);
foreach (var evt in events)
{
    Console.WriteLine($"[{evt.TimestampUtc}] {evt.Type}: {evt.Username} - {evt.Success}");
    if (!evt.Success)
    {
        Console.WriteLine($"  Failed: {evt.Details}");
    }
}
```

---

## User Roles

### Role Hierarchy

1. **User** (Role 0) - Standard user
   - Basic access to public modules
   - Can view own data
   
2. **Admin** (Role 1) - Administrator
   - Access to admin functions
   - Can view security events
   - Module management
   
3. **SuperAdmin** (Role 2) - Super Administrator
   - Unrestricted access
   - Full system control
   - User management

### Checking Permissions

```csharp
// User can access if they have at least User role
bool canAccess = authModule.HasPermission(user, "PublicModule", UserRole.User);

// Admin required
bool isAdmin = authModule.HasPermission(user, "AdminModule", UserRole.Admin);

// SuperAdmin only
bool isSuperAdmin = user.Role == UserRole.SuperAdmin;
```

---

## Troubleshooting

### Issue: "Authentication not available"
**Solution**: Ensure the Authentication module is loaded. Check console output for:
```
[Module:Authentication] INFO: Authentication module initialized with secure password hashing (PBKDF2)
```

### Issue: Token validation fails
**Solution**: 
- Check token hasn't expired (24 hours)
- Verify Authorization header format: `Bearer TOKEN`
- Ensure token is valid and hasn't been logged out

### Issue: "Insufficient permissions"
**Solution**: 
- Check user role with `/api/auth/validate`
- Ensure user has required role for the operation
- Default admin has SuperAdmin (role 2)

### Issue: Can't access admin dashboard
**Solution**:
- Login first at `/login.html`
- Ensure user has at least Admin role (role 1+)
- Check browser console for errors

---

## Production Deployment Checklist

- [ ] Change default admin password
- [ ] Enable HTTPS with valid certificates
- [ ] Configure CORS for production domains
- [ ] Set up persistent storage (database)
- [ ] Configure distributed caching for sessions
- [ ] Enable rate limiting
- [ ] Set up security monitoring and alerts
- [ ] Configure log aggregation
- [ ] Regular security audits
- [ ] Backup and recovery procedures

---

## Additional Resources

- **Full Documentation**: `SECURITY_ARCHITECTURE.md`
- **Module README**: `RaCore/Modules/Extensions/Authentication/README.md`
- **OWASP Guidelines**: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- **NIST Guidelines**: https://pages.nist.gov/800-63-3/sp800-63b.html

---

## Support

For issues or questions:
1. Check security event logs in admin dashboard
2. Review console output for error messages
3. Verify API responses for error details
4. Consult the comprehensive documentation

---

**Last Updated**: 2025-01-13  
**Version**: v4.8.9
