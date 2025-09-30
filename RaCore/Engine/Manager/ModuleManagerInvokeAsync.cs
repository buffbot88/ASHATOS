using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Engine.Manager;

/// <summary>
/// Async extension methods for invoking module commands in ModuleManager.
/// Supports both async ProcessAsync and sync Process entry points on updated core modules.
/// </summary>
public static class ModuleManagerInvokeAsync
{
    /// <summary>
    /// Asynchronously invoke a module by name, returning its string response (or null/no response).
    /// If the module exposes ProcessAsync/Process returning Task&lt;string&gt;, it will be awaited. 
    /// Otherwise the synchronous Process(string) will be executed on a threadpool thread.
    /// </summary>
    public static async Task<string?> SafeInvokeModuleByNameAsync(this ModuleManager mgr, string name, string input, int timeoutMs = 0, CancellationToken ct = default)
    {
        if (mgr == null)
            throw new ArgumentNullException(nameof(mgr));
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // locate instance via existing manager helpers
        var inst = mgr.GetModuleByName(name) ?? mgr.GetModuleInstanceByName(name);
        object? target = inst;
        ModuleWrapper? wrapper = null;
        if (inst == null)
        {
            // try wrapper lookup
            wrapper = mgr.GetWrapperByName(name);
            if (wrapper != null) target = wrapper;
        }

        if (target == null) return null;

        var t = target.GetType();

        // look for ProcessAsync returning Task<string> or Task
        var procAsync = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .FirstOrDefault(m => string.Equals(m.Name, "ProcessAsync", StringComparison.OrdinalIgnoreCase)
                                              && typeof(Task).IsAssignableFrom(m.ReturnType)
                                              && m.GetParameters().Length == 1
                                              && m.GetParameters()[0].ParameterType == typeof(string));

        Func<Task<string?>> call = async () =>
        {
            if (procAsync != null)
            {
                // invoke and await; handle Task<string> or Task
                var invoked = procAsync.Invoke(target, new object[] { input });
                if (invoked == null) return null;

                var task = (Task)invoked;
                await task.ConfigureAwait(false);

                // if Task<TResult>, get Result
                var ttype = task.GetType();
                if (ttype.IsGenericType)
                {
                    var resultProp = ttype.GetProperty("Result");
                    var val = resultProp?.GetValue(task);
                    return val?.ToString();
                }

                return null;
            }

            // fallback: look for sync Process(string)
            var proc = t.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            if (proc != null)
            {
                // run sync Process on threadpool
                return await Task.Run(() =>
                {
                    var outObj = proc.Invoke(target, new object[] { input });
                    if (outObj is string s && !string.IsNullOrEmpty(s)) return s;
                    return outObj?.ToString();
                }, ct).ConfigureAwait(false);
            }

            // last resort: try wrapper-level invocation if we had wrapper
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
                    var invoked = wprocAsync.Invoke(wrapper, new object[] { input });
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

                var wproc = wtype.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null)
                           ?? wtype.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                if (wproc != null)
                {
                    return await Task.Run(() =>
                    {
                        var outObj = wproc.Invoke(wrapper, new object[] { input });
                        if (outObj is string ss && !string.IsNullOrEmpty(ss)) return ss;
                        return outObj?.ToString();
                    }, ct).ConfigureAwait(false);
                }
            }

            return null;
        };

        if (timeoutMs > 0)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var timeoutTask = Task.Delay(timeoutMs, cts.Token);
            var invokeTask = call();
            var completed = await Task.WhenAny(invokeTask, timeoutTask).ConfigureAwait(false);
            if (completed == invokeTask)
            {
                cts.Cancel(); // cancel the delay
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
    /// Try candidates in order and return first non-empty response.
    /// </summary>
    public static async Task<string?> InvokeModuleProcessByNameFallbackAsync(this ModuleManager mgr, IEnumerable<string> candidateNames, string input, int timeoutMsPerCandidate = 0, CancellationToken ct = default)
    {
        if (mgr == null) throw new ArgumentNullException(nameof(mgr));
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