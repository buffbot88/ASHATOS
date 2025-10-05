using System.Text.Json;
using Abstractions;

namespace RaCore.Modules.Subconscious;

public static class SubconsciousItemSerializer
{
    public static byte[] Serialize(MemoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var json = JsonSerializer.Serialize(item);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public static MemoryItem? Deserialize(byte[] data)
    {
        if (data == null || data.Length == 0) return null;
        var json = System.Text.Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<MemoryItem>(json);
    }
}
