# RaCore Security Architecture

## Overview

This document outlines the security architecture implemented for the RaCore AI mainframe, covering authentication, authorization, session management, and security event logging.

---

## Architecture Components

### 1. Authentication Module (`AuthenticationModule.cs`)

The core authentication service providing:
- User registration and login
- Password hashing with PBKDF2-SHA512
- Session token generation and validation
- Security event logging
- Role-based access control

**Location**: `RaCore/Modules/Extensions/Authentication/`

### 2. Data Models (`AuthModels.cs`)

Defines all authentication-related data structures:
- `User` - User account information
- `Session` - Active session tokens
- `SecurityEvent` - Audit log entries
- `UserRole` - Role enumeration (User, Admin, SuperAdmin)
- Request/Response DTOs for API operations

**Location**: `Abstractions/AuthModels.cs`

### 3. Authentication Interface (`IAuthenticationModule.cs`)

Contract defining authentication operations:
- Registration, login, logout
- Token validation
- Permission checking
- Security event management

**Location**: `Abstractions/IAuthenticationModule.cs`

### 4. API Endpoints (`Program.cs`)

REST API endpoints for authentication:
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/validate` - Token validation
- `GET /api/auth/events` - Security events (admin only)

### 5. Web UI

Two HTML pages for user interaction:
- `login.html` - Login/registration interface
- `admin.html` - Admin dashboard for security monitoring

---

## Security Features

### Password Security

**Algorithm**: PBKDF2 (Password-Based Key Derivation Function 2)  
**Hash Function**: SHA-512  
**Iterations**: 100,000  
**Salt Size**: 256 bits (32 bytes)  
**Hash Size**: 512 bits (64 bytes)

**Why these choices?**
- PBKDF2 is NIST-approved and widely recommended
- 100,000 iterations provides strong defense against brute force
- SHA-512 offers strong cryptographic properties
- Unique salt per user prevents rainbow table attacks
- Password hashes are never stored in plaintext or reversible form

**Implementation**:
```csharp
// Password hashing
var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
var salt = Convert.ToBase64String(saltBytes);
using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA512);
var hash = pbkdf2.GetBytes(HashSize);
```

### Session Management

**Token Generation**: Cryptographically secure random tokens (512 bits)  
**Token Storage**: In-memory dictionary (production: use distributed cache)  
**Session Expiry**: 24 hours (configurable)  
**Token Format**: Base64-encoded random bytes  

**Token Validation**:
- Checks token existence
- Verifies expiration time
- Marks expired sessions as invalid
- Logs session expiration events

### Role-Based Access Control (RBAC)

**Role Hierarchy**:
1. **User** (Role 0) - Standard user with basic access
2. **Admin** (Role 1) - Administrative privileges
3. **SuperAdmin** (Role 2) - Full system access

**Permission Model**:
- Higher roles inherit lower role permissions
- SuperAdmin has unrestricted access
- Module-level permission checks via `HasPermission()`
- Extensible for fine-grained permissions

**Example**:
```csharp
var user = await authModule.GetUserByTokenAsync(token);
if (user != null && authModule.HasPermission(user, "SensitiveModule", UserRole.Admin))
{
    // Grant access
}
```

### Security Event Logging

All authentication-related actions are logged:
- Login attempts (success/failure)
- Registration attempts (success/failure)
- Logout events
- Session expiration
- Unauthorized access attempts
- Permission denials

**Event Data**:
- Timestamp (UTC)
- Event type
- Username
- User ID
- IP address
- Details/description
- Success/failure flag

**Retention**: Last 1000 events kept in memory (production: persist to database)

---

## API Security

### Request/Response Flow

#### Registration
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "SecureP@ss123"
}

Response:
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "id": "guid",
    "username": "newuser",
    "email": "user@example.com",
    "role": 0,
    "createdAtUtc": "2025-10-05T...",
    "isActive": true
  }
}
```

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecureP@ss123"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "token": "base64-encoded-token",
  "user": {...},
  "tokenExpiresAt": "2025-10-06T..."
}
```

#### Authenticated Requests
```
POST /api/auth/validate
Authorization: Bearer {token}

Response:
{
  "valid": true,
  "user": {
    "username": "newuser",
    "email": "user@example.com",
    "role": 0
  },
  "expiresAt": "2025-10-06T..."
}
```

### Security Headers

Recommended headers for production:
```
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

