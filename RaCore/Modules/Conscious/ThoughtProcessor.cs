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

        if (_aiLang != null)
        {
            // Build a conversational prompt that instructs the AI to respond naturally
            string contextInfo = BuildContextInfo(memoryContext, subconsciousSignals, currentMood);
            string chatPrompt = BuildConversationalPrompt(content, contextInfo);
            return await _aiLang.GenerateAsync("think", chatPrompt, "en", null);
        }
        else
        {
            // Fallback: diagnostic format when no AI available
            string agenticSummary = $"Thought: \"{content}\". Memory: [{string.Join("; ", memoryContext)}]. Subconscious: [{string.Join("; ", subconsciousSignals)}]. Mood: {currentMood}.";
            return new ModuleResponse { Text = agenticSummary };
        }
    }

    private static string BuildChatPrompt(string context)
    {
        return $"[INST] <<SYS>>\nYou are a helpful assistant.\n<</SYS>>\n{context} [/INST]";
    }

    private static string BuildConversationalPrompt(string userInput, string contextInfo)
    {
        // Create a prompt that encourages natural, conversational responses
        string instruction = "You are Ra, a helpful AI assistant. Respond naturally and conversationally to the user's input.";
        
        if (!string.IsNullOrWhiteSpace(contextInfo))
        {
            instruction += $" Consider the following context: {contextInfo}";
        }
        
        return $"[INST] <<SYS>>\n{instruction}\n<</SYS>>\n{userInput} [/INST]";
    }

    private static string BuildContextInfo(string[] memoryContext, string[] subconsciousSignals, Mood currentMood)
    {
        var parts = new List<string>();
        
        if (memoryContext.Length > 0)
        {
            parts.Add($"relevant memories: {string.Join(", ", memoryContext.Take(3))}");
        }
        
        if (subconsciousSignals.Length > 0)
        {
            parts.Add($"insights: {string.Join(", ", subconsciousSignals.Take(2))}");
        }
        
        if (currentMood != Mood.Neutral)
        {
            parts.Add($"current mood: {currentMood}");
        }
        
        return parts.Count > 0 ? string.Join("; ", parts) : string.Empty;
    }

    public async Task<ModuleResponse> ProcessRememberAsync(string args, object? memoryInst)
    {
        var parts = args.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            if (_aiLang != null)
            {
                string prompt = BuildConversationalPrompt("The user wants to remember something but didn't provide the correct format.", "Explain that they should use the format: remember key=value");
                return await _aiLang.GenerateAsync("remember", prompt, "en", null);
            }
            else
                return new ModuleResponse { Text = "To remember something, use: remember key=value" };
        }

        _ = await RememberCompatAsync(memoryInst, parts[0], parts[1]);
        
        if (_aiLang != null)
        {
            string prompt = BuildConversationalPrompt($"I successfully stored '{parts[0]}' with the value '{parts[1]}'.", "Acknowledge this in a natural, friendly way.");
            return await _aiLang.GenerateAsync("remember", prompt, "en", null);
        }
        else
        {
            string msg = $"Remembered \"{parts[0]}\" as \"{parts[1]}\".";
            return new ModuleResponse { Text = msg };
        }
    }

    public async Task<ModuleResponse> ProcessRecallAsync(string key, object? memoryInst)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            if (_aiLang != null)
            {
                string prompt = BuildConversationalPrompt("The user wants to recall something but didn't specify what.", "Ask them naturally what they'd like me to recall.");
                return await _aiLang.GenerateAsync("recall", prompt, "en", null);
            }
            else
                return new ModuleResponse { Text = "Please provide a key to recall." };
        }

        var result = await RecallCompatAsync(memoryInst, key);
        
        if (_aiLang != null)
        {
            string contextInfo = result?.Text != null 
                ? $"I found the memory for '{key}': {result.Text}" 
                : $"I couldn't find any memory for '{key}'.";
            string prompt = BuildConversationalPrompt($"The user asked me to recall '{key}'.", contextInfo);
            return await _aiLang.GenerateAsync("recall", prompt, "en", null);
        }
        else
        {
            string msg = result?.Text != null ? $"Recalled \"{key}\": \"{result.Text}\"" : $"I couldn't find any memory for \"{key}\".";
            return new ModuleResponse { Text = msg };
        }
    }

    public async Task<ModuleResponse> ProcessStatusAsync(
        Mood currentMood,
        ConcurrentQueue<Thought> thoughtHistory,
        object? memoryInst,
        ISubconscious? sub,
        int awakeFlag,
        DateTime? awakeAtUtc)
    {
        if (_aiLang != null)
        {
            // Create a natural language status summary
            var statusParts = new List<string>();
            statusParts.Add($"I am currently {(awakeFlag == 1 ? "awake and operational" : "in standby mode")}.");
            
            if (awakeAtUtc.HasValue)
            {
                var elapsed = DateTime.UtcNow - awakeAtUtc.Value;
                if (elapsed.TotalMinutes < 60)
                    statusParts.Add($"I've been active for about {(int)elapsed.TotalMinutes} minutes.");
                else if (elapsed.TotalHours < 24)
                    statusParts.Add($"I've been active for about {(int)elapsed.TotalHours} hours.");
            }
            
            if (currentMood != Mood.Neutral)
            {
                statusParts.Add($"My current mood is {currentMood.ToString().ToLower()}.");
            }
            
            if (thoughtHistory.Count > 0)
            {
                statusParts.Add($"I have {thoughtHistory.Count} thought(s) in my recent history.");
            }
            
            if (memoryInst != null)
            {
                statusParts.Add("My memory system is active.");
            }
            
            if (sub != null)
            {
                statusParts.Add("My subconscious processing is available.");
            }
            
            string naturalStatus = string.Join(" ", statusParts);
            string prompt = BuildConversationalPrompt("The user asked for my status.", naturalStatus);
            return await _aiLang.GenerateAsync("status", prompt, "en", null);
        }
        else
        {
            // Fallback: diagnostic format when no AI available
            var status = $"Status: Mood={currentMood}, Thoughts={thoughtHistory.Count}, MemoryBound={memoryInst != null}, SubconsciousAvailable={sub != null}, Awake={awakeFlag == 1}, AwakeAt={awakeAtUtc}";
            return new ModuleResponse { Text = status };
        }
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
