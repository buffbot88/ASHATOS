namespace RaCore.Modules.Extensions.Language;

internal static class AILanguageConstants
{
    public static readonly string Description =
        "Offline llama.cpp-powered language generation with rich context, multi-language, and privacy.";

    public static readonly IReadOnlyList<string> SupportedCommands =
        ["generate", "help", "status", "reload", "set-context", "set-model", "set-exe", "set-tokens"];

    public static readonly IReadOnlyList<string> SupportedLanguages =
        ["en", "fr", "es", "de", "ja", "zh", "ko"];
}
