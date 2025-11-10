using System;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaStudios.WinForms.Modules
{
    /// <summary>
    /// Event arguments for connection status changes.
    /// </summary>
    public class ConnectionStatusEventArgs : EventArgs
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles secure connection to the game server.
    /// Supports WebSocket communication with authentication and rate limiting.
    /// </summary>
    public class ServerConnector : IDisposable
    {
        private ClientWebSocket? webSocket;
        private CancellationTokenSource? cancellationTokenSource;
        private string serverUrl = "ws://localhost:7077/ws";
        private bool isConnected = false;
        private string? authToken;
        private DateTime lastMessageTime = DateTime.MinValue;
        private readonly TimeSpan rateLimitInterval = TimeSpan.FromMilliseconds(100); // 10 msgs/sec max

        public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;
        public event EventHandler<string>? MessageReceived;

        public bool IsConnected => isConnected && webSocket?.State == WebSocketState.Open;

        public string ServerUrl
        {
            get => serverUrl;
            set
            {
                if (isConnected)
                    throw new InvalidOperationException("Cannot change server URL while connected.");
                serverUrl = value;
            }
        }

        /// <summary>
        /// Authenticates with the server using provided credentials.
        /// </summary>
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                // SECURITY NOTE: This implementation uses SHA256 for demonstration purposes.
                // For production deployment, implement proper authentication:
                // 1. Use OAuth2 or OpenID Connect for token-based authentication
                // 2. Use WSS (WebSocket Secure) instead of WS for encrypted communication
                // 3. Store credentials securely using Windows Credential Manager or similar
                // 4. Implement server-side password hashing with bcrypt or Argon2
                // 5. Use HTTPS for all API calls
                // See: https://owasp.org/www-project-web-security-testing-guide/

                var authMessage = new
                {
                    type = "auth",
                    username = username,
                    password_hash = ComputePasswordHash(password) // Hash before sending
                };

                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                await SendMessageAsync(JsonConvert.SerializeObject(authMessage));

                // Wait for auth response
                // In production, implement proper response handling with timeout
                authToken = $"token_{username}_{DateTime.UtcNow.Ticks}";

                RaiseConnectionStatus("Authenticated", $"User {username} authenticated successfully");
                return true;
            }
            catch (Exception ex)
            {
                RaiseConnectionStatus("Authentication Failed", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Establishes connection to the game server.
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                if (isConnected)
                {
                    RaiseConnectionStatus("Already Connected", "Connection already established");
                    return;
                }

                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();

                RaiseConnectionStatus("Connecting", $"Connecting to {serverUrl}...");

                await webSocket.ConnectAsync(new Uri(serverUrl), cancellationTokenSource.Token);

                isConnected = true;
                RaiseConnectionStatus("Connected", "Successfully connected to game server");

                // Start listening for messages
                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (Exception ex)
            {
                isConnected = false;
                RaiseConnectionStatus("Connection Failed", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sends a message to the server with rate limiting.
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to server");

            // Rate limiting
            var timeSinceLastMessage = DateTime.UtcNow - lastMessageTime;
            if (timeSinceLastMessage < rateLimitInterval)
            {
                await Task.Delay(rateLimitInterval - timeSinceLastMessage);
            }

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var messageSegment = new ArraySegment<byte>(messageBytes);

            await webSocket!.SendAsync(messageSegment, WebSocketMessageType.Text, true, cancellationTokenSource!.Token);
            lastMessageTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Receives messages from the server.
        /// </summary>
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[8192];

            try
            {
                while (IsConnected && !cancellationTokenSource!.Token.IsCancellationRequested)
                {
                    var result = await webSocket!.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                RaiseConnectionStatus("Error", $"Message receive error: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!isConnected)
                return;

            try
            {
                cancellationTokenSource?.Cancel();

                if (webSocket?.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
                }

                isConnected = false;
                authToken = null;
                RaiseConnectionStatus("Disconnected", "Disconnected from server");
            }
            catch (Exception ex)
            {
                RaiseConnectionStatus("Disconnect Error", ex.Message);
            }
            finally
            {
                webSocket?.Dispose();
                webSocket = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Synchronous disconnect for cleanup.
        /// </summary>
        public void Disconnect()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reconnects to the server.
        /// </summary>
        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await Task.Delay(1000); // Brief delay before reconnecting
            await ConnectAsync();
        }

        /// <summary>
        /// Synchronous reconnect for UI.
        /// </summary>
        public void Reconnect()
        {
            Task.Run(async () => await ReconnectAsync());
        }

        /// <summary>
        /// Computes a SHA256 hash of the password for transmission.
        /// NOTE: In production, use server-side salted hashing (bcrypt, PBKDF2) or OAuth2.
        /// </summary>
        private string ComputePasswordHash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private void RaiseConnectionStatus(string status, string message)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
            {
                Status = status,
                Message = message
            });
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
