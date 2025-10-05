using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Conscious;

public class ThoughtProcessor(ModuleManager manager)
{
    private readonly IAILanguageModule? _aiLang = manager.GetModuleInstanceByName("AILanguage") as IAILanguageModule;

    public async Task<ModuleResponse> ProcessThoughtAsync(
        string content,
        object? memoryInst,
        ISubconscious? sub,
        ConcurrentQueue<Thought> _,
        Mood currentMood)
    {
        var memoryContext = GetMemorySnapshot(memoryInst);
        var subconsciousSignals = await GetSubconsciousSignalsAsync(sub, content);

        string agenticSummary = $"Thought: \"{content}\". Memory: [{string.Join("; ", memoryContext)}]. Subconscious: [{string.Join("; ", subconsciousSignals)}]. Mood: {currentMood}.";

        if (_aiLang != null)
        {
            string chatPrompt = BuildChatPrompt(agenticSummary);
            return await _aiLang.GenerateAsync("think", chatPrompt, "en", null);
        }
        else
        {
            return new ModuleResponse { Text = agenticSummary };
        }
    }

    private static string BuildChatPrompt(string context)
    {
        return $"[INST] <<SYS>>\nYou are a helpful assistant.\n<</SYS>>\n{context} [/INST]";
    }

    public async Task<ModuleResponse> ProcessRememberAsync(string args, object? memoryInst)
    {
        var parts = args.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            if (_aiLang != null)
                return await _aiLang.GenerateAsync("remember", BuildChatPrompt("To remember something, use: remember key=value"), "en", null);
            else
                return new ModuleResponse { Text = "To remember something, use: remember key=value" };
        }

        _ = await RememberCompatAsync(memoryInst, parts[0], parts[1]);
        string msg = $"Remembered \"{parts[0]}\" as \"{parts[1]}\".";
        if (_aiLang != null)
            return await _aiLang.GenerateAsync("remember", BuildChatPrompt(msg), "en", null);
        else
            return new ModuleResponse { Text = msg };
    }

    public async Task<ModuleResponse> ProcessRecallAsync(string key, object? memoryInst)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            if (_aiLang != null)
                return await _aiLang.GenerateAsync("recall", BuildChatPrompt("Please provide a key to recall."), "en", null);
            else
                return new ModuleResponse { Text = "Please provide a key to recall." };
        }

        var result = await RecallCompatAsync(memoryInst, key);
        string msg = result?.Text != null ? $"Recalled \"{key}\": \"{result.Text}\"" : $"I couldn't find any memory for \"{key}\".";
        if (_aiLang != null)
            return await _aiLang.GenerateAsync("recall", BuildChatPrompt(msg), "en", null);
        else
            return new ModuleResponse { Text = msg };
    }

    public async Task<ModuleResponse> ProcessStatusAsync(
        Mood currentMood,
        ConcurrentQueue<Thought> thoughtHistory,
        object? memoryInst,
        ISubconscious? sub,
        int awakeFlag,
        DateTime? awakeAtUtc)
    {
        var status = $"Status: Mood={currentMood}, Thoughts={thoughtHistory.Count}, MemoryBound={memoryInst != null}, SubconsciousAvailable={sub != null}, Awake={awakeFlag == 1}, AwakeAt={awakeAtUtc}";
        if (_aiLang != null)
            return await _aiLang.GenerateAsync("status", BuildChatPrompt(status), "en", null);
        else
            return new ModuleResponse { Text = status };
    }

    public static string[] GetMemorySnapshot(object? memoryInst)
    {
        if (memoryInst == null) return [];
        if (memoryInst is IMemory mem) return mem.GetAllItems()?.Select(i => i.Value ?? string.Empty).ToArray() ?? [];
        if (memoryInst is RaCore.Engine.Memory.MemoryModule mm) return mm.GetAllItems()?.Select(i => i.Value ?? string.Empty).ToArray() ?? [];
        return [];
    }

    private static async Task<string[]> GetSubconsciousSignalsAsync(ISubconscious? sub, string content)
    {
        if (sub == null) return [];
        try
        {
            var result = await sub.Probe(content, default);
            return string.IsNullOrWhiteSpace(result) ? [] : [result];
        }
        catch { return []; }
    }

    private static async Task<ModuleResponse?> RememberCompatAsync(object? memoryInst, string key, string value)
    {
        if (memoryInst == null) return null;
        if (memoryInst is IMemory mem) return await mem.RememberAsync(key, value);
        if (memoryInst is RaCore.Engine.Memory.MemoryModule mm) return await mm.RememberAsync(key, value);
        return null;
    }

    private static async Task<ModuleResponse?> RecallCompatAsync(object? memoryInst, string key)
    {
        if (memoryInst == null) return null;
        if (memoryInst is IMemory mem) return await mem.RecallAsync(key);
        if (memoryInst is RaCore.Engine.Memory.MemoryModule mm) return await mm.RecallAsync(key);
        return null;
    }

    internal Task<ModuleResponse> ProcessThoughtAsync(string trimmed, object? memoryInst, ISubconscious? sub, Mood currentMood)
    {
        throw new NotImplementedException();
    }
}
