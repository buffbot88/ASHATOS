using System.Collections.Generic;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.SubconsciousModule.Core
{
    public class SubconsciousItem : MemoryItemBase
    {
        public Dictionary<string, string>? Metadata { get; set; }
    }
}