using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ASHATGoddessClient.Networking;

/// <summary>
/// Network manager for multiplayer state synchronization
/// </summary>
public class NetworkManager
{
    private readonly Dictionary<string, NetworkClient> _clients = new();
    private readonly Dictionary<string, object> _networkState = new();
    private TcpListener? _server;
    private bool _isServer;
    private bool _isRunning;
    private readonly int _port;

    public event Action<string, NetworkMessage>? OnMessageReceived;
    public event Action<string>? OnClientConnected;
    public event Action<string>? OnClientDisconnected;

    public NetworkManager(int port = 7777)
    {
        _port = port;
    }

    /// <summary>
    /// Start as a server
    /// </summary>
    public async Task StartServerAsync()
    {
        _isServer = true;
        _isRunning = true;
        _server = new TcpListener(IPAddress.Any, _port);
        _server.Start();

        Console.WriteLine($"[Network] Server started on port {_port}");

        _ = Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var clientId = Guid.NewGuid().ToString();
                    var networkClient = new NetworkClient(client, clientId);
                    
                    _clients[clientId] = networkClient;
                    OnClientConnected?.Invoke(clientId);

                    _ = HandleClientAsync(networkClient);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"[Network] Error accepting client: {ex.Message}");
                    }
                }
            }
        });

        await Task.CompletedTask;
    }

    /// <summary>
    /// Connect to a server as a client
    /// </summary>
    public async Task<bool> ConnectAsync(string host, int? port = null)
    {
        try
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port ?? _port);

            var clientId = "server";
            var networkClient = new NetworkClient(tcpClient, clientId);
            _clients[clientId] = networkClient;

            Console.WriteLine($"[Network] Connected to {host}:{port ?? _port}");

            _ = HandleClientAsync(networkClient);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network] Failed to connect: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Handle messages from a client
    /// </summary>
    private async Task HandleClientAsync(NetworkClient client)
    {
        try
        {
            var buffer = new byte[4096];

            while (_isRunning && client.TcpClient.Connected)
            {
                var stream = client.TcpClient.GetStream();
                var bytesRead = await stream.ReadAsync(buffer);

                if (bytesRead == 0) break;

                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var message = JsonSerializer.Deserialize<NetworkMessage>(json);

                if (message != null)
                {
                    OnMessageReceived?.Invoke(client.Id, message);
                    await ProcessMessage(client.Id, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network] Client error: {ex.Message}");
        }
        finally
        {
            RemoveClient(client.Id);
        }
    }

    /// <summary>
    /// Process incoming network message
    /// </summary>
    private async Task ProcessMessage(string clientId, NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.StateUpdate:
                if (message.Data != null)
                {
                    var stateUpdate = JsonSerializer.Deserialize<StateUpdate>(message.Data.ToString()!);
                    if (stateUpdate != null)
                    {
                        _networkState[stateUpdate.Key] = stateUpdate.Value;
                        
                        // Broadcast to other clients if we're the server
                        if (_isServer)
                        {
                            await BroadcastToOthers(clientId, message);
                        }
                    }
                }
                break;

            case MessageType.Ping:
                await SendMessage(clientId, new NetworkMessage
                {
                    Type = MessageType.Pong,
                    Timestamp = DateTime.UtcNow
                });
                break;
        }
    }

    /// <summary>
    /// Send a message to a specific client
    /// </summary>
    public async Task SendMessage(string clientId, NetworkMessage message)
    {
        if (_clients.TryGetValue(clientId, out var client))
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                var stream = client.TcpClient.GetStream();
                await stream.WriteAsync(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Error sending message: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Broadcast a message to all clients
    /// </summary>
    public async Task BroadcastMessage(NetworkMessage message)
    {
        var tasks = new List<Task>();

        foreach (var client in _clients.Values)
        {
            tasks.Add(SendMessage(client.Id, message));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Broadcast to all clients except one
    /// </summary>
    private async Task BroadcastToOthers(string excludeClientId, NetworkMessage message)
    {
        foreach (var client in _clients.Values)
        {
            if (client.Id != excludeClientId)
            {
                await SendMessage(client.Id, message);
            }
        }
    }

    /// <summary>
    /// Synchronize a state value across the network
    /// </summary>
    public async Task SyncState(string key, object value)
    {
        _networkState[key] = value;

        var message = new NetworkMessage
        {
            Type = MessageType.StateUpdate,
            Timestamp = DateTime.UtcNow,
            Data = new StateUpdate { Key = key, Value = value }
        };

        if (_isServer)
        {
            await BroadcastMessage(message);
        }
        else if (_clients.ContainsKey("server"))
        {
            await SendMessage("server", message);
        }
    }

    /// <summary>
    /// Get a synchronized state value
    /// </summary>
    public T? GetState<T>(string key)
    {
        if (_networkState.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Remove a client
    /// </summary>
    private void RemoveClient(string clientId)
    {
        if (_clients.Remove(clientId))
        {
            OnClientDisconnected?.Invoke(clientId);
            Console.WriteLine($"[Network] Client disconnected: {clientId}");
        }
    }

    /// <summary>
    /// Stop the network manager
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _server?.Stop();

        foreach (var client in _clients.Values)
        {
            client.TcpClient.Close();
        }

        _clients.Clear();
        Console.WriteLine("[Network] Network manager stopped");
    }
}

/// <summary>
/// Network client wrapper
/// </summary>
public class NetworkClient
{
    public TcpClient TcpClient { get; }
    public string Id { get; }

    public NetworkClient(TcpClient tcpClient, string id)
    {
        TcpClient = tcpClient;
        Id = id;
    }
}

/// <summary>
/// Network message types
/// </summary>
public enum MessageType
{
    StateUpdate,
    Ping,
    Pong,
    VoiceData,
    Custom
}

/// <summary>
/// Network message structure
/// </summary>
public class NetworkMessage
{
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// State update data
/// </summary>
public class StateUpdate
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = new();
}
