using System;
using System.Collections.Generic;

namespace RaCore.Modules.Memory;

public class BinEntry
{
    public Guid Id { get; set; }
    public string Path { get; set; } = string.Empty; // e.g. "Moods/Happy"
    public DateTime Timestamp { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
    public BinChannel Channel { get; set; } = BinChannel.Other;
}

public enum BinChannel
{
    InputOutput,
    ErrorLog,
    Other
}
