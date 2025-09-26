using System;

namespace RaAI.Modules.MemoryModule
{
    public interface IMemory
    {
        Guid Remember(string key, string value);
        string Recall(string key);
    }
}