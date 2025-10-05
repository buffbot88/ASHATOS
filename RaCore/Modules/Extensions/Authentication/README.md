# RaCore Authentication Module

## Overview

The Authentication module provides robust security and user authentication features for the RaCore AI mainframe. It implements modern password hashing, session management, role-based access control, and comprehensive security event logging.

---

## Directory Structure

```
Extensions/
├── Authentication/
│   ├── AuthenticationModule.cs          # Core authentication module
│   ├── README.md                        # This documentation
```

---

## Key Features

### Secure Password Management
- **PBKDF2 with SHA512**: Industry-standard password hashing
- **100,000 iterations**: High computational cost to resist brute-force attacks
- **256-bit salt**: Unique salt per user prevents rainbow table attacks
- **512-bit hash**: Strong cryptographic hash for maximum security

### Session Management
- **Secure token generation**: 512-bit random tokens for session identification
- **24-hour expiry**: Automatic session expiration
- **Session validation**: Token validation with expiry checking
- **Session invalidation**: Clean logout with session termination

### Role-Based Access Control (RBAC)
- **User roles**: User, Admin, SuperAdmin
- **Permission hierarchy**: Higher roles inherit lower role permissions
- **Module-level access control**: Integration ready for module permissions
- **Future OAuth2 ready**: Architecture supports future OAuth2 integration

### Security Event Logging
- **Comprehensive logging**: All authentication events tracked
- **Audit trail**: Login attempts, registrations, password changes
- **Failed attempt tracking**: Security incident detection
- **Event retention**: Last 1000 events kept in memory

---

## API Highlights

### Module Commands
- `help` - Display help information
- `stats` - Show authentication statistics (users, sessions, events)
- `events` - Show recent security events

### IAuthenticationModule Interface
- `RegisterAsync(request, ipAddress)` - Register new user
- `LoginAsync(request, ipAddress, userAgent)` - Authenticate and create session
- `LogoutAsync(token)` - End user session
- `ValidateTokenAsync(token)` - Validate session token
- `GetUserByTokenAsync(token)` - Get user from token
- `HasPermission(user, moduleName, requiredRole)` - Check permissions
- `GetSecurityEventsAsync(limit)` - Retrieve security events
- `LogSecurityEventAsync(event)` - Log security event

---

## Integration

### With ModuleManager
The Authentication module integrates seamlessly with RaCore's ModuleManager:

```csharp
var manager = new ModuleManager();
manager.LoadModules();

// Get authentication module
var authModule = manager.Modules
    .Select(m => m.Instance)
    .OfType<IAuthenticationModule>()
    .FirstOrDefault();
```

### With HTTP API Endpoints
Create REST API endpoints for authentication:

```csharp
// Register endpoint
app.MapPost("/api/auth/register", async (RegisterRequest req, HttpContext ctx) =>
{
    var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    var response = await authModule.RegisterAsync(req, ipAddress);
    return Results.Json(response);
});

// Login endpoint
app.MapPost("/api/auth/login", async (LoginRequest req, HttpContext ctx) =>
{
    var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    var userAgent = ctx.Request.Headers["User-Agent"].ToString();
    var response = await authModule.LoginAsync(req, ipAddress, userAgent);
    return Results.Json(response);
});

// Logout endpoint
app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    var token = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var success = await authModule.LogoutAsync(token);
    return Results.Json(new { success });
});

// Validate token endpoint
app.MapPost("/api/auth/validate", async (HttpContext ctx) =>
{
    var token = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var session = await authModule.ValidateTokenAsync(token);
    return Results.Json(new { valid = session != null });
});
```

### With Other Modules
Modules can check user permissions:

```csharp
var user = await authModule.GetUserByTokenAsync(token);
if (user != null && authModule.HasPermission(user, "SensitiveModule", UserRole.Admin))
{
    // Allow access
}
```

---

## Security Events

