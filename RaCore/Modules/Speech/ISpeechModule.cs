using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Modules.Speech;

/// <summary>
/// Interface for AI speech modules capable of generating responses from input.
/// </summary>
public interface ISpeechModule
{
    /// <summary>
    /// Generates a natural language response to the specified input.
    /// </summary>
    Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default);
}