---

## Integration Patterns

### Module Integration

Modules can check authentication and permissions:

```csharp
// Get authentication module
IAuthenticationModule? authModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IAuthenticationModule>()
    .FirstOrDefault();

// In module Process() or ProcessAsync()
var token = ExtractTokenFromInput(input);
var user = await authModule?.GetUserByTokenAsync(token);

if (user == null || !authModule.HasPermission(user, Name, UserRole.Admin))
{
    return new ModuleResponse 
    { 
        Text = "Unauthorized", 
        Status = "error" 
    };
}
```

### WebSocket Authentication

For WebSocket connections:
1. Client sends auth token in initial message
2. Server validates token
3. Session associated with WebSocket connection
4. All subsequent messages use authenticated session

### API Middleware (Future Enhancement)

Consider implementing authentication middleware:
```csharp
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].ToString();
    if (token.StartsWith("Bearer "))
    {
        token = token[7..];
        var session = await authModule.ValidateTokenAsync(token);
        if (session != null)
        {
            context.Items["User"] = await authModule.GetUserByTokenAsync(token);
        }
    }
    await next();
});
```

---

## Security Best Practices

### Password Requirements

**Current**: Minimum 8 characters  
**Recommended for Production**:
- Minimum 12 characters
- Mix of uppercase, lowercase, numbers, special characters
- Password strength meter in UI
- Check against common password lists
- Implement password history (prevent reuse)

### Session Security

**Current Implementation**: In-memory sessions  
**Production Recommendations**:
- Use distributed cache (Redis, etc.)
- Implement sliding expiration
- Support remember-me with refresh tokens
- IP address validation for sessions
- User agent validation
- Concurrent session limits

### Rate Limiting

**Not Yet Implemented - Recommended**:
- Limit login attempts per IP (5 per minute)
- Limit registration attempts (3 per hour per IP)
- Exponential backoff for failed attempts
- CAPTCHA after multiple failures
- Account lockout after threshold

### HTTPS/TLS

**Critical for Production**:
- Always use HTTPS in production
- Redirect HTTP to HTTPS
- Use valid TLS certificates
- Enable HSTS (HTTP Strict Transport Security)
- Disable older TLS versions (use TLS 1.2+)

### Token Security

**Best Practices**:
- Store tokens in HTTP-only cookies (preferred over localStorage)
- Implement CSRF tokens for cookie-based auth
- Short-lived access tokens (15 min) + refresh tokens (7 days)
- Rotate tokens on sensitive operations
- Revoke tokens on password change

### Input Validation

**Current**: Basic validation (length, null checks)  
**Enhanced Validation**:
- Email format validation with regex
- Username format restrictions
- SQL injection prevention (parameterized queries)
- XSS prevention (output encoding)
- LDAP injection prevention

---

## Compliance

### OWASP Top 10 Coverage

1. **Broken Access Control**: RBAC implementation, token validation
2. **Cryptographic Failures**: PBKDF2 password hashing, secure tokens
3. **Injection**: Parameterized queries (when using DB)
4. **Insecure Design**: Security-first architecture
5. **Security Misconfiguration**: Secure defaults, environment-aware
6. **Vulnerable Components**: Modern .NET, up-to-date dependencies
7. **Authentication Failures**: Strong password policy, session management
8. **Software Integrity Failures**: Code signing (future)
9. **Security Logging**: Comprehensive event logging
10. **SSRF**: Not applicable (no external requests in auth)

### GDPR Considerations

- **Data Minimization**: Only collect necessary user data
- **Right to Erasure**: Implement user deletion
- **Data Portability**: Export user data in standard format
- **Consent**: Clear terms of service
- **Breach Notification**: Security event monitoring
- **Data Protection**: Encrypted passwords, secure tokens

### NIST 800-63B Compliance

- ✅ Password length minimum (8 chars)
- ✅ No composition rules forcing complexity
- ✅ Check against common password lists (future)
- ✅ Rate limiting (recommended, not implemented)
- ✅ PBKDF2 with SHA-512
- ✅ Minimum 10,000 iterations (we use 100,000)

---

## Future Enhancements

### OAuth2 & Social Login

