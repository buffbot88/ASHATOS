using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Engine.Manager;

/// <summary>
/// Extension methods for async loading of modules via ModuleManager.
/// Ensures compatibility with updated core modules.
/// </summary>
public static class ModuleManagerAsync
{
    /// <summary>
    /// Async wrapper for loading modules. Prefers an async method, falls back to sync.
    /// </summary>
    public static async Task<ModuleLoadResult> LoadModulesAsync(this ModuleManager mgr, int perModuleDefaultTimeoutMs = 2000, int overallTimeoutMs = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mgr);

        var type = mgr.GetType();

        // 1) Prefer an actual async method
        var asyncCandidate = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                (m.Name.Equals("LoadModulesAsync", StringComparison.OrdinalIgnoreCase) ||
                 m.Name.Equals("LoadModules", StringComparison.OrdinalIgnoreCase)) &&
                typeof(Task).IsAssignableFrom(m.ReturnType) &&
                (m.GetParameters().Length == 0 || m.GetParameters().Length == 1));

        if (asyncCandidate != null && typeof(Task).IsAssignableFrom(asyncCandidate.ReturnType))
        {
            try
            {
                var args = ResolveArgsForMethod(asyncCandidate, perModuleDefaultTimeoutMs);
                var invoked = asyncCandidate.Invoke(mgr, args);
                var task = (Task)invoked!;
                if (overallTimeoutMs > 0)
                {
                    var completed = await Task.WhenAny(task, Task.Delay(overallTimeoutMs, cancellationToken)).ConfigureAwait(false);
                    if (completed != task) throw new TimeoutException("LoadModulesAsync timed out.");
                }
                else
                {
                    await task.ConfigureAwait(false);
                }

                if (asyncCandidate.ReturnType.IsGenericType)
                {
                    // assume Task<ModuleLoadResult>
                    var resultProperty = task.GetType().GetProperty("Result");
                    return (ModuleLoadResult)resultProperty!.GetValue(task)!;
                }

                // if it was Task (non-generic), we don't have a ModuleLoadResult; throw or return default
                throw new InvalidOperationException("Async Load method did not return ModuleLoadResult.");
            }
            catch (TargetInvocationException tie) { throw tie.InnerException ?? tie; }
        }

        // 2) Fallback to synchronous method run on background thread
        var syncCandidates = new[] { "LoadModules", "ReloadModules", "LoadAllModules", "Load" };
        MethodInfo? syncMethod = null;
        foreach (var name in syncCandidates)
        {
            syncMethod = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (syncMethod != null) break;
        }

        if (syncMethod == null)
        {
            // try to find any sync method returning ModuleLoadResult
            syncMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.ReturnType == typeof(ModuleLoadResult) && m.GetParameters().Length <= 1);
        }

        if (syncMethod == null)
        {
            throw new MissingMethodException("No suitable LoadModules method found.");
        }

        try
        {
            var args = ResolveArgsForMethod(syncMethod, perModuleDefaultTimeoutMs);
            Task<ModuleLoadResult> runner = Task.Run(() =>
            {
                try
                {
                    var r = syncMethod.Invoke(mgr, args);
                    return (ModuleLoadResult)r!;
                }
                catch (TargetInvocationException tie) { throw tie.InnerException ?? tie; }
            }, cancellationToken);

            if (overallTimeoutMs > 0)
            {
                var completed = await Task.WhenAny(runner, Task.Delay(overallTimeoutMs, cancellationToken)).ConfigureAwait(false);
                if (completed != runner) throw new TimeoutException("LoadModulesAsync timed out.");
            }

            return await runner.ConfigureAwait(false);
        }
        catch (TargetInvocationException tie) { throw tie.InnerException ?? tie; }
    }

    private static object[] ResolveArgsForMethod(MethodInfo method, int perModuleDefaultTimeoutMs)
    {
        var parms = method.GetParameters();
        if (parms.Length == 0) return [];
        var pType = parms[0].ParameterType;

        object? arg;
        if (pType == typeof(int)) arg = perModuleDefaultTimeoutMs;
        else if (pType == typeof(long)) arg = (long)perModuleDefaultTimeoutMs;
        else if (pType == typeof(bool)) arg = true;
        else if (pType == typeof(TimeSpan)) arg = TimeSpan.FromMilliseconds(perModuleDefaultTimeoutMs);
        else if (pType.IsEnum) arg = Enum.ToObject(pType, perModuleDefaultTimeoutMs);
        else if (pType == typeof(string)) arg = perModuleDefaultTimeoutMs.ToString();
        else
        {
            try { arg = Convert.ChangeType(perModuleDefaultTimeoutMs, pType); }
            catch { arg = pType.IsValueType ? Activator.CreateInstance(pType) : null; }
        }

        return [arg!];
    }
}