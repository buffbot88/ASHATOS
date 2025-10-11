using System.Text;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules;

[RaModule(Category = "core")]
public class DiagnosticsModule : ModuleBase
{
    public override string Name => "Diagnostics";

    public override string Process(string input)
    {
        var cmd = input?.Trim().ToLowerInvariant();
        if (cmd == "start_diag" || cmd == "diagnostics")
            return RunKawaiiDiagnostics((ModuleManager)Manager!);

        if (cmd == "help")
            return "Type 'start_diag' or 'diagnostics' to run kawaii diagnostics! (｡•̀ᴗ-)✧";

        return "Unknown command! Type 'help' for kawaii diagnostics info. (´･ω･`)";
    }

    public string RunKawaiiDiagnostics(ModuleManager manager)
    {
        var sb = new StringBuilder();
        sb.AppendLine("｡ﾟ･:｡★ 𝒦𝒶𝓌𝒶𝒾𝒾 𝑅𝒶𝒞𝑜𝓇𝑒 𝒟𝒾𝒶𝑔𝓃𝑜𝓈𝓉𝒾𝒸𝓈 ★:･ﾟ｡");
        sb.AppendLine("      (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧  Let's check your modules together!  ✧ﾟ･: *ヽ(◕ヮ◕ヽ)");
        sb.AppendLine();

        // 1. Loaded modules, with details
        sb.AppendLine("🌸 Loaded Modules:");
        foreach (var mw in manager.Modules)
        {
            var inst = mw.Instance;
            var type = inst.GetType();
            var interfaces = type.GetInterfaces().Select(i => i.Name).ToArray();
            sb.AppendLine($"  • {inst.Name} [{type.Name}]");
            sb.AppendLine($"    Category: {mw.Category} | Interfaces: {string.Join(", ", interfaces)}");
        }
        sb.AppendLine($"  (｡•̀ᴗ-)✧ Total loaded modules: {manager.Modules.Count}");

        // 2. Cute category summary
        var categoryGroups = manager.Modules.GroupBy(m => m.Category ?? "(none)").OrderBy(g => g.Key);
        sb.AppendLine("\n🧸 Modules by Category:");
        foreach (var cat in categoryGroups)
        {
            sb.AppendLine($"  🌈 {cat.Key}: {cat.Count()}");
        }

        // 3. Interface implementations
        var allInterfaces = manager.Modules.SelectMany(m => m.Instance.GetType().GetInterfaces()).Distinct();
        sb.AppendLine("\n🍡 Interface Implementations:");
        foreach (var iface in allInterfaces)
        {
            var modulesWithIface = manager.Modules.Where(m => iface.IsAssignableFrom(m.Instance.GetType())).ToList();
            sb.AppendLine($"  • {iface.Name}: {modulesWithIface.Count} modules");
        }

        // 4. IMemory special check
        var imemoryType = allInterfaces.FirstOrDefault(i => i.Name == "IMemory");
        if (imemoryType != null)
        {
            var imemoryModules = manager.Modules.Where(m => imemoryType.IsAssignableFrom(m.Instance.GetType())).ToList();
            sb.AppendLine("\n💾 IMemory Modules:");
            foreach (var m in imemoryModules)
            {
                sb.AppendLine($"  • {m.Instance.Name} ({m.Instance.GetType().Name})");
            }
        }
        else
        {
            sb.AppendLine("\n(´･ω･`) No IMemory interface found in loaded modules... Sad panda!");
        }

        // 5. MemoryModule by name/type
        var memoryByName = manager.GetModuleInstanceByName("Memory");
        sb.AppendLine($"\n🧠 MemoryModule found by name: {(memoryByName != null ? memoryByName.GetType().Name + " (yay!)" : "NOPE (｡•́︿•̀｡)")}");

        // 6. Module load errors
        var loadResult = manager.LoadModules();
        sb.AppendLine("\n💔 Module Load Errors:");
        if (loadResult.Errors.Count == 0)
            sb.AppendLine("  (⁀ᗢ⁀)✧ None! Everything is sparkling!");
        else
            foreach (var err in loadResult.Errors)
                sb.AppendLine($"  • {err}");

        // 7. Cute summary
        sb.AppendLine("\n｡ﾟ･:｡★ 𝒟𝒊𝒂𝒈𝒏𝑜𝓈𝓉𝒾𝒸𝓈 𝒞𝑜𝓂𝓅𝓁𝑒𝓉𝑒! ★:･ﾟ｡");
        sb.AppendLine("  (≧◡≦) ♡ If you see missing modules, sad faces, or errors above, please give them some love!");
        sb.AppendLine("  (づ｡◕‿‿◕｡)づ  You can add, rebuild, or fix them—I'm cheering for you!");
        sb.AppendLine();
        sb.AppendLine("  ✿ Tip: Make sure your [RaModule] categories are set, and your interfaces match!");
        sb.AppendLine("  ✿ If you need more help, ask your friendly kawaii diagnostics bot again!");
        sb.AppendLine("｡ﾟ･:｡★ 𝒦𝒶𝓌𝒶𝒾𝒾 𝒟𝒾𝒶𝑔 𝑒𝓃𝒹 ★:･ﾟ｡ (´｡• ᵕ •｡`) ♡");
        return sb.ToString();
    }
}