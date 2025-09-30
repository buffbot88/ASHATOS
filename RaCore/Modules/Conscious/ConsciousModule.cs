using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RaCore.Engine.Manager;
using RaCore.Modules.Memory;
using RaCore.Modules.Subconscious;

namespace RaCore.Modules.Conscious;

[RaModule(Category = "core")]
public sealed class ConsciousModule : ModuleBase
{
    public override string Name => "Conscious";

    // Resolved in Initialize(ModuleManager)
    private ModuleManager? _manager;
    private object? _memoryInst;                 // IMemory or MemoryModule (or compatible)
    private ISubconscious? _sub;                 // optional

    // Public so tests can seed deterministically via reflection or direct access
    public readonly ConsciousThoughtProcessor _processor = new(ConsciousConfig.FeatureVectorSize, ConsciousConfig.LearningRate);

    private readonly ConcurrentQueue<Thought> _thoughtHistory = new();
    private readonly object _historyLock = new();

    // Keep mood local to avoid a hard dependency on DigitalFace types
    private Mood _currentMood = Mood.Neutral;

    // Wake state (ordered boot: Memory -> Subconscious -> Conscious -> Speech -> DigitalFace)
    private int _awakeFlag; // 0 = not awake, 1 = awake
    private DateTime? _awakeAtUtc;

    public override void Initialize(ModuleManager manager)
    {
        base.Initialize(manager);
        _manager = manager;

        // Resolve memory by name or type
        _memoryInst = manager.GetModuleInstanceByName("Memory")
                     ?? manager.GetModuleInstanceByName("MemoryModule");

        if (_memoryInst == null)
            LogWarn("Memory module not found. Conscious will operate in reduced mode.");

        // Wire training persistence into Memory (stores JSON under train/thought/{id})
        _processor.RememberHook = (key, value) => RememberCompatReturnGuid(key, value);

        // Resolve subconscious (must implement ISubconscious)
        var subInst = manager.GetModuleInstanceByName("Subconscious")
                     ?? manager.GetModuleInstanceByName("SubconsciousModule");
        _sub = subInst as ISubconscious;

        if (_sub == null)
            LogWarn("Subconscious module not found. Probe commands will be limited.");

        LogInfo("Conscious module initialized successfully.");
    }

    // ------------------ Ordered Wake Handlers (invoked by ModuleManager/Memory) ------------------

    // Preferred typed hook when MemoryReady is raised with MemoryModule payload
    private void OnMemoryReady(MemoryModule memory)
    {
        _memoryInst = memory;
        LogInfo("OnMemoryReady: bound to MemoryModule.");
    }

    // Alternate typed hook when MemoryReady is raised with IMemory payload
    private void OnMemoryReady(IMemory memory)
    {
        _memoryInst = memory;
        LogInfo("OnMemoryReady: bound to IMemory.");
    }

    // Fallback generic event hook
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

    // Wake is called after Subconscious in the ordered sequence
    private void OnWake()
    {
        if (Interlocked.CompareExchange(ref _awakeFlag, 1, 0) != 0)
            return; // already awake

        _awakeAtUtc = DateTime.UtcNow;
        _currentMood = Mood.Thinking;

        // Lightweight warm-up: store a heartbeat marker and precompute a tiny context
        try
        {
            var stamp = _awakeAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            RememberCompat("boot/conscious/heartbeat", $"awake:{stamp}");

            // Touch memory snapshot path to ensure compatibility
            _ = GetMemorySnapshot(-1);
            LogInfo("Conscious warm-up complete (heartbeat stored).");
        }
        catch (Exception ex)
        {
            LogWarn($"Warm-up encountered an issue: {ex.Message}");
        }
    }

    // Public convenience API
    public string Think(string input) => ProcessInput("think", input);
    public string Remember(string key, string value) => ProcessInput("remember", $"{key}={value}");
    public string Recall(string key) => ProcessInput("recall", key);
    public string ProbeSubconscious(string cmd) => ProcessInput("probe", cmd);

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "(empty input)";

