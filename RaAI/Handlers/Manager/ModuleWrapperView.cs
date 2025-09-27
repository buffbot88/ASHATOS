using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RaAI.Handlers.Manager
{
    /// <summary>
    /// Adapter around your existing ModuleWrapper (or any module object in ModuleManager.Modules).
    /// Provides a stable, reflection-based view for UI and tooling without changing framework types.
    /// </summary>
    public class ModuleWrapperView
    {
        private readonly object inner;
        private readonly Type innerType;

        public ModuleWrapperView(object moduleWrapper)
        {
            inner = moduleWrapper ?? throw new ArgumentNullException(nameof(moduleWrapper));
            innerType = inner.GetType();
        }

        public string Name
        {
            get
            {
                var p = innerType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return (p.GetValue(inner)?.ToString()) ?? innerType.Name;

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null) return (ip.GetValue(inst)?.ToString()) ?? inst.GetType().Name;
                }

                return innerType.Name;
            }
        }

        public bool Enabled
        {
            get
            {
                var p = innerType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var raw = p.GetValue(inner);
                    var conv = TryConvertToBoolean(raw);
                    return conv ?? true;
                }

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null)
                    {
                        var raw = ip.GetValue(inst);
                        var conv = TryConvertToBoolean(raw);
                        return conv ?? true;
                    }
                }

                return true;
            }
            set
            {
                var p = innerType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var converted = ConvertToTargetType(value, p.PropertyType);
                    if (converted != null) p.SetValue(inner, converted);
                    return;
                }

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null)
                    {
                        var converted = ConvertToTargetType(value, ip.PropertyType);
                        if (converted != null) ip.SetValue(inst, converted);
                    }
                }
            }
        }

        public int TimeoutMs
        {
            get
            {
                var p = innerType.GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var raw = p.GetValue(inner);
                    var conv = TryConvertToInt(raw);
                    return conv ?? 2000;
                }

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null)
                    {
                        var raw = ip.GetValue(inst);
                        var conv = TryConvertToInt(raw);
                        return conv ?? 2000;
                    }
                }

                return 2000;
            }
            set
            {
                var p = innerType.GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var converted = ConvertToTargetType(value, p.PropertyType);
                    if (converted != null) p.SetValue(inner, converted);
                    return;
                }

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("TimeoutMs", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null)
                    {
                        var converted = ConvertToTargetType(value, ip.PropertyType);
                        if (converted != null) ip.SetValue(inst, converted);
                    }
                }
            }
        }

        public IList<string> Logs
        {
            get
            {
                var p = innerType.GetProperty("Logs", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var v = p.GetValue(inner);
                    if (v is IList<string> list) return list;
                    if (v is IEnumerable<string> en) return en.ToList();
                }

                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("Logs", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null)
                    {
                        var v = ip.GetValue(inst);
                        if (v is IList<string> list2) return list2;
                        if (v is IEnumerable<string> en2) return en2.ToList();
                    }
                }
                return new List<string>();
            }
        }

        public Exception? LastException
        {
            get
            {
                var p = innerType.GetProperty("LastException", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && typeof(Exception).IsAssignableFrom(p.PropertyType)) return (Exception?)p.GetValue(inner);
                var inst = GetInstance();
                if (inst != null)
                {
                    var ip = inst.GetType().GetProperty("LastException", BindingFlags.Public | BindingFlags.Instance);
                    if (ip != null && typeof(Exception).IsAssignableFrom(ip.PropertyType)) return (Exception?)ip.GetValue(inst);
                }
                return null;
            }
            set
            {
                var p = innerType.GetProperty("LastException", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && typeof(Exception).IsAssignableFrom(p.PropertyType)) p.SetValue(inner, value);
            }
        }

        public bool LastInvocationTimedOut
        {
            get
            {
                var p = innerType.GetProperty("LastInvocationTimedOut", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var raw = p.GetValue(inner);
                    var conv = TryConvertToBoolean(raw);
                    return conv ?? false;
                }
                return false;
            }
            set
            {
                var p = innerType.GetProperty("LastInvocationTimedOut", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) p.SetValue(inner, value);
            }
        }

        public async Task<string> ProcessAsync(string input)
        {
            var mAsync = innerType.GetMethod("ProcessAsync", BindingFlags.Public | BindingFlags.Instance);
            if (mAsync != null)
            {
                var ret = mAsync.Invoke(inner, new object[] { input });
                if (ret is Task<string> taskStr) return await taskStr.ConfigureAwait(false);
                if (ret is Task t)
                {
                    await t.ConfigureAwait(false);
                    var resultProp = t.GetType().GetProperty("Result");
                    if (resultProp != null) return (resultProp.GetValue(t)?.ToString()) ?? string.Empty;
                    return string.Empty;
                }
            }

            var mSync = innerType.GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
            if (mSync != null)
            {
                var r = mSync.Invoke(inner, new object[] { input });
                return (r?.ToString()) ?? string.Empty;
            }

            var inst = GetInstance();
            if (inst != null)
            {
                var instMs = inst.GetType().GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
                if (instMs != null)
                {
                    var r = instMs.Invoke(inst, new object[] { input });
                    return (r?.ToString()) ?? string.Empty;
                }

                var instAsync = inst.GetType().GetMethod("ProcessAsync", BindingFlags.Public | BindingFlags.Instance);
                if (instAsync != null)
                {
                    var ret = instAsync.Invoke(inst, new object[] { input });
                    if (ret is Task<string> ts) return await ts.ConfigureAwait(false);
                    if (ret is Task t) { await t.ConfigureAwait(false); var resultProp = t.GetType().GetProperty("Result"); if (resultProp != null) return (resultProp.GetValue(t)?.ToString()) ?? string.Empty; }
                }
            }

            return string.Empty;
        }

        public void PreProcessInput(string input)
        {
            InvokeIfExists("PreProcessInput", input);
        }

        public void PostProcessOutput(string output)
        {
            InvokeIfExists("PostProcessOutput", output);
        }

        private void InvokeIfExists(string methodName, string arg)
        {
            try
            {
                var m = innerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (m != null) { m.Invoke(inner, new object[] { arg }); return; }

                var inst = GetInstance();
                if (inst != null)
                {
                    var mi = inst.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                    if (mi != null) mi.Invoke(inst, new object[] { arg });
                }
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }

        private object? GetInstance()
        {
            var p = innerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance);
            if (p != null) return p.GetValue(inner);
            return null;
        }

        private static object? ConvertToTargetType(object? value, Type targetType)
        {
            if (value == null) return null;
            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;
            try
            {
                if (t == typeof(bool))
                {
                    if (value is bool b) return b;
                    if (value is int i) return i != 0;
                    if (value is long l) return l != 0L;
                    if (value is string s && bool.TryParse(s, out var bb)) return bb;
                    if (value is string s2 && int.TryParse(s2, out var bi)) return bi != 0;
                    return Convert.ChangeType(value, t);
                }
                if (t == typeof(int))
                {
                    if (value is int) return value;
                    if (value is bool bb) return bb ? 1 : 0;
                    if (value is long ll) return (int)ll;
                    if (value is string ss && int.TryParse(ss, out var ii)) return ii;
                    return Convert.ChangeType(value, t);
                }
                if (t == typeof(long))
                {
                    if (value is long) return value;
                    if (value is int ii) return (long)ii;
                    if (value is bool bb) return bb ? 1L : 0L;
                    if (value is string ss && long.TryParse(ss, out var ll)) return ll;
                    return Convert.ChangeType(value, t);
                }
                if (t.IsEnum)
                {
                    if (value is string s) return Enum.Parse(t, s, true);
                    return Enum.ToObject(t, value);
                }
                if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal))
                    return Convert.ChangeType(value, t);
            }
            catch { }
            return null;
        }

        private static bool? TryConvertToBoolean(object? raw)
        {
            if (raw == null) return null;
            try
            {
                if (raw is bool b) return b;
                if (raw is int i) return i != 0;
                if (raw is long l) return l != 0L;
                if (raw is string s && bool.TryParse(s, out var bb)) return bb;
                if (raw is string s2 && int.TryParse(s2, out var bi)) return bi != 0;
                return Convert.ToBoolean(raw);
            }
            catch { return null; }
        }

        private static int? TryConvertToInt(object? raw)
        {
            if (raw == null) return null;
            try
            {
                if (raw is int i) return i;
                if (raw is long l) return (int)l;
                if (raw is bool b) return b ? 1 : 0;
                if (raw is string s && int.TryParse(s, out var ii)) return ii;
                return Convert.ToInt32(raw);
            }
            catch { return null; }
        }

        public object Raw => inner;
    }
}