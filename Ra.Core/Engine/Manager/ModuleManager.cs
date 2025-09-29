using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ra.Core.Engine.Manager;

public class ModuleManager : IDisposable
{
    public class ModuleSetting
    {
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public int TimeoutMs { get; set; }
    }

    private readonly List<ModuleWrapper> modules = new();
    private readonly HashSet<string> loadedAssemblySimpleNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> assemblySearchRoots = new();
    private bool resolverAttached = false;

    public IReadOnlyList<ModuleWrapper> Modules => modules.AsReadOnly();

    public string ModuleNamespacePrefix { get; set; } = "Ra.Core.Modules";

    public IReadOnlyList<string> ModuleSearchPaths => assemblySearchRoots.AsReadOnly();

    public ModuleManager()
    {
        TryAddSearchPath(AppContext.BaseDirectory);
        TryAddSearchPath(Path.Combine(AppContext.BaseDirectory, "Modules"));

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = asm.GetName().Name;
            if (!string.IsNullOrWhiteSpace(name))
                loadedAssemblySimpleNames.Add(name);
        }
    }

    public void AddSearchPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        TryAddSearchPath(path);
    }

    private void TryAddSearchPath(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full) && !assemblySearchRoots.Contains(full, StringComparer.OrdinalIgnoreCase))
            {
                assemblySearchRoots.Add(full);
            }
        }
        catch { }
    }

    public ModuleLoadResult LoadModules(bool requireAttributeOrNamespace = true)
    {
        var result = new ModuleLoadResult();

        LoadExternalModuleAssembliesRecursive();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var candidateTypes = new List<Type>();

        foreach (var asm in assemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException rex)
            {
                result.Errors.Add($"Failed to get types from assembly {asm.FullName}: {rex.Message}");
                foreach (var le in rex.LoaderExceptions ?? Array.Empty<Exception>())
                    result.Errors.Add($" - LoaderException: {le?.GetType().Name}: {le?.Message}");
                continue;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to get types from assembly {asm.FullName}: {ex.GetType().Name}: {ex.Message}");
                continue;
            }
            candidateTypes.AddRange(types);
        }

        var moduleTypes = candidateTypes
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsPublic &&
                typeof(IRaModule).IsAssignableFrom(t) &&
                t.GetConstructor(Type.EmptyTypes) != null &&
                (
                    t.GetCustomAttribute<RaModuleAttribute>() != null
                    || (!requireAttributeOrNamespace ? true : t.Namespace != null && t.Namespace.StartsWith(ModuleNamespacePrefix, StringComparison.Ordinal))
                )
            )
            .Distinct()
            .ToList();

        // Phase 1: instantiate all first
        var newWrappers = new List<ModuleWrapper>();
        foreach (var t in moduleTypes)
        {
            try
            {
                var inst = (IRaModule)Activator.CreateInstance(t)!;
                var wrapper = new ModuleWrapper(inst);
                newWrappers.Add(wrapper);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to instantiate module {t.FullName}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // Add to manager before initializing so cross-lookups work
        modules.AddRange(newWrappers);

        // Phase 2: initialize after all are present
        foreach (var wrapper in newWrappers)
        {
            try
            {
                wrapper.Initialize(this);
                result.Loaded.Add(new ModuleWrapperView(wrapper));
            }
            catch (Exception ex)
            {
                var t = wrapper.Instance?.GetType();
                var name = t?.FullName ?? "(unknown)";
                result.Errors.Add($"Failed to initialize module {name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // Announce system boot so providers (Memory) can orchestrate readiness
        try
        {
            RaiseSystemEvent("SystemBoot");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SystemBoot event error: {ex.Message}");
        }

        return result;
    }

    public void UnloadAllModules()
    {
        foreach (var w in modules.ToList())
        {
            try { w.Dispose(); } catch { }
        }
        modules.Clear();
    }

    public ModuleLoadResult ReloadModules(bool requireAttributeOrNamespace = true)
    {
        UnloadAllModules();
        return LoadModules(requireAttributeOrNamespace);
    }

    public IRaModule? GetModuleByName(string name)
    {
        return Modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public ModuleWrapper? GetWrapperByName(string name)
    {
        return modules.FirstOrDefault(w => string.Equals(w.Instance.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public IRaModule? GetModuleInstanceByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var inst = modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        if (inst != null) return inst;
        return modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public string? SafeInvokeModuleByName(string name, string input)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        ModuleWrapper? wrapper = modules.FirstOrDefault(w =>
            string.Equals(w.Instance?.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(w.GetType().Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(w.Instance?.GetType().Name, name, StringComparison.OrdinalIgnoreCase));

        object? targetInstance = wrapper?.Instance;
        if (targetInstance == null)
        {
            targetInstance = modules.Select(m => m.Instance).FirstOrDefault(i =>
                string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
            if (targetInstance == null) return null;
        }

        try
        {
            var t = targetInstance.GetType();

            var proc = t.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            if (proc != null)
            {
                var outObj = proc.Invoke(targetInstance, new object[] { input });
                if (outObj is string s && !string.IsNullOrEmpty(s)) return s;
                if (outObj != null) return outObj.ToString();
                return null;
            }

            if (wrapper != null)
            {
                var wtype = wrapper.GetType();
                var wproc = wtype.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null)
                            ?? wtype.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (wproc != null)
                {
                    var outObj = wproc.Invoke(wrapper, new object[] { input });
                    if (outObj is string s2 && !string.IsNullOrEmpty(s2)) return s2;
                    if (outObj != null) return outObj.ToString();
                }
            }

            var lastProp = t.GetProperty("LastResponse", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (lastProp != null)
            {
                var val = lastProp.GetValue(targetInstance);
                if (val is string s3 && !string.IsNullOrEmpty(s3)) return s3;
                if (val != null) return val.ToString();
            }
        }
        catch (TargetInvocationException tie)
        {
            Debug.WriteLine($"Module invocation failed [{name}]: {tie.InnerException?.Message ?? tie.Message}");
            return $"(module {name} invocation error: {tie.InnerException?.Message ?? tie.Message})";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Module invocation exception [{name}]: {ex.Message}");
            return $"(module {name} invocation exception: {ex.Message})";
        }

        return null;
    }

    public string? InvokeModuleProcessByNameFallback(IEnumerable<string> candidateNames, string input)
    {
        if (candidateNames == null) return null;
        foreach (var cname in candidateNames)
        {
            try
            {
                var res = SafeInvokeModuleByName(cname, input);
                if (!string.IsNullOrEmpty(res)) return res;
            }
            catch { }
        }
        return null;
    }

    // -------- System event bus (reflection-based) --------

    public void RaiseSystemEvent(string name, object? payload = null)
    {
        foreach (var w in modules)
        {
            var inst = w.Instance;
            if (inst == null) continue;
            var t = inst.GetType();

            try
            {
                // Typed hook: On<Name>(payloadType) e.g., OnMemoryReady(IMemory)
                if (payload != null)
                {
                    var typedName = "On" + name;
                    var mTyped = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  .FirstOrDefault(m =>
                                  {
                                      if (!string.Equals(m.Name, typedName, StringComparison.Ordinal)) return false;
                                      var ps = m.GetParameters();
                                      return ps.Length == 1 && ps[0].ParameterType.IsInstanceOfType(payload);
                                  });
                    if (mTyped != null)
                    {
                        mTyped.Invoke(inst, new[] { payload });
                        continue;
                    }
                }

                // Special-case zero-arg warmup: OnWarmup()
                if (string.Equals(name, "Warmup", StringComparison.OrdinalIgnoreCase))
                {
                    var onWarmup = t.GetMethod("OnWarmup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                    if (onWarmup != null)
                    {
                        onWarmup.Invoke(inst, null);
                        continue;
                    }
                }

                // Generic hook: OnSystemEvent(string, object?) or (string)
                var onEvt2 = t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object) }, null)

                ?? t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object) }, null);
                if (onEvt2 != null)
                {
                    onEvt2.Invoke(inst, new[] { name, payload! });
                    continue;
                }

                var onEvt1 = t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (onEvt1 != null)
                {
                    onEvt1.Invoke(inst, new object[] { name });
                    continue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"System event '{name}' handler error in {t.FullName}: {ex.Message}");
            }
        }
    }

    private void LoadExternalModuleAssembliesRecursive()
    {
        AttachAssemblyResolverOnce();

        foreach (var root in assemblySearchRoots.ToList())
        {
            if (!Directory.Exists(root)) continue;

            IEnumerable<string> dlls;
            try { dlls = Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories); }
            catch { continue; }

            foreach (var path in dlls)
            {
                var fileName = Path.GetFileName(path);
                var looksLikeModule =
                    path.IndexOf($"{Path.DirectorySeparatorChar}Modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    fileName.Contains("Module", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("Ra.Core.Modules", StringComparison.OrdinalIgnoreCase);

                if (!looksLikeModule) continue;

                try
                {
                    var an = AssemblyName.GetAssemblyName(path);
                    var simple = an.Name ?? "";
                    if (string.IsNullOrWhiteSpace(simple)) continue;
                    if (loadedAssemblySimpleNames.Contains(simple)) continue;

                    Assembly.LoadFrom(path);
                    loadedAssemblySimpleNames.Add(simple);
                }
                catch { }
            }
        }
    }

    private void AttachAssemblyResolverOnce()
    {
        if (resolverAttached) return;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveFromSearchRoots;
        resolverAttached = true;
    }

    private Assembly? ResolveFromSearchRoots(object? sender, ResolveEventArgs args)
    {
        try
        {
            var requested = new AssemblyName(args.Name);
            var targetName = requested.Name + ".dll";
            foreach (var root in assemblySearchRoots)
            {
                if (!Directory.Exists(root)) continue;

                var match = Directory.EnumerateFiles(root, targetName, SearchOption.AllDirectories).FirstOrDefault();
                if (match != null)
                {
                    try { return Assembly.LoadFrom(match); } catch { }
                }
            }
        }
        catch { }
        return null;
    }

    public void Dispose()
    {
        UnloadAllModules();
        GC.SuppressFinalize(this);
    }
}
