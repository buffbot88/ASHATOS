using System;
using System.Linq;

namespace RaAI.Handlers
{
    public class ModuleManagerRegistry(ModuleManager manager) : IModuleRegistry
    {
        private readonly ModuleManager _manager = manager ?? throw new ArgumentNullException(nameof(manager));

        public string[] GetModuleNames()
            => _manager.Modules.Select(w => w.Instance?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();
        public IRaModule? GetModuleByName(string name)
            => string.IsNullOrEmpty(name) ? null : _manager.GetModuleByName(name);
    }
}