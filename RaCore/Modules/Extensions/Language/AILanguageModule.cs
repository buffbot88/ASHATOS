using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Core.LanguageModelProcessor;

namespace RaCore.Modules.Extensions.Language;

[RaModule(Category = "extensions")]
public sealed class AILanguageModule : ModuleBase, IDisposable
{
    private string? _modelPath;
    private int _contextSize = 2048;
    private int _maxTokens = 128;
    private ModuleManager? _manager;
    private string _modelsDirectory = "models";

    public static string Description => AILanguageConstants.Description;
    public static IReadOnlyList<string> SupportedCommands => AILanguageConstants.SupportedCommands;
    public static IReadOnlyList<string> SupportedLanguages => AILanguageConstants.SupportedLanguages;
    public string Status => File.Exists(_modelPath ?? "") ? "ready" : "not loaded";
    public Dictionary<string, object> Capabilities => new()
    {
        { "contextSize", _contextSize },
        { "modelPath", _modelPath ?? "" },
        { "maxTokens", _maxTokens }
    };

    public override string Name => "AILanguage";

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = (ModuleManager?)manager;
        
        // Try to auto-detect model if not set
        if (_modelPath == null)
        {
            var availableModels = ScanForModels();
            if (availableModels.Count > 0)
            {
                _modelPath = availableModels[0];
                LogInfo($"Auto-detected model: {Path.GetFileName(_modelPath)}");
            }
            else
            {
                // Fallback to default path for backward compatibility
                _modelPath = Path.Combine("models", "llama-2-7b-chat.Q4_K_M.gguf");
            }
        }
        
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
        if (!File.Exists(_modelPath ?? ""))
        {
            LogWarn($"Model file not found at: {_modelPath}");
            var availableModels = ScanForModels();
            if (availableModels.Count > 0)
            {
                LogInfo($"Found {availableModels.Count} available model(s) in {_modelsDirectory}:");
                foreach (var model in availableModels)
                {
                    LogInfo($"  - {Path.GetFileName(model)}");
                }
                LogInfo("Use 'list-models' to see all models or 'select-model <name>' to choose one.");
            }
            else
            {
                LogWarn("AILanguage module will not be functional until a model is configured.");
                LogInfo("Use 'set-model <path>' to configure model path.");
            }
        }
        