        var trimmed = input.Trim();
        var firstSpace = trimmed.IndexOf(' ');
        var verb = firstSpace > 0 ? trimmed[..firstSpace].ToLowerInvariant() : trimmed.ToLowerInvariant();
        var args = firstSpace > 0 ? trimmed[(firstSpace + 1)..].Trim() : string.Empty;

        var result = verb switch
        {
            "help" => GetHelpText(),
            "think" => CommandThink(args),
            "remember" => CommandRemember(args),
            "recall" => CommandRecall(args),
            "status" => CommandStatus(),
            "history" => CommandHistory(args),
            "reset" => CommandReset(),
            "train" => CommandTrain(args),
            "probesub" => CommandProbeSub(args),
            "probe" => CommandProbeSub(args),
            _ when Regex.IsMatch(trimmed, @"\w+\s*=\s*.+") => CommandRemember(trimmed), // key=value
            _ => CommandThink(trimmed)
        };

        return result;
    }

    private string ProcessInput(string verb, string args)
    {
        try { return Process($"{verb} {args}".Trim()); }
        catch (Exception ex) { LogError($"Input processing error: {ex.Message}"); return $"(error) {ex.Message}"; }
    }

    private static string GetHelpText() => @"
Conscious Module Commands:
  think <input>            - Process input through consciousness
  remember key=value       - Store in memory
  recall key               - Retrieve from memory
  probe <text>             - Query subconscious
  status                   - Show current status
  history [count]          - View recent thoughts
  reset                    - Clear internal state
  train <id> <reward>      - Reinforce learning and store training event
  help                     - This help text".Trim();

    private string CommandThink(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Please provide content to think about.";

        var thought = new Thought
        {
            Id = _processor.GenerateThoughtId(),
            Content = content,
            Timestamp = DateTime.UtcNow,
            Source = "user"
        };

        try
        {
            var memoryContext = GetMemorySnapshot(thought.Id);
            var subconsciousSignals = GetSubconsciousSignals(thought.Content);
            var result = _processor.Process(
                thought,
                memoryContext,
                subconsciousSignals,
                ConsciousConfig.AssociationLimit
            );

            UpdateThoughtHistory(thought);
            UpdateMood(result);

            LogInfo($"Processed thought #{thought.Id}: {thought.Content}");
            return result;
        }
        catch (Exception ex)
        {
            LogError($"Thinking failed: {ex.Message}");
            return $"(thinking error) {ex.Message}";
        }
    }

    private string[] GetMemorySnapshot(int thoughtId)
    {
        if (_memoryInst == null) return Array.Empty<string>();

        try
        {
            var key = $"context_{thoughtId}";
            // Best-effort contextual stamp
            RememberCompat(key, DateTime.UtcNow.ToString("o"));

            // Build a short snapshot from recent memory items
            return GetAllItemsCompat()
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => $"{i.CreatedAt:HH:mm:ss} | {i.Key} = {i.Value}")
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private string[] GetSubconsciousSignals(string content)
    {
        if (_sub == null) return Array.Empty<string>();

        try
        {
            // Try to get a quick subconscious echo with a short timeout to avoid blocking
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
            var probeTask = _sub.Probe(content, cts.Token);
            var completed = probeTask.Wait(TimeSpan.FromMilliseconds(350));
            var text = completed ? (probeTask.Result ?? string.Empty) : string.Empty;
            return string.IsNullOrWhiteSpace(text) ? Array.Empty<string>() : new[] { text };
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private void UpdateThoughtHistory(Thought thought)
    {
        _thoughtHistory.Enqueue(thought);
        lock (_historyLock)
        {
            while (_thoughtHistory.Count > ConsciousConfig.ThoughtHistoryLimit)
                _thoughtHistory.TryDequeue(out _);
        }
    }

    private void UpdateMood(string processingResult)
    {
        _currentMood = DetermineMood(processingResult);
        LogInfo($"Mood set to: {_currentMood}");
    }

    private static Mood DetermineMood(string result)
    {
        if (result.IndexOf("thinking", StringComparison.OrdinalIgnoreCase) >= 0) return Mood.Thinking;
        if (result.IndexOf("speaking", StringComparison.OrdinalIgnoreCase) >= 0) return Mood.Speaking;
        if (result.IndexOf("confused", StringComparison.OrdinalIgnoreCase) >= 0) return Mood.Confused;
        if (result.IndexOf("happy", StringComparison.OrdinalIgnoreCase) >= 0) return Mood.Happy;
        return Mood.Neutral;
    }

    private string CommandHistory(string args)
    {
        // Show most recent first
        var items = _thoughtHistory.ToArray();
        Array.Reverse(items);

        if (string.IsNullOrWhiteSpace(args))
            return string.Join(Environment.NewLine, items.Select(t => $"{t.Id}: {t.Content}"));

        if (int.TryParse(args, out var count) && count > 0)
            return string.Join(Environment.NewLine, items.Take(count).Select(t => $"{t.Id}: {t.Content}"));

        return "Invalid history count. Please provide a number.";
    }

    private string CommandTrain(string args)
    {
        var parts = args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[0], out var id))
            return "Invalid train command. Usage: train <id> <reward>";

        if (!double.TryParse(parts[1], out var reward))
            return "Invalid reward value. Please provide a number.";

        var thought = _thoughtHistory.FirstOrDefault(t => t.Id == id);
        if (thought == null)
            return $"Thought with ID {id} not found.";

        try
        {
            // Train also persists a JSON record in Memory via RememberHook
            var msg = _processor.Train(thought, reward);
            return msg;
        }
        catch (Exception ex)
        {
            LogError($"Training failed: {ex.Message}");
            return $"(training error) {ex.Message}";
        }
    }

    private string CommandStatus()
    {
        var status = new StringBuilder();
        status.AppendLine("Conscious Module Status:");
        status.AppendLine($"- Current Mood: {_currentMood}");
        status.AppendLine($"- Thoughts in History: {_thoughtHistory.Count}");
        status.AppendLine($"- Memory Bound: {(_memoryInst != null ? "yes" : "no")}");
        status.AppendLine($"- Memory Entries: {CountCompat()}");
        status.AppendLine($"- Subconscious Available: {(_sub != null ? "yes" : "no")}");
        var awake = _awakeFlag == 1 ? "yes" : "no";
        var when = _awakeAtUtc?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "-";
        status.AppendLine($"- Awake: {awake} (at {when})");
        return status.ToString();
    }

    private string CommandReset()
    {
        lock (_historyLock) _thoughtHistory.Clear();
        ClearCompat();
        _currentMood = Mood.Neutral;
        Interlocked.Exchange(ref _awakeFlag, 0);
        _awakeAtUtc = null;
        return "Conscious module state has been reset.";
    }

    private string CommandRemember(string args)
    {
        var parts = args.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return "Usage: remember key=value";

        RememberCompat(parts[0], parts[1]);
        return $"Remembered: {parts[0]} = {parts[1]}";
    }

    private string CommandRecall(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return "Please provide a key to recall.";
        var value = RecallCompat(key);
        return value ?? $"No value found for key: {key}";
    }

    private string CommandProbeSub(string args)
    {
        if (string.IsNullOrWhiteSpace(args)) return "Please provide text to probe the subconscious.";
        if (_sub == null) return "Subconscious not available.";

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var task = _sub.Probe(args, cts.Token);
            var completed = task.Wait(TimeSpan.FromSeconds(2.2));
            if (!completed) return "(probe timeout)";
            return task.Result ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error probing subconscious: {ex.Message}";
        }
    }

    // ----------------- Memory compatibility helpers -----------------

    private void RememberCompat(string key, string value)
    {
        if (_memoryInst == null) return;

        // 1) Try IMemory
        if (_memoryInst is IMemory mem)
        {
            mem.Remember(key, value); // supports both void/Guid variants
            return;
        }

        // 2) Reflection fallback
        var t = _memoryInst.GetType();
        var m = t.GetMethod("Remember", new[] { typeof(string), typeof(string) });
        if (m != null) _ = m.Invoke(_memoryInst, new object[] { key, value });
    }

    // Try to return a Guid if underlying Remember provides one; otherwise null
    private Guid? RememberCompatReturnGuid(string key, string value)
    {
        if (_memoryInst == null) return null;

        try
        {
            if (_memoryInst is IMemory mem)
            {
                // IMemory may be void-returning in some builds; call it for side effects
                mem.Remember(key, value);
                // No Guid available through the interface here; fall through to reflection attempt
            }
        }
        catch
        {
            // Continue to reflection path
        }

        try
        {
            var t = _memoryInst.GetType();
            var m = t.GetMethod("Remember", new[] { typeof(string), typeof(string) });
            var res = m?.Invoke(_memoryInst, new object[] { key, value });
            if (res is Guid g) return g;
            if (res != null && Guid.TryParse(res.ToString(), out var parsed)) return parsed;
        }
        catch { }

        return null;
    }

    private string? RecallCompat(string key)
    {
        if (_memoryInst == null) return null;

        if (_memoryInst is IMemory mem) return mem.Recall(key);

        var t = _memoryInst.GetType();
        var m = t.GetMethod("Recall", new[] { typeof(string) });
        return m?.Invoke(_memoryInst, new object[] { key })?.ToString();
    }

    private IEnumerable<MemoryItem> GetAllItemsCompat()
    {
        if (_memoryInst == null) return Enumerable.Empty<MemoryItem>();

        if (_memoryInst is IMemory mem)
            return mem.GetAllItems() ?? Enumerable.Empty<MemoryItem>();

        var t = _memoryInst.GetType();
        var m = t.GetMethod("GetAllItems", Type.EmptyTypes);
        if (m != null)
        {
            var res = m.Invoke(_memoryInst, Array.Empty<object>());
            if (res is IEnumerable<MemoryItem> list) return list;
            if (res is System.Collections.IEnumerable seq)
            {
                var acc = new List<MemoryItem>();
                foreach (var it in seq) if (it is MemoryItem mi) acc.Add(mi);
                return acc;
            }
        }
        return Enumerable.Empty<MemoryItem>();
    }

    private int CountCompat()
    {
        try { return GetAllItemsCompat().Count(); }
        catch { return 0; }
    }

    private void ClearCompat()
    {
        if (_memoryInst == null) return;

        var t = _memoryInst.GetType();
        var clear = t.GetMethod("Clear", Type.EmptyTypes);
        if (clear != null) { _ = clear.Invoke(_memoryInst, Array.Empty<object>()); return; }

        var removeById = t.GetMethod("Remove", new[] { typeof(Guid) });
        if (removeById != null)
        {
            foreach (var item in GetAllItemsCompat())
            {
                try { removeById.Invoke(_memoryInst, new object[] { item.Id }); } catch { }
            }
            return;
        }

        var removeByKey = t.GetMethod("Remove", new[] { typeof(string) });
        if (removeByKey != null)
        {
            foreach (var key in GetAllItemsCompat().Select(i => i.Key).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try { removeByKey.Invoke(_memoryInst, new object[] { key }); } catch { }
            }
        }
    }
}

