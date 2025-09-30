namespace RaCore.Engine.Manager;

/// <summary>
/// Contract for querying available RaAI modules by name.
/// Used by LocalModuleServer and other infrastructure.
/// </summary>
public interface IModuleRegistry
{
    /// <summary>
    /// Returns an array of all available module names.
    /// </summary>
    string[] GetModuleNames();

    /// <summary>
    /// Gets a module instance by name, or null if not found.
    /// </summary>
    IRaModule? GetModuleByName(string name);
}
