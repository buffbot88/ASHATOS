using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Engine.Manager;

/// <summary>
/// Async extension methods for invoking module commands in ModuleManager.
/// Supports both async ProcessAsync and sync Process entry points.
/// </summary>
public static class ModuleManagerInvokeAsync
{
    /// <summary>
    /// Asynchronously invoke a module by name, returning string response or null.
    /// Prefers ProcessAsync if present, falls back to sync Process.
    /// </summary>
    public static async Task<string?> SafeInvokeModuleByNameAsync(this ModuleManager mgr, string name, string input, int timeoutMs = 0, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(mgr);

        if (string.IsNullOrWhiteSpace(name))
            return null;

        var inst = mgr.GetModuleByName(name) ?? mgr.GetModuleInstanceByName(name);
        object? target = inst;
        ModuleWrapper? wrapper = null;
        if (inst == null)
        {
            wrapper = mgr.GetWrapperByName(name);
            if (wrapper != null) target = wrapper;
        }

        if (target == null) return null;

        var t = target.GetType();

        var procAsync = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .FirstOrDefault(m => string.Equals(m.Name, "ProcessAsync", StringComparison.OrdinalIgnoreCase)
                                              && typeof(Task).IsAssignableFrom(m.ReturnType)
                                              && m.GetParameters().Length == 1
                                              && m.GetParameters()[0].ParameterType == typeof(string));

        async Task<string?> call()
        {
            if (procAsync != null)
            {
                var invoked = procAsync.Invoke(target, [input]);
                if (invoked == null) return null;

                var task = (Task)invoked;
                await task.ConfigureAwait(false);

                var ttype = task.GetType();
                if (ttype.IsGenericType)
                {
                    var resultProp = ttype.GetProperty("Result");
                    var val = resultProp?.GetValue(task);
                    return val?.ToString();
                }

                return null;
            }

            var proc = t.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null);
            if (proc != null)
            {
                return await Task.Run(() =>
                {
                    var outObj = proc.Invoke(target, [input]);
                    if (outObj is string s && !string.IsNullOrEmpty(s)) return s;
                    return outObj?.ToString();
                }, ct).ConfigureAwait(false);
            }

            if (wrapper != null)
            {
                var wtype = wrapper.GetType();
                var wprocAsync = wtype.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .FirstOrDefault(m => (string.Equals(m.Name, "ProcessAsync", StringComparison.OrdinalIgnoreCase) ||
                                                           string.Equals(m.Name, "InvokeAsync", StringComparison.OrdinalIgnoreCase)) &&
                                                           typeof(Task).IsAssignableFrom(m.ReturnType) &&
                                                           m.GetParameters().Length == 1 &&
                                                           m.GetParameters()[0].ParameterType == typeof(string));
                if (wprocAsync != null)
                {
                    var invoked = wprocAsync.Invoke(wrapper, [input]);
                    if (invoked is Task wt)
                    {
                        await wt.ConfigureAwait(false);
                        if (wt.GetType().IsGenericType)
                        {
                            var rp = wt.GetType().GetProperty("Result");
                            var val = rp?.GetValue(wt);
                            return val?.ToString();
                        }
                    }
                }

                var wproc = wtype.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null)
                           ?? wtype.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null);
                if (wproc != null)
                {
                    return await Task.Run(() =>
                    {
                        var outObj = wproc.Invoke(wrapper, [input]);
                        if (outObj is string ss && !string.IsNullOrEmpty(ss)) return ss;
                        return outObj?.ToString();
                    }, ct).ConfigureAwait(false);
                }
            }

            return null;
        }

        if (timeoutMs > 0)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var timeoutTask = Task.Delay(timeoutMs, cts.Token);
            var invokeTask = call();
            var completed = await Task.WhenAny(invokeTask, timeoutTask).ConfigureAwait(false);
            if (completed == invokeTask)
            {
                cts.Cancel();
                return await invokeTask.ConfigureAwait(false);
            }
            else
            {
                return $"(module {name} invocation timed out after {timeoutMs}ms)";
            }
        }
        else
        {
            return await call().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Try candidates in order, return first non-empty response.
    /// </summary>
    public static async Task<string?> InvokeModuleProcessByNameFallbackAsync(this ModuleManager mgr, IEnumerable<string> candidateNames, string input, int timeoutMsPerCandidate = 0, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(mgr);

        if (candidateNames == null) return null;

        foreach (var cname in candidateNames)
        {
            try
            {
                var res = await mgr.SafeInvokeModuleByNameAsync(cname, input, timeoutMsPerCandidate, ct).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(res)) return res;
            }
            catch
            {
                // continue
            }
        }
        return null;
    }
}
