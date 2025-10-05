using RaCore.Modules.Extensions.Language;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Handlers;

/// <summary>
/// Handles errors and exceptions, returning agentic output.
/// </summary>
public class ErrorHandlerModule : IHandlerModule, IDisposable
{
    public string Name => "ErrorHandler";
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
            return new ModuleResponse { Text = $"ERROR: {input}", Type = "error", Status = "error" };

        var resp = await _aiLang.GenerateAsync("error", input, "en", null);
        HandlerDiagnostics.RaiseErrorHandled(input, resp.Text);
        return resp;
    }

    public string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
