using RaAI.Handlers.Manager;
using RaAI.Modules.PlanningModule;
using RaAI.Modules.SkillsModule;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.ExecutionModule
{
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
            var plan = JsonSerializer.Deserialize<PlannerModule.Plan>(input);
            if (plan == null) return "(invalid plan)";

            var skills = _manager?.GetModuleInstanceByName("Skills") as SkillRegistryModule;
            if (skills == null) return "(skills registry unavailable)";

            var sb = new StringBuilder();
            int stepIndex = 0;

            foreach (var step in plan.Steps)
            {
                stepIndex++;

                if (!IsAllowed(step.Skill)) return $"(blocked by safety) {step.Skill}";

                var skill = skills.List().FirstOrDefault(s => s.Name.Equals(step.Skill, StringComparison.OrdinalIgnoreCase));
                if (skill == null) return $"(skill not found) {step.Skill}";

                var res = Task.Run(() => skill.InvokeAsync(step.ArgumentsJson ?? "{}", CancellationToken.None)).GetAwaiter().GetResult();
                if (!res.Success) return $"(step failed) {step.Skill}: {res.Error}";

                sb.AppendLine($"Step {stepIndex}: {step.Skill} -> {res.Output}");
            }

            var outText = sb.Length > 0 ? sb.ToString().TrimEnd() : "Plan executed.";
            return outText;
        }

        private bool IsAllowed(string skillName) => true; // integrate Safety if desired
    }
}