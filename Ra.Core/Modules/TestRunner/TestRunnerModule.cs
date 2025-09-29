using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using Ra.Core.Engine.Manager;

namespace Ra.Core.Modules.TestRunner;

public sealed class TestRunnerModule : ModuleBase, IDisposable
{
    public override string Name => "TestRunner";

    private ModuleManager? _manager;

    public override void Initialize(ModuleManager manager)
    {
        base.Initialize(manager);
        _manager = manager;
        LogInfo("TestRunner initialized. Commands: start | start fast | start json | start seed <n> | start verify | clean | status | help");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();
        if (string.Equals(text, "help", StringComparison.OrdinalIgnoreCase))
            return Help();

        if (text.StartsWith("start", StringComparison.OrdinalIgnoreCase))
        {
            var fast = text.IndexOf("fast", StringComparison.OrdinalIgnoreCase) >= 0;
            var asJson = text.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0;
            var verify = text.IndexOf("verify", StringComparison.OrdinalIgnoreCase) >= 0;
            int? seed = TryParseSeed(text);

            return RunSuite(fast, asJson, seed, verify);
        }

        if (string.Equals(text, "clean", StringComparison.OrdinalIgnoreCase))
            return CleanArtifacts();

        if (string.Equals(text, "status", StringComparison.OrdinalIgnoreCase))
            return ListModules();

        return "TestRunner: unknown command. Try: start | start fast | start json | start seed <n> | start verify | clean | status | help";
    }

