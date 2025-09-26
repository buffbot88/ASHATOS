using System;
using System.Threading;
using System.Threading.Tasks;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.SpeechModule
{
    public class IMemory(MemoryModule.IMemory memory)
    {
        private readonly MemoryModule.IMemory _memory = memory ?? throw new ArgumentNullException(nameof(memory));

        public Task RememberAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            // Call the Remember method of the MemoryModule
            var result = _memory.Remember(key, value);
            return Task.FromResult(result);
        }

        public Task<string> RecallAsync(string key, CancellationToken cancellationToken = default)
        {
            // Call the Recall method of the MemoryModule
            var result = _memory.Recall(key);
            return Task.FromResult(result);
        }
    }
}