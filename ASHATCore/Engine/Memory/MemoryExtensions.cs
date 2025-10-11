using System;
using System.Collections.Generic;
using System.Linq;
using Abstractions;

namespace ASHATCore.Engine.Memory
{
    /// <summary>
    /// Static helpers and extension methods for memory Operations.
    /// </summary>
    public static class MemoryExtensions
    {
        public static MemoryItem? GetLatest(this IEnumerable<MemoryItem> items, string key)
        {
            return items
                .Where(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefault();
        }

        public static IEnumerable<MemoryItem> FindByMetadata(this IEnumerable<MemoryItem> items, string metaKey, string metaValue)
        {
            return items.Where(item =>
                item.Metadata.TryGetValue(metaKey, out var value) &&
                string.Equals(value, metaValue, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<BinEntry> FilterByChannel(this IEnumerable<BinEntry> entries, BinChannel channel)
        {
            return entries.Where(e => e.Channel == channel);
        }

        public static IEnumerable<BinEntry> FilterByPath(this IEnumerable<BinEntry> entries, string path)
        {
            return entries.Where(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase));
        }

        public static Dictionary<string, object>? ToObjectMetadata(this Dictionary<string, string> meta)
        {
            if (meta == null)
                return null;
            return meta.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
        }
    }
}