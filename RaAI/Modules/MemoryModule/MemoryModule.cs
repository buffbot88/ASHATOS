using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RaAI.Handlers.Manager;
using System.Reflection;

namespace RaAI.Modules.MemoryModule
{
    // If you have RaModuleAttribute available, uncomment the next line to aid discovery.
    // [RaModule("Memory")]
    public class MemoryModule : ModuleBase
    {
        private readonly object _sync = new();
        private readonly string _binFilePath;
        private readonly Dictionary<Guid, MemoryItem> _memory = new();
        private readonly Dictionary<Guid, Candidate> _candidates = new();
        private readonly Dictionary<Guid, ConsciousEntry> _consciousIndex = new();

        // Boot/wake orchestration
        private ModuleManager? _manager;
        private int _bootStarted; // 0 = not started, 1 = started (Interlocked)

        public override string Name => "Memory";

        // IMPORTANT: Parameterless constructor required by the new ModuleManager
        public MemoryModule()
        {
            _binFilePath = Path.Combine(AppContext.BaseDirectory, "raai_memory.bin");
            SafeLoad();
            LogInfo("Memory module initialized (parameterless).");
        }

        // Optional convenience constructor (not used by ModuleManager discovery)
        public MemoryModule(string binFilePath)
        {
            _binFilePath = string.IsNullOrWhiteSpace(binFilePath)
                ? Path.Combine(AppContext.BaseDirectory, "raai_memory.bin")
                : binFilePath;
            SafeLoad();
            LogInfo($"Memory module initialized (custom path: {_binFilePath}).");
        }

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _manager = manager;

            // Kick off boot/wake orchestration after manager finishes loading others.
            // We defer slightly to allow a single-phase loader to add other modules,
            // but this remains idempotent even if called multiple times (reloads).
            _ = TryStartBootSequenceAsync();
        }

        // Provide a simple command interface so ModuleManager.SafeInvokeModuleByName can call Process(string)
        // Supported commands:
        //  - memory help
        //  - remember key=value
        //  - recall key
        //  - remove key <key>
        //  - remove id <guid>
        //  - list [top N]
        //  - query <text>
        //  - stats
        //  - boot (manual trigger for wake sequence)
        public override string Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return HelpText();

            var text = input.Trim();
            var lower = text.ToLowerInvariant();

            try
            {
                if (lower == "memory help" || lower == "help" || lower.StartsWith("memory:help"))
                    return HelpText();

                if (lower == "boot" || lower == "memory boot")
                {
                    _ = TryStartBootSequenceAsync(force: true);
                    return "Boot/wake sequence requested.";
                }

                if (lower.StartsWith("remember "))
                {
                    // remember key=value OR remember key: value
                    var rest = text.Substring("remember ".Length).Trim();
                    string key, value;
                    if (TrySplit(rest, out key, out value))
                    {
                        var id = Remember(key, value);
                        return $"Remembered [{key}] -> id={id}";
                    }
                    return "remember expects: remember key=value";
                }

                if (lower.StartsWith("recall "))
                {
                    var key = text.Substring("recall ".Length).Trim();
                    var val = Recall(key);
                    return val != null
                        ? $"Recall [{key}] => {val}"
                        : $"No value found for key [{key}]";
                }

                if (lower.StartsWith("remove id "))
                {
                    var idStr = text.Substring("remove id ".Length).Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        var ok = Remove(id);
                        return ok ? $"Removed item id={id}" : $"No item with id={id}";
                    }
                    return "Invalid guid. Usage: remove id <guid>";
                }

                if (lower.StartsWith("remove key "))
                {
                    var key = text.Substring("remove key ".Length).Trim();
                    var ok = Remove(key);
                    return ok ? $"Removed all items for key [{key}]" : $"No items removed for key [{key}]";
                }

