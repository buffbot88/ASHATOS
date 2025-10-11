namespace Abstractions;

/// <summary>
/// Attribute for marking classes as ASHATCore modules. Used for discovery and categorization.
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
