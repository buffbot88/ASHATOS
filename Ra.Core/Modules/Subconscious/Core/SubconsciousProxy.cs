using RaAI.Modules.MemoryModule;
using RaAI.Modules.SubconsciousModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SubconsciousModule.Core
{
    internal class SubconsciousProxy : ISubconscious
    {
        private readonly object _impl;
        private readonly Type _implType;

        public SubconsciousProxy()
        {
            _impl = FindAndCreateSubconsciousInstance() ?? throw new InvalidOperationException("No SubconsciousModule implementation found in loaded assemblies.");
            _implType = _impl.GetType();
        }

        private static object? FindAndCreateSubconsciousInstance()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                {
                    if (!t.IsClass) continue;
                    if (!string.Equals(t.Name, "SubconsciousModule", StringComparison.OrdinalIgnoreCase)) continue;
                    var ctor = t.GetConstructor(Type.EmptyTypes);
                    if (ctor == null) continue;
                    try
                    {
                        return Activator.CreateInstance(t);
                    }
                    catch
                    {
                        // ignore and continue
                    }
                }
            }
            return null;
        }

        public async Task<Guid> AddMemoryAsync(string text, Dictionary<string, string>? metadata = null, CancellationToken ct = default)
        {
            var m = FindMethod("AddMemoryAsync");
            var args = BuildArgsForMethod(m, text, metadata, ct);
            var taskObj = m.Invoke(_impl, args);
            var res = await UnwrapTaskResult<Guid>(taskObj).ConfigureAwait(false);
            return res;
        }

        public async Task<string?> GetMemoryAsync(Guid id, CancellationToken ct = default)
        {
            var m = FindMethod("GetMemoryAsync");
            var args = BuildArgsForMethod(m, id, ct);
            var taskObj = m.Invoke(_impl, args);
            var res = await UnwrapTaskResult<string?>(taskObj).ConfigureAwait(false);
            return res;
        }

        public async Task<List<MemoryItem>> QueryByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            var m = FindMethod("QueryByPrefixAsync");
            var args = BuildArgsForMethod(m, prefix, ct);
            var taskObj = m.Invoke(_impl, args);
            var raw = await UnwrapTaskResult<object?>(taskObj).ConfigureAwait(false);
            return MapResultToMemoryItems(raw);
        }

        public async Task<string?> SignMemoryAsync(Guid id, CancellationToken ct = default)
        {
            var m = FindMethod("SignMemoryAsync");
            var args = BuildArgsForMethod(m, id, ct);
            var taskObj = m.Invoke(_impl, args);
            var res = await UnwrapTaskResult<string?>(taskObj).ConfigureAwait(false);
            return res;
        }

        public async Task<string> Probe(string query, CancellationToken cancellationToken)
        {
            var m = FindMethod("Probe");
            var args = BuildArgsForMethod(m, query);
            var taskObj = m.Invoke(_impl, args);
            var res = await UnwrapTaskResult<string>(taskObj).ConfigureAwait(false);
            return res ?? string.Empty;
        }

        private MethodInfo FindMethod(string name)
        {
            var methods = _implType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => string.Equals(mi.Name, name, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (methods.Length == 0)
                throw new MissingMethodException($"{name} not found on {_implType.FullName}");

            // Prefer method that returns Task<T> or Task and has at least one parameter.
            var chosen = methods
                .OrderByDescending(m => m.GetParameters().Length)
                .First();

            return chosen;
        }

        private static object?[] BuildArgsForMethod(MethodInfo method, params object?[] provided)
        {
            var parms = method.GetParameters();
            var args = new object?[parms.Length];

            for (int i = 0; i < parms.Length; i++)
            {
                var pType = parms[i].ParameterType;
                object? chosen = null;

                // Match by type: if a provided item is assignable to parameter, use it.
                foreach (var candidate in provided)
                {
                    if (candidate == null) continue;
                    var candType = candidate.GetType();
                    if (pType.IsAssignableFrom(candType))
                    {
                        chosen = candidate;
                        break;
                    }
                    // special-case CancellationToken since it's a struct
                    if (pType == typeof(CancellationToken) && candidate is CancellationToken ct)
                    {
                        chosen = ct;
                        break;
                    }
                }

                // If nothing matched and parameter is CancellationToken, supply default
                if (chosen == null && pType == typeof(CancellationToken))
                {
                    chosen = CancellationToken.None;
                }

                // If nothing matched but parameter type is nullable reference, pass null
                args[i] = chosen;
            }

            return args;
        }

        private static async Task<T?> UnwrapTaskResult<T>(object? taskObj)
        {
            if (taskObj == null) return default;
            if (taskObj is Task<T> typed) return await typed.ConfigureAwait(false);
            if (taskObj is Task nonGeneric)
            {
                await nonGeneric.ConfigureAwait(false);
                var resProp = taskObj.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
                if (resProp != null)
                {
                    var val = resProp.GetValue(taskObj);
                    try
                    {
                        if (val is T tVal) return tVal;
                        return (T?)Convert.ChangeType(val, typeof(T));
                    }
                    catch
                    {
                        return default;
                    }
                }
                return default;
            }
            // If it is already T (rare), try cast
            if (taskObj is T direct) return direct;
            return default;
        }

        private static List<MemoryItem> MapResultToMemoryItems(object? raw)
        {
            var list = new List<MemoryItem>();
            if (raw == null) return list;

            if (raw is IEnumerable enumerable)
            {
                foreach (var obj in enumerable)
                {
                    if (obj == null) continue;
                    var mi = new MemoryItem();
                    var t = obj.GetType();
                    var idProp = t.GetProperty("Id") ?? t.GetProperty("id");
                    var textProp = t.GetProperty("Text") ?? t.GetProperty("text");
                    var createdAtProp = t.GetProperty("CreatedAt") ?? t.GetProperty("createdAt");
                    var metaProp = t.GetProperty("Metadata") ?? t.GetProperty("metadata");

                    if (idProp != null)
                    {
                        var v = idProp.GetValue(obj);
                        if (v is Guid g) mi.Id = g;
                        else if (Guid.TryParse(Convert.ToString(v), out var parsed)) mi.Id = parsed;
                    }

                    // Fix for CS8601: Possible null reference assignment.
                    // Ensure that mi.Value is never assigned a possible null value by using the null-coalescing operator.

                    if (textProp != null) mi.Value = Convert.ToString(textProp.GetValue(obj)) ?? string.Empty;
                    if (createdAtProp != null)
                    {
                        var cv = createdAtProp.GetValue(obj);
                        if (cv is DateTime dt) mi.CreatedAt = dt;
                        else if (DateTime.TryParse(Convert.ToString(cv), out var parsedDt)) mi.CreatedAt = parsedDt;
                    }

                    if (metaProp != null)
                    {
                        var md = metaProp.GetValue(obj);
                        try
                        {
                            if (md is IDictionary<string, string> dict)
                            {
                                mi.Metadata = new Dictionary<string, string>(dict);
                            }
                            else if (md is IDictionary hashtbl)
                            {
                                var d = new Dictionary<string, string>();
                                foreach (DictionaryEntry de in hashtbl)
                                {
                                    d[Convert.ToString(de.Key)!] = Convert.ToString(de.Value)!;
                                }
                                mi.Metadata = d;
                            }
                            else if (md != null)
                            {
                                var mdType = md.GetType();
                                var props = mdType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                                var d = new Dictionary<string, string>();
                                foreach (var p in props)
                                {
                                    var val = p.GetValue(md);
                                    d[p.Name] = val?.ToString() ?? string.Empty;
                                }
                                mi.Metadata = d;
                            }
                        }
                        catch
                        {
                            mi.Metadata = new Dictionary<string, string>();
                        }
                    }

                    list.Add(mi);
                }
            }
            else
            {
                // single object
                var obj = raw;
                var t = obj.GetType();
                var mi = new MemoryItem();
                var idProp = t.GetProperty("Id") ?? t.GetProperty("id");
                var textProp = t.GetProperty("Text") ?? t.GetProperty("text");
                var createdAtProp = t.GetProperty("CreatedAt") ?? t.GetProperty("createdAt");
                var metaProp = t.GetProperty("Metadata") ?? t.GetProperty("metadata");

                if (idProp != null)
                {
                    var v = idProp.GetValue(obj);
                    if (v is Guid g) mi.Id = g;
                    else if (Guid.TryParse(Convert.ToString(v), out var parsed)) mi.Id = parsed;
                }
                // Fix for CS8601: Possible null reference assignment.
                // Ensure that mi.Value is never assigned a possible null value by using the null-coalescing operator.

                if (textProp != null) mi.Value = Convert.ToString(textProp.GetValue(obj)) ?? string.Empty;
                if (createdAtProp != null)
                {
                    var cv = createdAtProp.GetValue(obj);
                    if (cv is DateTime dt) mi.CreatedAt = dt;
                    else if (DateTime.TryParse(Convert.ToString(cv), out var parsedDt)) mi.CreatedAt = parsedDt;
                }
                if (metaProp != null)
                {
                    var md = metaProp.GetValue(obj);
                    if (md is IDictionary<string, string> dict) mi.Metadata = new Dictionary<string, string>(dict);
                }
                list.Add(mi);
            }

            return list;
        }

        public void ReceiveMessage(string message)
        {
            throw new NotImplementedException();
        }

        public string GetResponse()
        {
            throw new NotImplementedException();
        }
    }
}