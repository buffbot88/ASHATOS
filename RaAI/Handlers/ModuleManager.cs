using RaAI.Handlers;
using RaAI.Modules.SubconsciousModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RaAI.Handlers
{
    public class ModuleManager : IDisposable
    { 
        // helper DTO (put anywhere public in RaAI.Handlers)
        public class ModuleSetting 
        { 
            public string Name { get; set; } = string.Empty; 
            public bool Enabled { get; set; } 
            public int TimeoutMs { get; set; } 
        }

        private readonly List<ModuleWrapper> modules = [];

        // Public read-only snapshot
        public IReadOnlyList<ModuleWrapper> Modules => modules.AsReadOnly();

        // Configuration: what namespace to treat as "in-project" modules (drop your .cs files there)
        public string ModuleNamespacePrefix { get; set; } = "RaAI.Modules";

        // Discover, instantiate and initialize modules from currently-loaded assemblies
        // Returns results with loaded wrappers and any errors encountered
        public ModuleLoadResult LoadModules(bool requireAttributeOrNamespace = true)
        {
            var result = new ModuleLoadResult();

            // Gather candidate types from all loaded assemblies
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

            // Filter candidate types to module types
            var moduleTypes = candidateTypes
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.IsPublic &&
                    typeof(IRaModule).IsAssignableFrom(t) &&
                    t.GetConstructor(Type.EmptyTypes) != null &&
                    (
                        // either marked explicitly with [RaModule]
                        t.GetCustomAttribute<RaModuleAttribute>() != null
                        // or in the modules namespace (allow drop-in .cs files in that namespace)
                        || (!requireAttributeOrNamespace ? true : (t.Namespace != null && t.Namespace.StartsWith(ModuleNamespacePrefix)))
                    )
                )
                .Distinct()
                .ToList();

            foreach (var t in moduleTypes)
            {
                try
                {
                    // instantiate
                    var inst = (IRaModule)Activator.CreateInstance(t)!;
                    var wrapper = new ModuleWrapper(inst);
                    // initialize (passes manager for compatibility)
                    wrapper.Initialize(this);
                    modules.Add(wrapper);
                    result.Loaded.Add(new ModuleWrapperView(wrapper));
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to instantiate/initialize module {t.FullName}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            return result;
        }

        // Dispose/Remove all loaded modules and clear list
        public void UnloadAllModules()
        {
            foreach (var w in modules.ToList())
            {
                try
                {
                    w.Dispose();
                }
                catch { }
            }
            modules.Clear();
        }

        // Convenience: reload everything (unload then load)
        public ModuleLoadResult ReloadModules(bool requireAttributeOrNamespace = true)
        {
            UnloadAllModules();
            return LoadModules(requireAttributeOrNamespace);
        }

        // Lookups

        public IRaModule? GetModuleByName(string name)
        {
            return Modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public ModuleWrapper? GetWrapperByName(string name)
        {
            return modules.FirstOrDefault(w => string.Equals(w.Instance.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // --- NEW: Find underlying instance by name (best-effort) ---
        public IRaModule? GetModuleInstanceByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            // exact match on declared Name
            var inst = modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (inst != null) return inst;
            // match on type name
            return modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // Safe invoke: attempt to invoke a module's Process(string) (or wrapper-level Process/Invoke) and return string
        // Returns null if module not found or no meaningful response
        public string? SafeInvokeModuleByName(string name, string input)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            // find wrapper first
            ModuleWrapper? wrapper = modules.FirstOrDefault(w =>
                string.Equals(w.Instance?.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.GetType().Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.Instance?.GetType().Name, name, StringComparison.OrdinalIgnoreCase));

            // If wrapper not found, try to find instance by type name or declared Name
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

                // Preferred: a Process(string) method on the module instance
                var proc = t.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (proc != null)
                {
                    var outObj = proc.Invoke(targetInstance, new object[] { input });
                    if (outObj is string s && !string.IsNullOrEmpty(s)) return s;
                    if (outObj != null) return outObj.ToString();
                    return null;
                }

                // Next: wrapper-level methods (if we located wrapper)
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

                // Last resort: try any parameterless ToString-like response or property named "LastResponse" (best-effort)
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

        // Try candidates in-order and return the first non-empty response (null if none responded)
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
                catch { /* swallow and try next */ }
            }
            return null;
        }

        // Overload: accept tuple form (backwards-compatible)
        public void SaveModuleSettings(IEnumerable<(string Name, bool Enabled, int TimeoutMs)> settings)
        {
            if (settings == null) return;
            var mapped = settings.Select(t => new ModuleSetting { Name = t.Name, Enabled = t.Enabled, TimeoutMs = t.TimeoutMs }).ToList();
            SaveModuleSettings(mapped);
        }

        // Main implementation: operate on ModuleSetting DTO
        public void SaveModuleSettings(IEnumerable<ModuleSetting> settings)
        {
            if (settings == null) return;
            var setList = settings.ToList();
            if (setList.Count == 0) return;

            foreach (var w in modules)
            {
                var inst = w.Instance;
                var moduleName = inst?.Name ?? w.GetType().Name;
                var s = setList.FirstOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.OrdinalIgnoreCase));
                if (s == null) continue;

                // If ModuleWrapper type has a strongly-typed Enabled/TimeoutMs property, prefer setting directly
                try
                {
                    // try direct typed access first (if property exists at compile-time)
                    var wrapperType = typeof(ModuleWrapper);
                    var prop = wrapperType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                    {
                        // find the specific wrapper instance in our modules list and set it
                        var target = w;
                        prop.SetValue(target, ConvertToPropertyType(s.Enabled, prop.PropertyType));
                    }
                }
                catch
                {
                    // ignore and fall back to reflection based per-instance below
                }

                // Try to set on ModuleWrapper (if it exposes Enabled/TimeoutMs)
                var wt = w.GetType();
                var wpEnabled = wt.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                if (wpEnabled != null && wpEnabled.CanWrite)
                    SetPropertySafely(w, wpEnabled, s.Enabled);

                var wpTimeout = wt.GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                if (wpTimeout != null && wpTimeout.CanWrite)
                    SetPropertySafely(w, wpTimeout, s.TimeoutMs);

                // Try to set on the underlying IRaModule instance as well (if it exposes the properties)
                if (inst != null)
                {
                    var it = inst.GetType();
                    var ipEnabled = it.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    if (ipEnabled != null && ipEnabled.CanWrite)
                        SetPropertySafely(inst, ipEnabled, s.Enabled);

                    var ipTimeout = it.GetType().GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                    if (ipTimeout != null && ipTimeout.CanWrite)
                        SetPropertySafely(inst, ipTimeout, s.TimeoutMs);
                }
            }
        }

        // Safe setter/conversion helpers
        private static object? ConvertToPropertyType(object? value, Type propertyType)
        {
            if (value == null) return null;

            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            try
            {
                // If already assignable
                if (targetType.IsInstanceOfType(value)) return value;

                // Enums
                if (targetType.IsEnum)
                {
                    if (value is string s) return Enum.Parse(targetType, s, true);
                    return Enum.ToObject(targetType, value);
                }

                // bool conversions
                if (targetType == typeof(bool))
                {
                    if (value is bool b) return b;
                    if (value is int i) return i != 0;
                    if (value is long l) return l != 0L;
                    if (value is string ss && bool.TryParse(ss, out var bb)) return bb;
                    if (value is string ss2 && int.TryParse(ss2, out var bi)) return bi != 0;
                }

                // int conversions
                if (targetType == typeof(int))
                {
                    if (value is int) return value;
                    if (value is bool bb) return bb ? 1 : 0;
                    if (value is long ll) return (int)ll;
                    if (value is string ss && int.TryParse(ss, out var ii)) return ii;
                    return Convert.ChangeType(value, targetType);
                }

                // long conversions
                if (targetType == typeof(long))
                {
                    if (value is long) return value;
                    if (value is int ii) return (long)ii;
                    if (value is bool bb) return bb ? 1L : 0L;
                    if (value is string ss && long.TryParse(ss, out var ll)) return ll;
                    return Convert.ChangeType(value, targetType);
                }

                // primitive/string/decimal fallback
                if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
                    return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // ignore conversion errors - will return null below
            }

            return null;
        }

        private static void SetPropertySafely(object target, PropertyInfo prop, object? value)
        {
            if (prop == null) return;

            var converted = ConvertToPropertyType(value, prop.PropertyType);

            if (converted == null)
            {
                // If convert failed but original is assignable, use it
                if (value != null && prop.PropertyType.IsInstanceOfType(value))
                    converted = value;
                else
                {
                    // Skip assignment when conversion not possible (avoid ArgumentException)
                    Debug.WriteLine($"Skipping assignment: {target.GetType().FullName}.{prop.Name} expects {prop.PropertyType.FullName} but provided value type {value?.GetType().FullName ?? "null"}");
                    return;
                }
            }

            // Final guard for non-nullable value types
            if (converted == null && prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
            {
                Debug.WriteLine($"Cannot assign null to non-nullable {prop.PropertyType.FullName} for {target.GetType().FullName}.{prop.Name}");
                return;
            }

            prop.SetValue(target, converted);
        }

        // Dispose pattern to clean up modules
        public void Dispose()
        {
            UnloadAllModules();
            GC.SuppressFinalize(this);
        }
    }
}