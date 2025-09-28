using RaAI.Handlers.Manager;
using RaAI.Modules.PlanningModule;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaAI.Modules.SafetyModule
{
    // [RaModule("EthicsGuard")]
    public sealed class EthicsGuardModule : ModuleBase
    {
        public override string Name => "EthicsGuard";

        private readonly SafetyPolicy _policy = new();
        private ModuleManager? _manager;

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _manager = manager;
        }

        // Input: PlannerModule.Plan JSON
        // Output:
        //  - "APPROVED:<plan json>"
        //  - "CONFIRM:<reason>\n<plan json>"
        //  - "BLOCKED:<reason>"
        public override string Process(string input)
        {
            PlannerModule.Plan? plan;
            try { plan = JsonSerializer.Deserialize<PlannerModule.Plan>(input); }
            catch { return "BLOCKED: invalid plan json"; }
            if (plan == null || plan.Steps == null || plan.Steps.Count == 0)
                return "BLOCKED: empty plan";

            double maxRisk = 0;
            var analyses = plan.Steps.Select(step =>
            {
                var def = _policy.SkillDefaults.TryGetValue(step.Skill, out var d) ? d : _policy.UnknownSkillDefault;
                var risk = RiskScorer.ScoreStepRisk(step.Skill, step.ArgumentsJson ?? "{}", def);
                maxRisk = Math.Max(maxRisk, risk);
                return new { step.Skill, Risk = risk, Harm = def.harms.ToString(), Severity = def.severity };
            }).ToArray();

            // Consent check: if any step requires consent and we don't have it, request confirm
            var consent = (_manager?.GetModuleInstanceByName("Consent") as ConsentRegistryModule);
            bool missingConsent = plan.Steps.Any(s =>
            {
                var def = _policy.SkillDefaults.TryGetValue(s.Skill, out var d) ? d : _policy.UnknownSkillDefault;
                var requires = def.severity >= 0.3; // heuristic
                return requires && (consent == null || !consent.HasConsent(s.Skill));
            });

            var analysisJson = JsonSerializer.Serialize(analyses, new JsonSerializerOptions { WriteIndented = false });

            if (maxRisk >= _policy.BlockThreshold)
                return $"BLOCKED: violates 'harm none' (risk={maxRisk:F2}). Steps={analysisJson}";

            if (maxRisk >= _policy.ConfirmThreshold || missingConsent)
                return $"CONFIRM: action requires human approval (risk={maxRisk:F2}, consent={(missingConsent ? "missing" : "ok")}).\n{input}";

            return $"APPROVED:{input}";
        }
    }
}