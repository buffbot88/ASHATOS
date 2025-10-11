using System.Security.Cryptography;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Core.LanguageModelProcessor;

/// <summary>
/// Language Model Processor for ASHATOS 5.0
/// Detects, validates, and loads .gguf language model files during boot sequence
/// Includes self-healing capabilities for corrupt or failed model loads
/// </summary>
[RaModule(Category = "core")]
public sealed class LanguageModelProcessorModule : ModuleBase
{
    public override string Name => "LanguageModelProcessor";
    
    private ModuleManager? _manager;
    private readonly List<ModelInfo> _loadedModels = new();
    private readonly List<ModelFailure> _modelFailures = new();
    private string _modelsDirectory = "models";
    private bool _initialized = false;
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
    }
    
    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "LanguageModelProcessor: Use 'status', 'scan', 'heal', or 'report'";
        
        var command = input.Trim().ToLowerInvariant();
        
        return command switch
        {
            "status" => GetStatus(),
            "scan" => ScanAndLoadModels(),
            "heal" => AttemptHealingAllFailures(),
            "report" => GeneratedetailedReport(),
            _ => "Unknown command. Use: status, scan, heal, report"
        };
    }
    
    /// <summary>
    /// Main boot sequence integration - scans and loads .gguf models
    /// </summary>
    public async Task<bool> ProcessModelsAsync()
    {
        LogInfo($"Starting Language Model Processor for ASHATOS v{ASHATVersion.Current}...");
        
        try
        {
            // Scan for .gguf files
            var models = await ScanForModelsAsync();
            
            if (models.Count == 0)
            {
                LogWarn($"No .gguf model files found in {_modelsDirectory}");
                _initialized = true;
                return true; // Not a fatal error - continue boot
            }
            
            LogInfo($"Found {models.Count} .gguf model file(s)");
            
            // Process each model
            foreach (var modelPath in models)
            {
                await ProcessSingleModelAsync(modelPath);
            }
            
            // Report summary
            var successCount = _loadedModels.Count;
            var failureCount = _modelFailures.Count;
            
            LogInfo($"Model processing complete: {successCount} loaded, {failureCount} failed");
            
            // Attempt healing for any failures
            if (failureCount > 0)
            {
                LogWarn($"Attempting self-healing for {failureCount} failed model(s)...");
                await AttemptHealingAsync();
            }
            
            _initialized = true;
            return true; // Always continue boot, even with failures
        }
        catch (Exception ex)
        {
            LogError($"Error processing models: {ex.Message}");
            _initialized = true;
            return true; // Continue boot even on error
        }
    }
    
    private async Task<List<string>> ScanForModelsAsync()
    {
        await Task.CompletedTask; // Async placeholder
        
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
                LogInfo($"Scanned {modelsDir} and found {models.Count} .gguf model(s)");
            }
            else
            {
                LogWarn($"Models directory not found: {modelsDir}");
                
                // Try to create the directory
                try
                {
                    Directory.CreateDirectory(modelsDir);
                    LogInfo($"Created models directory: {modelsDir}");
                }
                catch (Exception ex)
                {
                    LogWarn($"Could not create models directory: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"Error scanning for models: {ex.Message}");
        }
        
        return models;
    }
    
    public async Task ProcessSingleModelAsync(string modelPath)
    {
        await Task.CompletedTask; // Async placeholder
        
        var modelName = Path.GetFileName(modelPath);
        LogInfo($"Processing model: {modelName}");
        
        try
        {
            // Verify file exists
            if (!File.Exists(modelPath))
            {
                RecordFailure(modelPath, "File not found", FailureType.FileNotFound);
                return;
            }
            
            // Verify file is not corrupted (basic checks)
            var validationResult = ValidateModelFile(modelPath);
            if (!validationResult.IsValid)
            {
                RecordFailure(modelPath, validationResult.Error ?? "Validation failed", FailureType.CorruptFile);
                return;
            }
            
            // Try to get file info
            var fileInfo = new FileInfo(modelPath);
            var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
            
            // Record successful load
            var modelInfo = new ModelInfo
            {
                Path = modelPath,
                Name = modelName,
                SizeMB = sizeInMB,
                LoadedAt = DateTime.UtcNow,
                Checksum = validationResult.Checksum
            };
            
            _loadedModels.Add(modelInfo);
            LogInfo($"Successfully processed model: {modelName} ({sizeInMB:F2} MB)");
        }
        catch (Exception ex)
        {
            RecordFailure(modelPath, ex.Message, FailureType.ProcessingError);
        }
    }
    
    private ModelValidationResult ValidateModelFile(string modelPath)
    {
        try
        {
            // Check if file is readable
            using var fs = File.OpenRead(modelPath);
            
            // Verify GGUF magic number (first 4 bytes should be "GGUF")
            var buffer = new byte[4];
            var bytesRead = fs.Read(buffer, 0, 4);
            
            if (bytesRead != 4)
            {
                return new ModelValidationResult 
                { 
                    IsValid = false, 
                    Error = "File too small to be a valid GGUF file" 
                };
            }
            
            var magic = System.Text.Encoding.ASCII.GetString(buffer);
            if (magic != "GGUF")
            {
                return new ModelValidationResult 
                { 
                    IsValid = false, 
                    Error = "Invalid GGUF magic number - file may be corrupted" 
                };
            }
            
            // Calculate checksum for integrity verification
            fs.Seek(0, SeekOrigin.Begin);
            using var sha256 = SHA256.Create();
            
            // For large files, only hash first 1MB to save time during boot
            var hashSize = Math.Min(fs.Length, 1024 * 1024);
            var hashBuffer = new byte[hashSize];
            var hashBytesRead = fs.Read(hashBuffer, 0, (int)hashSize);
            
            var hash = sha256.ComputeHash(hashBuffer, 0, hashBytesRead);
            var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            return new ModelValidationResult 
            { 
                IsValid = true, 
                Checksum = checksum 
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ModelValidationResult 
            { 
                IsValid = false, 
                Error = $"Access denied: {ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new ModelValidationResult 
            { 
                IsValid = false, 
                Error = $"Validation error: {ex.Message}" 
            };
        }
    }
    
    private void RecordFailure(string modelPath, string error, FailureType type)
    {
        var failure = new ModelFailure
        {
            Path = modelPath,
            Name = Path.GetFileName(modelPath),
            Error = error,
            Type = type,
            FailedAt = DateTime.UtcNow,
            HealingAttempts = 0
        };
        
        _modelFailures.Add(failure);
        LogError($"Model failed: {failure.Name} - {error}");
    }
    
    private async Task AttemptHealingAsync()
    {
        foreach (var failure in _modelFailures.ToList())
        {
            if (failure.HealingAttempts >= 3)
            {
                LogWarn($"Max healing attempts reached for {failure.Name}, skipping");
                continue;
            }
            
            LogInfo($"Attempting to heal model: {failure.Name}");
            failure.HealingAttempts++;
            
            var healed = await TryHealModelAsync(failure);
            if (healed)
            {
                _modelFailures.Remove(failure);
                LogInfo($"Successfully healed model: {failure.Name}");
            }
        }
    }
    
    private async Task<bool> TryHealModelAsync(ModelFailure failure)
    {
        await Task.CompletedTask; // Async placeholder
        
        try
        {
            switch (failure.Type)
            {
                case FailureType.FileNotFound:
                    LogWarn($"Cannot heal {failure.Name}: file not found");
                    // In a real implementation, this could attempt to re-download
                    return false;
                
                case FailureType.CorruptFile:
                    LogInfo($"Attempting to verify {failure.Name} again...");
                    // Retry validation - file might have been in use
                    if (File.Exists(failure.Path))
                    {
                        var result = ValidateModelFile(failure.Path);
                        if (result.IsValid)
                        {
                            // File is now valid, re-process it
                            await ProcessSingleModelAsync(failure.Path);
                            return true;
                        }
                    }
                    LogWarn($"File still corrupt or inaccessible: {failure.Name}");
                    return false;
                
                case FailureType.ProcessingError:
                    LogInfo($"Retrying processing for {failure.Name}...");
                    // Retry processing
                    if (File.Exists(failure.Path))
                    {
                        await ProcessSingleModelAsync(failure.Path);
                        return !_modelFailures.Any(f => f.Path == failure.Path && f != failure);
                    }
                    return false;
                
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            LogError($"Healing failed for {failure.Name}: {ex.Message}");
            return false;
        }
    }
    
    private string GetStatus()
    {
        if (!_initialized)
            return "LanguageModelProcessor: Not yet initialized";
        
        var status = $"LanguageModelProcessor Status:\n";
        status += $"  Models Directory: {_modelsDirectory}\n";
        status += $"  Loaded Models: {_loadedModels.Count}\n";
        status += $"  Failed Models: {_modelFailures.Count}\n";
        
        if (_loadedModels.Count > 0)
        {
            status += "\nLoaded Models:\n";
            foreach (var model in _loadedModels)
            {
                status += $"  ✓ {model.Name} ({model.SizeMB:F2} MB)\n";
            }
        }
        
        if (_modelFailures.Count > 0)
        {
            status += "\nFailed Models:\n";
            foreach (var failure in _modelFailures)
            {
                status += $"  ✗ {failure.Name} - {failure.Error}\n";
            }
        }
        
        return status;
    }
    
    private string ScanAndLoadModels()
    {
        LogInfo("Manual scan requested...");
        var task = ProcessModelsAsync();
        task.Wait();
        return GetStatus();
    }
    
    private string AttemptHealingAllFailures()
    {
        if (_modelFailures.Count == 0)
            return "No failures to heal";
        
        LogInfo("Manual healing requested...");
        var task = AttemptHealingAsync();
        task.Wait();
        return GetStatus();
    }
    
    private string GeneratedetailedReport()
    {
        var report = "=== Language Model Processor Report ===\n\n";
        report += $"Status: {(_initialized ? "Initialized" : "Not Initialized")}\n";
        report += $"Models Directory: {_modelsDirectory}\n";
        report += $"Total Models Found: {_loadedModels.Count + _modelFailures.Count}\n";
        report += $"Successfully Loaded: {_loadedModels.Count}\n";
        report += $"Failed: {_modelFailures.Count}\n\n";
        
        if (_loadedModels.Count > 0)
        {
            report += "=== Successfully Loaded Models ===\n";
            foreach (var model in _loadedModels)
            {
                report += $"\nModel: {model.Name}\n";
                report += $"  Path: {model.Path}\n";
                report += $"  Size: {model.SizeMB:F2} MB\n";
                report += $"  Loaded: {model.LoadedAt:yyyy-MM-dd HH:mm:ss} UTC\n";
                report += $"  Checksum: {model.Checksum?.Substring(0, 16)}...\n";
            }
        }
        
        if (_modelFailures.Count > 0)
        {
            report += "\n=== Failed Models ===\n";
            foreach (var failure in _modelFailures)
            {
                report += $"\nModel: {failure.Name}\n";
                report += $"  Path: {failure.Path}\n";
                report += $"  Error: {failure.Error}\n";
                report += $"  Type: {failure.Type}\n";
                report += $"  Failed: {failure.FailedAt:yyyy-MM-dd HH:mm:ss} UTC\n";
                report += $"  Healing Attempts: {failure.HealingAttempts}\n";
            }
        }
        
        return report;
    }
}

// Supporting types
internal class ModelInfo
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double SizeMB { get; set; }
    public DateTime LoadedAt { get; set; }
    public string? Checksum { get; set; }
}

internal class ModelFailure
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public FailureType Type { get; set; }
    public DateTime FailedAt { get; set; }
    public int HealingAttempts { get; set; }
}

internal class ModelValidationResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public string? Checksum { get; set; }
}

internal enum FailureType
{
    FileNotFound,
    CorruptFile,
    ProcessingError
}
