using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Abstractions;

namespace RaCore.Engine.Manager
{
    /// <summary>
    /// Wraps an IRaModule instance for runtime control, logging, and async invocation.
    /// Supports module lifecycle, enable/disable, hooks, and async/sync invocation.
    /// </summary>
    public class ModuleWrapper(IRaModule instance) : IDisposable
    {
        public IRaModule Instance { get; } = instance ?? throw new ArgumentNullException(nameof(instance));
        public Type ModuleType => Instance.GetType();
        public bool Initialized { get; private set; } = false;

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                var invoked = TryInvokeInstanceVoidMethod(value ? "OnEnable" : "OnDisable");
                if (invoked == null)
                    TryInvokeInstanceVoidMethod(value ? "Enable" : "Disable");
            }
        }

        public int TimeoutMs { get; set; } = 0;
        public List<string> Logs { get; } = [];
        public Exception? LastException { get; set; }
        public bool LastInvocationTimedOut { get; set; }

        public string Name => Instance?.Name ?? ModuleType.Name;

        public void Initialize(ModuleManager manager)
        {
            if (Initialized) return;
            Instance.Initialize(manager);
            Initialized = true;
        }

        public void Dispose()
        {
            try { Instance.Dispose(); } catch { }
            GC.SuppressFinalize(this);
        }

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
            catch { }
            return null;
        }

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
                var mi = Instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, [typeof(string)], null);
                mi?.Invoke(Instance, [arg]);
            }
            catch { }
        }

        /// <summary>
        /// Async-friendly invocation: prefers ProcessAsync if present, falls back to sync Process.
        /// </summary>
        public Task<string> ProcessAsync(string input)
        {
            var t = Instance.GetType();
            var asyncMethod = t.GetMethod("ProcessAsync", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, [typeof(string)], null);
            if (asyncMethod != null)
            {
                try
                {
                    var ret = asyncMethod.Invoke(Instance, [input]);
                    if (ret is Task<string> ts) return ts;
                    if (ret is Task taskNoResult)
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

        public string Category
        {
            get
            {
                var attr = Instance.GetType()
                    .GetCustomAttributes(typeof(RaModuleAttribute), true)
                    .OfType<RaModuleAttribute>()
                    .FirstOrDefault();
                return attr?.Category ?? string.Empty;
            }
        }
    }
}