using RaCore.Modules.Conscious;
using System.Text.Json;
using RaCore.Modules.Extensions.Planning;

using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Safety;

[RaModule(Category = "extensions")]
public sealed class EthicsGuardModule : ModuleBase
{
    public override string Name => "EthicsGuard";

    private readonly SafetyPolicy _policy = new();
    private ModuleManager? _manager;
    private ThoughtProcessor? _thoughtProcessor;

    // Change the method signature to match the base class:
    // The base class ModuleBase defines: public virtual void Initialize(object? manager);
    // So, override with the same signature.

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        if (_manager != null)
            _thoughtProcessor = new ThoughtProcessor(_manager);
    }

    // Input: PlannerModule.Plan JSON
    // Output is always routed through ThoughtProcessor
    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        Plan? plan;
        try { plan = JsonSerializer.Deserialize<Plan>(input); }
        catch
        {
            return await ThoughtOutputAsync("BLOCKED: invalid plan json", Mood.Confused);
        }
        if (plan == null || plan.Steps == null || plan.Steps.Count == 0)
            return await ThoughtOutputAsync("BLOCKED: empty plan", Mood.Confused);

        double maxRisk = 0;
        var analyses = plan.Steps.Select(step =>
        {
            var def = _policy.SkillDefaults.TryGetValue(step.Skill, out var d) ? d : _policy.UnknownSkillDefault;
            var risk = RiskScorer.ScoreStepRisk(step.Skill, step.ArgumentsJson ?? "{}", def);
            maxRisk = Math.Max(maxRisk, risk);
            return new { step.Skill, Risk = risk, Harm = def.harms.ToString(), Severity = def.severity };
        }).ToArray();

        var consent = (_manager?.GetModuleInstanceByName("Consent") as ConsentRegistryModule);
        bool missingConsent = plan.Steps.Any(s =>
        {
            var (harms, severity) = _policy.SkillDefaults.TryGetValue(s.Skill, out var d) ? d : _policy.UnknownSkillDefault;
            var requires = severity >= 0.3;
            return requires && (consent == null || !consent.HasConsent(s.Skill));
        });

        var analysisJson = JsonSerializer.Serialize(analyses);

        if (maxRisk >= _policy.BlockThreshold)
            return await ThoughtOutputAsync($"BLOCKED: violates 'harm none' (risk={maxRisk:F2}). Steps={analysisJson}", Mood.Confused);

        if (maxRisk >= _policy.ConfirmThreshold || missingConsent)
            return await ThoughtOutputAsync($"CONFIRM: action requires human approval (risk={maxRisk:F2}, consent={(missingConsent ? "missing" : "ok")}).\n{input}", Mood.Thinking);

        return await ThoughtOutputAsync($"APPROVED:{input}", Mood.Happy);
    }

    private async Task<ModuleResponse> ThoughtOutputAsync(string text, Mood mood)
    {
        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = text, Type = "status", Status = "ok" };
        return await _thoughtProcessor.ProcessThoughtAsync(text, null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), mood);
    }
}
