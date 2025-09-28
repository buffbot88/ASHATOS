using System;
using System.Text.Json;

namespace RaAI.Modules.SafetyModule
{
    public static class RiskScorer
    {
        // Simple risk = severity * probability. Improve with context, PII detection, device states, time-of-day, etc.
        public static double ScoreStepRisk(string skillName, string argumentsJson, (HarmType harms, double severity) defaults, Func<string, double>? probabilityHeuristic = null)
        {
            double severity = Clamp01(defaults.severity);
            double probability = 0.2; // unknown baseline

            try
            {
                using var doc = JsonDocument.Parse(argumentsJson ?? "{}");
                var root = doc.RootElement;

                // Heuristics: if target looks like file delete or contains sensitive words, bump probability
                var text = root.GetRawText().ToLowerInvariant();
                if (text.Contains("delete") || text.Contains("\"action\":\"delete\"")) probability = Math.Max(probability, 0.6);
                if (text.Contains("password") || text.Contains("secret") || text.Contains("ssn")) probability = Math.Max(probability, 0.7);
                if (text.Contains("transfer") || text.Contains("payment")) probability = Math.Max(probability, 0.6);

                if (probabilityHeuristic != null) probability = Math.Max(probability, Clamp01(probabilityHeuristic(text)));
            }
            catch { }

            return Clamp01(severity * probability);
        }

        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
    }
}