namespace RaAI.Modules.MemoryModule
{
    /// <summary>
    /// Represents an entry in the conscious index.
    /// </summary>
    public class ConsciousEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier of the conscious entry.
        /// </summary>
        /// public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the entry was promoted to the conscious index.
        /// </summary>
        public DateTime PromotedAt { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the conscious entry.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
        public Guid Id { get; set; }
    }
}