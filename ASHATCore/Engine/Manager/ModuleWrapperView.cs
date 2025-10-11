using Abstractions;
using System;
using System.Reflection;

namespace ASHATCore.Engine.Manager
{
    /// <summary>
    /// Reflection adapter for Modulewrapper, exposes the IRaModule instance for UI and diagnostics.
    /// </summary>
    public class ModulewrapperView
    {
        private readonly object inner;
        private readonly Type innerType;

        public ModulewrapperView(object modulewrapper)
        {
            inner = modulewrapper ?? throw new ArgumentNullException(nameof(modulewrapper));
            innerType = inner.GetType();
        }

        public IRaModule? ModuleInstance
        {
            get
            {
                var p = innerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return p.GetValue(inner) as IRaModule;
                return null;
            }
        }

        public string Name => ModuleInstance?.Name ?? innerType.Name;
        public object Raw => inner;
    }
}