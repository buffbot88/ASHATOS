using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RaAI.Handlers.Manager
{
    /// <summary>
    /// Wraps an IRaModule instance for runtime control, logging, and async invocation.
    /// Supports core module lifecycle, enable/disable, hooks, and async/sync process.
    /// </summary>
    public class ModuleWrapper : IDisposable
    {
        public IRaModule Instance { get; }
        public Type ModuleType => Instance.GetType();
        public bool Initialized { get; private set; } = false;

        // runtime-control properties used by the UI / manager
        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;

                // Try OnEnable/OnDisable first; if not found, try Enable/Disable
                var invoked = TryInvokeInstanceVoidMethod(value ? "OnEnable" : "OnDisable");
                if (invoked == null)
                    TryInvokeInstanceVoidMethod(value ? "Enable" : "Disable");
            }
        }

        // Per-module timeout (ms) that the UI/manager can set
        public int TimeoutMs { get; set; } = 0;

        // Execution/diagnostic surface that ModuleWrapperView or UI can read/write
        public List<string> Logs { get; } = new List<string>();
        public Exception? LastException { get; set; }
        public bool LastInvocationTimedOut { get; set; }

        public string Name => Instance?.Name ?? ModuleType.Name;

        public ModuleWrapper(IRaModule instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public void Initialize(ModuleManager manager)
        {
            if (Initialized) return;
            Instance.Initialize(manager);
            Initialized = true;
        }

        public void Dispose()
        {
            try { Instance.Dispose(); } catch { /* best-effort */ }
        }

        // Try to call a synchronous parameterless void method on the instance (returns MethodInfo if invoked)
        private MethodInfo? TryInvokeInstanceVoidMethod(string methodName)
        {
            try
            {
                var mi = Instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
                if (mi != null)
                {
                    mi.Invoke(Instance, null);
                    return mi;
                }
            }
            catch
            {
                // ignore invocation errors
            }
            return null;
        }

        // Pre/Post hooks: try to call methods on the instance if they exist, otherwise noop.
        public void PreProcessInput(string input)
        {
            TryInvokeInstanceWithSingleStringArg("PreProcessInput", input);
        }

        public void PostProcessOutput(string output)
        {
            TryInvokeInstanceWithSingleStringArg("PostProcessOutput", output);
        }

        private void TryInvokeInstanceWithSingleStringArg(string methodName, string arg)
        {
            try
            {
                var mi = Instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, new[] { typeof(string) }, null);
                if (mi != null)
                    mi.Invoke(Instance, new object[] { arg });
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Unified async-friendly invocation: prefer an instance ProcessAsync(string) if present,
        /// otherwise run synchronous Process(string) and return Task&lt;string&gt;.
        /// </summary>
        public Task<string> ProcessAsync(string input)
        {
            // Look for Task<string> ProcessAsync(string)
            var t = Instance.GetType();
            var asyncMethod = t.GetMethod("ProcessAsync", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, new[] { typeof(string) }, null);
            if (asyncMethod != null)
            {
                try
                {
                    var ret = asyncMethod.Invoke(Instance, new object[] { input });
                    if (ret is Task<string> ts) return ts;
                    if (ret is Task taskNoResult) // Task with no result
                        return taskNoResult.ContinueWith(_ => string.Empty);
                }
                catch (TargetInvocationException tie)
                {
                    return Task.FromException<string>(tie.InnerException ?? tie);
                }
                catch (Exception ex)
                {
                    return Task.FromException<string>(ex);
                }
            }

            // Fallback to synchronous Process(string)
            try
            {
                var result = Instance.Process(input);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromException<string>(ex);
            }
        }
    }
}