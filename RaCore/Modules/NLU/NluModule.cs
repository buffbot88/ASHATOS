using System;
using System.Text.RegularExpressions;
using System.Text.Json;
using RaCore.Engine.Manager;

namespace RaCore.Modules.NLU;

// [RaModule("NLU")]
public sealed class NluModule : ModuleBase
{
    public override string Name => "NLU";

    public override string Process(string input)
    {
        var text = (input ?? "").Trim();

        // Simple examples; extend with more patterns or LLM fallback
        if (Regex.IsMatch(text, @"^turn on (.+)$", RegexOptions.IgnoreCase))
            return Intent("device.control", new { action = "on", target = Regex.Match(text, @"^turn on (.+)$", RegexOptions.IgnoreCase).Groups[1].Value });

        if (Regex.IsMatch(text, @"^open (.+)$", RegexOptions.IgnoreCase))
            return Intent("system.open", new { target = Regex.Match(text, @"^open (.+)$", RegexOptions.IgnoreCase).Groups[1].Value });

        // Default: freeform query
        return Intent("chat.query", new { text });
    }

    private static string Intent(string type, object slots)
    {
        var obj = new { intent = type, slots, ts = DateTime.UtcNow };
        return JsonSerializer.Serialize(obj);
    }
}
