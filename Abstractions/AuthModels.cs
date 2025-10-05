namespace Abstractions;

/// <summary>
/// Represents a user in the RaCore authentication system.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// User roles for access control.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1,
    SuperAdmin = 2
}

/// <summary>
/// Represents an active session in the system.
/// </summary>
public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
}

/// <summary>
/// Represents a security event for audit logging.
/// </summary>
public class SecurityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public SecurityEventType Type { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool Success { get; set; }
}

/// <summary>
/// Types of security events.
/// </summary>
public enum SecurityEventType
{
    LoginAttempt,
    LoginSuccess,
    LoginFailure,
    Logout,
    RegistrationAttempt,
    RegistrationSuccess,
    RegistrationFailure,
    PasswordChange,
    SessionExpired,
    SessionInvalidated,
    UnauthorizedAccess,
    PermissionDenied
}

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response model for authentication operations.
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public User? User { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
}
