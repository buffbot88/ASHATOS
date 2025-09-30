using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RaCore.Engine.Manager;
using RaCore.Modules.Memory;
using RaCore.Modules.Subconscious;

namespace RaCore.Modules.Speech;

[RaModule(Category = "core")]
public class SpeechModule : ModuleBase, ISpeechModule
{
    public override string Name => "Speech";

    // Resolved in Initialize(ModuleManager)
    private ModuleManager? _manager;
    private object? _memoryInst;        // IMemory or MemoryModule (or compatible)
    private ISubconscious? _sub;        // optional

    // Wake/boot state (ordered boot: Memory -> Subconscious -> Conscious -> Speech -> DigitalFace)
    private int _awakeFlag;             // 0 = not awake, 1 = awake
    private DateTime? _awakeAtUtc;

    // Confirmation state for EthicsGuard (Ultron capability, Wiccan creed gate)
    private readonly object _pendingLock = new();
    private string? _pendingPlanJson;
    private string? _pendingReason;
    private DateTime? _pendingUntilUtc;

    // Precompiled lightweight routes
    private static readonly Regex RxStart = new(@"^\s*start(?:\s.*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxFeatures = new(@"^\s*features(?:\s.*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxRecall = new(@"^(?:recall|get)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxThink = new(@"^(?:think(?:\s+about)?|ponder)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxProbe = new(@"^(?:ask\s+subconscious|probe|ask)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxRememberExplicit = new(@"^remember\s+(.+?)\s*(?:is|=)\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxRememberImplicit = new(@"^(.+?)\s*=\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxStatusOrHelp = new(@"^\s*(status|help)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Agentic routes
    private static readonly Regex RxDo = new(@"^(?:do|please|execute)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxYes = new(@"^\s*(yes|approve|confirm)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxNo = new(@"^\s*(no|deny|cancel)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Parameterless ctor required for ModuleManager discovery
    public SpeechModule() { }

    // Legacy DI-friendly constructor (not used by ModuleManager)
    public SpeechModule(IMemory memoryClient, ISubconscious subconsciousClient)
    {
        _memoryInst = memoryClient ?? throw new ArgumentNullException(nameof(memoryClient));
        _sub = subconsciousClient ?? throw new ArgumentNullException(nameof(subconsciousClient));
        LogInfo("Speech module initialized (injected).");
    }

    public override void Initialize(ModuleManager manager)
    {
        base.Initialize(manager);
        _manager = manager;

        // Resolve memory by name or type (binding will also be updated by OnMemoryReady)
        _memoryInst = manager.GetModuleInstanceByName("Memory")
                     ?? manager.GetModuleInstanceByName("MemoryModule");

        if (_memoryInst == null)
            LogWarn("Memory module not found. Speech will operate in reduced mode (remember/recall limited).");

        // Resolve subconscious (must implement ISubconscious)
        var subInst = manager.GetModuleInstanceByName("Subconscious")
                     ?? manager.GetModuleInstanceByName("SubconsciousModule");
        _sub = subInst as ISubconscious;

        if (_sub == null)
            LogWarn("Subconscious module not found. Probe commands will be limited.");

        LogInfo("Speech module initialized.");
    }

    // -------------- Ordered wake handlers (raised by Memory/ModuleManager) --------------

    private void OnMemoryReady(MemoryModule memory)
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

    private void OnSystemEvent(string name, object? payload)
    {
        if (string.Equals(name, "MemoryReady", StringComparison.OrdinalIgnoreCase))
        {
            if (payload is MemoryModule mm) OnMemoryReady(mm);
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
            return; // already awake

        _awakeAtUtc = DateTime.UtcNow;

        try
        {
            var stamp = _awakeAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            RememberCompat("boot/speech/heartbeat", $"awake:{stamp}");
            _ = _manager?.SafeInvokeModuleByName("Conscious", "status")
             ?? _manager?.SafeInvokeModuleByName("ConsciousModule", "status");

            // Auto-wake the Face bus on Speech wake
            FaceCmd("face wake");

            LogInfo("Speech warm-up complete (heartbeat stored).");
        }
        catch (Exception ex)
        {
            LogWarn($"Speech warm-up issue: {ex.Message}");
        }
    }

    // -------------- Primary async entry --------------

    public async Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "(no input)";

        var text = input.Trim();

        // quick routes: TestRunner (forward all args, e.g., fast/json/verify/seed ...)
        if (RxStart.IsMatch(text))
        {
            var r = CallModule("TestRunner", text);
            if (!string.IsNullOrWhiteSpace(r)) return r!;
        }

        // quick routes: FeatureExplorer (forward all args)
        if (RxFeatures.IsMatch(text))
        {
            var r = CallModule("FeatureExplorer", text);
            if (!string.IsNullOrWhiteSpace(r)) return r!;
        }

        // Agentic pipeline (NLU -> Planner -> EthicsGuard -> (confirm?) -> Executor)
        var mDo = RxDo.Match(text);
        if (mDo.Success)
        {
            var utterance = mDo.Groups[1].Value.Trim();
            return await HandleAgenticCommandAsync(utterance, cancellationToken);
        }

        // Confirmation handling for pending plans
        if (RxYes.IsMatch(text))
            return await HandleConfirmationAsync(approve: true, cancellationToken);

        if (RxNo.IsMatch(text))
            return await HandleConfirmationAsync(approve: false, cancellationToken);

        // recall: "recall key" or "get key"
        var match = RxRecall.Match(text);
        if (match.Success)
        {
            var key = NormalizeKey(match.Groups[1].Value.Trim());
            var val = RecallCompat(key);
            return val ?? $"No value found for key: {key}";
        }

        // think: "think about X" or "think X" or "ponder X"
        match = RxThink.Match(text);
        if (match.Success)
        {
            var content = match.Groups[1].Value.Trim();
            return await ThinkAsync(content, cancellationToken);
        }

        // probe subconscious: "ask subconscious X", "probe X", "ask X"
        match = RxProbe.Match(text);
        if (match.Success)
        {
            var content = match.Groups[1].Value.Trim();
            return await ProbeSubconsciousAsync(content, cancellationToken);
        }

        // remember (explicit or '=' implicit)
        match = RxRememberExplicit.Match(text);
        if (!match.Success) match = RxRememberImplicit.Match(text);

        if (match.Success)
        {
            var key = NormalizeKey(match.Groups[1].Value);
            var value = match.Groups[2].Value.Trim();
            return RememberCompat(key, value);
        }

        // Pass-through convenience for new modules (optional)
        if (text.StartsWith("skills", StringComparison.OrdinalIgnoreCase))
        {
            var r = CallModule("Skills", text);
            if (!string.IsNullOrWhiteSpace(r)) return r!;
        }
        if (text.StartsWith("consent", StringComparison.OrdinalIgnoreCase))
        {
            var r = CallModule("Consent", text);
            if (!string.IsNullOrWhiteSpace(r)) return r!;
        }

        // status/help
        if (RxStatusOrHelp.IsMatch(text))
        {
            return GetHelpOrStatus(text);
        }

        // Default: Treat as think
        return await ThinkAsync(text, cancellationToken);
    }

    // -------------- IRaModule sync entry --------------

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

    // -------------- Agentic pipeline (JARVIS + Ultron capability, Wiccan creed guard) --------------

    private async Task<string> HandleAgenticCommandAsync(string utterance, CancellationToken ct)
    {
        if (_manager == null)
            return "(agent pipeline unavailable)";

        // Face: focused/working posture while planning/executing
        FaceCmd("face set mood Thinking");
        FaceCmd("face set attention 0.80");

        try
        {
            // NLU
            var intentJson = _manager.SafeInvokeModuleByName("NLU", utterance)
                           ?? _manager.SafeInvokeModuleByName("NluModule", utterance);
            if (string.IsNullOrWhiteSpace(intentJson))
                return "(could not derive intent)";

            RememberCompat("agent/last/intent", intentJson);

            // Plan
            var planJson = _manager.SafeInvokeModuleByName("Planner", intentJson)
                         ?? _manager.SafeInvokeModuleByName("PlannerModule", intentJson);
            if (string.IsNullOrWhiteSpace(planJson))
                return "(could not generate a plan)";

            RememberCompat("agent/last/plan", planJson);

            // Ethics (Wiccan creed: harm none)
            var verdict = _manager.SafeInvokeModuleByName("EthicsGuard", planJson)
                       ?? _manager.SafeInvokeModuleByName("EthicsGuardModule", planJson);
            if (string.IsNullOrWhiteSpace(verdict))
                return "(ethics guard unavailable)";

            if (verdict.StartsWith("APPROVED:", StringComparison.OrdinalIgnoreCase))
            {
                var approvedPlan = verdict.Substring("APPROVED:".Length).Trim();

                // Brief “acting” hint
                FaceCmd("face set attention 0.90");

                var outcome = ExecutePlan(approvedPlan);
                RememberCompat("agent/last/outcome", outcome);

                // Success flourish → relax
                FaceCmd("face set mood Happy");
                FaceCmd("face set attention 0.35");

                return StyleOk(outcome);
            }
            if (verdict.StartsWith("CONFIRM:", StringComparison.OrdinalIgnoreCase))
            {
                // Format: "CONFIRM: <reason>\n<plan json>"
                var reason = verdict;
                var idx = verdict.IndexOf('\n');
                var plan = planJson;
                if (idx >= 0)
                {
                    reason = verdict.Substring(0, idx).Trim();
                    plan = verdict[(idx + 1)..].Trim();
                }
                StartPendingConfirmation(plan, reason);

                // Maintain focused/caution posture while awaiting approval
                FaceCmd("face set mood Thinking");
                FaceCmd("face set attention 0.60");

                return StyleConfirm($"{reason}\nShall I proceed? (yes/no)");
            }
            if (verdict.StartsWith("BLOCKED:", StringComparison.OrdinalIgnoreCase))
            {
                // Show confusion on block
                FaceCmd("face set mood Confused");
                FaceCmd("face set attention 0.25");

                return StyleBlocked(verdict);
            }

            return $"(unexpected ethics verdict) {verdict}";
        }
        finally
        {
            // If we’re not pending confirmation, relax attention back a bit
            bool pending;
            lock (_pendingLock) { pending = _pendingPlanJson != null; }
            if (!pending)
            {
                FaceCmd("face set mood Neutral");
                FaceCmd("face set attention 0.30");
            }
        }
    }

    private async Task<string> HandleConfirmationAsync(bool approve, CancellationToken ct)
    {
        string? plan; string? reason; DateTime? until;
        lock (_pendingLock)
        {
            plan = _pendingPlanJson;
            reason = _pendingReason;
            until = _pendingUntilUtc;
        }

        if (string.IsNullOrWhiteSpace(plan))
            return "(nothing pending confirmation)";

        if (until.HasValue && DateTime.UtcNow > until.Value)
        {
            ClearPending();

            // Return to neutral on expiry
            FaceCmd("face set mood Neutral");
            FaceCmd("face set attention 0.30");

            return "(request expired)";
        }

        if (!approve)
        {
            ClearPending();

            // Calm down after cancel
            FaceCmd("face set mood Neutral");
            FaceCmd("face set attention 0.30");

            return "Understood. I have canceled the requested action.";
        }

        // Acting
        FaceCmd("face set attention 0.85");

        var outcome = ExecutePlan(plan!);
        RememberCompat("agent/last/outcome", outcome);
        ClearPending();

        // Success flourish → relax
        FaceCmd("face set mood Happy");
        FaceCmd("face set attention 0.35");

        return StyleOk(outcome);
    }

    private string ExecutePlan(string planJson)
    {
        try
        {
            var outText = _manager?.SafeInvokeModuleByName("Executor", planJson)
                       ?? _manager?.SafeInvokeModuleByName("ExecutorModule", planJson);
            return string.IsNullOrWhiteSpace(outText) ? "(executor produced no output)" : outText!;
        }
        catch (Exception ex)
        {
            return $"(execution error) {ex.Message}";
        }
    }

    private void StartPendingConfirmation(string planJson, string reason)
    {
        lock (_pendingLock)
        {
            _pendingPlanJson = planJson;
            _pendingReason = reason;
            _pendingUntilUtc = DateTime.UtcNow.AddMinutes(2);
        }
        RememberCompat("agent/pending/plan", planJson);
        RememberCompat("agent/pending/reason", reason);
    }

    private void ClearPending()
    {
        lock (_pendingLock)
        {
            _pendingPlanJson = null;
            _pendingReason = null;
            _pendingUntilUtc = null;
        }
    }

    // Persona-aware phrasing (lightweight)
    private static string StyleOk(string s) => $"By your leave. {s}";
    private static string StyleConfirm(string s) => $"Caution, in keeping with 'harm none': {s}";
    private static string StyleBlocked(string s) => $"I won’t do that. It would violate 'harm none'. Detail: {s}";

    // -------------- Command handlers --------------

    private string RememberCompat(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return "Invalid remember command. Usage: remember key=value";
        if (_memoryInst == null) return "(memory unavailable)";

        try
        {
            if (_memoryInst is IMemory mem)
            {
                mem.Remember(key, value); // supports both void/Guid variants in your codebase
                return $"Remembered: {key} = {value}";
            }

            // Reflection fallback
            var t = _memoryInst.GetType();
            var m = t.GetMethod("Remember", new[] { typeof(string), typeof(string) });
            if (m != null)
            {
                _ = m.Invoke(_memoryInst, new object[] { key, value });
                return $"Remembered: {key} = {value}";
            }
        }
        catch (Exception ex)
        {
            LogError($"Remember failed: {ex.Message}");
            return $"(remember error) {ex.Message}";
        }

        return "(memory remember not supported)";
    }

    private string? RecallCompat(string key)
    {
        if (_memoryInst == null) return null;

        try
        {
            if (_memoryInst is IMemory mem)
                return mem.Recall(key);

            var t = _memoryInst.GetType();
            var m = t.GetMethod("Recall", new[] { typeof(string) });
            return m?.Invoke(_memoryInst, new object[] { key })?.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Recall failed: {ex.Message}");
            return null;
        }
    }

    private async Task<string> ThinkAsync(string content, CancellationToken ct)
    {
        // Face: thinking posture
        FaceCmd("face set mood Thinking");
        FaceCmd("face set attention 0.70");

        try
        {
            if (_manager != null)
            {
                var res = _manager.SafeInvokeModuleByName("Conscious", $"think {content}")
                       ?? _manager?.SafeInvokeModuleByName("ConsciousModule", $"think {content}");
                if (!string.IsNullOrWhiteSpace(res))
                    return res!;
            }

            await Task.Yield();
            return $"Thought about: {content}";
        }
        finally
        {
            // Relax after thinking
            FaceCmd("face set mood Neutral");
            FaceCmd("face set attention 0.35");
        }
    }

    private async Task<string> ProbeSubconsciousAsync(string content, CancellationToken ct)
    {
        // Face: thinking posture
        FaceCmd("face set mood Thinking");
        FaceCmd("face set attention 0.60");

        try
        {
            if (_sub != null)
            {
                var probeTask = _sub.Probe(content, ct);
                var completed = await Task.WhenAny(probeTask, Task.Delay(TimeSpan.FromSeconds(2), ct)) == probeTask;
                if (!completed) return "(probe timeout)";
                var response = await probeTask;
                return $"Subconscious response: {response}";
            }
        }
        catch (Exception ex)
        {
            LogError($"Subconscious probe failed: {ex.Message}");
            return $"(probe error) {ex.Message}";
        }
        finally
        {
            // Relax posture
            FaceCmd("face set mood Neutral");
            FaceCmd("face set attention 0.35");
        }

        return "(subconscious unavailable)";
    }

    private string GetHelpOrStatus(string cmd)
    {
        if (cmd.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return @"
                Speech Module:
                - remember key=value        Store a memory entry
                - recall key                Retrieve a memory entry
                - think <text>              Process input (delegates to Conscious if available)
                - probe <text>              Query Subconscious if available
                - do <request>              Agentic action (NLU → Plan → EthicsGuard → Execute)
                - yes | no                  Approve/deny a pending action
                - start [options]           Run TestRunner (fast, json, verify, seed <n>, ...)
                - features [options]        Explore loaded features/capabilities (full, json, ...)
                - skills ...                Skill registry (list/describe)
                - consent ...               Consent registry (grant/revoke/list)
                - status                    Show availability
                - help                      This help text".Trim();
        }

        var hasMem = _memoryInst != null;
        var hasSub = _sub != null;
        var hasCon = _manager?.GetModuleInstanceByName("Conscious") != null
                  || _manager?.GetModuleInstanceByName("ConsciousModule") != null;
        var awake = _awakeFlag == 1 ? "yes" : "no";
        var when = _awakeAtUtc?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "-";

        // Pending info
        string pendingInfo;
        lock (_pendingLock)
        {
            pendingInfo = _pendingPlanJson != null
                ? $"yes (expires {(_pendingUntilUtc?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "-")})"
                : "no";
        }

        var sb = new StringBuilder();
        sb.Append($"Speech status: memory={(hasMem ? "yes" : "no")}, subconscious={(hasSub ? "yes" : "no")}, conscious={(hasCon ? "yes" : "no")}, awake={awake}, awakeAt={when}");
        sb.Append($", pendingAction={pendingInfo}");
        return sb.ToString();
    }

    private string? CallModule(string name, string command)
    {
        return _manager?.SafeInvokeModuleByName(name, command)
            ?? _manager?.SafeInvokeModuleByName(name + "Module", command);
    }

    private static string NormalizeKey(string s) =>
        string.IsNullOrWhiteSpace(s) ? s : s.Trim().Replace(" ", "");

    public string GenerateResponse(string input)
    {
        return GenerateResponseAsync(input, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private void FaceCmd(string cmd)
    {
        try { _manager?.SafeInvokeModuleByName("Face", cmd); } catch { }
    }
}
