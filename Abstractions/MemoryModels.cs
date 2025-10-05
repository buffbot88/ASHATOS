namespace Abstractions
{
    /// <summary>
    /// Core memory record.
    /// </summary>
    public class MemoryItem
    {
        public Guid Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public class BinEntry
    {
        public Guid Id { get; set; }
        public string Path { get; set; } = string.Empty;
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

    public class Candidate
    {
        public Guid CandidateId { get; set; }
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public DateTime CreatedAt { get; set; }
        public bool RequireConsent { get; set; }
    }

    public class ConsciousEntry
    {
        public Guid Id { get; set; }
        public DateTime PromotedAt { get; set; }
    }
}