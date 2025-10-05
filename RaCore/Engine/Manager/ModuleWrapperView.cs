using Abstractions;
using System;
using System.Reflection;

namespace RaCore.Engine.Manager
{
    /// <summary>
    /// Reflection adapter for ModuleWrapper, exposes the IRaModule instance for UI and diagnostics.
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