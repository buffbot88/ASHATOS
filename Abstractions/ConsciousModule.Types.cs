namespace Abstractions;

public enum Mood { Neutral, Thinking, Speaking, Confused, Happy }

public sealed class Thought
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = "user";
    public double Score { get; internal set; }
    public override string ToString() => $"#{Id} [{Timestamp:HH:mm:ss}] {Source}: {Content}";
}

public static class ConsciousConfig
{
    public static int FeatureVectorSize { get; set; } = 128;
    public static double LearningRate { get; set; } = 0.01;
    public static int AssociationLimit { get; set; } = 8;
    public static int ThoughtHistoryLimit { get; set; } = 200;
}
