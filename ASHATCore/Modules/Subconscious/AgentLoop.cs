using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Subconscious;

public sealed class AgentLoop
{
    private readonly ModuleManager _manager;
    private TimeSpan _tick;
    private CancellationTokenSource? _cts;
    private readonly IAILanguageModule? _aiLang;

    public event Action<string, string, string>? OnMessage; // moduleName, input, response

    public AgentLoop(ModuleManager manager, TimeSpan? tick = null)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _tick = tick ?? TimeSpan.FromSeconds(5);
        _aiLang = manager.GetModuleInstanceByName("AILanguage") as IAILanguageModule;
    }

    public void Start()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => RunAsync(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    public void SetTick(TimeSpan tick) => _tick = tick;

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // 1) Observe
                var obs = await _manager.SafeInvokeModuleByNameAsync("Memory", "status", ct: ct)
                       ?? await _manager.SafeInvokeModuleByNameAsync("Conscious", "status", ct: ct)
                       ?? "";

                var lastPlan = await _manager.SafeInvokeModuleByNameAsync("Memory", "recall last_plan", ct: ct);
                var lastResult = await _manager.SafeInvokeModuleByNameAsync("Memory", "recall last_result", ct: ct);

                var obsMsg = await AgenticMsgAsync("observe", $"{obs}\nLastPlan:{lastPlan}\nLastResult:{lastResult}");
                OnMessage?.Invoke("Agent", "observe", obsMsg);

                // 2) Plan
                var plan = $"think {SuggestNextFocus(lastResult)}";
                var planMsg = await AgenticMsgAsync("plan", plan);
                OnMessage?.Invoke("Agent", "plan", planMsg);

                // 3) Act
                var actRes = await _manager.SafeInvokeModuleByNameAsync("Conscious", plan, ct: ct)
                          ?? await _manager.SafeInvokeModuleByNameAsync("Speech", plan, ct: ct)
                          ?? "";
                var actMsg = await AgenticMsgAsync("act", actRes);
                OnMessage?.Invoke("Agent", "act", actMsg);

                // 4) Learn
                await _manager.SafeInvokeModuleByNameAsync("Memory", $"remember last_plan={plan}", ct: ct);
                await _manager.SafeInvokeModuleByNameAsync("Memory", $"remember last_result={Trim(actRes, 200)}", ct: ct);

                // 5) Reflect
                var reflection = $"Reflected on: {Trim(actRes, 120)}";
                var reflectMsg = await AgenticMsgAsync("reflect", reflection);
                OnMessage?.Invoke("Agent", "reflect", reflectMsg);

                // 6) Trace
                await _manager.SafeInvokeModuleByNameAsync("Memory", $"remember agent_Trace={DateTime.UtcNow:s}|{plan}|{Trim(actRes, 100)}", ct: ct);
            }
            catch (Exception ex)
            {
                var errorMsg = await AgenticMsgAsync("error", $"{ex.GetType().Name}: {ex.Message}");
                OnMessage?.Invoke("Agent", "error", errorMsg);
                await _manager.SafeInvokeModuleByNameAsync("Memory", $"remember agent_error={ex.Message}", ct: ct);
            }

            await Task.Delay(_tick, ct);
        }
    }

    private async Task<string> AgenticMsgAsync(string intent, string text)
    {
        if (_aiLang == null) return text;
        var resp = await _aiLang.GenerateAsync(intent, text, "en", null);
        return resp.Text ?? text;
    }

    private static string SuggestNextFocus(string? lastResult)
    {
        if (string.IsNullOrWhiteSpace(lastResult)) return "What should I focus on next?";
        if (lastResult.Contains("error", StringComparison.OrdinalIgnoreCase)) return "How can I resolve the last error?";
        return $"Based on last result: {lastResult.Trim()}";
    }

    private static string Trim(string s, int n) => string.IsNullOrEmpty(s) ? s : (s.Length > n ? s[..n] + "..." : s);
}