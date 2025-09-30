using System;

namespace RaCore.Engine.Manager;

/// <summary>
/// Minimal module contract for RaAI modules.
/// All core modules must implement this for compatibility with ModuleManager.
/// </summary>
public interface IRaModule : IDisposable
{
    /// <summary>
    /// Gets the module's display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Called once after the module is instantiated. Manager passes itself for legacy compatibility.
    /// </summary>
    void Initialize(ModuleManager manager);

    /// <summary>
    /// Simple text processing entry point for interactive testing.
    /// </summary>
    string Process(string input);
}
