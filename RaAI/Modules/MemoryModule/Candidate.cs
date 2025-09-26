namespace RaAI.Modules.MemoryModule
{
    /// <summary>
    /// Represents a candidate item to be added to the memory.
    /// </summary>
    public class Candidate
    {
        /// <summary>
        /// Gets or sets the unique identifier of the candidate.
        /// </summary>
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Gets or sets the text content of the candidate.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metadata associated with the candidate.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets the creation timestamp of the candidate.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether consent is required for this candidate.
        /// </summary>
        public bool RequireConsent { get; set; }
    }
}