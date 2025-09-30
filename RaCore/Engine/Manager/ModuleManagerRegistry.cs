using System;
using System.Linq;

namespace RaCore.Engine.Manager;

/// <summary>
/// Provides IModuleRegistry implementation for ModuleManager.
/// Enables querying and lookup by name for integration with LocalModuleServer and other infrastructure.
/// </summary>
public class ModuleManagerRegistry : IModuleRegistry
{
    private readonly ModuleManager _manager;

    public ModuleManagerRegistry(ModuleManager manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public string[] GetModuleNames()
        => _manager.Modules.Select(w => w.Instance?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToArray();

    public IRaModule? GetModuleByName(string name)
        => string.IsNullOrEmpty(name) ? null : _manager.GetModuleByName(name);
}