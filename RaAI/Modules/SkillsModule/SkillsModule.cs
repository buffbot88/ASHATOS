using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SkillsModule
{
    public interface ISkill
    {
        string Name { get; }
        string Description { get; }
        // Simple schema description for arguments (could be JSON Schema)
        string? ParametersSchema { get; }

        Task<SkillResult> InvokeAsync(string argumentsJson, CancellationToken ct = default);
    }

    public sealed class SkillResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string? Error { get; set; }
        public string? Json { get; set; } // optional structured payload
    }
}