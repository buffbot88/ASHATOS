using System.Text;
using System.Text.Json;
using RaCore.Modules.Conscious;
using Abstractions;

namespace RaCore.Modules.Extensions.Knowledge;

/// <summary>
/// IKnowledgeStore implemented on top of IMemory.
/// Stores each KnowledgeItem as JSON under key = "knowledge:{id}".
/// All ingest/query/consolidate/decay output is routed through ThoughtProcessor (Phase 2).
/// </summary>
public sealed class MemoryBackedKnowledgeStore(IMemory memory, IEmbeddingProvider embedder, ThoughtProcessor? thoughtProcessor = null) : IKnowledgeStore
{
    private readonly IMemory _memory = memory ?? throw new ArgumentNullException(nameof(memory));
    private readonly IEmbeddingProvider _embedder = embedder ?? throw new ArgumentNullException(nameof(embedder));
    private readonly ThoughtProcessor? _thoughtProcessor = thoughtProcessor;

    private const string Prefix = "knowledge:";

    public async Task<Guid> IngestAsync(
        string text,
        string source,
        IEnumerable<string>? tags = null,
        float? importance = null,
        string? jsonPayload = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var emb = await _embedder.EmbedAsync(text, ct);
        var item = new KnowledgeItem
        {
            Id = Guid.NewGuid(),
            Text = text ?? "",
            Source = string.IsNullOrWhiteSpace(source) ? "user" : source.Trim(),
            Tags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                   ?? [],
            CreatedAt = DateTime.UtcNow,
            Importance = importance ?? EstimateImportance(text ?? string.Empty),
            Embedding = emb,
            Decay = 0f,
            Confidence = 1.0f,
            JsonPayload = jsonPayload
        };

        var key = BuildKey(item.Id);
        var json = JsonSerializer.Serialize(item, JsonOpts);
        await _memory.RememberAsync(key, json);

        // Agentic output
        if (_thoughtProcessor != null)
        {
            var resp = await _thoughtProcessor.ProcessThoughtAsync(
                $"Knowledge ingested: {text}", _memory, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Thinking
            );
        }

        return item.Id;
    }

    public async Task<IReadOnlyList<KnowledgeItem>> QueryAsync(KnowledgeQuery query, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var qText = query?.Text?.Trim() ?? "";
        var qEmb = await _embedder.EmbedAsync(qText, ct);

        var all = SafeGetAllKnowledgeItems();
        var filtered = ApplyTagFilters(all, query?.MustTags, query?.AnyTags);

        var scored = filtered
            .Select(k =>
            {
                var sim = Cosine(k.Embedding, qEmb);
                var kw = KeywordScore(k.Text, qText);
                var recency = RecencyBoost(k.CreatedAt);
                var importance = Math.Clamp(k.Importance, 0f, 1f);
                var decayPenalty = 1f - Math.Clamp(k.Decay, 0f, 1f);

                var score = 0.65f * sim + 0.20f * kw + 0.10f * recency + 0.05f * importance;
                score *= decayPenalty;

                return (k, score);
            })
            .OrderByDescending(t => t.score)
            .Take(Math.Max(1, query?.TopK ?? 8))
            .Select(t => t.k)
            .ToList();

        // Agentic output
        if (_thoughtProcessor != null)
        {
            var summary = $"Knowledge query completed for: '{qText}'. Top {scored.Count} results.";
            var resp = await _thoughtProcessor.ProcessThoughtAsync(
                summary, _memory, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Thinking
            );
        }

        return scored;
    }

    public async Task ConsolidateAsync(CancellationToken ct = default)
    {
        var all = SafeGetAllKnowledgeItems();
        var groups = all.GroupBy(i => Normalize(i.Text));
        foreach (var g in groups)
        {
            ct.ThrowIfCancellationRequested();
            var keep = g.OrderByDescending(x => x.Importance)
                        .ThenByDescending(x => x.CreatedAt)
                        .FirstOrDefault();
            if (keep == null) continue;

            foreach (var rem in g)
            {
                if (rem.Id == keep.Id) continue;
                TryRemoveByKey(BuildKey(rem.Id));
            }
        }

        if (_thoughtProcessor != null)
        {
            var resp = await _thoughtProcessor.ProcessThoughtAsync(
                "Knowledge consolidation complete.", _memory, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Happy
            );
        }
    }

