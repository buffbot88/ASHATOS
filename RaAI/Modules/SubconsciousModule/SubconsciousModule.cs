using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RaAI.Handlers.Manager;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.SubconsciousModule
{
    // [RaModule("Subconscious")]
    public class SubconsciousModule : ModuleBase, ISubconscious, IDisposable
    {
        private const string AutonomousPrefix = "Autonomous/";
        private object? _memoryInst; // IMemory or MemoryModule or other
        private readonly object _sync = new();

        public override string Name => "Subconscious";

        public SubconsciousModule() { }

        public SubconsciousModule(IMemory memory)
        {
            _memoryInst = memory ?? throw new ArgumentNullException(nameof(memory));
            LogInfo("Subconscious module initialized with injected IMemory.");
        }

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _memoryInst = manager.GetModuleInstanceByName("Memory")
                        ?? manager.GetModuleInstanceByName("MemoryModule");
            if (_memoryInst == null)
                LogWarn("Memory module not found. Subconscious will operate in reduced mode until Memory is available.");
            else
                LogInfo($"Linked to Memory instance: {_memoryInst.GetType().FullName}");
        }

        public Guid AddAutonomousMemory(string key, string value, Dictionary<string, string>? metadata = null)
        {
            EnsureMemoryOrThrow();

            var id = Guid.NewGuid();
            var pathKey = $"{AutonomousPrefix}{key}:{id}";

            var valueToStore = value;
            if (metadata != null && metadata.Count > 0)
            {
                var metaJson = JsonSerializer.Serialize(metadata);
                valueToStore += $"|meta:{metaJson}";
            }

            // Store; we don't need the return of Remember because we already minted the id
            RememberCompat(pathKey, valueToStore);
            LogInfo($"Autonomous memory stored: {pathKey}");
            return id;
        }

        public List<MemoryItem> QueryAutonomousByPrefix(string prefix)
        {
            var fullPrefix = $"{AutonomousPrefix}{prefix}";
            var results = GetAllItems()
                .Where(item => (item.Key ?? string.Empty).StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            LogInfo($"Queried {results.Count} autonomous items with prefix '{prefix}'.");
            return results;
        }

        public string? RecallAutonomous(string key)
        {
            var fullKey = $"{AutonomousPrefix}{key}";
            var val = Recall(fullKey);
            LogInfo($"Recall autonomous key '{fullKey}': '{val}'");
            return val;
        }

        public string? SignAutonomousMemory(Guid id)
        {
            var item = GetItemById(id);
            if (item == null || item.Value == null) return null;

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(item.Value));
            return Convert.ToBase64String(hash);
        }

        public async Task<string> Probe(string query, CancellationToken cancellationToken = default)
        {
            var results = GetAllItems()
                .Where(item => (item.Key ?? string.Empty).StartsWith(AutonomousPrefix, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(item.Value)
                            && item.Value!.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(item => item.Value!)
                .ToList();
            return await Task.FromResult(string.Join("; ", results));
        }

        public void ReceiveMessage(string message)
        {
            LogInfo($"Received message in subconscious: {message}");
        }

        public string GetResponse() => string.Empty;

        public override string Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return HelpText();

            var text = input.Trim();
            var lower = text.ToLowerInvariant();

            try
            {
                if (lower == "sub help" || lower == "subconscious help" || lower == "help" || lower.StartsWith("sub:help"))
                    return HelpText();

                if (lower.StartsWith("sub add ") || lower.StartsWith("subconscious add "))
                {
                    var rest = text[(text.IndexOf(" add ", StringComparison.OrdinalIgnoreCase) + 5)..].Trim();
                    var (kv, metaPart) = SplitOnce(rest, " meta ");
                    if (!TrySplitKeyValue(kv, out var key, out var value))
                        return "Usage: sub add key=value [meta k1=v1;k2=v2]";

                    Dictionary<string, string>? meta = null;
                    if (!string.IsNullOrWhiteSpace(metaPart))
                        meta = ParseMetadata(metaPart);

                    var id = AddAutonomousMemory(key, value, meta);
                    return $"OK: stored Autonomous/{key}:{id}";
                }

                if (lower.StartsWith("sub recall ") || lower.StartsWith("subconscious recall "))
                {
                    var key = text[(text.IndexOf(" recall ", StringComparison.OrdinalIgnoreCase) + 8)..].Trim();
                    var val = RecallAutonomous(key);
                    return val != null ? $"Recall {AutonomousPrefix}{key} => {val}" : $"No value for {AutonomousPrefix}{key}";
                }

                if (lower.StartsWith("sub query ") || lower.StartsWith("subconscious query "))
                {
                    var prefix = text[(text.IndexOf(" query ", StringComparison.OrdinalIgnoreCase) + 7)..].Trim();
                    var list = QueryAutonomousByPrefix(prefix);
                    if (list.Count == 0) return $"No Autonomous items with prefix '{prefix}'.";
                    return string.Join(Environment.NewLine,
                        list.OrderByDescending(i => i.CreatedAt)
                            .Take(100)
                            .Select(i => $"{i.CreatedAt:yyyy-MM-dd HH:mm:ss} | {i.Id} | {i.Key} = {i.Value}"));
                }

                if (lower.StartsWith("sub sign ") || lower.StartsWith("subconscious sign "))
                {
                    var idStr = text[(text.IndexOf(" sign ", StringComparison.OrdinalIgnoreCase) + 6)..].Trim();
                    if (!Guid.TryParse(idStr, out var id)) return "Invalid guid. Usage: sub sign <guid>";
                    var sig = SignAutonomousMemory(id);
                    return sig != null ? $"Signature: {sig}" : "Item not found.";
                }

                if (lower.StartsWith("sub probe ") || lower.StartsWith("subconscious probe "))
                {
                    var q = text[(text.IndexOf(" probe ", StringComparison.OrdinalIgnoreCase) + 7)..].Trim();
                    return Probe(q).GetAwaiter().GetResult();
                }

                return HelpText();
            }
            catch (Exception ex)
            {
                LogError($"Process error: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        // ---- Compatibility helpers (handle both Guid-returning and void-returning Remember) ----

        private void RememberCompat(string key, string value)
        {
            if (_memoryInst == null) throw new InvalidOperationException("Memory module is not available.");

            lock (_sync)
            {
                switch (_memoryInst)
                {
                    case MemoryModule.MemoryModule mm:
                        _ = mm.Remember(key, value); // returns Guid, but we don't need it here
                        break;

                    case IMemory mem:
                        // IMemory may return void in your codebase; just invoke it.
                        mem.Remember(key, value);
                        break;

                    default:
                        // Reflection fallback; accept both Guid and void
                        var t = _memoryInst.GetType();
                        var m = t.GetMethod("Remember", new[] { typeof(string), typeof(string) });
                        if (m == null) throw new MissingMethodException(t.FullName, "Remember(string,string)");
                        _ = m.Invoke(_memoryInst, new object[] { key, value });
                        break;
                }
            }
        }

        private string? Recall(string key)
        {
            if (_memoryInst == null) return null;

            lock (_sync)
            {
                switch (_memoryInst)
                {
                    case IMemory mem:
                        return mem.Recall(key);
                    case MemoryModule.MemoryModule mm:
                        return mm.Recall(key);
                    default:
                        var t = _memoryInst.GetType();
                        var m = t.GetMethod("Recall", new[] { typeof(string) });
                        return m?.Invoke(_memoryInst, new object[] { key })?.ToString();
                }
            }
        }

        private IEnumerable<MemoryItem> GetAllItems()
        {
            if (_memoryInst == null) return Enumerable.Empty<MemoryItem>();

            lock (_sync)
            {
                switch (_memoryInst)
                {
                    case IMemory mem:
                        return mem.GetAllItems().ToList();
                    case MemoryModule.MemoryModule mm:
                        return mm.GetAllItems().ToList();
                    default:
                        var t = _memoryInst.GetType();
                        var m = t.GetMethod("GetAllItems", Type.EmptyTypes);
                        if (m != null)
                        {
                            var res = m.Invoke(_memoryInst, Array.Empty<object>());
                            if (res is IEnumerable<MemoryItem> typed) return typed.ToList();
                            if (res is System.Collections.IEnumerable seq)
                            {
                                var list = new List<MemoryItem>();
                                foreach (var it in seq)
                                    if (it is MemoryItem mi) list.Add(mi);
                                return list;
                            }
                        }
                        return Enumerable.Empty<MemoryItem>();
                }
            }
        }

        private MemoryItem? GetItemById(Guid id)
        {
            if (_memoryInst == null) return null;

            lock (_sync)
            {
                switch (_memoryInst)
                {
                    case IMemory mem:
                        return mem.GetItemById(id);
                    case MemoryModule.MemoryModule mm:
                        return mm.GetItemById(id);
                    default:
                        var t = _memoryInst.GetType();
                        var m = t.GetMethod("GetItemById", new[] { typeof(Guid) });
                        var res = m?.Invoke(_memoryInst, new object[] { id });
                        return res as MemoryItem;
                }
            }
        }

        private static (string left, string right) SplitOnce(string text, string delimiter)
        {
            var idx = text.IndexOf(delimiter, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return (text, string.Empty);
            return (text[..idx], text[(idx + delimiter.Length)..]);
        }

        private static bool TrySplitKeyValue(string text, out string key, out string value)
        {
            key = string.Empty; value = string.Empty;

            var eq = text.IndexOf('=');
            if (eq > 0)
            {
                key = text[..eq].Trim();
                value = text[(eq + 1)..].Trim();
                return !string.IsNullOrWhiteSpace(key);
            }

            var colon = text.IndexOf(':');
            if (colon > 0)
            {
                key = text[..colon].Trim();
                value = text[(colon + 1)..].Trim();
                return !string.IsNullOrWhiteSpace(key);
            }

            return false;
        }

        private static Dictionary<string, string> ParseMetadata(string metaPart)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = metaPart.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pairs)
            {
                var trimmed = p.Trim();
                if (TrySplitKeyValue(trimmed, out var k, out var v))
                    dict[k] = v;
            }
            return dict;
        }

        private static string HelpText()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Subconscious Module Commands:",
                "  sub help",
                "  sub add key=value [meta k1=v1;k2=v2]",
                "  sub recall <key>",
                "  sub query <prefix>",
                "  sub sign <guid>",
                "  sub probe <text>"
            });
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        private void EnsureMemoryOrThrow()
        {
            if (_memoryInst != null) return;
            throw new InvalidOperationException("Memory module is not available. Ensure 'Memory' is loaded before using Subconscious.");
        }
    }
}