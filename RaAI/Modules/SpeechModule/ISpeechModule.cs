using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SpeechModule
{
    public interface ISpeechModule
    {
        Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default);
    }
}