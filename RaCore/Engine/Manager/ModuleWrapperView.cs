using System;
using System.Reflection;
using RaCore.Engine.Manager;

namespace RaCore.Engine.Manager
{
    /// <summary>
    /// Reflection adapter for ModuleWrapper, exposes the actual IRaModule instance for UI.
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

        /// <summary>
        /// Direct access to IRaModule instance (or null if not present)
        /// </summary>
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
