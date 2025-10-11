using Abstractions;
using ASHATCore.Engine.Manager;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ASHATCoreWebSocketHandler(ModuleManager moduleManager, ISpeechModule? speechModule) : IDisposable
{
    private readonly ModuleManager _moduleManager = moduleManager;
    private readonly ISpeechModule? _speechModule = speechModule;
    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();

    public async Task HandleAsync(WebSocket webSocket)
    {
        var clientId = Guid.NewGuid();
        _clients[clientId] = webSocket;

        var buffer = new byte[4096];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    string response = await ProcessCommandAsync(receivedText);

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        var responseBuffer = Encoding.UTF8.GetBytes(response);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session closed", CancellationToken.None);
                }
            }
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            webSocket.Dispose();
        }
    }

    private async Task<string> ProcessCommandAsync(string input)
    {
        if (_speechModule == null)
            return "No SpeechModule loaded.";

        // Always route through SpeechModule for consistency
        var speechResult = await _speechModule.GenerateResponseAsync(input);
        return speechResult ?? "(no response)";
    }

    public async Task BroadcastAsync(string message, Guid? except = null)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var kvp in _clients)
        {
            if (except.HasValue && kvp.Key == except.Value)
                continue;

            if (kvp.Value.State == WebSocketState.Open)
            {
                try
                {
                    await kvp.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch { /* ignore failed sends */ }
            }
        }
    }
    public void Dispose()
    {
        foreach (var kvp in _clients)
        {
            try
            {
                kvp.Value.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait();
            }
            catch { /* ignore */ }
            kvp.Value.Dispose();
        }
        _clients.Clear();
    }
}