namespace ASHATCore.Modules.Subconscious;

public static class SubconsciousExtensions
{
    public static string SummarizeProbeHistory(this IEnumerable<string> probes)
    {
        if (probes == null) return "No probe history.";
        var lastFew = probes.Reverse().Take(5).ToList();
        return lastFew.Count == 0 ? "No probe history." : $"Recent subconscious probes: {string.Join("; ", lastFew)}";
    }

    public static string ToAgenticPrompt(this string input)
    {
        return $"[INST] <<SYS>>\nSubconscious agent: process and respond helpfully.\n<</SYS>>\n{input} [/INST]";
    }
}
