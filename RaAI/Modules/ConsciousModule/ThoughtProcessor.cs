using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RaAI.Modules.ConsciousModule
{
    // Simulated "neural network": hashed token features and tunable weights.
    public class ThoughtProcessor(int vectorSize = 1024, double learningRate = 0.05)
    {
        private readonly int _vectorSize = Math.Max(64, vectorSize);
        private readonly Random _rnd = new();
        private readonly ConcurrentDictionary<int, double> _weights = new();
        private int _thoughtCounter = 0;

        // Add this method to generate unique thought IDs
        public int GenerateThoughtId()
        {
            return Interlocked.Increment(ref _thoughtCounter);
        }

        // Process a thought combining memory signals and subconscious signals; returns a human-friendly textual result.
        public string Process(Thought t, IEnumerable<string> memorySignals, IEnumerable<string> subconsciousSignals, int associationLimit = 5)
        {
            var features = Vectorize(t.Content);
            var memFeatures = Vectorize(string.Join(" ", memorySignals ?? []));
            var subFeatures = Vectorize(string.Join(" ", subconsciousSignals ?? []));

            // Combine features: simple addition with weights
            var score = ScoreForFeatures(features) * 1.0
                      + ScoreForFeatures(memFeatures) * 0.8
                      + ScoreForFeatures(subFeatures) * 0.6;

            t.Score = score;

            // generate associations: pick tokens and attach adjectives
            var tokens = Tokenize(t.Content).Where(s => s.Length > 0).Distinct().ToArray();
            var adjectives = new[] { "bright", "distant", "warm", "strange", "familiar", "urgent", "calm", "noisy", "quiet" };

            var picks = new List<string>();
            for (int i = 0; i < Math.Min(associationLimit, Math.Max(1, tokens.Length)); i++)
            {
                var token = tokens[_rnd.Next(tokens.Length)];
                var adj = adjectives[_rnd.Next(adjectives.Length)];
                picks.Add($"{token}-{adj}");
            }

            // small synthetic narrative
            var sb = new StringBuilder();
            sb.Append($"Thought #{t.Id}: I reflected on \"{TrimTo(t.Content, 120)}\". ");
            sb.Append($"Score={score:F2}. ");
            if (picks.Count > 0) sb.Append($"Associations: {string.Join(", ", picks)}. ");
            // Replace this line:
            // if (!string.IsNullOrWhiteSpace(string.Join(" ", values: memorySignals)))

            // With this null-safe version:
            if (memorySignals != null && !string.IsNullOrWhiteSpace(string.Join(" ", memorySignals)))
                sb.Append("Memory cues influenced the thought. ");
            if (subconsciousSignals != null && !string.IsNullOrWhiteSpace(string.Join(" ", subconsciousSignals)))
                sb.Append("Subconscious echoes detected. ");

            return sb.ToString().Trim();
        }

        // Apply a simple reinforcement signal: positive reward boosts weights for features present in this thought,
        // negative reward reduces them.
        public void Train(Thought t, double reward)
        {
            var features = Vectorize(t.Content);
            foreach (var idx in features)
            {
                _weights.AddOrUpdate(idx, reward * learningRate, (k, old) => old + reward * learningRate);
            }
        }

        // Convert text into a set of hashed feature indices
        private int[] Vectorize(string text)
        {
            var tokens = Tokenize(text);
            var set = new HashSet<int>();
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                int h = Math.Abs(token.GetHashCode()) % _vectorSize;
                set.Add(h);
                // create a few n-gram-ish variations
                for (int i = 1; i <= Math.Min(2, token.Length); i++)
                {
                    var sub = token[..i];
                    int h2 = (Math.Abs((token + sub).GetHashCode()) + 13) % _vectorSize;
                    set.Add(h2);
                }
            }
            return [.. set];
        }

        private double ScoreForFeatures(int[] features)
        {
            double s = 0.0;
            foreach (var f in features)
            {
                if (_weights.TryGetValue(f, out var w)) s += w;
            }
            // small sigmoid-ish normalization
            return Math.Tanh(s);
        }

        private static string[] Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return [];
            return [.. text
                .ToLowerInvariant()
                .Split([' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '/'], StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())];
        }

        private static string TrimTo(string s, int len) => s?.Length > len ? s[..(len - 3)] + "..." : s ?? string.Empty;
    }
}