        if (File.Exists(_modelPath ?? ""))
        {
            LogInfo("AILanguage module initialized successfully and ready.");
            LogInfo("Note: This module now uses the LanguageModelProcessor system for .gguf model handling.");
        }
        else
        {
            LogWarn("AILanguage module initialization incomplete - missing model file.");
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

        if (!File.Exists(_modelPath ?? ""))
        {
            var errorMsg = $"(error: model not found)\nModel: {_modelPath}";
            LogError(errorMsg);
            LogError("Please configure AILanguage module using 'set-model' command.");
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
                Text = "Commands: help, status, reload, list-models, select-model <name-or-number>, set-model <path>, set-context <n>, set-tokens <n>\nNote: This module now uses the LanguageModelProcessor system.",
                Type = "help",
                Status = "ok",
                Language = "en",
                Metadata = Capabilities
            };

        if (text.Equals("list-models", StringComparison.OrdinalIgnoreCase))
        {
            var modelsList = GetAvailableModelsText();
            return new ModuleResponse
            {
                Text = modelsList,
                Type = "list-models",
                Status = "ok",
                Language = "en",
                Metadata = Capabilities
            };
        }

        if (text.StartsWith("select-model ", StringComparison.OrdinalIgnoreCase))
        {
            var arg = text["select-model ".Length..].Trim();
            if (!string.IsNullOrWhiteSpace(arg))
            {
                var models = ScanForModels();
                if (models.Count == 0)
                {
                    return new ModuleResponse
                    {
                        Text = $"No .gguf models found in {_modelsDirectory}.",
                        Type = "error",
                        Status = "error",
                        Language = "en"
                    };
                }

                string? selectedModel = null;

                // Try to parse as number first
                if (int.TryParse(arg, out var index) && index >= 1 && index <= models.Count)
                {
                    selectedModel = models[index - 1];
                }
                else
                {
                    // Try to match by filename (partial or full)
                    foreach (var model in models)
                    {
                        var fileName = Path.GetFileName(model);
                        if (fileName.Equals(arg, StringComparison.OrdinalIgnoreCase) ||
                            fileName.Contains(arg, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedModel = model;
                            break;
                        }
                    }
                }

                if (selectedModel != null)
                {
                    _modelPath = selectedModel;
                    LogInfo($"Model selected: {Path.GetFileName(_modelPath)}");
                    Initialize(_manager!);
                    
                    return new ModuleResponse
                    {
                        Text = Status == "ready" 
                            ? $"Model switched to {Path.GetFileName(_modelPath)} and loaded successfully." 
                            : $"Model set to {Path.GetFileName(_modelPath)} but failed to load.",
                        Type = "select-model",
                        Status = Status == "ready" ? "ok" : "error",
                        Language = "en",
                        Metadata = Capabilities
                    };
                }
                else
                {
                    return new ModuleResponse
                    {
                        Text = $"Model not found: {arg}\n\n{GetAvailableModelsText()}",
                        Type = "error",
                        Status = "error",
                        Language = "en"
                    };
                }
            }
            
            return new ModuleResponse
            {
                Text = "Usage: select-model <name-or-number>\n\n" + GetAvailableModelsText(),
                Type = "error",
                Status = "error",
                Language = "en"
            };
        }

        if (text.StartsWith("status", StringComparison.OrdinalIgnoreCase))
            return new ModuleResponse
            {
                Text = $"AILanguageModule: {Status}\nModel path: {_modelPath}\nContext size: {_contextSize}\nMax tokens: {_maxTokens}\nNote: Using LanguageModelProcessor system",
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
            LogWarn("set-exe command is no longer supported. The module now uses LanguageModelProcessor.");
            return new ModuleResponse
            {
                Text = "The set-exe command is deprecated. AILanguage module now uses the LanguageModelProcessor system which handles .gguf files directly without requiring llama.cpp executables.",
                Type = "deprecated",
                Status = "error",
                Language = "en"
            };
        }

        // Default: Delegate to LanguageModelProcessor
        LogInfo("AILanguage module text generation is deprecated. Please use the LanguageModelProcessor module directly.");
        return new ModuleResponse
        {
            Text = "Text generation via AILanguage module is deprecated. This module now serves as a compatibility layer. Use the LanguageModelProcessor module for .gguf model processing.",
            Type = "deprecated",
            Status = "ok",
            Language = "en",
            Metadata = Capabilities
        };
    }

    public async Task<ModuleResponse> GenerateAsync(string intent, string context, string language = "en", Dictionary<string, object>? metadata = null)
    {
        LogInfo($"GenerateAsync called with intent: {intent}, language: {language}");
        LogInfo("Note: AILanguage module generation is deprecated. Use LanguageModelProcessor instead.");
        
        if (!File.Exists(_modelPath ?? ""))
        {
            var errorMsg = $"(error: model not found)\nModel: {_modelPath}";
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

        // Return deprecation notice
        return new ModuleResponse
        {
            Text = "Text generation via AILanguage module is deprecated. This module now serves as a compatibility layer. Use the LanguageModelProcessor module for .gguf model processing.",
            Type = "deprecated",
            Status = "ok",
            Language = language,
            Metadata = metadata ?? Capabilities
        };
    }

    /// <summary>
    /// Scans the models directory for all available .gguf model files.
    /// </summary>
    /// <returns>List of full paths to .gguf files found</returns>
    private List<string> ScanForModels()
    {
        var models = new List<string>();
        
        try
        {
            // Try to get full path of models directory
            string modelsDir;
            try
            {
                modelsDir = Path.GetFullPath(_modelsDirectory);
            }
            catch
            {
                modelsDir = _modelsDirectory;
            }
            
            if (Directory.Exists(modelsDir))
            {
                var ggufFiles = Directory.GetFiles(modelsDir, "*.gguf", SearchOption.TopDirectoryOnly);
                models.AddRange(ggufFiles);
                LogInfo($"Scanned {modelsDir} and found {models.Count} .gguf model(s).");
            }
            else
            {
                LogInfo($"Models directory not found: {modelsDir}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error scanning for models: {ex.Message}");
        }
        
        return models;
    }

    /// <summary>
    /// Gets a formatted list of available models.
    /// </summary>
    /// <returns>Formatted string listing all available models</returns>
    private string GetAvailableModelsText()
    {
        var models = ScanForModels();
        
        if (models.Count == 0)
        {
            return $"No .gguf models found in {_modelsDirectory}.\nPlace your model files there or use 'set-model <path>' to specify a custom location.";
        }
        
        var result = $"Available models ({models.Count} found in {_modelsDirectory}):\n";
        for (int i = 0; i < models.Count; i++)
        {
            var fileName = Path.GetFileName(models[i]);
            var isCurrentModel = models[i].Equals(_modelPath, StringComparison.OrdinalIgnoreCase);
            result += $"  {i + 1}. {fileName}{(isCurrentModel ? " (current)" : "")}\n";
        }
        result += "\nUse 'select-model <name-or-number>' to switch to a different model.";
        
        return result;
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override void OnSystemEvent(string name, object? payload = null) { }

}
