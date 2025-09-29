using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ra.Core.Engine.Manager;

namespace Ra.Core.Modules.Safety;

// [RaModule("Consent")]
public sealed class ConsentRegistryModule : ModuleBase
{
    public override string Name => "Consent";

    private readonly Dictionary<string, ConsentRecord> _consents = new(StringComparer.OrdinalIgnoreCase);

    public override string Process(string input)
    {
        var t = (input ?? "").Trim();
        if (t.Equals("help", StringComparison.OrdinalIgnoreCase))
            return "Consent commands:\n  consent grant <skill> [scope]\n  consent revoke <skill>\n  consent list";

        if (t.StartsWith("consent grant ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = t.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            var skill = parts.Length >= 3 ? parts[2] : "";
            var scope = "";
            var idx = t.IndexOf(skill, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) scope = t[(idx + skill.Length)..].Trim();
            if (string.IsNullOrWhiteSpace(skill)) return "Usage: consent grant <skill> [scope]";
            _consents[skill] = new ConsentRecord { Skill = skill, Scope = scope, GrantedAtUtc = DateTime.UtcNow };
            return $"Granted consent for {skill} {(string.IsNullOrWhiteSpace(scope) ? "" : $"(scope: {scope})")}".Trim();
        }

        if (t.StartsWith("consent revoke ", StringComparison.OrdinalIgnoreCase))
        {
            var skill = t.Substring("consent revoke ".Length).Trim();
            _consents.Remove(skill);
            return $"Revoked consent for {skill}.";
        }

        if (t.Equals("consent list", StringComparison.OrdinalIgnoreCase))
        {
            if (_consents.Count == 0) return "No consents.";
            var arr = _consents.Values.Select(c => new { c.Skill, c.Scope, c.GrantedAtUtc });
            return JsonSerializer.Serialize(arr, new JsonSerializerOptions { WriteIndented = true });
        }

        return "Unknown command. Try: help";
    }

    public bool HasConsent(string skill, string? scope = null)
    {
        if (!_consents.TryGetValue(skill, out var rec)) return false;
        if (string.IsNullOrWhiteSpace(scope)) return true;
        return (rec.Scope ?? "").IndexOf(scope, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private sealed class ConsentRecord
    {
        public string Skill { get; set; } = "";
        public string? Scope { get; set; }
        public DateTime GrantedAtUtc { get; set; }
    }
}
