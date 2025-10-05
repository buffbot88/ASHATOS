using System.Diagnostics;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Language;

[RaModule(Category = "extensions")]
public sealed class AILanguageModule : ModuleBase, IDisposable
{
    private string? _modelPath;
    private string _llamaExePath = "llama.cpp\\main.exe";
    private int _contextSize = 2048;
    private int _maxTokens = 128;
    private ModuleManager? _manager;

    public static string Description => AILanguageConstants.Description;
    public static IReadOnlyList<string> SupportedCommands => AILanguageConstants.SupportedCommands;
    public static IReadOnlyList<string> SupportedLanguages => AILanguageConstants.SupportedLanguages;
    public string Status => File.Exists(_llamaExePath) && File.Exists(_modelPath ?? "") ? "ready" : "not loaded";
    public Dictionary<string, object> Capabilities => new()
    {
        { "contextSize", _contextSize },
        { "modelPath", _modelPath ?? "" },
        { "llamaExePath", _llamaExePath },
        { "maxTokens", _maxTokens }
    };

    public override string Name => "AILanguage";

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = (ModuleManager?)manager;
        _modelPath ??= Path.Combine("llama.cpp", "models", "llama-2-7b-chat.Q4_K_M.gguf");
        try { _modelPath = Path.GetFullPath(_modelPath!); } catch { /* ignore */ }
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text ?? "";
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? "").Trim();

        if (!File.Exists(_llamaExePath) || !File.Exists(_modelPath ?? ""))
        {
            return new ModuleResponse
            {
                Text = $"(error: llama.cpp executable or model not found)\nExe: {_llamaExePath}\nModel: {_modelPath}",
                Type = "error",
                Status = "error",
                Language = "en",
                Metadata = Capabilities
            };
        }

        if (string.IsNullOrWhiteSpace(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            return new ModuleResponse
            {
                Text = "Commands: help, status, reload, set-model <path>, set-context <n>, set-tokens <n>, set-exe <path>",
                Type = "help",
                Status = "ok",
                Language = "en",
                Metadata = Capabilities
            };

        if (text.StartsWith("status", StringComparison.OrdinalIgnoreCase))
            return new ModuleResponse
            {
                Text = $"AILanguageModule: {Status}\nModel path: {_modelPath}\nContext size: {_contextSize}\nMax tokens: {_maxTokens}\nExe: {_llamaExePath}",
                Type = "status",
                Status = Status == "ready" ? "ok" : "error",
                Language = "en",
                Metadata = Capabilities
            };

        if (text.StartsWith("reload", StringComparison.OrdinalIgnoreCase))
        {
            Initialize(_manager!);
            return new ModuleResponse
            {
                Text = Status == "ready" ? $"Reloaded. Model: {_modelPath}" : "Reload failed.",
                Type = "reload",
                Status = Status == "ready" ? "ok" : "error",
                Language = "en",
                Metadata = Capabilities
            };
        }

        if (text.StartsWith("set-model ", StringComparison.OrdinalIgnoreCase))
        {
            var newPath = text["set-model ".Length..].Trim();
            if (!string.IsNullOrWhiteSpace(newPath))
            {
                try { _modelPath = Path.GetFullPath(newPath); } catch { _modelPath = newPath; }
                Initialize(_manager!);
                return new ModuleResponse
                {
                    Text = Status == "ready" ? $"Model set to {_modelPath} and loaded." : "Failed to load model.",
                    Type = "set-model",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            return new ModuleResponse
            {
                Text = "Usage: set-model <path-to-gguf>",
                Type = "error",
                Status = "error",
                Language = "en"
            };
        }

        if (text.StartsWith("set-context ", StringComparison.OrdinalIgnoreCase))
        {
            var arg = text["set-context ".Length..].Trim();
            if (int.TryParse(arg, out var newSize) && newSize > 0 && newSize <= 8192)
            {
                _contextSize = newSize;
                Initialize(_manager!);
                return new ModuleResponse
                {
                    Text = $"Context size set to {_contextSize}.",
                    Type = "set-context",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            return new ModuleResponse
            {
                Text = "Usage: set-context <integer 1-8192>",
                Type = "error",
                Status = "error",
                Language = "en"
            };
        }

        if (text.StartsWith("set-tokens ", StringComparison.OrdinalIgnoreCase))
        {
            var arg = text["set-tokens ".Length..].Trim();
            if (int.TryParse(arg, out var newMax) && newMax > 0 && newMax <= 2048)
            {
                _maxTokens = newMax;
                return new ModuleResponse
                {
                    Text = $"Max tokens set to {_maxTokens}.",
                    Type = "set-tokens",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            return new ModuleResponse
            {
                Text = "Usage: set-tokens <integer 1-2048>",
                Type = "error",
                Status = "error",
                Language = "en"
            };
        }

        if (text.StartsWith("set-exe ", StringComparison.OrdinalIgnoreCase))
        {
            var arg = text["set-exe ".Length..].Trim();
            if (!string.IsNullOrWhiteSpace(arg) && File.Exists(arg))
            {
                _llamaExePath = arg;
                return new ModuleResponse
                {
                    Text = $"llama.cpp executable path set to {_llamaExePath}.",
                    Type = "set-exe",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            return new ModuleResponse
            {
                Text = "Usage: set-exe <path-to-main.exe>",
                Type = "error",
                Status = "error",
                Language = "en"
            };
        }

        // Default: Run llama.cpp for generation
        string resultText;
        try
        {
            resultText = await RunLlamaCppAsync(text);
        }
        catch (Exception ex)
        {
            resultText = $"(error: {ex.GetType().Name}: {ex.Message})";
        }

        return new ModuleResponse
        {
            Text = resultText,
            Type = "generate",
            Status = "ok",
            Language = "en",
            Metadata = Capabilities
        };
    }

    public async Task<ModuleResponse> GenerateAsync(string intent, string context, string language = "en", Dictionary<string, object>? metadata = null)
    {
        if (!File.Exists(_llamaExePath) || !File.Exists(_modelPath ?? ""))
        {
            return new ModuleResponse
            {
                Text = $"(error: llama.cpp executable or model not found)\nExe: {_llamaExePath}\nModel: {_modelPath}",
                Type = "error",
                Status = "error",
                Language = language,
                Metadata = Capabilities
            };
        }

        string prompt = context;
        if (intent == "chat" || intent == "think") // Wrap in agentic chat prompt
            prompt = $"[INST] <<SYS>>\nYou are a helpful assistant.\n<</SYS>>\n{context} [/INST]";

        string output;
        try
        {
            output = await RunLlamaCppAsync(prompt);
        }
        catch (Exception ex)
        {
            output = $"(error: {ex.GetType().Name}: {ex.Message})";
        }

        return new ModuleResponse
        {
            Text = output,
            Type = intent,
            Status = "ok",
            Language = language,
            Metadata = metadata ?? Capabilities
        };
    }

    private async Task<string> RunLlamaCppAsync(string prompt)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _llamaExePath,
            Arguments = $"-m \"{_modelPath}\" -p \"{prompt}\" -n {_maxTokens} --ctx-size {_contextSize}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
        {
            output += $"\n(error log):\n{error}";
        }

        return output.Trim();
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public static void OnSystemEvent(string? name, object? payload = null) { }

}
