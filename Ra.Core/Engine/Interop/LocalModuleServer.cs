using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using RaAI.Modules;
using RaAI.Handlers.Manager;

namespace RaAI.Handlers
{
    /// <summary>
    /// LocalModuleServer serves HTTP and WebSocket endpoints for module interaction.
    /// Compatible with updated core modules and IModuleRegistry.
    /// </summary>
    public class LocalModuleServer
    {
        private readonly int _port;
        private readonly string _token;
        private readonly IModuleRegistry _modules;
        private readonly Action<string, string> _externalNotifyCallback;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<WebSocket> _wsClients = new();
        private HttpListener? _listener;

        public LocalModuleServer(int port, string token, IModuleRegistry modules, Action<string, string> externalNotifyCallback)
        {
            _port = port;
            _token = token;
            _modules = modules;
            _externalNotifyCallback = externalNotifyCallback;
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();
            Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener?.Stop();
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await _listener!.GetContextAsync();
                }
                catch
                {
                    break;
                }

                if (ctx.Request.IsWebSocketRequest && ctx.Request.Url?.AbsolutePath == "/ws")
                {
                    await HandleWebSocket(ctx);
                }
                else
                {
                    await Task.Run(() => HandleHttp(ctx), token);
                }
            }
        }

        private async Task HandleWebSocket(HttpListenerContext ctx)
        {
            var wsContext = await ctx.AcceptWebSocketAsync(null);
            var ws = wsContext.WebSocket;
            lock (_wsClients) { _wsClients.Add(ws); }
            await EchoLoop(ws);
            lock (_wsClients) { _wsClients.Remove(ws); }
        }

        private async Task HandleHttp(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            if (req.HttpMethod == "GET" && req.Url?.AbsolutePath == "/api/modules")
            {
                var names = _modules.GetModuleNames();
                var json = JsonSerializer.Serialize(names);
                resp.ContentType = "application/json";
                await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
                resp.Close();
                return;
            }

            if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/api/command")
            {
                if (!Authenticate(req))
                {
                    resp.StatusCode = 401;
                    await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("unauthorized"));
                    resp.Close();
                    return;
                }

                CommandRequest? commandReq;
                using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                {
                    var body = await reader.ReadToEndAsync();
                    commandReq = JsonSerializer.Deserialize<CommandRequest>(body);
                }

                if (commandReq == null || string.IsNullOrEmpty(commandReq.Module) || string.IsNullOrEmpty(commandReq.Command))
                {
                    resp.StatusCode = 400;
                    await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("bad payload"));
                    resp.Close();
                    return;
                }

                var module = _modules.GetModuleByName(commandReq.Module);
                if (module == null)
                {
                    resp.StatusCode = 404;
                    await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("module-not-found"));
                    resp.Close();
                    return;
                }

                _externalNotifyCallback?.Invoke(commandReq.Module, commandReq.Command);

                string result;
                try
                {
                    result = module.Process(commandReq.Command);
                }
                catch (Exception ex) { result = "(error) " + ex.Message; }

                var commandResp = new CommandResponse { Ok = true, Result = result, Timestamp = DateTime.UtcNow };
                var json = JsonSerializer.Serialize(commandResp);
                resp.ContentType = "application/json";
                await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
                resp.Close();
                return;
            }

            resp.StatusCode = 404;
            await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("not found"));
            resp.Close();
        }

        private bool Authenticate(HttpListenerRequest req)
        {
            if (string.IsNullOrEmpty(_token)) return true;
            var auth = req.Headers["Authorization"];
            if (string.IsNullOrEmpty(auth)) return false;
            if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
            var tok = auth["Bearer ".Length..].Trim();
            return tok == _token;
        }

        public void NotifyMessage(string module, string command, string response)
        {
            var msg = JsonSerializer.Serialize(new { module, command, response, timestamp = DateTime.UtcNow });
            var bytes = Encoding.UTF8.GetBytes(msg);
            lock (_wsClients)
            {
                foreach (var ws in _wsClients.ToArray())
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        _ = ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        private static async Task EchoLoop(WebSocket ws)
        {
            var buffer = new byte[4096];
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var res = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", CancellationToken.None);
                        break;
                    }
                    // ignore incoming WS messages for now
                }
                catch
                {
                    break;
                }
            }
        }

        private class CommandRequest { public string? Module { get; set; } public string? Command { get; set; } }
        private class CommandResponse { public bool Ok { get; set; } public string? Result { get; set; } public DateTime Timestamp { get; set; } }
    }
}