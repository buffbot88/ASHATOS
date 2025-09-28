using RaAI.Handlers.Manager;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SkillsModule
{
    // [RaModule("System.Open")]
    public sealed class SystemOpenSkillModule : ModuleBase, ISkill
    {
        public override string Name => "System.Open";

        public string Description => "Open a file, URL, or application on the host system";
        public string? ParametersSchema => "{\"type\":\"object\",\"properties\":{\"intent\":{\"type\":\"string\"},\"slots\":{\"type\":\"object\"}}}";

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            LogInfo("System.Open skill ready.");
        }

        // Optional: simple help/status via Process
        public override string Process(string input)
        {
            var t = (input ?? "").Trim();
            if (t.Equals("help", System.StringComparison.OrdinalIgnoreCase))
                return "System.Open: invoke via agentic pipeline (do open <target>) or Skills registry.";
            if (t.Equals("status", System.StringComparison.OrdinalIgnoreCase))
                return "System.Open: available";
            return "Unknown command. Try: help | status";
        }

        public Task<SkillResult> InvokeAsync(string argumentsJson, CancellationToken ct = default)
        {
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var root = doc.RootElement;

                // Expecting: { intent:"system.open", slots:{ target:"..." }, ts:... }
                string? target = null;
                if (root.TryGetProperty("slots", out var slots) && slots.TryGetProperty("target", out var tgtEl))
                    target = tgtEl.GetString();

                // Fallback: allow direct invocation with {"target":"..."}
                if (string.IsNullOrWhiteSpace(target) && root.TryGetProperty("target", out var direct))
                    target = direct.GetString();

                if (string.IsNullOrWhiteSpace(target))
                    return Task.FromResult(new SkillResult { Success = false, Error = "missing target" });

                // Fully qualify to avoid shadowing by this class's Process(string) method
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });

                return Task.FromResult(new SkillResult
                {
                    Success = true,
                    Output = $"Opened: {target}"
                });
            }
            catch (System.Exception ex)
            {
                return Task.FromResult(new SkillResult { Success = false, Error = ex.Message });
            }
        }
    }
}