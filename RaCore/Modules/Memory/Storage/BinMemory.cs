using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RaCore.Modules.Memory;

public class BinMemory : IBinMemory
{
    private readonly string _binFilePath;
    private readonly List<BinEntry> _entries = new();
    private readonly object _lock = new();

    public BinMemory(string binFilePath)
    {
        _binFilePath = binFilePath;
        LoadFromFile();
    }

    public Guid AddEntry(string path, string key, string value, Dictionary<string, string>? metadata = null, BinChannel channel = BinChannel.Other)
    {
        var entry = new BinEntry
        {
            Id = Guid.NewGuid(),
            Path = path,
            Timestamp = DateTime.UtcNow,
            Key = key,
            Value = value,
            Metadata = metadata,
            Channel = channel
        };
        lock (_lock)
        {
            _entries.Add(entry);
            AppendToFile(entry);
        }
        return entry.Id;
    }

    public IEnumerable<BinEntry> GetEntries(string? path = null, BinChannel? channel = null)
    {
        lock (_lock)
        {
            return _entries.Where(e =>
                (path == null || e.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) &&
                (channel == null || e.Channel == channel)).ToList();
        }
    }

    public BinEntry? GetEntryById(Guid id)
    {
        lock (_lock)
        {
            return _entries.FirstOrDefault(e => e.Id == id);
        }
    }

    public IEnumerable<BinEntry> Query(Func<BinEntry, bool> predicate)
    {
        lock (_lock)
        {
            return _entries.Where(predicate).ToList();
        }
    }

    public bool RemoveEntry(Guid id)
    {
        lock (_lock)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
            {
                _entries.Remove(entry);
                // Optionally: rewrite bin file without this entry (or just mark as deleted in future)
                SaveAllToFile();
                return true;
            }
            return false;
        }
    }

    public void Compact()
    {
        lock (_lock)
        {
            SaveAllToFile();
        }
    }

    public void Export(string exportPath)
    {
        lock (_lock)
        {
            File.Copy(_binFilePath, exportPath, true);
        }
    }

    public void Import(string importPath)
    {
        lock (_lock)
        {
            File.Copy(importPath, _binFilePath, true);
            LoadFromFile();
        }
    }

    private void AppendToFile(BinEntry entry)
    {
        var json = JsonSerializer.Serialize(entry);
        File.AppendAllText(_binFilePath, json + Environment.NewLine);
    }

    private void SaveAllToFile()
    {
        var allJson = _entries.Select(e => JsonSerializer.Serialize(e));
        File.WriteAllLines(_binFilePath, allJson);
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_binFilePath))
            return;
        _entries.Clear();
        foreach (var line in File.ReadAllLines(_binFilePath))
        {
            try
            {
                var entry = JsonSerializer.Deserialize<BinEntry>(line);
                if (entry != null)
                    _entries.Add(entry);
            }
            catch { /* ignore bad lines for now */ }
        }
    }

    public void Dispose()
    {
        // No unmanaged resources, no-op
    }
}