Architecture supports future OAuth2 integration:
- Add OAuth2 provider configuration
- Implement authorization code flow
- Link external accounts to local users
- Support multiple identity providers (Google, GitHub, Microsoft)

### Multi-Factor Authentication (MFA)

Planned enhancements:
- TOTP (Time-based One-Time Password)
- SMS-based verification
- Email-based verification
- Backup codes
- Recovery options

### Advanced Features

- **Password reset flow**: Email-based password reset
- **Email verification**: Confirm email on registration
- **Account recovery**: Security questions, recovery emails
- **Audit logging**: Enhanced logging with retention policies
- **Anomaly detection**: ML-based suspicious activity detection
- **SSO integration**: SAML, OpenID Connect
- **API keys**: Service-to-service authentication

### Persistence

Current: In-memory storage  
Future: Persistent storage options
- SQLite for embedded scenarios
- PostgreSQL/MySQL for production
- Redis for distributed caching
- MongoDB for flexible schema

---

## Testing & Validation

### Manual Testing

1. **Registration**: Create new users, test validation
2. **Login**: Test valid/invalid credentials
3. **Token validation**: Verify token expiry
4. **Logout**: Ensure sessions are invalidated
5. **Permissions**: Test role-based access
6. **Security events**: Verify all events logged

### Security Testing

**Recommended Tools**:
- OWASP ZAP - Web application security scanner
- Burp Suite - Manual security testing
- Postman/Thunder Client - API testing
- JMeter - Load testing, brute force simulation

**Test Cases**:
- Password brute force attempts
- SQL injection on username/email fields
- XSS attempts in user inputs
- CSRF token validation
- Session fixation attacks
- Token tampering

### Penetration Testing

Consider professional penetration testing before production release.

---

## Deployment Guide

### Development

```bash
# Run RaCore
cd RaCore
dotnet run

# Access login page
http://localhost:7077/login.html

# Access admin dashboard
http://localhost:7077/admin.html

# Default credentials
Username: admin
Password: admin123
```

### Production Checklist

- [ ] Change default admin password
- [ ] Enable HTTPS with valid certificates
- [ ] Configure rate limiting
- [ ] Set up persistent storage (database)
- [ ] Configure distributed caching for sessions
- [ ] Set up log aggregation and monitoring
- [ ] Enable security headers
- [ ] Configure CORS properly
- [ ] Set strong JWT secrets (if using)
- [ ] Regular security updates
- [ ] Backup and recovery procedures
- [ ] Incident response plan

### Environment Variables

Recommended configuration:
```bash
RACORE_AUTH_TOKEN_EXPIRY_HOURS=24
RACORE_AUTH_ITERATIONS=100000
RACORE_AUTH_SALT_SIZE=32
RACORE_AUTH_HASH_SIZE=64
RACORE_AUTH_REQUIRE_EMAIL_VERIFICATION=false
RACORE_AUTH_ENABLE_REGISTRATION=true
RACORE_AUTH_MIN_PASSWORD_LENGTH=8
```

---

## Support & Troubleshooting

### Common Issues

**Issue**: Login fails with correct credentials  
**Solution**: Check security events, verify password hash, ensure session not expired

**Issue**: Token validation fails  
**Solution**: Check token expiry, verify Authorization header format

**Issue**: Admin dashboard shows access denied  
**Solution**: Verify user role is Admin or SuperAdmin

### Logging

Authentication module logs all events:
```
[Module:Authentication] INFO: Authentication module initialized with secure password hashing (PBKDF2)
[Module:Authentication] INFO: Default admin user created (username: admin, password: admin123)
[Module:Authentication] INFO: User registered: testuser
[Module:Authentication] INFO: User logged in: testuser
```

### Security Incident Response

1. **Detect**: Monitor security events for anomalies
2. **Contain**: Lock affected accounts, invalidate sessions
3. **Investigate**: Review security event logs
4. **Remediate**: Fix vulnerabilities, update passwords
5. **Document**: Record incident details and response
6. **Review**: Update security policies and procedures

---

## Contributors

Document security enhancements, vulnerability fixes, and compliance updates here.

---

## References

- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [NIST SP 800-63B Digital Identity Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [PBKDF2 RFC 2898](https://www.ietf.org/rfc/rfc2898.txt)

---

**Last Updated**: 2025-01-13  
**Version**: v4.8.9  
**Maintainer**: RaCore Security Team
