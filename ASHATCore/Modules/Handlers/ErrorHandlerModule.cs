using Abstractions;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Extensions;
using ASHATCore.Modules.Speech;
using System.Threading;

namespace ASHATCore.Modules.Handlers;

/// <summary>
/// Handles errors and exceptions, returning agentic output.
/// </summary>
public class ErrorHandlerModule : IDisposable
{
    public string Name => "ErrorHandler";
    private LlamaCppProcessor? _aiLang;

    public void Initialize(ModuleManager manager)
    {
        _aiLang = manager.GetModuleInstanceByName("LlamaCppProcessor") as LlamaCppProcessor;
    }

    public async Task<ModuleResponse> Process(string input, CancellationToken cancellationToken)
    {
        input = input?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(input))
        {
            return new ModuleResponse
            {
                Text = "ERROR: Input is empty",
                Type = "error",
                Status = "error"
            };
        }

        if (_aiLang == null)
        {
            return new ModuleResponse
            {
                Text = $"ERROR: AI Language module not initialized for input '{input}'",
                Type = "error",
                Status = "error"
            };
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // _aiLang.GenerateAsync returns string
            string aiResponse = await _aiLang.GenerateAsync(input);

            if (string.IsNullOrEmpty(aiResponse))
            {
                Console.WriteLine($"[Process] Warning: AI returned null or empty for input '{input}'");
                aiResponse = "(No response from AI)";
            }

            // Log diagnostics
            HandlerDiagnostics.RaiseErrorHandled(input, aiResponse);

            // Wrap the string into a ModuleResponse and return
            return new ModuleResponse
            {
                Text = aiResponse,
                Type = "response",
                Status = "success"
            };
        }
        catch (OperationCanceledException)
        {
            return new ModuleResponse
            {
                Text = "Operation cancelled",
                Type = "error",
                Status = "cancelled"
            };
        }
        catch (Exception ex)
        {
            return new ModuleResponse
            {
                Text = $"Exception in Process: {ex.Message}",
                Type = "error",
                Status = "error"
            };
        }
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
