using System.Security.Cryptography;
using System.Text;

namespace ASHATCore.Modules.Extensions.Knowledge;

public sealed class HashEmbeddingProvider(int dimension = 256) : IEmbeddingProvider
{
    private readonly int _dim = Math.Max(64, dimension);

    public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var vec = new float[_dim];
        if (string.IsNullOrWhiteSpace(text)) return Task.FromResult(vec);

        var tokens = text.ToLowerInvariant()
                         .Split([' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '/', '\"', '\'', '(', ')', '[', ']'],
                                StringSplitOptions.RemoveEmptyEntries)
                         .Select(t => t.Trim())
                         .Where(t => t.Length > 0);

        foreach (var t in tokens)
        {
            var h = HashToken(t);
            var idx = Math.Abs(h % _dim);
            vec[idx] += 1f;
        }

        // L2 normalize
        double sum = 0;
        for (int i = 0; i < _dim; i++) sum += vec[i] * vec[i];
        if (sum > 1e-9)
        {
            var inv = (float)(1.0 / Math.Sqrt(sum));
            for (int i = 0; i < _dim; i++) vec[i] *= inv;
        }

        return Task.FromResult(vec);
    }

    private static int HashToken(string s)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return BitConverter.ToInt32(bytes, 0);
    }
}