    public async Task DecayAsync(TimeSpan halfLife, CancellationToken ct = default)
    {
        if (halfLife <= TimeSpan.Zero) return;

        var now = DateTime.UtcNow;
        var all = SafeGetAllKnowledgeItems();
        var ln2 = MathF.Log(2f);

        foreach (var item in all)
        {
            ct.ThrowIfCancellationRequested();
            var age = (float)(now - item.CreatedAt).TotalSeconds;
            var hl = (float)halfLife.TotalSeconds;
            var d = 1f - MathF.Exp(-ln2 * age / hl);
            var newDecay = Math.Clamp(d, 0f, 1f);

            if (Math.Abs(newDecay - item.Decay) > 0.001f)
            {
                item.Decay = newDecay;
                await Save(item);
            }
        }

        if (_thoughtProcessor != null)
        {
            var _ = await _thoughtProcessor.ProcessThoughtAsync(
                "Knowledge decay update complete.", _memory, null, new System.Collections.Concurrent.ConcurrentQueue<Thought>(), Mood.Neutral
            );
        }
    }

    // ----------------- Helpers -----------------

    private IEnumerable<KnowledgeItem> SafeGetAllKnowledgeItems()
    {
        var items = _memory.GetAllItems() ?? [];
        foreach (var m in items)
        {
            if (m == null || string.IsNullOrWhiteSpace(m.Key)) continue;
            if (!m.Key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrWhiteSpace(m.Value)) continue;

            KnowledgeItem? ki = null;
            try { ki = JsonSerializer.Deserialize<KnowledgeItem>(m.Value, JsonOpts); }
            catch { }
            if (ki != null) yield return ki;
        }
    }

    private static IEnumerable<KnowledgeItem> ApplyTagFilters(IEnumerable<KnowledgeItem> items, string[]? must, string[]? any)
    {
        var data = items;

        if (must is { Length: > 0 })
        {
            var set = new HashSet<string>(must.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.OrdinalIgnoreCase);
            data = data.Where(i => i.Tags != null && set.IsSubsetOf(i.Tags));
        }

        if (any is { Length: > 0 })
        {
            var set = new HashSet<string>(any.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.OrdinalIgnoreCase);
            data = data.Where(i => i.Tags != null && i.Tags.Any(t => set.Contains(t)));
        }

        return data;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        var sb = new StringBuilder(s.Length);
        bool lastWasSpace = false;

        foreach (var ch in s.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasSpace = false;
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
            }
        }

        return sb.ToString().Trim();
    }

    private async Task Save(KnowledgeItem item)
    {
        var key = BuildKey(item.Id);
        var json = JsonSerializer.Serialize(item, JsonOpts);
        await _memory.RememberAsync(key, json);
    }

    private void TryRemoveByKey(string key)
    {
        try
        {
            var item = _memory.GetAllItems()?.FirstOrDefault(mi => mi.Key == key);
            if (item != null && item.Id != Guid.Empty)
            {
                var t = _memory.GetType();
                var removeById = t.GetMethod("Remove", [typeof(Guid)]);
                if (removeById != null)
                {
                    _ = removeById.Invoke(_memory, [item.Id]);
                    return;
                }
            }
            var tk = _memory.GetType();
            var removeByKey = tk.GetMethod("Remove", [typeof(string)]);
            if (removeByKey != null)
            {
                _ = removeByKey.Invoke(_memory, [key]);
            }
        }
        catch { }
    }

    private static string BuildKey(Guid id) => $"{Prefix}{id:D}";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static float EstimateImportance(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0f;
        var len = Math.Min(text.Length, 400);
        var exclam = text.Count(c => c == '!' || c == '?');
        return Math.Clamp(0.2f + 0.0015f * len + 0.05f * Math.Min(4, exclam), 0f, 1f);
    }

    private static float KeywordScore(string doc, string query)
    {
        if (string.IsNullOrWhiteSpace(doc) || string.IsNullOrWhiteSpace(query)) return 0f;
        var q = Tokenize(query);
        var d = Tokenize(doc);
        if (q.Count == 0 || d.Count == 0) return 0f;

        var set = new HashSet<string>(d);
        var hit = q.Count(t => set.Contains(t));
        return Math.Clamp(hit / (float)Math.Max(1, q.Count), 0f, 1f);
    }

    private static float RecencyBoost(DateTime createdAt)
    {
        var hours = (float)(DateTime.UtcNow - createdAt).TotalHours;
        if (hours <= 1) return 1.0f;
        if (hours <= 24) return 0.7f;
        if (hours <= 168) return 0.4f;
        return 0.2f;
    }

    private static List<string> Tokenize(string s) =>
        [.. (s ?? "").ToLowerInvariant()
                 .Split([' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '/', '\"', '\'', '(', ')', '[', ']'],
                        StringSplitOptions.RemoveEmptyEntries)
                 .Select(t => t.Trim())
                 .Where(t => t.Length > 1)];

    private static float Cosine(float[] a, float[] b)
    {
        if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0f;
        var n = Math.Min(a.Length, b.Length);
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < n; i++)
        {
            var x = a[i];
            var y = b[i];
            dot += x * y;
            na += x * x;
            nb += y * y;
        }
        if (na <= 1e-9 || nb <= 1e-9) return 0f;
        return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb)));
    }
}
