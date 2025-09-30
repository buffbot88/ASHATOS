using System;

namespace RaCore.Engine.Manager;

/// <summary>
/// Attribute for marking classes as RaAI modules.
/// Used for discovery and categorization in ModuleManager.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RaModuleAttribute : Attribute
{
    /// <summary>
    /// Optional category for the module (e.g., "core", "ui", "ai").
    /// </summary>
    public string? Category { get; set; }

    public RaModuleAttribute() { }

    public RaModuleAttribute(string category) { Category = category; }
}