    private string RunSuite(bool fast, bool asJson, int? seed, bool verifyOrder)
    {
        var sw = Stopwatch.StartNew();
        var results = new List<(string Label, bool Ok, string? Detail)>();
        void Add(string label, bool ok, string? detail = null) => results.Add((label, ok, detail));

        if (_manager == null)
        {
            Add("ModuleManager reference available", false, "manager is null");
            return Format(results, 0, asJson);
        }

        // Optional: seed Conscious RNG for deterministic associations
        if (seed.HasValue)
        {
            var seeded = TrySeedConscious(seed.Value, out var detail);
            Add("Conscious RNG seed", seeded, detail);
        }

        if (!fast) Thread.Sleep(300);

        // Presence checks
        var hasMem = ModulePresent("Memory") || ModulePresent("MemoryModule");
        var hasSub = ModulePresent("Subconscious") || ModulePresent("SubconsciousModule");
        var hasCon = ModulePresent("Conscious") || ModulePresent("ConsciousModule");
        var hasSpc = ModulePresent("Speech") || ModulePresent("SpeechModule");
        var hasDfx = ModulePresent("DigitalFace") || ModulePresent("DigitalFaceControl") || HasType("Ra.Core.Modules.DigitalFace.DigitalFaceControl");

        Add("Memory present", hasMem);
        Add("Subconscious present", hasSub);
        Add("Conscious present", hasCon);
        Add("Speech present", hasSpc);
        Add("DigitalFace present (optional)", hasDfx);

        // Memory basic ops
        var memStats = Call("Memory", "stats");
        Add("Memory stats", !string.IsNullOrWhiteSpace(memStats), memStats);

        var rememberRes = Call("Memory", "remember testrunner=ok");
        var recallRes = Call("Memory", "recall testrunner");
        Add("Memory remember/recall", (recallRes ?? "").IndexOf("ok", StringComparison.OrdinalIgnoreCase) >= 0, recallRes);

        // Wake heartbeats
        if (!fast) Thread.Sleep(200);
        var subWake = Call("Memory", "recall Autonomous/boot/subconscious");
        var conWake = Call("Memory", "recall boot/conscious/heartbeat");
        var spcWake = Call("Memory", "recall boot/speech/heartbeat");

        Add("Subconscious wake heartbeat", !string.IsNullOrWhiteSpace(subWake), subWake);
        Add("Conscious wake heartbeat", !string.IsNullOrWhiteSpace(conWake), conWake);
        Add("Speech wake heartbeat", !string.IsNullOrWhiteSpace(spcWake), spcWake);

        if (verifyOrder)
        {
            var ok = TryVerifyWakeOrder(subWake, conWake, spcWake, out var detail);
            Add("Ordered wake timestamps", ok, detail);
        }

        // Subconscious flows
        var subAdd = Call("Subconscious", "sub add foo=bar meta sig=1");
        var subQuery = Call("Subconscious", "sub query foo");
        Add("Subconscious add", !string.IsNullOrWhiteSpace(subAdd), subAdd);
        Add("Subconscious query", !string.IsNullOrWhiteSpace(subQuery) && subQuery!.IndexOf("foo", StringComparison.OrdinalIgnoreCase) >= 0, subQuery);

        // Conscious think
        var conThink = Call("Conscious", "think Testing ordered wake sequence from TestRunner");
        Add("Conscious think", !string.IsNullOrWhiteSpace(conThink), Trim(conThink, 120));

        // Speech remember/recall + think delegation
        var spRemember = Call("Speech", "remember runnerKey=runnerVal");
        var spRecall = Call("Speech", "recall runnerKey");
        Add("Speech remember", !string.IsNullOrWhiteSpace(spRemember), spRemember);
        Add("Speech recall", !string.IsNullOrWhiteSpace(spRecall) && (spRecall ?? "").IndexOf("runnerVal", StringComparison.OrdinalIgnoreCase) >= 0, spRecall);

        var spThink = Call("Speech", "think about integration flow");
        Add("Speech think (delegates to Conscious if present)", !string.IsNullOrWhiteSpace(spThink), Trim(spThink, 120));

        // Broadcast warmup
        _manager.RaiseSystemEvent("Warmup");
        Add("Broadcast Warmup", true);

        // DigitalFace OnWake (optional)
        bool faceWoke = false;
        try
        {
            var dfxInst = _manager.GetModuleInstanceByName("DigitalFace")
                       ?? _manager.GetModuleInstanceByName("DigitalFaceControl");
            if (dfxInst != null)
            {
                var mi = dfxInst.GetType().GetMethod("OnWake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                mi?.Invoke(dfxInst, null);
                faceWoke = mi != null;
            }
        }
        catch { }
        Add("DigitalFace OnWake (optional)", faceWoke || !hasDfx, faceWoke ? "invoked" : hasDfx ? "not found" : "not present");

        sw.Stop();
        return Format(results, sw.ElapsedMilliseconds, asJson);
    }

    private static int? TryParseSeed(string text)
    {
        var m = Regex.Match(text, @"\bseed\s+(\d+)\b", RegexOptions.IgnoreCase);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var s)) return s;
        return null;
    }

    private bool TrySeedConscious(int seed, out string detail)
    {
        detail = "no conscious processor found";
        try
        {
            var con = _manager?.GetModuleInstanceByName("Conscious")
                   ?? _manager?.GetModuleInstanceByName("ConsciousModule");
            if (con == null) return false;

            // Try public field _processor or property Processor
            var t = con.GetType();
            var f = t.GetField("_processor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var p = t.GetProperty("Processor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var proc = f?.GetValue(con) ?? p?.GetValue(con);
            if (proc == null) return false;

            var mi = proc.GetType().GetMethod("SetRandomSeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null) { detail = "SetRandomSeed not available"; return false; }

            mi.Invoke(proc, new object[] { seed });
            detail = $"seed={seed}";
            return true;
        }
        catch (Exception ex)
        {
            detail = ex.Message;
            return false;
        }
    }

    private static bool TryVerifyWakeOrder(string? subWake, string? conWake, string? spcWake, out string detail)
    {
        detail = "";
        try
        {
            var tsSub = ParseAwakeTimestamp(subWake);
            var tsCon = ParseAwakeTimestamp(conWake);
            var tsSpc = ParseAwakeTimestamp(spcWake);

            if (tsSub == null || tsCon == null || tsSpc == null)
            {
                detail = $"timestamps missing: sub={tsSub?.ToString("O") ?? "-"}, con={tsCon?.ToString("O") ?? "-"}, spc={tsSpc?.ToString("O") ?? "-"}";
                return false;
            }

            var ok = tsSub <= tsCon && tsCon <= tsSpc;
            detail = $"sub={tsSub:O} <= con={tsCon:O} <= spc={tsSpc:O}";
            return ok;
        }
        catch (Exception ex)
        {
            detail = ex.Message;
            return false;
        }
    }

    private static DateTime? ParseAwakeTimestamp(string? recallLine)
    {
        if (string.IsNullOrWhiteSpace(recallLine)) return null;
        // expected: "... => awake:yyyy-MM-dd HH:mm:ss UTC"
        var m = Regex.Match(recallLine, @"awake:(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})", RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        var ts = m.Groups[1].Value;
        if (DateTime.TryParseExact(ts, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            return dt;
        return null;
    }

    private string Format(List<(string Label, bool Ok, string? Detail)> results, long elapsedMs, bool asJson)
    {
        if (asJson)
        {
            var obj = new
            {
                elapsedMs,
                summary = new
                {
                    pass = results.Count(r => r.Ok),
                    fail = results.Count(r => !r.Ok),
                    total = results.Count
                },
                results = results.Select(r => new { label = r.Label, ok = r.Ok, detail = r.Detail })
            };
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        var lines = new List<string> { $"TestRunner complete in {elapsedMs} ms" };
        foreach (var (Label, Ok, Detail) in results)
            lines.Add($"{(Ok ? "[PASS]" : "[FAIL]")} {Label}{(string.IsNullOrWhiteSpace(Detail) ? "" : " - " + Detail)}");
        return string.Join(Environment.NewLine, lines);
    }

    private string CleanArtifacts()
    {
        if (_manager == null) return "Manager unavailable.";

        int removed = 0;

        // Remove exact keys we created
        TryCall("Memory", "remove key testrunner", ref removed);
        TryCall("Memory", "remove key runnerKey", ref removed);

        // Remove Autonomous/foo entries by querying then removing by id
        var q = Call("Memory", "query Autonomous/foo");
        if (!string.IsNullOrWhiteSpace(q))
        {
            var lines = q.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // Each line format: "yyyy-mm-dd ... | {id} | {key} = {value}"
                var parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    var idStr = parts[1].Trim();
                    if (Guid.TryParse(idStr, out var id))
                    {
                        var res = Call("Memory", $"remove id {id}");
                        if (!string.IsNullOrWhiteSpace(res)) removed++;
                    }
                }
            }
        }

        return $"Clean complete. Removed ~{removed} items.";
    }

    private void TryCall(string module, string cmd, ref int count)
    {
        try { var _ = Call(module, cmd); count++; } catch { }
    }

    private string? Call(string preferredModuleName, string input)
    {
        if (_manager == null) return null;
        var alternates = new[] { preferredModuleName, preferredModuleName + "Module" };
        return _manager.InvokeModuleProcessByNameFallback(alternates, input)
            ?? _manager.SafeInvokeModuleByName(preferredModuleName, input)
            ?? _manager.SafeInvokeModuleByName(preferredModuleName + "Module", input);
    }

    private bool ModulePresent(string name)
    {
        if (_manager == null) return false;
        return _manager.GetModuleInstanceByName(name) != null;
    }

    private static bool HasType(string fullName)
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (t != null) return true;
                }
                catch { }
            }
        }
        catch { }
        return false;
    }

    private string ListModules()
    {
        if (_manager == null) return "Manager unavailable.";
        var list = _manager.Modules
                           .Select(w => $"{w.Instance?.Name} ({w.Instance?.GetType().Name})")
                           .ToList();
        if (list.Count == 0) return "No modules loaded.";
        return "Loaded modules:\n- " + string.Join("\n- ", list);
    }

    private static string Trim(string? s, int n)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= n ? s : s.Substring(0, n) + "...";
    }

    private static string Help()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "TestRunner commands:",
            "  start                - run the full integration test suite",
            "  start fast           - run with minimal delays",
            "  start json           - run suite and output JSON",
            "  start seed <number>  - seed Conscious RNG for deterministic output",
            "  start verify         - verify ordered wake timestamps",
            "  clean                - remove test artifacts from Memory",
            "  status               - list loaded modules",
            "  help                 - show this help"
        });
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