### Event Types
- `LoginAttempt` - User attempted to log in
- `LoginSuccess` - Successful login
- `LoginFailure` - Failed login attempt
- `Logout` - User logged out
- `RegistrationAttempt` - User attempted to register
- `RegistrationSuccess` - Successful registration
- `RegistrationFailure` - Failed registration
- `PasswordChange` - Password was changed
- `SessionExpired` - Session expired
- `SessionInvalidated` - Session manually invalidated
- `UnauthorizedAccess` - Unauthorized access attempt
- `PermissionDenied` - Permission check failed

### Monitoring Security Events
```csharp
var events = await authModule.GetSecurityEventsAsync(100);
foreach (var evt in events)
{
    Console.WriteLine($"{evt.TimestampUtc}: {evt.Type} - {evt.Username} - Success: {evt.Success}");
}
```

---

## Default Credentials

**⚠️ IMPORTANT: Change these credentials immediately after first deployment!**

- **Username**: `admin`
- **Password**: `admin123`
- **Role**: SuperAdmin

---

## Extending

### Custom User Properties
Add custom properties to the `User` model in `Abstractions/AuthModels.cs`:

```csharp
public class User
{
    // ... existing properties ...
    public string CustomField { get; set; } = string.Empty;
}
```

### Additional Security Events
Add new event types to `SecurityEventType` enum:

```csharp
public enum SecurityEventType
{
    // ... existing types ...
    TwoFactorEnabled,
    TwoFactorDisabled
}
```

### OAuth2 Integration (Future)
The authentication module is designed to support OAuth2:
- Add OAuth2 provider configuration
- Implement external authentication flows
- Link external accounts to local users
- Support social login (Google, GitHub, etc.)

---

## Best Practices

### Password Requirements
- Minimum 8 characters (enforced)
- Recommend: Mix of uppercase, lowercase, numbers, and symbols
- Consider implementing password strength meter in UI

### Session Management
- Always use HTTPS in production
- Store tokens securely (HTTP-only cookies recommended)
- Implement CSRF protection for web applications
- Regular token rotation for long-lived sessions

### Security Logging
- Monitor for failed login attempts
- Alert on suspicious activity patterns
- Regular audit of security events
- Export logs for long-term storage

### Access Control
- Follow principle of least privilege
- Regular review of user roles
- Implement multi-factor authentication (future enhancement)

---

## Compliance

### OWASP Guidelines
This implementation follows OWASP Authentication Cheat Sheet recommendations:
- Strong password hashing (PBKDF2 with SHA512)
- Secure session management
- Protection against brute force attacks (via logging)
- Secure credential storage (never storing plaintext passwords)

### Standards Compatibility
- **NIST SP 800-63B**: Password security guidelines
- **.NET Security Best Practices**: Modern cryptographic APIs
- **CWE-257, CWE-259, CWE-798**: Secure credential storage

---

## Example Usage

### User Registration
```csharp
var request = new RegisterRequest
{
    Username = "newuser",
    Email = "user@example.com",
    Password = "SecureP@ssw0rd"
};

var response = await authModule.RegisterAsync(request, "192.168.1.1");
if (response.Success)
{
    Console.WriteLine($"User {response.User.Username} registered successfully");
}
```

### User Login
```csharp
var request = new LoginRequest
{
    Username = "newuser",
    Password = "SecureP@ssw0rd"
};

var response = await authModule.LoginAsync(request, "192.168.1.1", "Mozilla/5.0...");
if (response.Success)
{
    Console.WriteLine($"Token: {response.Token}");
    Console.WriteLine($"Expires: {response.TokenExpiresAt}");
}
```

### Permission Check
```csharp
var user = await authModule.GetUserByTokenAsync(token);
if (user != null)
{
    var canAccessAdmin = authModule.HasPermission(user, "AdminModule", UserRole.Admin);
    Console.WriteLine($"Admin access: {canAccessAdmin}");
}
```

---

## Contributors

Document new authentication features, security enhancements, and OAuth2 integration progress here.

---

**Last Updated:** 2025-01-05  
**Version:** 1.0
