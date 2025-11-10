using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Services
{
    /// <summary>
    /// Service for building .NET projects and DLLs.
    /// </summary>
    public class BuildService
    {
        private readonly BuildConfiguration config;

        public event EventHandler<string>? OnStatusUpdate;
        public event EventHandler<string>? OnError;

        public BuildService(BuildConfiguration configuration)
        {
            config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Builds a C# code snippet into a DLL.
        /// </summary>
        public async Task<BuildResult> BuildCodeAsync(string code, string projectName)
        {
            var result = new BuildResult();
            var tempDir = Path.Combine(Path.GetTempPath(), $"RaStudios_Build_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(tempDir);
                RaiseStatusUpdate($"Building project {projectName}...");

                // Create project file
                var projectFile = Path.Combine(tempDir, $"{projectName}.csproj");
                var projectContent = GenerateProjectFile(projectName);
                await File.WriteAllTextAsync(projectFile, projectContent);

                // Create code file
                var codeFile = Path.Combine(tempDir, $"{projectName}.cs");
                await File.WriteAllTextAsync(codeFile, code);

                // Restore NuGet packages if enabled
                if (config.EnableNuGetRestore)
                {
                    RaiseStatusUpdate("Restoring NuGet packages...");
                    var restoreResult = await RunDotNetCommandAsync("restore", tempDir);
                    if (!restoreResult.Success)
                    {
                        result.Success = false;
                        result.ErrorOutput = restoreResult.ErrorOutput;
                        return result;
                    }
                }

                // Build project
                RaiseStatusUpdate("Compiling code...");
                var buildArgs = $"build \"{projectFile}\" -c {config.BuildConfigurationType}";
                var buildProcess = await RunDotNetCommandAsync(buildArgs, tempDir);

                result.Success = buildProcess.Success;
                result.Output = buildProcess.Output;
                result.ErrorOutput = buildProcess.ErrorOutput;

                if (result.Success)
                {
                    // Find built DLL
                    var outputDir = Path.Combine(tempDir, "bin", config.BuildConfigurationType, config.TargetFramework);
                    var dllPath = Path.Combine(outputDir, $"{projectName}.dll");

                    if (File.Exists(dllPath))
                    {
                        result.BuiltFiles = new[] { dllPath };
                        RaiseStatusUpdate($"Build succeeded. DLL created at: {dllPath}");
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorOutput = "DLL file not found after build.";
                        RaiseError(result.ErrorOutput);
                    }
                }
                else
                {
                    RaiseError($"Build failed: {result.ErrorOutput}");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorOutput = $"Build error: {ex.Message}";
                RaiseError(result.ErrorOutput);
                return result;
            }
            finally
            {
                // Cleanup temp directory
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Runs a dotnet command asynchronously.
        /// </summary>
        private async Task<BuildResult> RunDotNetCommandAsync(string arguments, string workingDirectory)
        {
            var result = new BuildResult();
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = config.DotNetPath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        RaiseStatusUpdate(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                result.Success = process.ExitCode == 0;
                result.Output = outputBuilder.ToString();
                result.ErrorOutput = errorBuilder.ToString();

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorOutput = $"Process execution error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Generates a basic .csproj file for building.
        /// </summary>
        private string GenerateProjectFile(string projectName)
        {
            return $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{config.TargetFramework}</TargetFramework>
    <OutputType>Library</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>";
        }

        private void RaiseStatusUpdate(string message)
        {
            OnStatusUpdate?.Invoke(this, message);
        }

        private void RaiseError(string message)
        {
            OnError?.Invoke(this, message);
        }
    }
}
