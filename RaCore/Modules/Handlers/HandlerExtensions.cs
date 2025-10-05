using System;

namespace RaCore.Modules.Handlers;

/// <summary>
/// Static helpers and extension methods for handler modules.
/// </summary>
public static class HandlerExtensions
{
    public static string ToAgenticInfoPrompt(this string input)
    {
        return $"[INST] <<SYS>>\nYou are an informative assistant. Respond clearly.\n<</SYS>>\n{input} [/INST]";
    }

    public static string ToAgenticErrorPrompt(this string input)
    {
        return $"[INST] <<SYS>>\nYou are an error handler. Explain the problem helpfully.\n<</SYS>>\n{input} [/INST]";
    }
}
