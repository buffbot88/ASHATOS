using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;


namespace RaCore.Modules.Conscious;

[RaModule(Category = "core")]
public partial class ConsciousModule : ModuleBase
{
    public override string Name => "Conscious";

    private ModuleManager? _manager;
    private object? _memoryInst;
    private ISubconscious? _sub;
    private ThoughtProcessor? _thoughtProcessor;
    private IHandlerModule? _errorHandler;
    private IHandlerModule? _infoHandler;

    private readonly ConcurrentQueue<Thought> _thoughtHistory = new();
    private readonly object _historyLock = new();

    public Mood _currentMood = Mood.Neutral;
    private int _awakeFlag;
    private DateTime? _awakeAtUtc;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _memoryInst = _manager?.Modules.Select(m => m.Instance).OfType<IMemory>().FirstOrDefault();
        _memoryInst ??= _manager?.GetModuleInstanceByName("Memory");

        if (_memoryInst == null)
            LogWarn("Memory module not found. Conscious will operate in reduced mode.");

        var subInst = _manager?.GetModuleInstanceByName("Subconscious")
                     ?? _manager?.GetModuleInstanceByName("SubconsciousModule");
        _sub = subInst as ISubconscious;

        _errorHandler = _manager?.GetModuleInstanceByName("ErrorHandler") as IHandlerModule;
        _infoHandler = _manager?.GetModuleInstanceByName("InfoHandler") as IHandlerModule;

        if (_manager != null)
            _thoughtProcessor = new ThoughtProcessor(manager: _manager);
        else
            LogWarn("ModuleManager is null. ThoughtProcessor will not be initialized.");

        LogInfo("Conscious module initialized successfully.");
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public Task<ModuleResponse> ProcessAsync(string input)
    {
        return ProcessAsync(input, _errorHandler);
    }

    public async Task<ModuleResponse> ProcessAsync(string input, IHandlerModule? _errorHandlerInst)
    {
        if (string.IsNullOrWhiteSpace(input))
            return await (_infoHandler?.ProcessAsync("(empty input)") ?? Task.FromResult(new ModuleResponse { Text = "(empty input)" }));

        var trimmed = input.Trim();
        var firstSpace = trimmed.IndexOf(' ');
        var verb = firstSpace > 0 ? trimmed[..firstSpace].ToLowerInvariant() : trimmed.ToLowerInvariant();
        var args = firstSpace > 0 ? trimmed[(firstSpace + 1)..].Trim() : string.Empty;

        try
        {
            return verb switch
            {
                "help" => await (_infoHandler?.ProcessAsync(GetHelpText()) ?? Task.FromResult(new ModuleResponse { Text = GetHelpText() })),
                "think" => await _thoughtProcessor!.ProcessThoughtAsync(args, _memoryInst, _sub, _thoughtHistory, _currentMood),
                "remember" => await _thoughtProcessor!.ProcessRememberAsync(args, _memoryInst),
                "recall" => await _thoughtProcessor!.ProcessRecallAsync(args, _memoryInst),
                "status" => await _thoughtProcessor!.ProcessStatusAsync(_currentMood, _thoughtHistory, _memoryInst, _sub, _awakeFlag, _awakeAtUtc),
                _ => await ((Task<ModuleResponse>)_thoughtProcessor!.ProcessThoughtAsync(trimmed, _memoryInst, _sub, _thoughtHistory, _currentMood)), // Explicit cast to resolve ambiguity
            };
        }
        catch (Exception ex)
        {
            if (_errorHandlerInst != null)
                return await _errorHandlerInst.ProcessAsync(ex.Message);
            else
                return new ModuleResponse { Text = $"Error: {ex.Message}", Type = "error", Status = "error" };
        }
    }

    private static string GetHelpText() => @"
Conscious Module Commands:
  think <input>            - Process input through consciousness
  remember key=value       - Store in memory
  recall key               - Retrieve from memory
  status                   - Show current status
  help                     - This help text".Trim();
}
