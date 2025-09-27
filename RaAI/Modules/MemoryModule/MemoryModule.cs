using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RaAI.Handlers.Manager;

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
            // Nothing extra required right now; data already loaded in ctor.
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
                "  stats                       - show memory counters"
            });
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