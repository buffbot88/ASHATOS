using System.Text.RegularExpressions;
using RaCore.Modules.Conscious;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Speech;

[RaModule(Category = "core")]
public partial class SpeechModule : ModuleBase, ISpeechModule
{
    public override string Name => "Speech";

    private ModuleManager? _manager;
    private IMemory? _memoryInst;
    private ISubconscious? _sub;
    private IAILanguageModule? _aiLang;
    private ThoughtProcessor? _thoughtProcessor;

    private int _awakeFlag;
    private DateTime? _awakeAtUtc;

    private readonly object _pendingLock = new();

    // Regex patterns for command routing
    [GeneratedRegex(@"^\s*start(?:\s.*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxStart();

    [GeneratedRegex(@"^\s*features(?:\s.*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxFeatures();

    [GeneratedRegex(@"^(?:recall|get)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxRecall();

    [GeneratedRegex(@"^(?:think(?:\s+about)?|ponder)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxThink();

    [GeneratedRegex(@"^(?:ask\s+subconscious|probe|ask)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxProbe();

    [GeneratedRegex(@"^remember\s+(.+?)\s*(?:is|=)\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxRememberExplicit();

    [GeneratedRegex(@"^(.+?)\s*=\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxRememberImplicit();

    [GeneratedRegex(@"^\s*(status|help)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxStatusOrHelp();

    [GeneratedRegex(@"^(?:do|please|execute)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxDo();

    [GeneratedRegex(@"^\s*(yes|approve|confirm)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxYes();

    [GeneratedRegex(@"^\s*(no|deny|cancel)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxNo();

    [GeneratedRegex(@"^\s*(start_diag|diagnostics)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RxDiagnostics();

    public SpeechModule() { }

    public SpeechModule(IMemory memoryClient, ISubconscious subconsciousClient)
    {
        _memoryInst = memoryClient ?? throw new ArgumentNullException(nameof(memoryClient));
        _sub = subconsciousClient ?? throw new ArgumentNullException(nameof(subconsciousClient));
        LogInfo("Speech module initialized (injected).");
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _aiLang = _manager?.GetModuleInstanceByName("AILanguage") as IAILanguageModule;
        _thoughtProcessor = new ThoughtProcessor(_manager);

        _memoryInst = _manager?.GetModuleInstanceByName("Memory") as IMemory;

        if (_memoryInst == null)
            LogWarn("Memory module not found. Speech will operate in reduced mode (remember/recall limited).");

        var subInst = _manager?.GetModuleInstanceByName("Subconscious") ?? _manager?.GetModuleInstanceByName("SubconsciousModule");
        _sub = subInst as ISubconscious;

        if (_sub == null)
            LogWarn("Subconscious module not found. Probe commands will be limited.");

        LogInfo("Speech module initialized.");
    }

    // ---- Core Command Routing ----
    public override string Process(string input)
    {
        try
        {
            return GenerateResponseAsync(input, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LogError($"Process error: {ex.Message}");
            return $"(error) {ex.Message}";
        }
    }

    public async Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";

        if (string.IsNullOrWhiteSpace(input))
            return (await _thoughtProcessor.ProcessStatusAsync(
                Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;

        var text = input.Trim();

        // --- Diagnostics command router ---
        if (RxDiagnostics().IsMatch(text))
        {
            var r = CallModule("Diagnostics", text);
            if (!string.IsNullOrWhiteSpace(r))
                return r;
        }

        // --- Command routing ---
        if (RxStart().IsMatch(text))
        {
            var r = CallModule("TestRunner", text);
            if (!string.IsNullOrWhiteSpace(r))
                return (await _thoughtProcessor.ProcessStatusAsync(
                    Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;
        }

        if (RxFeatures().IsMatch(text))
        {
            var r = CallModule("FeatureExplorer", text);
            if (!string.IsNullOrWhiteSpace(r))
                return (await _thoughtProcessor.ProcessStatusAsync(
                    Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;
        }

        var mDo = RxDo().Match(text);
        if (mDo.Success)
        {
            var utterance = mDo.Groups[1].Value.Trim();
            return await HandleAgenticCommandAsync(utterance, cancellationToken);
        }

        if (RxYes().IsMatch(text))
            return await HandleConfirmationAsync(true, cancellationToken);

        if (RxNo().IsMatch(text))
            return await HandleConfirmationAsync(false, cancellationToken);

        var match = RxRecall().Match(text);
        if (match.Success)
        {
            var key = NormalizeKey(match.Groups[1].Value.Trim());
            var val = await RecallCompatAsync(key);
            return val ?? $"No value found for key: {key}";
        }

        match = RxThink().Match(text);
        if (match.Success)
        {
            var content = match.Groups[1].Value.Trim();
            var result = await ThinkAsync(content, cancellationToken);
            return result;
        }

        match = RxProbe().Match(text);
        if (match.Success)
        {
            var content = match.Groups[1].Value.Trim();
            var result = await ProbeSubconsciousAsync(content, cancellationToken);
            return result;
        }

        match = RxRememberExplicit().Match(text);
        if (!match.Success) match = RxRememberImplicit().Match(text);

        if (match.Success)
        {
            var key = NormalizeKey(match.Groups[1].Value);
            var value = match.Groups[2].Value.Trim();
            var result = await RememberCompatAsync(key, value);
            return result;
        }

        if (text.StartsWith("skills", StringComparison.OrdinalIgnoreCase))
        {
            var r = CallModule("Skills", text);
            if (!string.IsNullOrWhiteSpace(r))
                return (await _thoughtProcessor.ProcessStatusAsync(
                    Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;
        }
        if (text.StartsWith("consent", StringComparison.OrdinalIgnoreCase))
        {
            var r = CallModule("Consent", text);
            if (!string.IsNullOrWhiteSpace(r))
                return (await _thoughtProcessor.ProcessStatusAsync(
                    Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;
        }

        if (RxStatusOrHelp().IsMatch(text))
        {
            return (await _thoughtProcessor.ProcessStatusAsync(
                Mood.Neutral, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), _memoryInst, _sub, _awakeFlag, _awakeAtUtc)).Text;
        }

        // Route natural utterances through AILanguageModule with chat template
        if (_aiLang != null)
        {
            string chatPrompt = BuildChatPrompt(text);
            var response = await _aiLang.GenerateAsync("chat", chatPrompt, "en", null);
            if (!string.IsNullOrWhiteSpace(response.Text))
                return response.Text;
        }

        // Fallback: use ThoughtProcessor
        var resultDefault = await ThinkAsync(text, cancellationToken);
        return resultDefault;
    }

    // ---- Helpers ----
    private string? CallModule(string name, string command)
    {
        return _manager?.SafeInvokeModuleByName(name, command)
            ?? _manager?.SafeInvokeModuleByName(name + "Module", command);
    }

    private static string NormalizeKey(string s) =>
        string.IsNullOrWhiteSpace(s) ? s : s.Trim().Replace(" ", "");

    private async Task<string> ThinkAsync(string content, CancellationToken _)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";
        var resp = await _thoughtProcessor.ProcessThoughtAsync(content, _memoryInst, _sub, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        return resp.Text;
    }

    private async Task<string> ProbeSubconsciousAsync(string content, CancellationToken _)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";
        var resp = await _thoughtProcessor.ProcessThoughtAsync(content, _memoryInst, _sub, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        return resp.Text;
    }

    private async Task<string?> RecallCompatAsync(string key)
    {
        if (_memoryInst == null)
            return null;
        if (_memoryInst is IMemory mem)
        {
            var result = await mem.RecallAsync(key);
            return result.Text;
        }
        return null;
    }

    private async Task<string> RememberCompatAsync(string key, string value)
    {
        if (_memoryInst == null)
            return "(Memory unavailable)";
        if (_memoryInst is IMemory mem)
        {
            var result = await mem.RememberAsync(key, value);
            return result.Text;
        }
        return "(Memory unavailable)";
    }

    private async Task<string> HandleAgenticCommandAsync(string utterance, CancellationToken _)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";
        var resp = await _thoughtProcessor.ProcessThoughtAsync(utterance, _memoryInst, _sub, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        return resp.Text;
    }

    private async Task<string> HandleConfirmationAsync(bool approve, CancellationToken _)
    {
        if (_thoughtProcessor == null)
            return "(ThoughtProcessor unavailable)";
        string msg = approve ? "User approved action." : "User denied action.";
        var resp = await _thoughtProcessor.ProcessThoughtAsync(msg, _memoryInst, _sub, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);
        return resp.Text;
    }

    // ---- Events ----
    private void OnMemoryReady(RaCore.Engine.Memory.MemoryModule memory)
    {
        _memoryInst = memory;
        LogInfo("OnMemoryReady: bound to MemoryModule.");
    }

    private void OnMemoryReady(IMemory memory)
    {
        _memoryInst = memory;
        LogInfo("OnMemoryReady: bound to IMemory.");
    }

    private void OnWarmup()
    {
        try
        {
            _ = _manager?.SafeInvokeModuleByName("Conscious", "status")
             ?? _manager?.SafeInvokeModuleByName("ConsciousModule", "status");
        }
        catch { }
    }

    public void OnSystemEvent(string name, object? payload)
    {
        if (string.Equals(name, "MemoryReady", StringComparison.OrdinalIgnoreCase))
        {
            if (payload is RaCore.Engine.Memory.MemoryModule mm) OnMemoryReady(mm);
            else if (payload is IMemory mem) OnMemoryReady(mem);
            else LogWarn($"MemoryReady payload type not recognized: {payload?.GetType().FullName ?? "null"}");
        }
        else if (string.Equals(name, "Wake", StringComparison.OrdinalIgnoreCase))
        {
            OnWake();
        }
    }

    private void OnWake()
    {
        if (Interlocked.CompareExchange(ref _awakeFlag, 1, 0) != 0)
            return;

        _awakeAtUtc = DateTime.UtcNow;

        try
        {
            var stamp = _awakeAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            _ = RememberCompatAsync("boot/speech/heartbeat", $"awake:{stamp}");
            _ = _manager?.SafeInvokeModuleByName("Conscious", "status")
             ?? _manager?.SafeInvokeModuleByName("ConsciousModule", "status");

            LogInfo("Speech warm-up complete (heartbeat stored).");
        }
        catch (Exception ex)
        {
            LogWarn($"Speech warm-up issue: {ex.Message}");
        }
    }

    private static string BuildChatPrompt(string userInput)
    {
        // Use a template suitable for Llama-2-Chat, Vicuna, etc.
        return $"[INST] <<SYS>>\nYou are a helpful assistant.\n<</SYS>>\n{userInput} [/INST]";
    }
}