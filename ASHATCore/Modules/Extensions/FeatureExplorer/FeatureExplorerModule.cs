using System.Diagnostics;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Conscious;

namespace ASHATCore.Modules.Extensions.FeatureExplorer;

[RaModule(Category = "extensions")]
public sealed class FeatureExplorerModule : ModuleBase, IDisposable
{
    public override string Name => "FeatureExplorer";

    private ModuleManager? _manager;
    private ThoughtProcessor? _thoughtProcessor;

    // Add this static field to cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions CachedJsonOptions = new() { WriteIndented = true };
    private static readonly string[] value =
        [
            "FeatureExplorer commands:",
            "  features        - list detected modules and their key commands",
            "  features full   - include per-module help and status/ stats",
            "  features json   - JSON output (full detail)",
            "  features help   - show this help"
        ];

    // Change the method signature to match the base class/interface
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _thoughtProcessor = _manager != null ? new ThoughtProcessor(_manager) : null;
        LogInfo("FeatureExplorer ready. Commands: features | features full | features json | features help");
    }

    public override string Process(string input)
    {
        return ProcessAsync(input).GetAwaiter().GetResult().Text;
    }

    public async Task<ModuleResponse> ProcessAsync(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (_thoughtProcessor == null)
            return new ModuleResponse { Text = "(ThoughtProcessor unavailable)", Type = "error", Status = "error" };

        if (string.IsNullOrWhiteSpace(text) || string.Equals(text, "features", StringComparison.OrdinalIgnoreCase))
            return await AgenticReport(_: false, json: false);

        if (text.Equals("features full", StringComparison.OrdinalIgnoreCase))
            return await AgenticReport(_: true, json: false);

        if (text.Equals("features json", StringComparison.OrdinalIgnoreCase))
            return await AgenticReport(_: true, json: true);

        if (text.Equals("features help", StringComparison.OrdinalIgnoreCase) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            return await _thoughtProcessor.ProcessThoughtAsync(Help(), null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral);

        return await _thoughtProcessor.ProcessThoughtAsync("FeatureExplorer: unknown command. Try: features | features full | features json | features help", null, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused);
    }

    private async Task<ModuleResponse> AgenticReport(bool _, bool json)
    {
        var sw = Stopwatch.StartNew();
        var now = DateTime.UtcNow;

        var features = new FeatureReport
        {
            GeneratedAtUtc = now,
            HostInfo = new HostInfo
            {
                Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription
            }
        };

        if (_manager == null)
        {
            features.Notes.Add("ModuleManager is not available.");
            if (_thoughtProcessor != null)
            {
                return await _thoughtProcessor.ProcessThoughtAsync(
                    Format(features, json, sw.ElapsedMilliseconds), null, null,
                    new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Confused
                );
            }
            else
            {
                return new ModuleResponse
                {
                    Text = Format(features, json, sw.ElapsedMilliseconds),
                    Type = "error",
                    Status = "error"
                };
            }
        }

        foreach (var w in _manager.Modules)
        {
            var inst = w.Instance;
            if (inst == null) continue;

            var mi = new ModuleInfo
            {
                Name = inst.Name,
                Type = inst.GetType().FullName ?? inst.GetType().Name,
                Status = GetStatusForModule(inst.Name),
                Help = GetHelpForModule(inst.Name)
            };

            if (IsMemory(inst.Name))
                mi.Stats = SafeCall(inst.Name, "stats");

            mi.Commands = ExtractCommands(mi.Help);

            features.Modules.Add(mi);
        }

        features.OtherComponents = DetectOtherComponents();
        sw.Stop();
        features.GeneratedInMs = sw.ElapsedMilliseconds;

        var monitoringSummary = $"Feature report Generated at {features.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss} UTC. Modules detected: {features.Modules.Count}.";
        var details = Format(features, json, sw.ElapsedMilliseconds);

        if (_thoughtProcessor != null)
        {
            return await _thoughtProcessor.ProcessThoughtAsync(
                $"{monitoringSummary}\n\n{details}", null, null,
                new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral
            );
        }
        else
        {
            return new ModuleResponse
            {
                Text = $"{monitoringSummary}\n\n{details}",
                Type = "info",
                Status = "ok"
            };
        }
    }

    private string? GetStatusForModule(string name)
    {
        try
        {
            if (IsMemory(name)) return SafeCall(name, "stats");
            if (IsSubconscious(name)) return SafeCall(name, "sub status") ?? SafeCall(name, "status");
            return SafeCall(name, "status");
        }
        catch { return null; }
    }

    private string? GetHelpForModule(string name)
    {
        try
        {
            if (name.Equals("Planner", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("NLU", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Nlu", StringComparison.OrdinalIgnoreCase))
            {
                return $"{name}: status | help | accepts JSON or natural language (see docs)";
            }
            var help = SafeCall(name, "help");
            if (string.IsNullOrWhiteSpace(help))
                help = SafeCall(name, $"{name.ToLowerInvariant()} help");
            return help;
        }
        catch { return null; }
    }

    private static bool IsMemory(string name) =>
        name.Equals("Memory", StringComparison.OrdinalIgnoreCase) || name.Equals("MemoryModule", StringComparison.OrdinalIgnoreCase);

    private static bool IsSubconscious(string name) =>
        name.Equals("Subconscious", StringComparison.OrdinalIgnoreCase) || name.Equals("SubconsciousModule", StringComparison.OrdinalIgnoreCase);

    private string? SafeCall(string preferredModuleName, string input)
    {
        if (_manager == null) return null;
        var alternates = new[] { preferredModuleName, preferredModuleName + "Module" };

        try
        {
            return _manager.InvokeModuleProcessByNameFallback(alternates, input)
                ?? _manager.SafeInvokeModuleByName(preferredModuleName, input)
                ?? _manager.SafeInvokeModuleByName(preferredModuleName + "Module", input);
        }
        catch { return null; }
    }

    private static List<string> ExtractCommands(string? helpText)
    {
        var commands = new List<string>();
        if (string.IsNullOrWhiteSpace(helpText)) return commands;

        foreach (var line in helpText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("- ")) trimmed = trimmed[2..].TrimStart();
            if (trimmed.Length == 0 || !char.IsLetterOrDigit(trimmed[0])) continue;

            var idx = trimmed.IndexOf("  ", StringComparison.Ordinal);
            var cmd = idx > 0 ? trimmed[..idx].Trim() : trimmed;
            if (cmd.Contains("help", StringComparison.OrdinalIgnoreCase) && trimmed.IndexOf("help", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (cmd.Length > 80) cmd = cmd[..80] + "...";
            if (!commands.Contains(cmd, StringComparer.OrdinalIgnoreCase))
                commands.Add(cmd);
        }
        return commands;
    }

    private static List<string> DetectOtherComponents()
    {
        var list = new List<string>();
        TryDetect("ASHATCore.Modules.DigitalFace.DigitalFaceControl", list, "DigitalFaceControl (UI)");
        TryDetect("ASHATCore.Modules.DigitalFace.Rendering.GdiParticleRenderer", list, "GdiParticleRenderer");
        TryDetect("ASHATCore.Modules.DigitalFace.Rendering.FaceInputs", list, "FaceInputs");
        return list;
    }

    private static void TryDetect(string fullTypeName, List<string> list, string label)
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullTypeName, throwOnError: false, ignoreCase: false);
                if (t != null) { list.Add(label); return; }
            }
        }
        catch { }
    }

    private static string Format(FeatureReport report, bool asJson, long _)
    {
        if (asJson)
        {
            return JsonSerializer.Serialize(report, CachedJsonOptions);
        }

        var lines = new List<string>
        {
            $"Features report Generated at {report.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss} UTC in {report.GeneratedInMs} ms",
            $"Host: {report.HostInfo.Framework} on {report.HostInfo.OS}",
            "",
            "Modules:"
        };

        foreach (var m in report.Modules.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
        {
            lines.Add($"- {m.Name} ({m.Type})");
            if (!string.IsNullOrWhiteSpace(m.Status))
                lines.Add($"  status: {OneLine(m.Status)}");
            if (!string.IsNullOrWhiteSpace(m.Stats))
                lines.Add($"  stats: {OneLine(m.Stats)}");
            if (m.Commands.Count > 0)
                lines.Add($"  commands: {string.Join(" | ", m.Commands)}");
            if (!string.IsNullOrWhiteSpace(m.Help))
            {
                lines.Add("  help:");
                foreach (var hl in m.Help.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
                    lines.Add("    " + hl);
            }
        }

        if (report.OtherComponents?.Count > 0)
        {
            lines.Add("");
            lines.Add("Other components detected:");
            foreach (var oc in report.OtherComponents)
                lines.Add($"- {oc}");
        }

        if (report.Notes.Count > 0)
        {
            lines.Add("");
            lines.Add("Notes:");
            foreach (var n in report.Notes) lines.Add($"- {n}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string OneLine(string s)
    {
        var one = s.Replace("\r", " ").Replace("\n", " ").Trim();
        return one.Length > 200 ? one[..200] + "..." : one;
    }

    private static string Help() => string.Join(Environment.NewLine, value);

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // DTOs
    private sealed class FeatureReport
    {
        public DateTime GeneratedAtUtc { get; set; }
        public long GeneratedInMs { get; set; }
        public HostInfo HostInfo { get; set; } = new();
        public List<ModuleInfo> Modules { get; set; } = [];
        public List<string> OtherComponents { get; set; } = [];
        public List<string> Notes { get; set; } = [];
    }

    private sealed class HostInfo
    {
        public string Framework { get; set; } = "";
        public string OS { get; set; } = "";
    }

    private sealed class ModuleInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Help { get; set; }
        public string? Status { get; set; }
        public string? Stats { get; set; }
        public List<string> Commands { get; set; } = [];
    }
}
