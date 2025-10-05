namespace RaCore.Modules.Extensions.Skills;

public interface ISkill
{
    string Name { get; }
    string Description { get; }
    string? ParametersSchema { get; }

    Task<SkillResult> InvokeAsync(string argumentsJson, CancellationToken ct = default);
}

public sealed class SkillResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string? Error { get; set; }
    public string? Json { get; set; }
}
