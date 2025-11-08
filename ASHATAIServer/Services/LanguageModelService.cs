using System.Security.Cryptography;
using System.Text;

namespace ASHATAIServer.Services;

/// <summary>
/// Service for managing and processing .gguf language model files
/// </summary>
public class LanguageModelService
{
    private readonly string _modelsDirectory;
    private readonly ILogger<LanguageModelService> _logger;
    private readonly Dictionary<string, ModelInfo> _loadedModels = new();
    private readonly Dictionary<string, FailedModel> _failedModels = new();

    public LanguageModelService(ILogger<LanguageModelService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _modelsDirectory = configuration["ModelsDirectory"] ?? "models";
        
        // Ensure models directory exists
        if (!Directory.Exists(_modelsDirectory))
        {
            Directory.CreateDirectory(_modelsDirectory);
            _logger.LogInformation("Created models directory: {Directory}", _modelsDirectory);
        }
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting Language Model Processor for ASHATAIServer...");
        await ScanAndLoadModelsAsync();
        
        // Attempt to heal failed models
        if (_failedModels.Any())
        {
            await HealFailedModelsAsync();
        }
        
        _logger.LogInformation("Language Model Processor initialization complete. Loaded: {Count}, Failed: {FailedCount}",
            _loadedModels.Count, _failedModels.Count);
    }

    public async Task ScanAndLoadModelsAsync()
    {
        _loadedModels.Clear();
        _failedModels.Clear();

        if (!Directory.Exists(_modelsDirectory))
        {
            _logger.LogWarning("Models directory does not exist: {Directory}", _modelsDirectory);
            return;
        }

        var ggufFiles = Directory.GetFiles(_modelsDirectory, "*.gguf", SearchOption.TopDirectoryOnly);
        _logger.LogInformation("Found {Count} .gguf files in {Directory}", ggufFiles.Length, _modelsDirectory);

        foreach (var filePath in ggufFiles)
        {
            await ProcessModelFileAsync(filePath);
        }
    }

    private async Task ProcessModelFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                _failedModels[fileName] = new FailedModel
                {
                    FileName = fileName,
                    Path = filePath,
                    FailureType = ModelFailureType.FileNotFound,
                    ErrorMessage = "File not found",
                    AttemptCount = 0
                };
                _logger.LogError("Model file not found: {FilePath}", filePath);
                return;
            }

            // Validate GGUF format
            var isValid = await ValidateGGUFFormatAsync(filePath);
            if (!isValid)
            {
                _failedModels[fileName] = new FailedModel
                {
                    FileName = fileName,
                    Path = filePath,
                    FailureType = ModelFailureType.CorruptFile,
                    ErrorMessage = "Invalid GGUF magic number - file may be corrupted",
                    AttemptCount = 0
                };
                _logger.LogError("Invalid GGUF format: {FilePath}", filePath);
                return;
            }

            // Calculate checksum
            var checksum = await CalculateChecksumAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            var modelInfo = new ModelInfo
            {
                FileName = fileName,
                Path = filePath,
                SizeBytes = fileInfo.Length,
                Checksum = checksum,
                LoadedAt = DateTime.UtcNow
            };

            _loadedModels[fileName] = modelInfo;
            _logger.LogInformation("Successfully loaded model: {FileName} ({Size} MB)", 
                fileName, (fileInfo.Length / 1024.0 / 1024.0).ToString("F2"));
        }
        catch (Exception ex)
        {
            _failedModels[fileName] = new FailedModel
            {
                FileName = fileName,
                Path = filePath,
                FailureType = ModelFailureType.ProcessingError,
                ErrorMessage = ex.Message,
                AttemptCount = 0
            };
            _logger.LogError(ex, "Error processing model: {FilePath}", filePath);
        }
    }

    private async Task<bool> ValidateGGUFFormatAsync(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[4];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 4));
            if (bytesRead < 4)
                return false;
            var magic = Encoding.ASCII.GetString(buffer);
            return magic == "GGUF";
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> CalculateChecksumAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        // For performance, calculate checksum on first 1MB
        var bufferSize = (int)Math.Min(1024 * 1024, stream.Length);
        var buffer = new byte[bufferSize];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));
        
        // Use only the bytes that were actually read
        var hash = SHA256.HashData(buffer.AsSpan(0, bytesRead));
        return Convert.ToHexString(hash);
    }

    private async Task HealFailedModelsAsync()
    {
        _logger.LogInformation("Attempting to heal {Count} failed models...", _failedModels.Count);
        
        var modelsToHeal = _failedModels.Values.Where(m => m.AttemptCount < 3).ToList();
        
        foreach (var failedModel in modelsToHeal)
        {
            failedModel.AttemptCount++;
            _logger.LogInformation("Healing attempt {Attempt}/3 for: {FileName}", 
                failedModel.AttemptCount, failedModel.FileName);

            if (failedModel.FailureType == ModelFailureType.FileNotFound)
            {
                _logger.LogWarning("Cannot heal missing file: {FileName}", failedModel.FileName);
                continue;
            }

            // Retry processing
            await ProcessModelFileAsync(failedModel.Path);
            
            // Check if healing succeeded
            if (_loadedModels.ContainsKey(failedModel.FileName))
            {
                _failedModels.Remove(failedModel.FileName);
                _logger.LogInformation("Successfully healed model: {FileName}", failedModel.FileName);
            }
        }
    }

    public ModelStatus GetStatus()
    {
        return new ModelStatus
        {
            ModelsDirectory = _modelsDirectory,
            LoadedModels = _loadedModels.Values.ToList(),
            FailedModels = _failedModels.Values.ToList()
        };
    }

    public async Task<ProcessingResult> ProcessPromptAsync(string prompt, string? modelName = null)
    {
        // If no model specified, use the first loaded model
        var model = string.IsNullOrEmpty(modelName) 
            ? _loadedModels.Values.FirstOrDefault()
            : _loadedModels.Values.FirstOrDefault(m => m.FileName == modelName);

        if (model == null)
        {
            return new ProcessingResult
            {
                Success = false,
                Error = "No models available. Please load a .gguf model file.",
                ModelUsed = null,
                Response = null
            };
        }

        // Simulate AI processing (in a real implementation, this would use llama.cpp or similar)
        _logger.LogInformation("Processing prompt with model: {ModelName}", model.FileName);
        
        await Task.Delay(100); // Simulate processing time
        
        return new ProcessingResult
        {
            Success = true,
            ModelUsed = model.FileName,
            Response = $"[ASHATAIServer Response using {model.FileName}]\n\n" +
                      $"I have processed your prompt: \"{prompt}\"\n\n" +
                      $"This is a placeholder response. In production, this would be generated by the language model.",
            ProcessingTimeMs = 100
        };
    }
}

public class ModelInfo
{
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; }
}

public class FailedModel
{
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public ModelFailureType FailureType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
}

public enum ModelFailureType
{
    FileNotFound,
    CorruptFile,
    ProcessingError
}

public class ModelStatus
{
    public string ModelsDirectory { get; set; } = string.Empty;
    public List<ModelInfo> LoadedModels { get; set; } = new();
    public List<FailedModel> FailedModels { get; set; } = new();
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ModelUsed { get; set; }
    public string? Response { get; set; }
    public long ProcessingTimeMs { get; set; }
}
