using System;
using System.Collections.Generic;
using System.Text.Json;
using Ra.Core.Engine.Manager;

namespace Ra.Core.Modules.Planning;

// [RaModule("Planner")]
public sealed class PlannerModule : ModuleBase
{
    public override string Name => "Planner";
    private ModuleManager? _manager;

    public override void Initialize(ModuleManager manager)
    {
        base.Initialize(manager);
        _manager = manager;
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        // Friendly commands
        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            return GetHelpText();
        if (text.Equals("status", StringComparison.OrdinalIgnoreCase))
            return "Planner: available";

        // Allow "plan {json}" prefix
        const string planPrefix = "plan ";
        if (text.StartsWith(planPrefix, StringComparison.OrdinalIgnoreCase))
            text = text.Substring(planPrefix.Length).Trim();

        string intentJson = text;

        // If not JSON, ask NLU to create intent
        if (!(text.StartsWith("{") || text.StartsWith("[")))
        {
            var nlu = _manager?.SafeInvokeModuleByName("NLU", text)
                   ?? _manager?.SafeInvokeModuleByName("NluModule", text);

            if (string.IsNullOrWhiteSpace(nlu))
                return "(invalid intent: expected JSON or resolvable via NLU)";

            intentJson = nlu;
        }

        try
        {
            using var doc = JsonDocument.Parse(intentJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("intent", out var itEl))
                return "(invalid intent json: missing 'intent')";

            var type = (itEl.GetString() ?? string.Empty).Trim().ToLowerInvariant();

            var plan = type switch
            {
                "device.control" => new Plan
                {
                    Goal = "Control device",
                    Steps = new()
                    {
                        new Step { Skill = "Device.Control", ArgumentsJson = intentJson }
                    }
                },
                "system.open" => new Plan
                {
                    Goal = "Open target",
                    Steps = new()
                    {
                        new Step { Skill = "System.Open", ArgumentsJson = intentJson }
                    }
                },
                _ => new Plan
                {
                    Goal = "Answer query",
                    Steps = new()
                    {
                        new Step { Skill = "Chat.Answer", ArgumentsJson = intentJson }
                    }
                }
            };

            return JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException jex)
        {
            return $"(invalid intent json) {jex.Message}";
        }
        catch (Exception ex)
        {
            return $"(planner error) {ex.Message}";
        }
    }

    private static string GetHelpText() => @"
Planner commands:
  - status                      : Planner availability
  - help                        : This help
  - {intent json}               : Returns a plan for the given intent JSON
  - plan {intent json}          : Same as above
  - natural language utterance  : Planner will call NLU to derive the intent
".Trim();

    public sealed class Plan
    {
        public string Goal { get; set; } = "";
        public List<Step> Steps { get; set; } = new();
    }

    public sealed class Step
    {
        public string Skill { get; set; } = "";
        public string ArgumentsJson { get; set; } = "{}";
    }
}
