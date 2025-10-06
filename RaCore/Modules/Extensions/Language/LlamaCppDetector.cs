using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaCore.Modules.Extensions.Language;

/// <summary>
/// Handles llama.cpp runtime detection and validation.
/// </summary>
public class LlamaCppDetector
{
    private readonly AILanguageModule _module;
    private string? _cachedLlamaCppPath;

    public LlamaCppDetector(AILanguageModule module)
    {
        _module = module;
    }

    /// <summary>
    /// Finds llama.cpp executable path by checking common installation locations.
    /// </summary>
    public string? FindLlamaCppExecutable()
    {
        if (_cachedLlamaCppPath != null)
        {
            return _cachedLlamaCppPath;
        }

        _module.LogInfoPublic("Starting llama.cpp executable auto-detection...");

        // Try local llama.cpp folder first (same directory as RaCore.exe server root)
        var serverRoot = Directory.GetCurrentDirectory();
        var localLlamaFolder = Path.Combine(serverRoot, "llama.cpp");
        
        // Build list of possible paths
        var possiblePaths = new List<string>();

        // Local paths - Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var localExecutables = new[]
            {
                "llama-cli.exe",
                "llama-server.exe", 
                "llama-run.exe",
                "main.exe"
            };

            foreach (var exe in localExecutables)
            {
                possiblePaths.Add(Path.Combine(localLlamaFolder, exe));
                possiblePaths.Add(Path.Combine(localLlamaFolder, "bin", exe));
            }
        }
        else
        {
            // Local paths - Linux/macOS
            var localExecutables = new[]
            {
                "llama-cli",
                "llama-server",
                "llama-run",
                "main"
            };

            foreach (var exe in localExecutables)
            {
                possiblePaths.Add(Path.Combine(localLlamaFolder, exe));
                possiblePaths.Add(Path.Combine(localLlamaFolder, "bin", exe));
            }
        }

        // Add executables that might be in PATH
        possiblePaths.Add("llama-cli");
        possiblePaths.Add("llama-server");
        possiblePaths.Add("llama-run");
        
        // Add Linux/macOS common paths
        possiblePaths.Add("/usr/bin/llama-cli");
        possiblePaths.Add("/usr/local/bin/llama-cli");
        possiblePaths.Add("/usr/bin/llama-server");
        possiblePaths.Add("/usr/local/bin/llama-server");

        // Add Windows-specific paths with multiple drive letters
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var driveLetters = new[] { "C", "D", "E", "F" };
            var llamaPaths = new[]
            {
                @"\llama.cpp\llama-cli.exe",
                @"\llama.cpp\llama-server.exe",
                @"\llama.cpp\llama-run.exe",
                @"\llama.cpp\main.exe",
                @"\llama.cpp\bin\llama-cli.exe",
                @"\llama.cpp\bin\llama-server.exe",
                @"\llama.cpp\bin\llama-run.exe",
                @"\llama.cpp\bin\main.exe",
                @"\Program Files\llama.cpp\llama-cli.exe",
                @"\Program Files\llama.cpp\llama-server.exe",
                @"\Program Files\llama.cpp\bin\llama-cli.exe",
                @"\Program Files\llama.cpp\bin\llama-server.exe"
            };
            
            foreach (var drive in driveLetters)
            {
                foreach (var llamaPath in llamaPaths)
                {
                    possiblePaths.Add($"{drive}:{llamaPath}");
                }
            }
        }

        _module.LogInfoPublic($"Checking {possiblePaths.Count} possible llama.cpp locations...");

        // Try each path
        foreach (var path in possiblePaths)
        {
            try
            {
                // Check if file exists for absolute paths
                if (Path.IsPathRooted(path) && !File.Exists(path))
                {
                    continue;
                }

                _module.LogInfoPublic($"Testing: {path}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    if (process.ExitCode == 0)
                    {
                        _cachedLlamaCppPath = path;
                        _module.LogInfoPublic($"✓ Found llama.cpp executable: {path}");
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log only for rooted paths that exist (skip PATH commands that don't exist)
                if (Path.IsPathRooted(path) && File.Exists(path))
                {
                    _module.LogInfoPublic($"  Failed to execute {path}: {ex.Message}");
                }
                continue;
            }
        }

        _module.LogWarnPublic("llama.cpp executable not found in any common location.");
        _module.LogInfoPublic("Searched locations include:");
        _module.LogInfoPublic("  - Local: ./llama.cpp/");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _module.LogInfoPublic("  - C:/llama.cpp/, D:/llama.cpp/, E:/llama.cpp/, F:/llama.cpp/");
            _module.LogInfoPublic("  - C:/Program Files/llama.cpp/");
        }
        else
        {
            _module.LogInfoPublic("  - /usr/bin/, /usr/local/bin/");
        }
        _module.LogInfoPublic("Use 'set-exe <path>' to manually configure the llama.cpp executable path.");

        return null;
    }

    /// <summary>
    /// Gets the version information from llama.cpp executable.
    /// </summary>
    public string GetLlamaCppVersion(string llamaCppPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = llamaCppPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit(5000);
                
                // Try to extract version from output
                var result = !string.IsNullOrWhiteSpace(output) ? output : error;
                var lines = result.Split('\n');
                return lines.Length > 0 ? lines[0].Trim() : "Unknown";
            }
        }
        catch { }
        
        return "Unknown";
    }

    /// <summary>
    /// Clears the cached llama.cpp path, forcing re-detection.
    /// </summary>
    public void ClearCache()
    {
        _cachedLlamaCppPath = null;
    }
}
