namespace Abstractions;

public record ModuleResponse
{
    public string Text { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Language { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? Error { get; set; }
}
