namespace Abstractions;


/// <summary>
/// Minimal interface for RaCore modules. All modules must implement this for manager compatibility.
/// </summary>
public interface IRaModule : IDisposable
{
    /// <summary>
    /// Display name for the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Called after instantiation; manager is passed for context/wiring.
    /// </summary>
    void Initialize(object? manager);

    /// <summary>
    /// Synchronous text processing entry point.
    /// </summary>
    string Process(string input);
    new void Dispose();
}
