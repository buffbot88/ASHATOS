using Abstractions;

namespace RaCore.Engine.Manager
{

    /// <summary>
    /// Contract for querying available modules by name for the RaCore system.
    /// Used by servers and infrastructure to access and enumerate modules.
    /// </summary>
    public interface IModuleRegistry
    {
        /// <summary>
        /// Returns all available module names.
        /// </summary>
        string[] GetModuleNames();

        /// <summary>
        /// Gets a module instance by name, or null if not found.
        /// </summary>
        IRaModule? GetModuleByName(string name);
    }
}