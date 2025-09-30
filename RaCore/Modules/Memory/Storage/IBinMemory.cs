using System;
using System.Collections.Generic;

namespace RaCore.Modules.Memory;

public interface IBinMemory : IDisposable
{
    Guid AddEntry(string path, string key, string value, Dictionary<string, string>? metadata = null, BinChannel channel = BinChannel.Other);
    IEnumerable<BinEntry> GetEntries(string? path = null, BinChannel? channel = null);
    BinEntry? GetEntryById(Guid id);
    IEnumerable<BinEntry> Query(Func<BinEntry, bool> predicate);
    bool RemoveEntry(Guid id);
    void Compact();
    void Export(string exportPath);
    void Import(string importPath);
}
