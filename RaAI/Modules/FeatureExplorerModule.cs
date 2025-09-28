using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using RaAI.Handlers.Manager;

namespace RaAI.Modules
{
    // [RaModule("FeatureExplorer")]
    public sealed class FeatureExplorerModule : ModuleBase, IDisposable
    {
        public override string Name => "FeatureExplorer";

        private ModuleManager? _manager;

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _manager = manager;
            LogInfo("FeatureExplorer ready. Commands: features | features full | features json | features help");
        }

        public override string Process(string input)
        {
            var text = (input ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text) || string.Equals(text, "features", StringComparison.OrdinalIgnoreCase))
                return GenerateReport(full: false, json: false);

            if (text.Equals("features full", StringComparison.OrdinalIgnoreCase))
                return GenerateReport(full: true, json: false);

            if (text.Equals("features json", StringComparison.OrdinalIgnoreCase))
                return GenerateReport(full: true, json: true);

            if (text.Equals("features help", StringComparison.OrdinalIgnoreCase) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
                return Help();

            return "FeatureExplorer: unknown command. Try: features | features full | features json | features help";
        }

        private string GenerateReport(bool full, bool json)
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
                return Format(features, json, sw.ElapsedMilliseconds);
            }

            foreach (var w in _manager.Modules)
            {
                var inst = w.Instance;
                if (inst == null) continue;

                var mi = new ModuleInfo
                {
                    Name = inst.Name,
                    Type = inst.GetType().FullName ?? inst.GetType().Name
                };

                // Prefer status first (safer than help for some modules like Planner/NLU)
                mi.Status = GetStatusForModule(inst.Name);

                // Get help text only for modules that are known to support it safely
                mi.Help = GetHelpForModule(inst.Name);

                if (IsMemory(inst.Name))
                    mi.Stats = SafeCall(inst.Name, "stats");

                mi.Commands = ExtractCommands(mi.Help);

                features.Modules.Add(mi);
            }

            features.OtherComponents = DetectOtherComponents();

            sw.Stop();
            features.GeneratedInMs = sw.ElapsedMilliseconds;

            return Format(features, json, sw.ElapsedMilliseconds);
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
                // Avoid sending "help" to modules that historically parse input as JSON
                if (name.Equals("Planner", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("NLU", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Nlu", StringComparison.OrdinalIgnoreCase))
                {
                    // Provide a minimal inline help to avoid triggering exceptions
                    return $"{name}: status | help | accepts JSON or natural language (see docs)";
                }

                var help = SafeCall(name, "help");
                if (string.IsNullOrWhiteSpace(help))
                    help = SafeCall(name, $"{name.ToLowerInvariant()} help");
                return help;
            }
            catch
            {
                return null;
            }
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
            catch
            {
                return null;
            }
        }

        private static List<string> ExtractCommands(string? helpText)
        {
            var commands = new List<string>();
            if (string.IsNullOrWhiteSpace(helpText)) return commands;

            foreach (var line in helpText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("- ")) trimmed = trimmed.Substring(2).TrimStart();
                if (trimmed.Length == 0 || !char.IsLetterOrDigit(trimmed[0])) continue;

                var idx = trimmed.IndexOf("  ", StringComparison.Ordinal);
                var cmd = idx > 0 ? trimmed[..idx].Trim() : trimmed;
                if (cmd.IndexOf("help", StringComparison.OrdinalIgnoreCase) >= 0 && trimmed.IndexOf("help", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (cmd.Length > 80) cmd = cmd[..80] + "...";
                if (!commands.Contains(cmd, StringComparer.OrdinalIgnoreCase))
                    commands.Add(cmd);
            }
            return commands;
        }

        private List<string> DetectOtherComponents()
        {
            var list = new List<string>();
            TryDetect("RaAI.Modules.DigitalFace.DigitalFaceControl", list, "DigitalFaceControl (UI)");
            TryDetect("RaAI.Modules.DigitalFace.Rendering.GdiParticleRenderer", list, "GdiParticleRenderer");
            TryDetect("RaAI.Modules.DigitalFace.Rendering.FaceInputs", list, "FaceInputs");
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

        private static string Format(FeatureReport report, bool asJson, long elapsedMs)
        {
            if (asJson)
            {
                return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            }

            var lines = new List<string>
            {
                $"Features report generated at {report.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss} UTC in {report.GeneratedInMs} ms",
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
                    foreach (var hl in m.Help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
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

        private static string Help()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "FeatureExplorer commands:",
                "  features        - list detected modules and their key commands",
                "  features full   - include per-module help and status/ stats",
                "  features json   - JSON output (full detail)",
                "  features help   - show this help"
            });
        }

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
            public List<ModuleInfo> Modules { get; set; } = new();
            public List<string> OtherComponents { get; set; } = new();
            public List<string> Notes { get; set; } = new();
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
            public List<string> Commands { get; set; } = new();
        }
    }
}