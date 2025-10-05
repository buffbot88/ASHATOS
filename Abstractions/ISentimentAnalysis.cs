namespace Abstractions;

/// <summary>
/// Sentiment analysis interface for emotional intelligence capabilities
/// </summary>
public interface ISentimentAnalysis
{
    /// <summary>
    /// Analyzes the sentiment of the input text
    /// </summary>
    Task<SentimentResult> AnalyzeAsync(string text);
}

/// <summary>
/// Result of sentiment analysis
/// </summary>
public class SentimentResult
{
    public SentimentType Sentiment { get; set; } = SentimentType.Neutral;
    public double Score { get; set; }
    public double Confidence { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// Types of sentiment
/// </summary>
public enum SentimentType
{
    VeryNegative = -2,
    Negative = -1,
    Neutral = 0,
    Positive = 1,
    VeryPositive = 2
}
