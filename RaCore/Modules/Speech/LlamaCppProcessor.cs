using System.Diagnostics;

namespace RaCore.Modules.Speech;

public class LlamaCppProcessor(string exePath, string modelPath, int contextSize = 2048, int maxTokens = 128) : IDisposable
{
    private readonly string _exePath = exePath;
    private readonly string _modelPath = modelPath;
    private readonly int _contextSize = contextSize;
    private readonly int _maxTokens = maxTokens;

    public async Task<string> GenerateAsync(string prompt)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _exePath,
            Arguments = $"-m \"{_modelPath}\" -p \"{prompt}\" -n {_maxTokens} --ctx-size {_contextSize}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
        {
            output += $"\n(error log):\n{error}";
        }

        return output.Trim();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
