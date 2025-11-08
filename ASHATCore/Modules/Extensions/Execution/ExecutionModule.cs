using ASHATCore.Modules.Extensions.Skills;
using ASHATCore.Modules.Conscious;
using System.Text;
using System.Text.Json;
using ASHATCore.Modules.Extensions.Planning;

using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Execution;

[RaModule(Category = "extensions")]
public sealed class ExecutionModule : ModuleBase
{
    public override string Name => "Executor";
    private ModuleManager? _manager;
    private ThoughtProcessor? _thoughtProcessor;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        if (_manager != null)
            _thoughtProcessor = new ThoughtProcessor(_manager);
    }

    public override string Process(string input)
    {
        // Ensure .Text is not null by using null-coalescing operator
        return ProcessAsync(input).GetAwaiter().GetResult().Text ?? string.Empty;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        // User help/status
        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return await _thoughtProcessor.ProcessThoughtAsync(GetHelp(), null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        }
        if (text.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            return await _thoughtProcessor.ProcessThoughtAsync("Executor: available", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy);
        }

        // Allow "execute {json}" prefix
        const string execPrefix = "execute ";
        if (text.StartsWith(execPrefix, StringComparison.OrdinalIgnoreCase))
            text = text[execPrefix.Length..].Trim();

        // Require JSON input
        if (!(text.StartsWith('{') || text.StartsWith('[')))
        {
            var usageMsg = "(usage) Executor expects a Planner plan JSON. Generate one with Planner, then pass it here.";
            return await _thoughtProcessor.ProcessThoughtAsync(usageMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
        }

        Plan? plan;
        try
        {
            plan = JsonSerializer.Deserialize<Plan>(text);
        }
        catch (JsonException jex)
        {
            return await _thoughtProcessor.ProcessThoughtAsync($"(invalid plan json) {jex.Message}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
        }
        catch (Exception ex)
        {
            return await _thoughtProcessor.ProcessThoughtAsync($"(executor error while parsing) {ex.Message}", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
        }

        if (plan == null)
            return await _thoughtProcessor.ProcessThoughtAsync("(invalid plan)", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
        if (plan.Steps == null || plan.Steps.Count == 0)
            return await _thoughtProcessor.ProcessThoughtAsync("(empty plan)", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);

        if (_manager?.GetModuleInstanceByName("Skills") is not SkillsRegistryModule skills)
            return await _thoughtProcessor.ProcessThoughtAsync("(skills registry unavailable)", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);

        var sb = new StringBuilder();
        int stepIndex = 0;

        foreach (var step in plan.Steps)
        {
            stepIndex++;

            if (!IsAllowed(step.Skill))
            {
                var blockedMsg = $"(blocked by safety) {step.Skill}";
                return await _thoughtProcessor.ProcessThoughtAsync(blockedMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
            }

            var skill = skills.List().FirstOrDefault(s => s.Name.Equals(step.Skill, StringComparison.OrdinalIgnoreCase));
            if (skill == null)
            {
                var notFoundMsg = $"(skill not found) {step.Skill}";
                return await _thoughtProcessor.ProcessThoughtAsync(notFoundMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
            }

            SkillResult res;
            try
            {
                var args = string.IsNullOrWhiteSpace(step.ArgumentsJson) ? "{}" : step.ArgumentsJson;
                res = await skill.InvokeAsync(args, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var cASHATshMsg = $"(step cASHATshed) {step.Skill}: {ex.Message}";
                return await _thoughtProcessor.ProcessThoughtAsync(cASHATshMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
            }

            if (!res.Success)
            {
                var failMsg = $"(step failed) {step.Skill}: {res.Error}";
                return await _thoughtProcessor.ProcessThoughtAsync(failMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
            }

            var output = string.IsNullOrWhiteSpace(res.Output) ? "(ok)" : res.Output;
            sb.AppendLine($"Step {stepIndex}: {step.Skill} -> {output}");
        }

        var resultMsg = sb.Length > 0 ? sb.ToString().TrimEnd() : "Plan executed.";
        return await _thoughtProcessor.ProcessThoughtAsync(resultMsg, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy);
    }

    private static bool IsAllowed(string skillName)
    {
        _ = skillName; // Suppress IDE0060
        return true; // integRate Safety if desired
    }

    private static string GetHelp() => @"
Executor:
  - status                  : availability
  - help                    : this help
  - execute {plan json}     : execute the provided Planner plan JSON
  - {plan json}             : same as above (if input starts with '{')
Usage:
  1) Ask Planner for a plan (natural language or intent JSON)
  2) Pass the plan JSON here to run it
".Trim();
}
