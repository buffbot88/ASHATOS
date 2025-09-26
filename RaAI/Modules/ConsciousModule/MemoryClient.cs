using RaAI.Handlers;

namespace RaAI.Modules.ConsciousModule
{
    public class MemoryClient
    {
        private readonly Dictionary<string, string> _memoryStore = [];
        private readonly Lock _lock = new();

        public void Remember(string key, string value)
        {
            lock (_lock)
            {
                _memoryStore[key] = value;
            }
        }

        public string? Recall(string key)
        {
            lock (_lock)
            {
                if (_memoryStore.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }
        }

        public string Dump()
        {
            lock (_lock)
            {
                return string.Join(Environment.NewLine,
                    _memoryStore.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _memoryStore.Clear();
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _memoryStore.Count;
            }
        }
    }
}
