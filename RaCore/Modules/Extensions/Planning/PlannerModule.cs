using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Conscious;

namespace RaCore.Modules.Extensions.Planning;

[RaModule(Category = "extensions")]
public sealed class PlannerModule : ModuleBase
{
    public override string Name => "Planner";
    private ModuleManager? _manager;
    private ThoughtProcessor? _thoughtProcessor;

    // Cache the JsonSerializerOptions instance to avoid CA1869
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        if (_manager != null)
            _thoughtProcessor = new ThoughtProcessor(_manager);
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        // Friendly commands
        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            var help = GetHelpText();
            return await _thoughtProcessor.ProcessThoughtAsync(help, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }

        if (text.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            var msg = "Planner: available";
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy);
        }

        // Allow "plan {json}" prefix
        const string planPrefix = "plan ";
        if (text.StartsWith(planPrefix, StringComparison.OrdinalIgnoreCase))
            text = text[planPrefix.Length..].Trim();

        string intentJson = text;

        // If not JSON, try to resolve with NLU
        if (!(text.StartsWith('{') || text.StartsWith('[')))
        {
            var nlu = _manager?.SafeInvokeModuleByName("NLU", text)
                   ?? _manager?.SafeInvokeModuleByName("NluModule", text);

            if (string.IsNullOrWhiteSpace(nlu))
                return await _thoughtProcessor.ProcessThoughtAsync(
                    "(invalid intent: expected JSON or resolvable via NLU)", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
                );

            intentJson = nlu;
        }

        try
        {
            using var doc = JsonDocument.Parse(intentJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("intent", out var itEl))
                return await _thoughtProcessor.ProcessThoughtAsync(
                    "(invalid intent json: missing 'intent')", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
                );

            var type = (itEl.GetString() ?? string.Empty).Trim().ToLowerInvariant();

            var plan = type switch
            {
                "device.control" => new Plan
                {
                    Goal = "Control device",
                    Steps =
                    [
                        new Step { Skill = "Device.Control", ArgumentsJson = intentJson }
                    ]
                },
                "system.open" => new Plan
                {
                    Goal = "Open target",
                    Steps =
                    [
                        new Step { Skill = "System.Open", ArgumentsJson = intentJson }
                    ]
                },
                _ => new Plan
                {
                    Goal = "Answer query",
                    Steps =
                    [
                        new Step { Skill = "Chat.Answer", ArgumentsJson = intentJson }
                    ]
                }
            };

            // Use the cached JsonSerializerOptions instance
            var planJson = JsonSerializer.Serialize(plan, CachedJsonSerializerOptions);
            return await _thoughtProcessor.ProcessThoughtAsync(planJson, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Thinking);
        }
        catch (JsonException jex)
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                $"(invalid intent json) {jex.Message}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
            );
        }
        catch (Exception ex)
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                $"(planner error) {ex.Message}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
            );
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
}
