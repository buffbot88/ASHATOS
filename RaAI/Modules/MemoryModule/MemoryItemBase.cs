using System;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.MemoryModule
{
    public abstract class MemoryItemBase
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}