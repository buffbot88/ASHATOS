namespace ASHATCore.Engine.Manager
{
    /// <summary>
    /// Result of module loading, including loaded modules and errors.
    /// </summary>
    public class ModuleLoadResult
    {
        /// <summary>
        /// Successfully loaded modules (views).
        /// </summary>
        public List<ModulewrapperView> Loaded { get; } = [];

        /// <summary>
        /// Error messages encountered during module loading.
        /// </summary>
        public List<string> Errors { get; } = [];
    }
}