using System.Collections.Generic;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.MemoryModule
{
    public class MemoryItem : MemoryItemBase
    {
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}