                if (lower.StartsWith("list"))
                {
                    var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int take = 50; // default
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var n) && n > 0) take = n;

                    var items = GetAllItems()
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(take)
                        .ToList();

                    if (items.Count == 0) return "Memory is empty.";
                    return string.Join(Environment.NewLine,
                        items.Select(i => $"{i.CreatedAt:yyyy-MM-dd HH:mm:ss} | {i.Id} | {i.Key} = {i.Value}"));
                }

                if (lower.StartsWith("query "))
                {
                    var q = text.Substring("query ".Length).Trim();
                    var items = Query(i => (i.Key?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                                         || (i.Value?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                                         .OrderByDescending(i => i.CreatedAt)
                                         .Take(50)
                                         .ToList();

                    if (items.Count == 0) return $"No matches for [{q}].";
                    return string.Join(Environment.NewLine,
                        items.Select(i => $"{i.CreatedAt:yyyy-MM-dd HH:mm:ss} | {i.Id} | {i.Key} = {i.Value}"));
                }

                if (lower == "stats" || lower == "memory stats")
                {
                    lock (_sync)
                    {
                        return $"Memory stats: items={_memory.Count}, candidates={_candidates.Count}, conscious={_consciousIndex.Count}";
                    }
                }

                // Fallback: treat as "recall <input>" if no explicit command matched
                var recall = Recall(text);
                return recall != null
                    ? $"Recall [{text}] => {recall}"
                    : $"Unknown command. Try: memory help";
            }
            catch (Exception ex)
            {
                LogError($"Process error: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        // Store a key/value pair and persist to bin file
        public Guid Remember(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var item = new MemoryItem
            {
                Id = Guid.NewGuid(),
                Key = key.Trim(),
                Value = value.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            lock (_sync)
            {
                _memory[item.Id] = item;
                AppendToBinFile(item);
            }

            LogInfo($"Remembered: {item.Key}='{item.Value}'");
            return item.Id;
        }

        // Recall latest value for given key
        public string? Recall(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_sync)
            {
                var item = _memory.Values
                    .Where(i => string.Equals(i.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefault();

                if (item != null)
                {
                    LogInfo($"Recalled: {key}='{item.Value}'");
                    return item.Value;
                }
            }

            LogInfo($"Key not found in memory: {key}");
            return null;
        }

        // Enumerate all memory items
        public IEnumerable<MemoryItem> GetAllItems()
        {
            lock (_sync)
            {
                return _memory.Values.ToList();
            }
        }

        // Retrieve a memory item by its Guid
        public MemoryItem? GetItemById(Guid id)
        {
            lock (_sync)
            {
                _memory.TryGetValue(id, out var item);
                return item;
            }
        }

        // Remove a memory item by Guid
        public bool Remove(Guid id)
        {
            lock (_sync)
            {
                if (_memory.Remove(id))
                {
                    SaveAllToBinFile();
                    return true;
                }
            }
            return false;
        }

        // Remove a memory item by key (removes all matching keys)
        public bool Remove(string key)
        {
            var removed = false;
            lock (_sync)
            {
                var itemsToRemove = _memory.Values
                    .Where(i => string.Equals(i.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();

                foreach (var id in itemsToRemove)
                {
                    _memory.Remove(id);
                    removed = true;
                }

                if (removed)
                    SaveAllToBinFile();
            }
            return removed;
        }

        // Query by predicate
        public IEnumerable<MemoryItem> Query(Func<MemoryItem, bool> predicate)
        {
            lock (_sync)
            {
                return _memory.Values.Where(predicate).ToList();
            }
        }

        // Candidate/Conscious logic (kept for compatibility)
        public Guid AddCandidate(string text, Dictionary<string, string> metadata, bool requireConsent)
        {
            var candidate = new Candidate
            {
                CandidateId = Guid.NewGuid(),
                Text = text,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                RequireConsent = requireConsent
            };
            lock (_sync)
            {
                _candidates[candidate.CandidateId] = candidate;
            }
            return candidate.CandidateId;
        }

        public void PromoteCandidateToMemory(Guid candidateId)
        {
            lock (_sync)
            {
                if (_candidates.TryGetValue(candidateId, out var candidate))
                {
                    var memoryItem = new MemoryItem
                    {
                        Id = candidate.CandidateId,
                        Key = candidate.Text,
                        Value = candidate.Text,
                        CreatedAt = DateTime.UtcNow
                    };
                    _memory[memoryItem.Id] = memoryItem;
                    AppendToBinFile(memoryItem);
                    _candidates.Remove(candidateId);
                }
            }
        }

        public void PromoteToConscious(Guid itemId)
        {
            lock (_sync)
            {
                if (_memory.TryGetValue(itemId, out var memoryItem))
                {
                    var consciousEntry = new ConsciousEntry
                    {
                        Id = memoryItem.Id,
                        PromotedAt = DateTime.UtcNow
                    };
                    _consciousIndex[consciousEntry.Id] = consciousEntry;
                }
            }
        }

        // --- Persistence Logic ---

        private void AppendToBinFile(MemoryItem item)
        {
            try
            {
                var json = JsonSerializer.Serialize(item);
                File.AppendAllText(_binFilePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                LogError($"AppendToBinFile failed: {ex.Message}");
            }
        }

        private void SaveAllToBinFile()
        {
            try
            {
                var allJson = _memory.Values.Select(i => JsonSerializer.Serialize(i));
                File.WriteAllLines(_binFilePath, allJson);
            }
            catch (Exception ex)
            {
                LogError($"SaveAllToBinFile failed: {ex.Message}");
            }
        }

        private void LoadFromBinFile()
        {
            if (!File.Exists(_binFilePath))
                return;

            _memory.Clear();

            foreach (var line in File.ReadAllLines(_binFilePath))
            {
                try
                {
                    var item = JsonSerializer.Deserialize<MemoryItem>(line);
                    if (item != null)
                        _memory[item.Id] = item;
                }
                catch
                {
                    // Ignore bad lines
                }
            }
        }

        private void SafeLoad()
        {
            try
            {
                // Ensure directory exists if a path includes directories
                var dir = Path.GetDirectoryName(_binFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                LoadFromBinFile();
            }
            catch (Exception ex)
            {
                LogError($"Load failed: {ex.Message}");
            }
        }

        // Helpers

        private static bool TrySplit(string text, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;

            // Try "key=value"
            var eq = text.IndexOf('=');
            if (eq > 0)
            {
                key = text[..eq].Trim();
                value = text[(eq + 1)..].Trim();
                return !string.IsNullOrWhiteSpace(key);
            }

            // Try "key: value"
            var colon = text.IndexOf(':');
            if (colon > 0)
            {
                key = text[..colon].Trim();
                value = text[(colon + 1)..].Trim();
                return !string.IsNullOrWhiteSpace(key);
            }

            return false;
        }

        private static string HelpText()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Memory Module Commands:",
                "  memory help                 - show this help",
                "  remember key=value          - store a key/value pair",
                "  recall key                  - recall the latest value for key",
                "  remove key <key>            - remove all entries with the key",
                "  remove id <guid>            - remove an entry by identifier",
                "  list [N]                    - list most recent N (default 50)",
                "  query <text>                - search key/value contains text",
                "  stats                       - show memory counters",
                "  boot                        - trigger the boot/wake sequence (manual)"
            });
        }

        // -------------------- Boot/Wake Orchestration (Memory-led) --------------------

        // If ModuleManager has an event bus (RaiseSystemEvent), we'll use it.
        // Otherwise we will reflectively invoke hooks on specific modules in sequence.
        private async Task TryStartBootSequenceAsync(bool force = false)
        {
            if (!force)
            {
                if (Interlocked.CompareExchange(ref _bootStarted, 1, 0) != 0)
                    return; // already started
            }
            else
            {
                // forced trigger skips the "already started" guard
                Interlocked.Exchange(ref _bootStarted, 1);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            LogInfo("Boot sequence starting...");

            // Light touch delay to allow single-phase loaders to finish adding modules.
            await Task.Delay(50);

            // Wait briefly for required modules to appear (best-effort).
            var targets = new[] { "Subconscious", "Conscious", "Speech", "DigitalFace" };
            await WaitForModulesAsync(targets, TimeSpan.FromSeconds(2));

            // 0) Optional broadcast: SystemBoot (if manager supports it)
            TryRaiseSystemEvent("SystemBoot", null);

            // 1) Memory is ready -> announce to others
            AnnounceMemoryReady();

            // 2) Wake in strict order
            WakeModuleByName("Subconscious");
            WakeModuleByName("Conscious");
            WakeModuleByName("Speech");
            WakeModuleByName("DigitalFace");

            // 3) Optional broadcast warmup
            TryRaiseSystemEvent("Warmup", null);

            sw.Stop();
            LogInfo($"Boot sequence complete in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task WaitForModulesAsync(IEnumerable<string> preferredNames, TimeSpan timeout)
        {
            if (_manager == null) return;

            var names = preferredNames?.ToArray() ?? Array.Empty<string>();
            if (names.Length == 0) return;

            var end = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < end)
            {
                if (names.All(n => FindModuleInstanceByName(n) != null))
                    break;

                await Task.Delay(50);
            }
        }

        private void AnnounceMemoryReady()
        {
            // Prefer manager's event bus if available
            if (TryRaiseSystemEvent("MemoryReady", this))
                return;

            // Otherwise, notify each target directly
            NotifyModuleMemoryReady("Subconscious");
            NotifyModuleMemoryReady("Conscious");
            NotifyModuleMemoryReady("Speech");
            NotifyModuleMemoryReady("DigitalFace");
        }

        private void NotifyModuleMemoryReady(string name)
        {
            var target = FindModuleInstanceByName(name);
            if (target == null) return;

            // Try OnMemoryReady(MemoryModule or compatible)
            if (!TryInvokeTyped(target, "OnMemoryReady", this))
            {
                // Fall back to generic event
                TryInvokeEvent(target, "OnSystemEvent", "MemoryReady", this);
            }
        }

        private void WakeModuleByName(string name)
        {
            var target = FindModuleInstanceByName(name);
            if (target == null) return;

            // Try OnWake()
            if (!TryInvokeNoArg(target, "OnWake"))
            {
                // Fall back to generic event name
                TryInvokeEvent(target, "OnSystemEvent", "Wake", null);
            }
        }

        private IRaModule? FindModuleInstanceByName(string preferred)
        {
            if (_manager == null || string.IsNullOrWhiteSpace(preferred)) return null;

            // Prefer declared Name match
            var inst = _manager.GetModuleInstanceByName(preferred);
            if (inst != null) return inst;

            // Fallbacks: try suffix "Module"
            inst = _manager.GetModuleInstanceByName(preferred + "Module");
            if (inst != null) return inst;

            // Last resort: exact type name scan
            return _manager.Modules
                           .Select(w => w.Instance)
                           .FirstOrDefault(i => string.Equals(i?.GetType().Name, preferred, StringComparison.OrdinalIgnoreCase));
        }

        // --------------- Reflection helpers ---------------

        private bool TryRaiseSystemEvent(string name, object? payload)
        {
            if (_manager == null) return false;

            try
            {
                var mi = _manager.GetType().GetMethod("RaiseSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    var pars = mi.GetParameters();
                    if (pars.Length == 2)
                        mi.Invoke(_manager, new[] { name, payload });
                    else if (pars.Length == 1)
                        mi.Invoke(_manager, new object[] { name });
                    else
                        mi.Invoke(_manager, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogWarn($"RaiseSystemEvent failed: {ex.Message}");
            }
            return false;
        }

        private static bool TryInvokeTyped(object target, string methodName, object? payload)
        {
            if (target == null || payload == null) return false;
            var t = target.GetType();

            try
            {
                var m = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .FirstOrDefault(mth =>
                         {
                             if (!string.Equals(mth.Name, methodName, StringComparison.Ordinal)) return false;
                             var ps = mth.GetParameters();
                             return ps.Length == 1 && ps[0].ParameterType.IsInstanceOfType(payload);
                         });

                if (m != null)
                {
                    m.Invoke(target, new[] { payload });
                    return true;
                }

                // Allow assignable parameter (e.g., interface/base class)
                m = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .FirstOrDefault(mth =>
                     {
                         if (!string.Equals(mth.Name, methodName, StringComparison.Ordinal)) return false;
                         var ps = mth.GetParameters();
                         return ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(payload.GetType());
                     });

                if (m != null)
                {
                    m.Invoke(target, new[] { payload });
                    return true;
                }
            }
            catch
            {
                // swallow
            }
            return false;
        }

        private static bool TryInvokeNoArg(object target, string methodName)
        {
            if (target == null) return false;
            var t = target.GetType();

            try
            {
                var m = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (m != null)
                {
                    m.Invoke(target, null);
                    return true;
                }
            }
            catch
            {
                // swallow
            }
            return false;
        }

        private static bool TryInvokeEvent(object target, string methodName, string name, object? payload)
        {
            if (target == null) return false;
            var t = target.GetType();

            try
            {
                // Prefer (string, object) signature
                var m2 = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object) }, null);
                if (m2 != null)
                {
                    m2.Invoke(target, new[] { name, payload! });
                    return true;
                }

                // Fallback: (string) only
                var m1 = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (m1 != null)
                {
                    m1.Invoke(target, new object[] { name });
                    return true;
                }
            }
            catch
            {
                // swallow
            }
            return false;
        }

        // Optional: receive a SystemBoot broadcast from ModuleManager (if available)
        // to trigger the same memory-led sequence (idempotent).
        private void OnSystemEvent(string name, object? payload)
        {
            if (string.Equals(name, "SystemBoot", StringComparison.OrdinalIgnoreCase))
            {
                _ = TryStartBootSequenceAsync();
            }
        }
    }

    // Data records (keep as-is if already defined elsewhere in your project)
    public class MemoryItem
    {
        public Guid Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class Candidate
    {
        public Guid CandidateId { get; set; }
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool RequireConsent { get; set; }
    }

    public class ConsciousEntry
    {
        public Guid Id { get; set; }
        public DateTime PromotedAt { get; set; }
    }
}