// Local enum to avoid a hard dependency on DigitalFace types
internal enum Mood { Neutral, Thinking, Speaking, Confused, Happy }

// The following types are assumed to exist in your project. If they differ, adjust signatures accordingly.

public sealed class Thought
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = "user";
    public double Score { get; internal set; }
    public override string ToString() => $"#{Id} [{Timestamp:HH:mm:ss}] {Source}: {Content}";
}

public static class ConsciousConfig
{
    public static int FeatureVectorSize { get; set; } = 128;
    public static double LearningRate { get; set; } = 0.01;
    public static int AssociationLimit { get; set; } = 8;
    public static int ThoughtHistoryLimit { get; set; } = 200;
}

// Public so other modules/tests can seed deterministically or inspect state.
public sealed class ConsciousThoughtProcessor
{
    private readonly int _vectorSize;
    private readonly double _learningRate;

    private readonly ConcurrentDictionary<int, double> _weights = new();

    private readonly object _rngLock = new();
    private Random _rnd = new();

    private int _thoughtCounter = 0;

    // Optional hook to persist training/thought events (e.g., into Memory)
    public Func<string, string, Guid?>? RememberHook { get; set; }

    public ConsciousThoughtProcessor(int featureVectorSize, double learningRate)
    {
        _vectorSize = Math.Max(8, featureVectorSize);
        _learningRate = Math.Max(0.0, learningRate);
    }

