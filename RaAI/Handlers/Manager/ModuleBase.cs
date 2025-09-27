using System;

namespace RaAI.Handlers.Manager
{
    /// <summary>
    /// Base class for all modules. Implements IRaModule and provides logging helpers.
    /// Compatible with all updated RaAI core modules.
    /// </summary>
    public abstract class ModuleBase : IRaModule
    {
        public abstract string Name { get; }

        /// <summary>
        /// Called by ModuleManager. Override in modules.
        /// </summary>
        public virtual void Initialize(ModuleManager manager) { LogInfo($"Initialized (default)"); }

        /// <summary>
        /// Process input and return output. Override in modules.
        /// </summary>
        public virtual string Process(string input) => string.Empty;

        /// <summary>
        /// Clean up any resources. Override if needed.
        /// </summary>
        public virtual void Dispose() { /* override if needed */ }

        // Simple logging helpers (modules can override to pipe into host logging)
        protected void LogInfo(string msg) => Console.WriteLine($"[Module:{Name}] INFO: {msg}");
        protected void LogWarn(string msg) => Console.WriteLine($"[Module:{Name}] WARN: {msg}");
        protected void LogError(string msg) => Console.WriteLine($"[Module:{Name}] ERROR: {msg}");
    }

    /// <summary>
    /// Manifest entry for binary or serialized module data.
    /// </summary>
    public class ManifestEntry
    {
        public Guid Id { get; set; }
        public long Offset { get; set; }
        public int TotalLength { get; set; }
        public long CreatedAtTicks { get; set; }
        public byte EntryType { get; set; }
    }

    /// <summary>
    /// Semantic info for entries in a module manifest.
    /// </summary>
    public class EntryInfo
    {
        public Guid Id { get; set; }
        public long Offset { get; set; }
        public int TotalLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte EntryType { get; set; }
    }
}