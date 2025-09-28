namespace Ra.Host;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Globalization;
using System.Text;
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        // Services
        builder.Services.AddCors(o =>
        {
            o.AddDefaultPolicy(p => p
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });
        builder.Services.AddDirectoryBrowser();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<EngineHost>();

        var app = builder.Build();

        // Middleware
        app.UseCors();
        app.UseDefaultFiles(); // serves wwwroot/index.html
        app.UseStaticFiles();

        // Wire SignalR hub bridge
        FaceSignalRBridge.Hub = app.Services.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<FaceHub>>();

        // Health
        app.MapGet("/api/health", () => Results.Json(new { ok = true, timeUtc = DateTime.UtcNow }));

        // Features
        app.MapGet("/api/features", (EngineHost eng) =>
        {
            var res = eng.Invoke("FeatureExplorer", "features json") ?? "";
            var t = res.TrimStart();
            if (t.StartsWith("{") || t.StartsWith("[")) return Results.Text(res, "application/json", Encoding.UTF8);
            return Results.Json(new { ok = false, error = "FeatureExplorer did not return JSON", detail = string.IsNullOrWhiteSpace(res) ? "(no response)" : res }, new JsonSerializerOptions { WriteIndented = true });
        });
        app.MapGet("/api/features/text", (EngineHost eng) =>
        {
            var res = eng.Invoke("FeatureExplorer", "features full");
            return Results.Text(string.IsNullOrWhiteSpace(res) ? "(no response)" : res, "text/plain", Encoding.UTF8);
        });

        // Modules
        app.MapGet("/api/modules", (EngineHost eng) =>
        {
            var lines = eng.ListModules();
            return Results.Text(string.Join("\n", lines), "text/plain", Encoding.UTF8);
        });

        // Speech
        app.MapPost("/api/speech", async (HttpRequest req, EngineHost eng) =>
        {
            var text = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(text)) return Results.Text("(no input)");
            var res = eng.Invoke("Speech", text);
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });

        // Agentic
        app.MapPost("/api/agent/do", async (HttpRequest req, EngineHost eng) =>
        {
            var utterance = await new StreamReader(req.Body).ReadToEndAsync();
            var res = eng.Invoke("Speech", $"do {utterance}");
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });
        app.MapGet("/api/agent/pending", (EngineHost eng) =>
        {
            var status = eng.Invoke("Speech", "status");
            var reason = eng.Invoke("Memory", "recall agent/pending/reason");
            var plan = eng.Invoke("Memory", "recall agent/pending/plan");
            var lastIntent = eng.Invoke("Memory", "recall agent/last/intent");
            var lastPlan = eng.Invoke("Memory", "recall agent/last/plan");
            var lastOutcome = eng.Invoke("Memory", "recall agent/last/outcome");

            var obj = new
            {
                status,
                pending = !string.IsNullOrWhiteSpace(plan),
                reason = string.IsNullOrWhiteSpace(reason) ? null : reason,
                plan = string.IsNullOrWhiteSpace(plan) ? null : plan,
                last = new
                {
                    intent = string.IsNullOrWhiteSpace(lastIntent) ? null : lastIntent,
                    plan = string.IsNullOrWhiteSpace(lastPlan) ? null : lastPlan,
                    outcome = string.IsNullOrWhiteSpace(lastOutcome) ? null : lastOutcome
                }
            };
            return Results.Json(obj, new JsonSerializerOptions { WriteIndented = true });
        });
        app.MapPost("/api/agent/confirm", async (HttpRequest req, EngineHost eng) =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var approve = body.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);
            try
            {
                if (!approve)
                {
                    using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                    if (doc.RootElement.TryGetProperty("approve", out var a) && a.ValueKind == JsonValueKind.True) approve = true;
                }
            }
            catch { }
            var res = eng.Invoke("Speech", approve ? "yes" : "no");
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });

        // Skills & consent
        app.MapGet("/api/skills", (EngineHost eng) =>
        {
            var res = eng.Invoke("Skills", "skills list");
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });
        app.MapGet("/api/skills/{name}", (string name, EngineHost eng) =>
        {
            var res = eng.Invoke("Skills", $"skills describe {name}");
            return Results.Text(res, "application/json", Encoding.UTF8);
        });
        app.MapPost("/api/consent", async (HttpRequest req, EngineHost eng) =>
        {
            var cmd = await new StreamReader(req.Body).ReadToEndAsync();
            var res = eng.Invoke("Consent", cmd);
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });

        // Memory
        app.MapGet("/api/memory/stats", (EngineHost eng) =>
        {
            var res = eng.Invoke("Memory", "stats");
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });
        app.MapGet("/api/memory/recall/{key}", (string key, EngineHost eng) =>
        {
            var res = eng.Invoke("Memory", $"recall {key}");
            return Results.Text(res ?? "", "text/plain", Encoding.UTF8);
        });
        app.MapPost("/api/memory/remember", async (HttpRequest req, EngineHost eng) =>
        {
            using var doc = await JsonDocument.ParseAsync(req.Body);
            if (!doc.RootElement.TryGetProperty("key", out var k) || !doc.RootElement.TryGetProperty("value", out var v))
                return Results.BadRequest(new { error = "Expected JSON { key, value }" });
            var key = k.GetString() ?? "";
            var value = v.GetString() ?? "";
            var res = eng.Invoke("Memory", $"remember {key}={value}");
            return Results.Text(res, "text/plain", Encoding.UTF8);
        });
        app.MapGet("/api/memory/query", (HttpRequest req, EngineHost eng) =>
        {
            var q = req.Query["q"].ToString();
            if (string.IsNullOrWhiteSpace(q)) return Results.BadRequest(new { error = "missing ?q=" });
            var res = eng.Invoke("Memory", $"query {q}");
            return Results.Text(res ?? "", "text/plain", Encoding.UTF8);
        });
        app.MapGet("/api/memory/list", (HttpRequest req, EngineHost eng) =>
        {
            var nStr = req.Query["n"].ToString();
            if (!int.TryParse(nStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) n = 50;
            var res = eng.Invoke("Memory", $"list {n}");
            return Results.Text(res ?? "", "text/plain", Encoding.UTF8);
        });
        app.MapDelete("/api/memory/remove/key/{key}", (string key, EngineHost eng) =>
        {
            var res = eng.Invoke("Memory", $"remove key {key}");
            return Results.Text(res ?? "", "text/plain", Encoding.UTF8);
        });
        app.MapDelete("/api/memory/remove/id/{id}", (string id, EngineHost eng) =>
        {
            if (!Guid.TryParse(id, out var _))
                return Results.BadRequest(new { error = "invalid guid" });
            var res = eng.Invoke("Memory", $"remove id {id}");
            return Results.Text(res ?? "", "text/plain", Encoding.UTF8);
        });

        // Face APIs (no redirects)
        app.MapGet("/api/face/state", (EngineHost eng) =>
        {
            var json = eng.Invoke("Face", "face state");
            return Results.Text(string.IsNullOrWhiteSpace(json) ? "{}" : json, "application/json", Encoding.UTF8);
        });
        app.MapPost("/api/face/cmd", async (HttpRequest req, EngineHost eng) =>
        {
            var cmd = await new StreamReader(req.Body).ReadToEndAsync();
            var res = eng.Invoke("Face", cmd);
            return Results.Text(res);
        });
        app.MapPost("/api/face/anchors", async (HttpRequest req, EngineHost eng) =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest(new { error = "missing JSON body" });
            var res = eng.Invoke("Face", "face anchors " + body);
            return Results.Text(res);
        });
        app.MapPost("/api/face/text", async (HttpRequest req, EngineHost eng) =>
        {
            using var doc = await JsonDocument.ParseAsync(req.Body);
            var root = doc.RootElement;
            if (!root.TryGetProperty("text", out var tProp))
                return Results.BadRequest(new { error = "missing 'text'" });
            var anchors = new
            {
                id = "text-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                particleCount = (int?)null,
                shapes = new object[]
                {
                    new {
                        type = "text",
                        text = tProp.GetString() ?? "",
                        font = root.TryGetProperty("font", out var f)? f.GetString() : "600 180px Segoe UI",
                        x = root.TryGetProperty("x", out var x)? x.GetDouble() : 0.5,
                        y = root.TryGetProperty("y", out var y)? y.GetDouble() : 0.5,
                        align = root.TryGetProperty("align", out var a)? a.GetString() : "center",
                        sample = root.TryGetProperty("sample", out var s)? s.GetString() : "edge",
                        density = root.TryGetProperty("density", out var d)? d.GetDouble() : 1.0
                    }
                }
            };
            var json = JsonSerializer.Serialize(anchors);
            var res = eng.Invoke("Face", "face anchors " + json);
            return Results.Text(res);
        });

        // SignalR hub for the face
        app.MapHub<FaceHub>("/faceHub");

        // Root redirect to dashboard
        app.MapGet("/", () => Results.Redirect("/index.html"));

        var engine = app.Services.GetRequiredService<EngineHost>();
        app.Lifetime.ApplicationStopping.Register(() => { try { engine.Dispose(); } catch { } });

        app.Run();
    }
}