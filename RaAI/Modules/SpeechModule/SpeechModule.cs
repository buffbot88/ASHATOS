using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RaAI.Handlers.Manager;
using RaAI.Modules.MemoryModule;
using RaAI.Modules.SubconsciousModule;

namespace RaAI.Modules.SpeechModule
{
    // If you use attribute-based discovery, you can enable this:
    // [RaModule("Speech")]
    public class SpeechModule : ModuleBase, ISpeechModule
    {
        public override string Name => "Speech";

        // Resolved in Initialize(ModuleManager)
        private ModuleManager? _manager;
        private object? _memoryInst;        // IMemory or MemoryModule (or compatible)
        private ISubconscious? _sub;        // optional

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

            // Resolve memory by name or type
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

        // -------------- Primary async entry --------------

        public async Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "(no input)";

            var text = input.Trim();

            // remember: "X is Y" or "remember X is Y" or "remember X=Y"
            var match = Regex.Match(text, @"^(?:remember\s+)?(.+?)\s*(?:is|=)\s*(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var key = NormalizeKey(match.Groups[1].Value);
                var value = match.Groups[2].Value.Trim();
                return RememberCompat(key, value);
            }

            // recall: "recall key"
            match = Regex.Match(text, @"^(?:recall|get)\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var key = NormalizeKey(match.Groups[1].Value.Trim());
                var val = RecallCompat(key);
                return val ?? $"No value found for key: {key}";
            }

            // think: "think about X" or "think X" or "ponder X"
            match = Regex.Match(text, @"^(?:think(?: about)?|ponder)\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                return await ThinkAsync(content, cancellationToken);
            }

            // probe subconscious: "ask subconscious X", "probe X", "ask X"
            match = Regex.Match(text, @"^(?:ask\s+subconscious|probe|ask)\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                return await ProbeSubconsciousAsync(content, cancellationToken);
            }

            // status/help
            if (Regex.IsMatch(text, @"^\s*(status|help)\s*$", RegexOptions.IgnoreCase))
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
                // Reuse the async pipeline synchronously for ModuleManager.SafeInvokeModuleByName
                return GenerateResponseAsync(input, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogError($"Process error: {ex.Message}");
                return $"(error) {ex.Message}";
            }
        }

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
            // Prefer delegating to Conscious if available via ModuleManager
            if (_manager != null)
            {
                // Try both names
                var res = _manager.SafeInvokeModuleByName("Conscious", $"think {content}")
                       ?? _manager.SafeInvokeModuleByName("ConsciousModule", $"think {content}");
                if (!string.IsNullOrWhiteSpace(res))
                    return res;
            }

            // Fallback simple response
            await Task.Yield();
            return $"Thought about: {content}";
        }

        private async Task<string> ProbeSubconsciousAsync(string content, CancellationToken ct)
        {
            try
            {
                if (_sub != null)
                {
                    var response = await _sub.Probe(content, ct);
                    return $"Subconscious response: {response}";
                }
            }
            catch (Exception ex)
            {
                LogError($"Subconscious probe failed: {ex.Message}");
                return $"(probe error) {ex.Message}";
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
                - status                    Show availability
                - help                      This help text".Trim();
            }

            var hasMem = _memoryInst != null;
            var hasSub = _sub != null;
            var hasCon = _manager?.GetModuleInstanceByName("Conscious") != null
                      || _manager?.GetModuleInstanceByName("ConsciousModule") != null;

            return $"Speech status: memory={(hasMem ? "yes" : "no")}, subconscious={(hasSub ? "yes" : "no")}, conscious={(hasCon ? "yes" : "no")}";
        }

        private static string NormalizeKey(string s) =>
            string.IsNullOrWhiteSpace(s) ? s : s.Trim().Replace(" ", "");

        public string GenerateResponse(string input)
        {
            return GenerateResponseAsync(input, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}