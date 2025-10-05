using RaCore.Modules.Conscious;

using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Subconscious;

[RaModule(Category = "core")]
public class SubconsciousModule : ModuleBase, ISubconscious, IDisposable
{
    private object? _memoryInst;
    private readonly object _sync = new();
    private ModuleManager? _manager;
    private IAILanguageModule? _aiLang;
    private ThoughtProcessor? _thoughtProcessor;

    public override string Name => "Subconscious";

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        if (_manager != null)
        {
            _aiLang = _manager.GetModuleInstanceByName("AILanguage") as IAILanguageModule;
            _thoughtProcessor = new ThoughtProcessor(_manager);
            _memoryInst = _manager.GetModuleInstanceByName("Memory")
                         ?? _manager.GetModuleInstanceByName("MemoryModule");
            if (_memoryInst == null)
                LogWarn("Memory module not found. Subconscious will operate in reduced mode until Memory is available.");
            else
                LogInfo($"Linked to Memory instance: {_memoryInst.GetType().FullName}");
        }
    }

    public async Task<string> Probe(string query, CancellationToken cancellationToken = default)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";

        var resp = await _thoughtProcessor.ProcessThoughtAsync(
            $"subconscious probe: {query}",
            _memoryInst,
            null,
            new System.Collections.Concurrent.ConcurrentQueue<Thought>(),
            Mood.Thinking
        );
        SubconsciousDiagnostics.RaiseProbe(query, resp.Text);
        return resp.Text;
    }

    public void ReceiveMessage(string message)
    {
        if (_thoughtProcessor != null)
        {
            var resp = _thoughtProcessor.ProcessThoughtAsync(
                $"subconscious received: {message}",
                _memoryInst,
                null,
                new System.Collections.Concurrent.ConcurrentQueue<Thought>(),
                Mood.Speaking
            ).GetAwaiter().GetResult();
            LogInfo(resp.Text);
            SubconsciousDiagnostics.RaiseReceiveMessage(message, resp.Text);
        }
        else
        {
            LogInfo($"Subconscious received message: {message}");
        }
    }

    public string GetResponse()
    {
        if (_thoughtProcessor != null)
        {
            var resp = _thoughtProcessor.ProcessThoughtAsync(
                "subconscious response",
                _memoryInst,
                null,
                new System.Collections.Concurrent.ConcurrentQueue<Thought>(),
                Mood.Neutral
            ).GetAwaiter().GetResult();
            SubconsciousDiagnostics.RaiseGetResponse(resp.Text);
            return resp.Text;
        }
        return "Subconscious response generated.";
    }

    public override string Process(string input)
    {
        return $"Module '{Name}' does not support direct command processing.";
    }
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
