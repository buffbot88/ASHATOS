namespace Abstractions;

public interface IAILanguageModule
{
    Task<ModuleResponse> GenerateAsync(string intent, string context, string language, Dictionary<string, object>? metadata);
}
