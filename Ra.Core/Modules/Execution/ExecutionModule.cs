using Ra.Core.Engine.Manager;
using Ra.Core.Modules.Planning;
using Ra.Core.Modules.Skills;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ra.Core.Modules.Execution;

// [RaModule("Executor")]
public sealed class ExecutorModule : ModuleBase
{
    public override string Name => "Executor";
    private ModuleManager? _manager;

    public override void Initialize(ModuleManager manager)
    {
        base.Initialize(manager);
        _manager = manager;
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        // Friendly routes for FeatureExplorer and manual use
        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            return GetHelp();
        if (text.Equals("status", StringComparison.OrdinalIgnoreCase))
            return "Executor: available";

        // Allow "execute {json}" prefix
        const string execPrefix = "execute ";
        if (text.StartsWith(execPrefix, StringComparison.OrdinalIgnoreCase))
            text = text.Substring(execPrefix.Length).Trim();

        // Require JSON-looking input; avoid throwing on plain words like "status"
        if (!(text.StartsWith("{") || text.StartsWith("[")))
            return "(usage) Executor expects a Planner plan JSON. Generate one with Planner, then pass it here.";

        PlannerModule.Plan? plan;
        try
        {
            plan = JsonSerializer.Deserialize<PlannerModule.Plan>(text);
        }
        catch (JsonException jex)
        {
            return $"(invalid plan json) {jex.Message}";
        }
        catch (Exception ex)
        {
            return $"(executor error while parsing) {ex.Message}";
        }

        if (plan == null) return "(invalid plan)";
        if (plan.Steps == null || plan.Steps.Count == 0) return "(empty plan)";

        var skills = _manager?.GetModuleInstanceByName("Skills") as SkillRegistryModule;
        if (skills == null) return "(skills registry unavailable)";

        var sb = new StringBuilder();
        int stepIndex = 0;

        foreach (var step in plan.Steps)
        {
            stepIndex++;

            if (!IsAllowed(step.Skill))
                return $"(blocked by safety) {step.Skill}";

            var skill = skills.List().FirstOrDefault(s => s.Name.Equals(step.Skill, StringComparison.OrdinalIgnoreCase));
            if (skill == null)
                return $"(skill not found) {step.Skill}";

            SkillResult res;
            try
            {
                // Ensure non-null arguments JSON
                var args = string.IsNullOrWhiteSpace(step.ArgumentsJson) ? "{}" : step.ArgumentsJson;
                res = Task.Run(() => skill.InvokeAsync(args, CancellationToken.None)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return $"(step crashed) {step.Skill}: {ex.Message}";
            }

            if (!res.Success)
                return $"(step failed) {step.Skill}: {res.Error}";

            var output = string.IsNullOrWhiteSpace(res.Output) ? "(ok)" : res.Output;
            sb.AppendLine($"Step {stepIndex}: {step.Skill} -> {output}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "Plan executed.";
    }

    private bool IsAllowed(string skillName) => true; // integrate Safety if desired

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
