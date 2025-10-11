using System.Collections.Generic;

namespace ASHATCore.Modules.Extensions.Planning;

/// <summary>
/// Data type for agentic plan (goal and steps).
/// </summary>
public sealed class Plan
{
    public string Goal { get; set; } = "";
    public List<Step> Steps { get; set; } = new();
}

/// <summary>
/// Data type for a single skill/action step in a plan.
/// </summary>
public sealed class Step
{
    public string Skill { get; set; } = "";
    public string ArgumentsJson { get; set; } = "{}";
}
