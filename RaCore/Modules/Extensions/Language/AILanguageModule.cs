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
        try 
        { 
            _modelPath = Path.GetFullPath(_modelPath!); 
            LogInfo($"AILanguage module initializing with model path: {_modelPath}");
        } 
        catch (Exception ex)
        { 
            LogError($"Failed to resolve model path: {ex.Message}");
        }
        
        // Validate configuration
        if (!File.Exists(_llamaExePath))
        {
            LogWarn($"llama.cpp executable not found at: {_llamaExePath}");
            LogWarn("AILanguage module will not be functional until llama.cpp is configured.");
            LogInfo("Use 'set-exe <path>' to configure llama.cpp executable path.");
        }
        
        if (!File.Exists(_modelPath ?? ""))
        {
            LogWarn($"Model file not found at: {_modelPath}");
            LogWarn("AILanguage module will not be functional until a model is configured.");
            LogInfo("Use 'set-model <path>' to configure model path.");
        }
        
        if (File.Exists(_llamaExePath) && File.Exists(_modelPath ?? ""))
        {
            LogInfo("AILanguage module initialized successfully and ready.");
        }
        else
        {
            LogError("AILanguage module initialization incomplete - missing dependencies.");
        }
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text ?? "";
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? "").Trim();
        LogInfo($"Processing request: {(text.Length > 50 ? text.Substring(0, 50) + "..." : text)}");

        if (!File.Exists(_llamaExePath) || !File.Exists(_modelPath ?? ""))
        {
            var errorMsg = $"(error: llama.cpp executable or model not found)\nExe: {_llamaExePath}\nModel: {_modelPath}";
            LogError(errorMsg);
            LogError("Please configure AILanguage module using 'set-exe' and 'set-model' commands.");
            return new ModuleResponse
            {
                Text = errorMsg,
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
                LogInfo($"Model path set to: {_modelPath}");
                Initialize(_manager!);
                if (Status == "ready")
                {
                    LogInfo("Model loaded successfully.");
                }
                else
                {
                    LogError("Failed to load model - file may not exist or be inaccessible.");
                }
                return new ModuleResponse
                {
                    Text = Status == "ready" ? $"Model set to {_modelPath} and loaded." : "Failed to load model.",
                    Type = "set-model",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            LogWarn("set-model command called without a valid path.");
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
                LogInfo($"Context size set to {_contextSize}.");
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
            LogWarn($"Invalid context size: {arg}. Must be between 1 and 8192.");
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
                LogInfo($"Max tokens set to {_maxTokens}.");
                return new ModuleResponse
                {
                    Text = $"Max tokens set to {_maxTokens}.",
                    Type = "set-tokens",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            LogWarn($"Invalid max tokens: {arg}. Must be between 1 and 2048.");
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
                LogInfo($"llama.cpp executable path set to {_llamaExePath}.");
                if (Status == "ready")
                {
                    LogInfo("AILanguage module is now ready.");
                }
                else
                {
                    LogWarn("AILanguage module still not ready - check model path.");
                }
                return new ModuleResponse
                {
                    Text = $"llama.cpp executable path set to {_llamaExePath}.",
                    Type = "set-exe",
                    Status = Status == "ready" ? "ok" : "error",
                    Language = "en",
                    Metadata = Capabilities
                };
            }
            LogWarn($"Invalid executable path: {arg}. File not found.");
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
            LogInfo("Running llama.cpp for text generation...");
            resultText = await RunLlamaCppAsync(text);
            LogInfo($"Text generation completed. Output length: {resultText.Length} characters.");
        }
        catch (Exception ex)
        {
            LogError($"Text generation failed: {ex.GetType().Name}: {ex.Message}");
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
        LogInfo($"GenerateAsync called with intent: {intent}, language: {language}");
        
        if (!File.Exists(_llamaExePath) || !File.Exists(_modelPath ?? ""))
        {
            var errorMsg = $"(error: llama.cpp executable or model not found)\nExe: {_llamaExePath}\nModel: {_modelPath}";
            LogError(errorMsg);
            return new ModuleResponse
            {
                Text = errorMsg,
                Type = "error",
                Status = "error",
                Language = language,
                Metadata = Capabilities
            };
        }

        string prompt = context;
        
        // Apply intent-specific prompting for better natural language responses
        if (intent == "chat" || intent == "think" || intent == "status" || intent == "recall" || intent == "remember")
        {
            // The context already contains the formatted prompt from ThoughtProcessor or SpeechModule
            prompt = context;
        }
        else
        {
            // For other intents, use the context directly
            prompt = context;
        }

        string output;
        try
        {
            LogInfo($"Generating response for intent: {intent}");
            output = await RunLlamaCppAsync(prompt);
            LogInfo($"Generation successful. Output length: {output.Length} characters.");
        }
        catch (Exception ex)
        {
            LogError($"Generation failed: {ex.GetType().Name}: {ex.Message}");
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
        LogInfo("Starting llama.cpp process...");
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
        try
        {
            process.Start();
            LogInfo("llama.cpp process started successfully.");
        }
        catch (Exception ex)
        {
            LogError($"Failed to start llama.cpp process: {ex.Message}");
            throw;
        }

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();
        
        LogInfo($"llama.cpp process exited with code: {process.ExitCode}");

        if (!string.IsNullOrWhiteSpace(error))
        {
            LogWarn($"llama.cpp stderr output: {error.Substring(0, Math.Min(200, error.Length))}...");
            output += $"\n(error log):\n{error}";
        }

        return output.Trim();
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override void OnSystemEvent(string name, object? payload = null) { }

}
