using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Authentication;

/// <summary>
/// Authentication module for RaCore - handles user registration, login, sessions, and security logging.
/// Implements modern password hashing with PBKDF2 and session token management.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AuthenticationModule : ModuleBase, IAuthenticationModule
{
    public override string Name => "Authentication";

    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<string, Session> _sessions = new();
    private readonly List<SecurityEvent> _securityEvents = new();
    private readonly object _lock = new();
    private ILicenseModule? _licenseModule;
    
    // Security configuration
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 64; // 512 bits
    private const int Iterations = 100000; // PBKDF2 iterations
    private const int SessionExpiryHours = 24;
    private const int TokenLength = 64;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to license module
        if (manager is ModuleManager moduleManager)
        {
            _licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;
        }
        
        LogInfo("Authentication module initialized with secure password hashing (PBKDF2)");
        
        // Create default admin user if no users exist
        if (_users.Count == 0)
        {
            CreateDefaultAdminUser();
        }
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? string.Empty).Trim();
        
        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return new ModuleResponse
            {
                Text = GetHelp(),
                Type = "help",
                Status = "success"
            };
        }

        if (text.Equals("stats", StringComparison.OrdinalIgnoreCase))
        {
            var stats = new
            {
                TotalUsers = _users.Count,
                ActiveSessions = _sessions.Count(s => s.Value.IsValid && s.Value.ExpiresAtUtc > DateTime.UtcNow),
                SecurityEvents = _securityEvents.Count,
                RecentEvents = _securityEvents.TakeLast(10).Select(e => new
                {
                    e.Type,
                    e.Username,
                    e.Success,
                    e.TimestampUtc
                })
            };
            return new ModuleResponse
            {
                Text = JsonSerializer.Serialize(stats, _jsonOptions),
                Type = "stats",
                Status = "success"
            };
        }

        if (text.Equals("events", StringComparison.OrdinalIgnoreCase))
        {
            var events = await GetSecurityEventsAsync(50);
            return new ModuleResponse
            {
                Text = JsonSerializer.Serialize(events, _jsonOptions),
                Type = "events",
                Status = "success"
            };
        }

        return new ModuleResponse
        {
            Text = "Unknown command. Type 'help' for available commands.",
            Type = "error",
            Status = "error"
        };
    }

    #region IAuthenticationModule Implementation

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress = "")
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                LogSecurityEvent(SecurityEventType.RegistrationFailure, request.Username, null, ipAddress, "Invalid username or password", false);
                return new AuthResponse { Success = false, Message = "Username and password are required" };
            }

            if (request.Password.Length < 8)
            {
                LogSecurityEvent(SecurityEventType.RegistrationFailure, request.Username, null, ipAddress, "Password too short", false);
                return new AuthResponse { Success = false, Message = "Password must be at least 8 characters long" };
            }

            // Check if username already exists
            if (_users.Values.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            {
                LogSecurityEvent(SecurityEventType.RegistrationFailure, request.Username, null, ipAddress, "Username already exists", false);
                return new AuthResponse { Success = false, Message = "Username already exists" };
            }

            // Create user with hashed password
            var salt = GenerateSalt();
            var hash = HashPassword(request.Password, salt);
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.User,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            _users[user.Id] = user;
            
            LogSecurityEvent(SecurityEventType.RegistrationSuccess, user.Username, user.Id, ipAddress, "User registered successfully", true);
            LogInfo($"User registered: {user.Username}");

            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                User = SanitizeUser(user)
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress = "", string userAgent = "")
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            // Find user
            var user = _users.Values.FirstOrDefault(u => 
                u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
            {
                LogSecurityEvent(SecurityEventType.LoginFailure, request.Username, null, ipAddress, "User not found or inactive", false);
                return new AuthResponse { Success = false, Message = "Invalid username or password" };
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                LogSecurityEvent(SecurityEventType.LoginFailure, user.Username, user.Id, ipAddress, "Invalid password", false);
                return new AuthResponse { Success = false, Message = "Invalid username or password" };
            }

            // Check license for non-SuperAdmin users
            if (user.Role != UserRole.SuperAdmin && _licenseModule != null)
            {
                if (!_licenseModule.HasValidLicense(user))
                {
                    LogSecurityEvent(SecurityEventType.LicenseValidationFailure, user.Username, user.Id, ipAddress, "No valid license", false);
                    _ = _licenseModule.LogLicenseEventAsync(user.Id, "Login", "License validation failed", false);
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Access denied: Valid license required. Please contact sales to purchase a license." 
                    };
                }
            }

            // Create session
            var token = GenerateSecureToken();
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(SessionExpiryHours),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsValid = true
            };

            _sessions[token] = session;
            user.LastLoginUtc = DateTime.UtcNow;

            LogSecurityEvent(SecurityEventType.LoginSuccess, user.Username, user.Id, ipAddress, "Login successful", true);
            if (_licenseModule != null && user.Role != UserRole.SuperAdmin)
            {
                _ = _licenseModule.LogLicenseEventAsync(user.Id, "Login", "License validated successfully", true);
            }
            LogInfo($"User logged in: {user.Username}");

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = SanitizeUser(user),
                TokenExpiresAt = session.ExpiresAtUtc
            };
        }
    }

    public async Task<bool> LogoutAsync(string token)
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            if (_sessions.TryGetValue(token, out var session))
            {
                session.IsValid = false;
                var user = _users.GetValueOrDefault(session.UserId);
                LogSecurityEvent(SecurityEventType.Logout, user?.Username ?? "Unknown", session.UserId, "", "Logout successful", true);
                LogInfo($"User logged out: {user?.Username ?? "Unknown"}");
                return true;
            }
            return false;
        }
    }

    public async Task<Session?> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            if (_sessions.TryGetValue(token, out var session))
            {
                if (session.IsValid && session.ExpiresAtUtc > DateTime.UtcNow)
                {
                    return session;
                }
                else if (session.ExpiresAtUtc <= DateTime.UtcNow && session.IsValid)
                {
                    session.IsValid = false;
                    LogSecurityEvent(SecurityEventType.SessionExpired, "", session.UserId, "", "Session expired", false);
                }
            }
            return null;
        }
    }

    public async Task<User?> GetUserByTokenAsync(string token)
    {
        var session = await ValidateTokenAsync(token);
        if (session != null)
        {
            lock (_lock)
            {
                return _users.GetValueOrDefault(session.UserId);
            }
        }
        return null;
    }

    public bool HasPermission(User user, string moduleName, UserRole requiredRole = UserRole.User)
    {
        if (user == null || !user.IsActive)
            return false;

        // SuperAdmin has access to everything
        if (user.Role == UserRole.SuperAdmin)
            return true;

        // Check role hierarchy
        return user.Role >= requiredRole;
    }

    /// <summary>
    /// Check if user has both role permission AND valid license.
    /// SuperAdmin bypasses license check.
    /// </summary>
    public bool HasLicensePermission(User user, string moduleName, UserRole requiredRole = UserRole.User)
    {
        if (user == null || !user.IsActive)
            return false;

        // SuperAdmin has access to everything without license check
        if (user.Role == UserRole.SuperAdmin)
            return true;

        // Check role permission first
        if (user.Role < requiredRole)
            return false;

        // Check license
        if (_licenseModule != null)
        {
            if (!_licenseModule.HasValidLicense(user))
            {
                LogSecurityEvent(SecurityEventType.LicenseValidationFailure, user.Username, user.Id, "", 
                    $"License check failed for module: {moduleName}", false);
                return false;
            }
        }

        return true;
    }

    public async Task<List<SecurityEvent>> GetSecurityEventsAsync(int limit = 100)
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            return _securityEvents.TakeLast(limit).ToList();
        }
    }

    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        await Task.CompletedTask; // Async placeholder
        
        lock (_lock)
        {
            _securityEvents.Add(securityEvent);
            
            // Keep only last 1000 events to prevent memory bloat
            if (_securityEvents.Count > 1000)
            {
                _securityEvents.RemoveRange(0, _securityEvents.Count - 1000);
            }
        }
    }

    #endregion

    #region Password Hashing

    private static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA512);
        var hash = pbkdf2.GetBytes(HashSize);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var newHash = HashPassword(password, storedSalt);
        return newHash == storedHash;
    }

    private static string GenerateSecureToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(TokenLength);
        return Convert.ToBase64String(tokenBytes);
    }

    #endregion

    #region Helper Methods

    private void LogSecurityEvent(SecurityEventType type, string username, Guid? userId, string ipAddress, string details, bool success)
    {
        var securityEvent = new SecurityEvent
        {
            Type = type,
            Username = username,
            UserId = userId,
            IpAddress = ipAddress,
            Details = details,
            Success = success,
            TimestampUtc = DateTime.UtcNow
        };
        
        _securityEvents.Add(securityEvent);
        
        // Keep only last 1000 events
        if (_securityEvents.Count > 1000)
        {
            _securityEvents.RemoveRange(0, _securityEvents.Count - 1000);
        }
    }

    private static User SanitizeUser(User user)
    {
        return new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginUtc = user.LastLoginUtc,
            IsActive = user.IsActive,
            PasswordHash = "", // Never expose password hash
            PasswordSalt = ""  // Never expose salt
        };
    }

    private void CreateDefaultAdminUser()
    {
        var salt = GenerateSalt();
        var hash = HashPassword("admin123", salt); // Default password - should be changed
        
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@racore.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = UserRole.SuperAdmin,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        _users[admin.Id] = admin;
        LogInfo("Default admin user created (username: admin, password: admin123) - CHANGE THIS PASSWORD!");
    }

    private static string GetHelp() => @"
Authentication Module:
  - help         : Show this help
  - stats        : Show authentication statistics
  - events       : Show recent security events

API Integration:
  - Register: POST /api/auth/register with { username, email, password }
  - Login: POST /api/auth/login with { username, password }
  - Logout: POST /api/auth/logout with { token }
  - Validate: POST /api/auth/validate with { token }

Security Features:
  - PBKDF2 password hashing with SHA512 (100,000 iterations)
  - Secure session tokens (512-bit random)
  - Role-based access control (User, Admin, SuperAdmin)
  - Comprehensive security event logging
  - Session expiry (24 hours)

Default Admin:
  Username: admin
  Password: admin123
  ** CHANGE THIS PASSWORD IMMEDIATELY **
".Trim();

    #endregion

    public override void Dispose()
    {
        base.Dispose();
    }
}
