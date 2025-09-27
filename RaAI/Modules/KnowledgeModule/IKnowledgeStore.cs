using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.KnowledgeModule
{
    public sealed class KnowledgeItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = "";
        public string Source { get; set; } = "user";
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public float Importance { get; set; } = 0.0f;     // 0..1
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public float Decay { get; set; } = 0.0f;          // 0..1 increasing over time
        public float Confidence { get; set; } = 1.0f;
        public string? JsonPayload { get; set; }          // optional structured data
    }

    public interface IKnowledgeStore
    {
        Task<Guid> IngestAsync(
            string text,
            string source,
            IEnumerable<string>? tags = null,
            float? importance = null,
            string? jsonPayload = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<KnowledgeItem>> QueryAsync(KnowledgeQuery query, CancellationToken ct = default);

        Task ConsolidateAsync(CancellationToken ct = default); // optional summarize/merge
        Task DecayAsync(TimeSpan halfLife, CancellationToken ct = default); // update Decay based on time
    }
}