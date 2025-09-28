using System;
using System.Text.Json;
using RaAI.Modules.MemoryModule;

namespace RaAI.Modules.SubconsciousModule.Core
{
    /// <summary>
    /// Provides serialization and deserialization for SubconsciousItem objects.
    /// </summary>
    public static class SubconsciousItemSerializer
    {
        /// <summary>
        /// Serializes a SubconsciousItem to a byte array.
        /// </summary>
        public static byte[] Serialize(MemoryItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var json = JsonSerializer.Serialize(item);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserializes a byte array to a SubconsciousItem.
        /// </summary>
        public static MemoryItem? Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<MemoryItem>(json);
        }
    }
}