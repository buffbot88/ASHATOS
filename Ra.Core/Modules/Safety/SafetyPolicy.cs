using System;
using System.Collections.Generic;

namespace Ra.Core.Modules.Safety;

[Flags]
public enum HarmType
{
    None = 0,
    Physical = 1 << 0,
    Privacy = 1 << 1,
    Financial = 1 << 2,
    Psychological = 1 << 3,
    Environmental = 1 << 4,
    Security = 1 << 5
}

public sealed class SafetyPolicy
{
    public double BlockThreshold { get; set; } = 0.6;   // risk >= blocks
    public double ConfirmThreshold { get; set; } = 0.25; // risk >= confirm (else allow)
    public Dictionary<string, (HarmType harms, double severity)> SkillDefaults { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // Examples; extend per skill
        ["System.Open"] = (HarmType.None, 0.05),
        ["Device.Control"] = (HarmType.Physical | HarmType.Security, 0.6),
        ["File.Delete"] = (HarmType.Financial | HarmType.Privacy, 0.7),
        ["Net.Request"] = (HarmType.Privacy | HarmType.Security, 0.5),
        ["Chat.Answer"] = (HarmType.None, 0.05),
    };

    // Default for unknown skills
    public (HarmType harms, double severity) UnknownSkillDefault { get; set; } = (HarmType.Privacy | HarmType.Security, 0.5);
}
