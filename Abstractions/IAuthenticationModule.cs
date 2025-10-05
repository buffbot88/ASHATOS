namespace Abstractions;

/// <summary>
/// Interface for authentication module functionality.
/// </summary>
public interface IAuthenticationModule : IDisposable
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress = "");
    
    /// <summary>
    /// Authenticate a user and create a session.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress = "", string userAgent = "");
    
    /// <summary>
    /// End a user's session.
    /// </summary>
    Task<bool> LogoutAsync(string token);
    
    /// <summary>
    /// Validate a session token.
    /// </summary>
    Task<Session?> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Get user by token.
    /// </summary>
    Task<User?> GetUserByTokenAsync(string token);
    
    /// <summary>
    /// Check if user has permission for a module/action.
    /// </summary>
    bool HasPermission(User user, string moduleName, UserRole requiredRole = UserRole.User);
    
    /// <summary>
    /// Check if user has both role permission AND valid license.
    /// </summary>
    bool HasLicensePermission(User user, string moduleName, UserRole requiredRole = UserRole.User);
    
    /// <summary>
    /// Get recent security events.
    /// </summary>
    Task<List<SecurityEvent>> GetSecurityEventsAsync(int limit = 100);
    
    /// <summary>
    /// Log a security event.
    /// </summary>
    Task LogSecurityEventAsync(SecurityEvent securityEvent);
}
