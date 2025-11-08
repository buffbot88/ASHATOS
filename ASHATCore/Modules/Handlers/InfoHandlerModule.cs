using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Handlers;

/// <summary>
/// Handles informational output with agentic/multi-language support.
/// </summary>
public class InfoHandlerModule : IHandlerModule, IDisposable
{
    public string Name => "InfoHandler";
    private ModuleManager? _manager;
    private IAILanguageModule? _aiLang;

    public void Initialize(ModuleManager manager)
    {
        _manager = manager;
        _aiLang = manager.GetModuleInstanceByName("AILanguage") as IAILanguageModule;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        if (_aiLang == null)
            return new ModuleResponse { Text = input, Type = "info", Status = "ok" };

        var resp = await _aiLang.GenerateAsync("info", input, "en", null);
        HandlerDiagnostics.RaiseInfoHandled(input, resp.Text ?? string.Empty);
        return resp;
    }

    public string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text ?? string.Empty;
    }

    public void Dispose()
    {
        // No unmanaged resources to clean up, but follow IDisposable pattern
        GC.SuppressFinalize(this);
    }
}
