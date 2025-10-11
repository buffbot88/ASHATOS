using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Conscious;

namespace ASHATCore.Modules.Extensions.Skills;

[RaModule(Category = "skills")]
public sealed class SystemOpenSkillModule : ModuleBase, ISkill
{
    public override string Name => "System.Open";

    public string Description => "Open a file, URL, or application on the host system";
    public string? ParametersSchema => "{\"type\":\"object\",\"properties\":{\"intent\":{\"type\":\"string\"},\"slots\":{\"type\":\"object\"}}}";

    private ThoughtProcessor? _thoughtProcessor;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        if (manager is ModuleManager moduleManager)
            _thoughtProcessor = new ThoughtProcessor(moduleManager);
        LogInfo("System.Open skill ready.");
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        var t = (input ?? "").Trim();
        if (t.Equals("help", System.StringComparison.OrdinalIgnoreCase))
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                "System.Open: invoke via agentic pipeline (do open <target>) or Skills registry.",
                null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral
            );
        }
        if (t.Equals("status", System.StringComparison.OrdinalIgnoreCase))
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                "System.Open: available",
                null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy
            );
        }
        return await _thoughtProcessor.ProcessThoughtAsync(
            "Unknown command. Try: help | status",
            null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
        );
    }

    public async Task<SkillResult> InvokeAsync(string argumentsJson, CancellationToken ct = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;

            string? target = null;
            if (root.TryGetProperty("slots", out var slots) && slots.TryGetProperty("target", out var tgtEl))
                target = tgtEl.GetString();

            if (string.IsNullOrWhiteSpace(target) && root.TryGetProperty("target", out var direct))
                target = direct.GetString();

            if (string.IsNullOrWhiteSpace(target))
            {
                if (_thoughtProcessor != null)
                {
                    var resp = await _thoughtProcessor.ProcessThoughtAsync("System.Open failed: missing target", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
                    return new SkillResult { Success = false, Error = resp.Text };
                }
                return new SkillResult { Success = false, Error = "missing target" };
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });

            if (_thoughtProcessor != null)
            {
                var resp = await _thoughtProcessor.ProcessThoughtAsync($"Opened: {target}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy);
                return new SkillResult { Success = true, Output = resp.Text };
            }
            return new SkillResult { Success = true, Output = $"Opened: {target}" };
        }
        catch (System.Exception ex)
        {
            if (_thoughtProcessor != null)
            {
                var resp = await _thoughtProcessor.ProcessThoughtAsync($"System.Open failed: {ex.Message}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
                return new SkillResult { Success = false, Error = resp.Text };
            }
            return new SkillResult { Success = false, Error = ex.Message };
        }
    }
}
