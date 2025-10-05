using Abstractions;

namespace RaCore.Modules.Conscious;

public partial class ConsciousModule
{
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
        else if (string.Equals(name, "CustomAgentEvent", StringComparison.OrdinalIgnoreCase) && payload is string agentInfo)
        {
            _ = RememberCompatAsync("custom_agent_event", agentInfo);
            LogInfo($"Received custom agent event: {agentInfo}");
        }
    }

    private void OnWake()
    {
        if (Interlocked.CompareExchange(ref _awakeFlag, 1, 0) != 0)
            return; // already awake

        _awakeAtUtc = DateTime.UtcNow;
        _currentMood = Mood.Thinking;

        try
        {
            var stamp = _awakeAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            _ = RememberCompatAsync("boot/conscious/heartbeat", $"awake:{stamp}");

            var snapshot = ThoughtProcessor.GetMemorySnapshot(_memoryInst);
            LogInfo($"Conscious warm-up complete (heartbeat stored). Memory snapshot keys: {string.Join(", ", snapshot)}");
        }
        catch (Exception ex)
        {
            LogWarn($"Warm-up encountered an issue: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task RememberCompatAsync(string key, string value)
    {
        if (_thoughtProcessor == null) return;
        await _thoughtProcessor.ProcessRememberAsync($"{key}={value}", _memoryInst);
    }
}
