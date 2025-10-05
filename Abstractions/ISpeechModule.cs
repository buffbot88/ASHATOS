namespace Abstractions;

public interface ISpeechModule
{
    Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default);
}
