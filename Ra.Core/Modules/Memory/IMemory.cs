using System;
using System.Collections.Generic;
using System.Linq;
using Ra.Core.Engine;

namespace Ra.Core.Modules.Memory;

/// <summary>
/// Simple thread-safe memory storage for RaAI modules.
/// </summary>
public class IMemory
{
    private readonly Dictionary<string, string> _memoryStore = new();
    private readonly object _lock = new();

    /// <summary>
    /// Store a key-value pair in memory.
    /// </summary>
    public void Remember(string key, string value)
    {
        lock (_lock)
        {
            _memoryStore[key] = value;
        }
    }

    /// <summary>
    /// Retrieve a value by key. Returns null if not found.
    /// </summary>
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

    /// <summary>
    /// Dump all key-value pairs as a text block.
    /// </summary>
    public string Dump()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine,
                _memoryStore.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }

    /// <summary>
    /// Clear all memory.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _memoryStore.Clear();
        }
    }

    /// <summary>
    /// Get the number of stored items.
    /// </summary>
    public int Count()
    {
        lock (_lock)
        {
            return _memoryStore.Count;
        }
    }

    /// <summary>
    /// Get all items as MemoryItem objects (keys as metadata).
    /// </summary>
    public IEnumerable<MemoryItem> GetAllItems()
    {
        lock (_lock)
        {
            return _memoryStore.Select(kvp => new MemoryItem
            {
                Key = kvp.Key,
                Value = kvp.Value,
                Metadata = new Dictionary<string, string> { { kvp.Key, kvp.Value } }
            }).ToList();
        }
    }

    /// <summary>
    /// Get a MemoryItem by Guid (converted to string for key lookup).
    /// </summary>
    public MemoryItem? GetItemById(Guid id)
    {
        var key = id.ToString();
        lock (_lock)
        {
            if (_memoryStore.TryGetValue(key, out var value))
            {
                return new MemoryItem
                {
                    Key = key,
                    Value = value,
                    Metadata = new Dictionary<string, string> { { key, value } }
                };
            }
            return null;
        }
    }
}
