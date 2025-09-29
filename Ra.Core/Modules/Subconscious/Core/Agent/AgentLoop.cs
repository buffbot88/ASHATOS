using System;
using System.Threading;
using System.Threading.Tasks;
using Ra.Core.Engine.Manager;

namespace Ra.Core.Modules.Subconscious.Core.Agent;

public sealed class AgentLoop
{
    private readonly ModuleManager _manager;
    private readonly TimeSpan _tick;
    private CancellationTokenSource? _cts;

    public event Action<string, string, string>? OnMessage; // moduleName, input, response

    public AgentLoop(ModuleManager manager, TimeSpan? tick = null)
    {
        _manager = manager;
        _tick = tick ?? TimeSpan.FromSeconds(5);
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

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // 1) Observe
                var obs = _manager.SafeInvokeModuleByName("Memory", "status")
                       ?? _manager.SafeInvokeModuleByName("Conscious", "status")
                       ?? "";

                OnMessage?.Invoke("Agent", "observe", obs);

                // 2) Plan (simple heuristic placeholder; replace with LLM planner)
                var plan = "think What should I focus on now?";
                OnMessage?.Invoke("Agent", "plan", plan);

                // 3) Act
                var actRes = _manager.SafeInvokeModuleByName("Conscious", plan)
                          ?? _manager.SafeInvokeModuleByName("Speech", plan)
                          ?? "";
                OnMessage?.Invoke("Agent", "act", actRes);

                // 4) Learn (store plan + result)
                _manager.SafeInvokeModuleByName("Memory", $"remember last_plan={plan}");
                _manager.SafeInvokeModuleByName("Memory", $"remember last_result={Trim(actRes, 200)}");

                // 5) Reflect (placeholder)
                var reflection = $"Reflected on: {Trim(actRes, 120)}";
                OnMessage?.Invoke("Agent", "reflect", reflection);
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke("Agent", "error", ex.Message);
            }

            await Task.Delay(_tick, ct);
        }
    }

    private static string Trim(string s, int n) => string.IsNullOrEmpty(s) ? s : (s.Length > n ? s[..n] + "..." : s);
}
