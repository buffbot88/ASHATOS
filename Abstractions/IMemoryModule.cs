namespace Abstractions
{
    public interface IMemory : IDisposable
    {
        Task<ModuleResponse> RememberAsync(string key, string value, Dictionary<string, string>? metadata = null);
        Task<ModuleResponse> RememberAsync(string key, string value);
        Task<ModuleResponse> RecallAsync(string key);
        Task<ModuleResponse> GetAllItemsAsync();

        IEnumerable<MemoryItem> GetAllItems();
        MemoryItem? GetItemById(Guid id);

        bool Remove(Guid id);
        bool Remove(string key);
        void Clear();
        int Count();
    }
    public interface IBinMemory : IDisposable
    {
        Task<ModuleResponse> AddEntryAsync(string path, string key, string value, Dictionary<string, string>? metadata = null, BinChannel channel = BinChannel.Other);
        IEnumerable<BinEntry> GetEntries(string? path = null, BinChannel? channel = null);
        BinEntry? GetEntryById(Guid id);
        IEnumerable<BinEntry> Query(Func<BinEntry, bool> predicate);
        bool RemoveEntry(Guid id);
        void Compact();
        void Export(string exportPath);
        void Import(string importPath);
    }
}