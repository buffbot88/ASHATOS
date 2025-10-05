using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Conscious;

namespace RaCore.Modules.Extensions.Safety;

[RaModule(Category = "extensions")]
public sealed class ConsentRegistryModule : ModuleBase
{
    public override string Name => "Consent";

    private readonly Dictionary<string, ConsentRecord> _consents = new(StringComparer.OrdinalIgnoreCase);
    private ThoughtProcessor? _thoughtProcessor;

    // Cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        if (manager is ModuleManager moduleManager)
            _thoughtProcessor = new ThoughtProcessor(moduleManager);
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var t = (input ?? "").Trim();
        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        if (t.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                "Consent commands:\n  consent grant <skill> [scope]\n  consent revoke <skill>\n  consent list",
                null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral
            );
        }

        if (t.StartsWith("consent grant ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = t.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            var skill = parts.Length >= 3 ? parts[2] : "";
            var scope = "";
            var idx = t.IndexOf(skill, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) scope = t[(idx + skill.Length)..].Trim();
            if (string.IsNullOrWhiteSpace(skill))
                return await _thoughtProcessor.ProcessThoughtAsync("Usage: consent grant <skill> [scope]", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);

            _consents[skill] = new ConsentRecord { Skill = skill, Scope = scope, GrantedAtUtc = DateTime.UtcNow };
            var msg = $"Granted consent for {skill} {(string.IsNullOrWhiteSpace(scope) ? "" : $"(scope: {scope})")}".Trim();
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy);
        }

        if (t.StartsWith("consent revoke ", StringComparison.OrdinalIgnoreCase))
        {
            var skill = t["consent revoke ".Length..].Trim();
            _consents.Remove(skill);
            var msg = $"Revoked consent for {skill}.";
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }

        if (t.Equals("consent list", StringComparison.OrdinalIgnoreCase))
        {
            var msg = _consents.Count == 0 ? "No consents."
                : JsonSerializer.Serialize(_consents.Values.Select(c => new { c.Skill, c.Scope, c.GrantedAtUtc }), _jsonOptions);
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }

        return await _thoughtProcessor.ProcessThoughtAsync("Unknown command. Try: help", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
    }

    public bool HasConsent(string skill, string? scope = null)
    {
        if (!_consents.TryGetValue(skill, out var rec)) return false;
        if (string.IsNullOrWhiteSpace(scope)) return true;
        return (rec.Scope ?? "").Contains(scope, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ConsentRecord
    {
        public string Skill { get; set; } = "";
        public string? Scope { get; set; }
        public DateTime GrantedAtUtc { get; set; }
    }
}
