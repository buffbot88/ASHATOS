using Abstractions;
using LegendaryClientBuilder.Core;

namespace LegendaryClientBuilder.Builders;

/// <summary>
/// Base class for platform-specific client builders.
/// </summary>
public abstract class ClientBuilderBase
{
    protected readonly string OutputPath;

    protected ClientBuilderBase(string outputPath)
    {
        OutputPath = outputPath;
        Directory.CreateDirectory(outputPath);
    }

    /// <summary>
    /// Generate a client package.
    /// </summary>
    public abstract Task<string> GenerateAsync(
        GameClientPackage package,
        ClientTemplate? template = null);

    /// <summary>
    /// Get the size of a directory in bytes.
    /// </summary>
    protected long GetDirectorySize(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }

    /// <summary>
    /// Create a README file for the client.
    /// </summary>
    protected async Task CreateReadmeAsync(string clientDir, GameClientPackage package)
    {
        var config = package.Configuration;
        var readme = $@"# {config.GameTitle} - Game Client

Generated for License: {package.LicenseKey}
Platform: {package.Platform}
Created: {package.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC

## How to Use

1. Open index.html in a web browser
2. Click 'Connect' to connect to the game server
3. The client will authenticate with your license key
4. Use keyboard controls to interact with the game

## Configuration

- Server: {config.ServerUrl}:{config.ServerPort}
- Theme: {config.Theme}
- License: {package.LicenseKey}

## Support

For support, please contact your server administrator.

---
Powered by Legendary Client Builder v9.1.0
RaCore Game Client - Locally Hosted
";
        await File.WriteAllTextAsync(Path.Combine(clientDir, "README.md"), readme);
    }
}
