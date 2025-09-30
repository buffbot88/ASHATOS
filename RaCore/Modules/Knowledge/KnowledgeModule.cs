using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Modules.Knowledge;


public sealed class KnowledgeQuery
{
    public string Text { get; set; } = "";
    public int TopK { get; set; } = 8;
    public string[]? MustTags { get; set; }
    public string[]? AnyTags { get; set; }
}

public interface IEmbeddingProvider
{
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
}
