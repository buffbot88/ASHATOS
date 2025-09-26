using System;

namespace RaAI.Handlers
{
    public abstract class ModuleBase : IRaModule
    {
        public abstract string Name { get; }

        // Called by ModuleManager. Override in modules.
        public virtual void Initialize(ModuleManager manager) { LogInfo($"Intialized (default)"); }

        public virtual string Process(string input) => string.Empty;

        public virtual void Dispose() { /* override if needed */ }

        // Simple logging helpers (modules can override to pipe into host logging)
        protected void LogInfo(string msg) => Console.WriteLine($"[Module:{Name}] INFO: {msg}");
        protected void LogWarn(string msg) => Console.WriteLine($"[Module:{Name}] WARN: {msg}");
        protected void LogError(string msg) => Console.WriteLine($"[Module:{Name}] ERROR: {msg}");
    }
    public class ManifestEntry
    {
        public Guid Id { get; set; }
        public long Offset { get; set; }
        public int TotalLength { get; set; }
        public long CreatedAtTicks { get; set; }
        public byte EntryType { get; set; }
    }

    public class EntryInfo
    {
        public Guid Id { get; set; }
        public long Offset { get; set; }
        public int TotalLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte EntryType { get; set; }
    }
}