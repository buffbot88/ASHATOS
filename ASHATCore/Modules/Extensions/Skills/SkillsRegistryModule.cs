using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Conscious;

namespace ASHATCore.Modules.Extensions.Skills;

[RaModule(Category = "extensions")]
public sealed class SkillsRegistryModule : ModuleBase
{
    public override string Name => "Skills";

    private readonly Dictionary<string, ISkill> _skills = new(StringComparer.OrdinalIgnoreCase);
    private ThoughtProcessor? _thoughtProcessor;

    // Cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        var moduleManager = manager as ModuleManager;
        if (moduleManager == null)
        {
            LogError("Manager is not a ModuleManager.");
            return;
        }
        _thoughtProcessor = new ThoughtProcessor(moduleManager);

        foreach (var w in moduleManager.Modules)
            if (w.Instance is ISkill s)
                _skills[s.Name] = s;

        LogInfo($"Skills registered: {string.Join(", ", _skills.Keys)}");
    }

    public IReadOnlyCollection<ISkill> List() => _skills.Values;
    public ISkill? Get(string name) => _skills.TryGetValue(name, out var s) ? s : null;

    public override string Process(string input)
    {
        // Ensure that .Text is never null by using null-coalescing operator
        return ProcessAsync(input).GetAwaiter().GetResult().Text ?? string.Empty;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var t = (input ?? "").Trim();
        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        if (t.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            var msg = "Skills commands:\n  skills list\n  skills describe <name>";
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }

        if (t.Equals("skills list", StringComparison.OrdinalIgnoreCase))
        {
            var msg = string.Join("\n", _skills.Keys.OrderBy(k => k));
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }

        if (t.StartsWith("skills describe ", StringComparison.OrdinalIgnoreCase))
        {
            var name = t["skills describe ".Length..].Trim();
            var s = Get(name);
            var msg = s == null
                ? $"Skill '{name}' not found."
                : JsonSerializer.Serialize(new { s.Name, s.Description, s.ParametersSchema }, _jsonOptions);
            return await _thoughtProcessor.ProcessThoughtAsync(msg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), s != null ? Mood.Happy : Mood.Confused);
        }

        return await _thoughtProcessor.ProcessThoughtAsync("Unknown command. Try: help", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
    }
}
