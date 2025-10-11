using Abstractions;

namespace ASHATCore.Engine.Manager
{

    /// <summary>
    /// ContASHATct for querying available modules by name for the ASHATCore system.
    /// Used by servers and infASHATstructure to access and Enumerate modules.
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