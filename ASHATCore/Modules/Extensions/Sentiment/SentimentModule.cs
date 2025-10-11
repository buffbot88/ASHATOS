using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Sentiment;

/// <summary>
/// Sentiment analysis module for emotional intelligence
/// Implements basic rule-based sentiment analysis with keyword matching
/// </summary>
[RaModule(Category = "extensions")]
public sealed class SentimentModule : ModuleBase, ISentimentAnalysis
{
    public override string Name => "Sentiment";

    private static readonly Dictionary<string, (SentimentType type, double weight)> _sentimentKeywords = new()
    {
        // Very Positive
        { "excellent", (SentimentType.VeryPositive, 2.0) },
        { "amazing", (SentimentType.VeryPositive, 2.0) },
        { "wonderful", (SentimentType.VeryPositive, 2.0) },
        { "fantastic", (SentimentType.VeryPositive, 2.0) },
        { "perfect", (SentimentType.VeryPositive, 2.0) },
        { "love", (SentimentType.VeryPositive, 1.8) },
        
        // Positive
        { "good", (SentimentType.Positive, 1.0) },
        { "great", (SentimentType.Positive, 1.2) },
        { "nice", (SentimentType.Positive, 1.0) },
        { "happy", (SentimentType.Positive, 1.2) },
        { "thanks", (SentimentType.Positive, 1.0) },
        { "thank you", (SentimentType.Positive, 1.0) },
        { "helpful", (SentimentType.Positive, 1.0) },
        { "better", (SentimentType.Positive, 0.8) },
        
        // Negative
        { "bad", (SentimentType.Negative, -1.0) },
        { "poor", (SentimentType.Negative, -1.0) },
        { "wrong", (SentimentType.Negative, -1.0) },
        { "error", (SentimentType.Negative, -0.8) },
        { "problem", (SentimentType.Negative, -0.8) },
        { "issue", (SentimentType.Negative, -0.7) },
        { "fail", (SentimentType.Negative, -1.2) },
        
        // Very Negative
        { "terrible", (SentimentType.VeryNegative, -2.0) },
        { "awful", (SentimentType.VeryNegative, -2.0) },
        { "horrible", (SentimentType.VeryNegative, -2.0) },
        { "hate", (SentimentType.VeryNegative, -2.0) },
        { "worst", (SentimentType.VeryNegative, -2.0) },
        { "disaster", (SentimentType.VeryNegative, -1.8) }
    };

    public override string Process(string input)
    {
        var result = AnalyzeAsync(input).GetAwaiter().GetResult();
        return $"Sentiment: {result.Sentiment} (Score: {result.Score:F2}, Confidence: {result.Confidence:F2})";
    }

    public async Task<SentimentResult> AnalyzeAsync(string text)
    {
        await Task.CompletedTask; // Async placeholder for future enhancements
        
        if (string.IsNullOrWhiteSpace(text))
        {
            return new SentimentResult
            {
                Sentiment = SentimentType.Neutral,
                Score = 0,
                Confidence = 1.0,
                Explanation = "Empty input"
            };
        }

        var lowerText = text.ToLowerInvariant();
        double totalScore = 0;
        int matches = 0;
        var matchedKeywords = new List<string>();

        foreach (var kvp in _sentimentKeywords)
        {
            if (lowerText.Contains(kvp.Key))
            {
                totalScore += kvp.Value.weight;
                matches++;
                matchedKeywords.Add(kvp.Key);
            }
        }

        // Normalize score
        double normalizedScore = matches > 0 ? totalScore / matches : 0;
        
        // Determine sentiment type
        SentimentType sentiment;
        if (normalizedScore >= 1.5) sentiment = SentimentType.VeryPositive;
        else if (normalizedScore >= 0.3) sentiment = SentimentType.Positive;
        else if (normalizedScore <= -1.5) sentiment = SentimentType.VeryNegative;
        else if (normalizedScore <= -0.3) sentiment = SentimentType.Negative;
        else sentiment = SentimentType.Neutral;

        // Calculate confidence based on number of matches
        double confidence = Math.Min(0.5 + (matches * 0.1), 1.0);

        return new SentimentResult
        {
            Sentiment = sentiment,
            Score = normalizedScore,
            Confidence = confidence,
            Explanation = matches > 0 
                ? $"Detected keywords: {string.Join(", ", matchedKeywords)}"
                : "No sentiment keywords detected"
        };
    }
}
