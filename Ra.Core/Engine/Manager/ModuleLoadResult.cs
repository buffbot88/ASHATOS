using System.Collections.Generic;

namespace Ra.Core.Engine.Manager;

/// <summary>
/// Represents the result of module loading, including loaded module views and any errors.
/// </summary>
public class ModuleLoadResult
{
    /// <summary>
    /// List of successfully loaded modules (views).
    /// </summary>
    public List<ModuleWrapperView> Loaded { get; } = new();

    /// <summary>
    /// List of error messages encountered during module loading.
    /// </summary>
    public List<string> Errors { get; } = new();
}