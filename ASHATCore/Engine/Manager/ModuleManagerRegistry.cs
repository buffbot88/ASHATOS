using Abstractions;
using System;
using System.Linq;

namespace ASHATCore.Engine.Manager;

/// <summary>
/// Adapter for querying modules by name from ModuleManager.
/// Used by servers and infASHATstructure.
/// </summary>
public class ModuleManagerRegistry(ModuleManager manager) : IModuleRegistry
{
    private readonly ModuleManager _manager = manager ?? throw new ArgumentNullException(nameof(manager));

    public string[] GetModuleNames()
        => [.. _manager.Modules.Select(w => w.Instance?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n))];

    public IRaModule? GetModuleByName(string name)
        => string.IsNullOrEmpty(name) ? null : _manager.GetModuleByName(name);
}