    public void SetRandomSeed(int seed)
    {
        lock (_rngLock) { _rnd = new Random(seed); }
    }

    public int GenerateThoughtId() => Interlocked.Increment(ref _thoughtCounter);

    public string Process(Thought t, IEnumerable<string> memorySignals, IEnumerable<string> subconsciousSignals, int associationLimit = 5)
    {
        var features = Vectorize(t.Content ?? string.Empty);
        var memFeatures = Vectorize(string.Join(" ", memorySignals ?? Array.Empty<string>()));
        var subFeatures = Vectorize(string.Join(" ", subconsciousSignals ?? Array.Empty<string>()));

        // Combine features with current weights
        var score = ScoreForFeatures(features) * 1.0
                  + ScoreForFeatures(memFeatures) * 0.8
                  + ScoreForFeatures(subFeatures) * 0.6;

        t.Score = score;

        // Associations
        var tokens = Tokenize(t.Content ?? string.Empty).Where(s => s.Length > 0).Distinct().ToArray();
        var adjectives = new[] { "bright", "distant", "warm", "strange", "familiar", "urgent", "calm", "noisy", "quiet" };

        var picks = new List<string>();
        if (tokens.Length > 0)
        {
            var limit = Math.Min(associationLimit, Math.Max(1, tokens.Length));
            for (int i = 0; i < limit; i++)
            {
                int ti, ai;
                lock (_rngLock)
                {
                    ti = _rnd.Next(tokens.Length);
                    ai = _rnd.Next(adjectives.Length);
                }
                var token = tokens[ti];
                var adj = adjectives[ai];
                picks.Add($"{token}-{adj}");
            }
        }

        var sb = new StringBuilder();
        sb.Append($"Thought #{t.Id}: I reflected on \"{TrimTo(t.Content ?? string.Empty, 120)}\". ");
        sb.Append($"Score={score:F2}. ");
        if (picks.Count > 0) sb.Append($"Associations: {string.Join(", ", picks)}. ");
        if (memorySignals != null && !string.IsNullOrWhiteSpace(string.Join(" ", memorySignals)))
            sb.Append("Memory cues influenced the thought. ");
        if (subconsciousSignals != null && !string.IsNullOrWhiteSpace(string.Join(" ", subconsciousSignals)))
            sb.Append("Subconscious echoes detected. ");

        return sb.ToString().Trim();
    }

