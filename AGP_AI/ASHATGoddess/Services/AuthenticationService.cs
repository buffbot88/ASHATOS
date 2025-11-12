using System.Net.Http.Json;
using System.Text.Json;

namespace ASHATGoddessClient.Services
{
    /// <summary>
    /// Authentication service for ASHAT Goddess client that communicates with phpBB3.
    /// Handles user login, registration, and session management via the ASHATOS Authentication Bridge extension.
    /// </summary>
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phpbbBaseUrl;
        private string? _currentSessionId;
        private UserInfo? _currentUser;

        public bool IsAuthenticated => _currentSessionId != null && _currentUser != null;
        public UserInfo? CurrentUser => _currentUser;

        public AuthenticationService(string phpbbBaseUrl = "http://localhost/phpbb")
        {
            _phpbbBaseUrl = phpbbBaseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_phpbbBaseUrl)
            };
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
                {
                    username,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
                        _currentSessionId = result.SessionId;
                        _currentUser = new UserInfo
                        {
                            Id = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role
                        };

                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = result.SessionId,
                            User = _currentUser,
                            Message = result.Message ?? "Login successful"
                        };
                    }
                }

                var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return new AuthenticationResult
                {
                    Success = false,
                    Message = errorResult?.Message ?? "Login failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Authentication error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new
                {
                    username,
                    email,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
                        _currentSessionId = result.SessionId;
                        _currentUser = new UserInfo
                        {
                            Id = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role
                        };

                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = result.SessionId,
                            User = _currentUser,
                            Message = result.Message ?? "Registration successful"
                        };
                    }
                }

                var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return new AuthenticationResult
                {
                    Success = false,
                    Message = errorResult?.Message ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Registration error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Validate an existing session
        /// </summary>
        public async Task<AuthenticationResult> ValidateSessionAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/validate", new
                {
                    sessionId
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ValidateResponse>();
                    if (result?.Success == true && result.User != null)
                    {
                        _currentSessionId = sessionId;
                        _currentUser = new UserInfo
                        {
                            Id = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role
                        };

                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = sessionId,
                            User = _currentUser,
                            Message = "Session is valid"
                        };
                    }
                }

                _currentSessionId = null;
                _currentUser = null;
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid or expired session"
                };
            }
            catch (Exception ex)
            {
                _currentSessionId = null;
                _currentUser = null;
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Session validation error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public void Logout()
        {
            _currentSessionId = null;
            _currentUser = null;
        }

        // Response models
        private class LoginResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? SessionId { get; set; }
            public UserDto? User { get; set; }
        }

        private class ValidateResponse
        {
            public bool Success { get; set; }
            public UserDto? User { get; set; }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private class ErrorResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }

    /// <summary>
    /// Result of an authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// User information
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