    public string Train(Thought t, double reward)
    {
        var features = Vectorize(t.Content ?? string.Empty);

        foreach (var idx in features)
        {
            var delta = reward * _learningRate;
            _weights.AddOrUpdate(idx, delta, (_, old) => old + delta);
        }

        Guid? savedId = null;
        try
        {
            if (RememberHook != null)
            {
                var key = $"train/thought/{t.Id}";
                var payload = new
                {
                    id = t.Id,
                    content = t.Content,
                    reward,
                    timestamp = DateTime.UtcNow,
                    features
                };
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
                savedId = RememberHook(key, json);
            }
        }
        catch { /* non-fatal */ }

        var idText = savedId.HasValue && savedId.Value != Guid.Empty ? $" saved id={savedId}" : " saved";
        return $"trained thought #{t.Id} (reward {reward}){idText}";
    }

    private int[] Vectorize(string text)
    {
        var tokens = Tokenize(text);
        var set = new HashSet<int>();
        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token)) continue;
            int h = Math.Abs(token.GetHashCode()) % _vectorSize;
            set.Add(h);
            // a couple simple n-gram-ish variants
            for (int i = 1; i <= Math.Min(2, token.Length); i++)
            {
                var sub = token[..i];
                int h2 = (Math.Abs((token + sub).GetHashCode()) + 13) % _vectorSize;
                set.Add(h2);
            }
        }
        return set.ToArray();
    }

    private double ScoreForFeatures(int[] features)
    {
        double s = 0.0;
        foreach (var f in features)
            if (_weights.TryGetValue(f, out var w)) s += w;
        return Math.Tanh(s); // simple normalization
    }

    public static string[] Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
        return text
            .ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToArray();
    }

    public static string TrimTo(string s, int len) => s?.Length > len ? s[..(len - 3)] + "..." : s ?? string.Empty;
}
