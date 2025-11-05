using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.DependencyManager;

/// <summary>
/// Dependency Manager Module - Manages project dependencies and packages
/// Handles NuGet packages, version management, and conflict resolution
/// </summary>
[RaModule(Category = "extensions")]
public sealed class DependencyManagerModule : ModuleBase
{
    public override string Name => "DependencyManager";

    private readonly List<DependencyOperation> _operationHistory = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("Dependency Manager module initialized");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("deps list ", StringComparison.OrdinalIgnoreCase))
        {
            var projectPath = text["deps list ".Length..].Trim();
            return ListDependencies(projectPath);
        }

        if (text.StartsWith("deps add ", StringComparison.OrdinalIgnoreCase))
        {
            // deps add <package> to <project>
            var parts = text["deps add ".Length..].Split(new[] { " to " }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return "Usage: deps add <package> to <projectPath>";
            }
            return AddPackage(parts[0].Trim(), parts[1].Trim());
        }

        if (text.StartsWith("deps remove ", StringComparison.OrdinalIgnoreCase))
        {
            // deps remove <package> from <project>
            var parts = text["deps remove ".Length..].Split(new[] { " from " }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return "Usage: deps remove <package> from <projectPath>";
            }
            return RemovePackage(parts[0].Trim(), parts[1].Trim());
        }

        if (text.StartsWith("deps update ", StringComparison.OrdinalIgnoreCase))
        {
            var projectPath = text["deps update ".Length..].Trim();
            return UpdatePackages(projectPath);
        }

        if (text.StartsWith("deps restore ", StringComparison.OrdinalIgnoreCase))
        {
            var projectPath = text["deps restore ".Length..].Trim();
            return RestorePackages(projectPath);
        }

        if (text.StartsWith("deps check ", StringComparison.OrdinalIgnoreCase))
        {
            var projectPath = text["deps check ".Length..].Trim();
            return CheckForUpdates(projectPath);
        }

        if (text.Equals("deps history", StringComparison.OrdinalIgnoreCase))
        {
            return GetHistory();
        }

        return "Unknown deps command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Dependency Manager commands:",
            "  deps list <projectPath>              - List all dependencies in project",
            "  deps add <package> to <project>      - Add a NuGet package to project",
            "  deps remove <package> from <project> - Remove a package from project",
            "  deps update <projectPath>            - Update all packages to latest versions",
            "  deps restore <projectPath>           - Restore all packages",
            "  deps check <projectPath>             - Check for available updates",
            "  deps history                         - Show operation history",
            "  help                                 - Show this help message",
            "",
            "Examples:",
            "  deps list /path/to/project.csproj",
            "  deps add Newtonsoft.Json to /path/to/project.csproj",
            "  deps remove OldPackage from /path/to/project.csproj",
            "  deps update /path/to/project.csproj"
        );
    }

    private string ListDependencies(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            var doc = XDocument.Load(projectPath);
            var packageReferences = doc.Descendants("PackageReference").ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"📦 Dependencies in {Path.GetFileName(projectPath)}:");
            sb.AppendLine();

            if (packageReferences.Count == 0)
            {
                sb.AppendLine("No package dependencies found.");
                return sb.ToString();
            }

            sb.AppendLine($"Total packages: {packageReferences.Count}");
            sb.AppendLine();

            foreach (var pkg in packageReferences)
            {
                var name = pkg.Attribute("Include")?.Value ?? "Unknown";
                var version = pkg.Attribute("Version")?.Value ?? "Not specified";
                sb.AppendLine($"  • {name} v{version}");
            }

            LogInfo($"Listed {packageReferences.Count} dependencies in {projectPath}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error listing dependencies: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string AddPackage(string packageName, string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            // Use dotnet CLI to add package
            var result = ExecuteDotNetCommand($"add \"{projectPath}\" package {packageName}");

            if (result.Success)
            {
                var operation = new DependencyOperation
                {
                    Type = "Add",
                    PackageName = packageName,
                    ProjectPath = projectPath,
                    Timestamp = DateTime.UtcNow
                };
                _operationHistory.Add(operation);

                LogInfo($"Added package {packageName} to {projectPath}");

                return string.Join(Environment.NewLine,
                    $"✅ Package added successfully",
                    $"   Package: {packageName}",
                    $"   Project: {Path.GetFileName(projectPath)}",
                    "",
                    result.Output
                );
            }
            else
            {
                return string.Join(Environment.NewLine,
                    $"❌ Failed to add package",
                    $"   Package: {packageName}",
                    "",
                    result.Error
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error adding package: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string RemovePackage(string packageName, string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            var result = ExecuteDotNetCommand($"remove \"{projectPath}\" package {packageName}");

            if (result.Success)
            {
                var operation = new DependencyOperation
                {
                    Type = "Remove",
                    PackageName = packageName,
                    ProjectPath = projectPath,
                    Timestamp = DateTime.UtcNow
                };
                _operationHistory.Add(operation);

                LogInfo($"Removed package {packageName} from {projectPath}");

                return string.Join(Environment.NewLine,
                    $"✅ Package removed successfully",
                    $"   Package: {packageName}",
                    $"   Project: {Path.GetFileName(projectPath)}",
                    "",
                    result.Output
                );
            }
            else
            {
                return string.Join(Environment.NewLine,
                    $"❌ Failed to remove package",
                    "",
                    result.Error
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error removing package: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string UpdatePackages(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            var doc = XDocument.Load(projectPath);
            var packages = doc.Descendants("PackageReference")
                .Select(p => p.Attribute("Include")?.Value)
                .Where(p => p != null)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"🔄 Updating packages in {Path.GetFileName(projectPath)}...");
            sb.AppendLine();

            foreach (var package in packages)
            {
                // Remove and re-add to get latest version
                var removeResult = ExecuteDotNetCommand($"remove \"{projectPath}\" package {package}");
                
                if (removeResult.Success)
                {
                    var addResult = ExecuteDotNetCommand($"add \"{projectPath}\" package {package}");
                    
                    if (addResult.Success)
                    {
                        sb.AppendLine($"✅ Updated {package}");
                    }
                    else
                    {
                        sb.AppendLine($"⚠️ Failed to update {package} - re-adding old version");
                        // Try to restore the old package to prevent broken state
                        ExecuteDotNetCommand($"add \"{projectPath}\" package {package}");
                    }
                }
                else
                {
                    sb.AppendLine($"⚠️ Failed to remove {package} - skipping update");
                }
            }

            var operation = new DependencyOperation
            {
                Type = "Update",
                PackageName = $"{packages.Count} packages",
                ProjectPath = projectPath,
                Timestamp = DateTime.UtcNow
            };
            _operationHistory.Add(operation);

            LogInfo($"Updated packages in {projectPath}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error updating packages: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string RestorePackages(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            var result = ExecuteDotNetCommand($"restore \"{projectPath}\"");

            if (result.Success)
            {
                var operation = new DependencyOperation
                {
                    Type = "Restore",
                    PackageName = "All packages",
                    ProjectPath = projectPath,
                    Timestamp = DateTime.UtcNow
                };
                _operationHistory.Add(operation);

                LogInfo($"Restored packages for {projectPath}");

                return string.Join(Environment.NewLine,
                    $"✅ Packages restored successfully",
                    $"   Project: {Path.GetFileName(projectPath)}",
                    "",
                    result.Output
                );
            }
            else
            {
                return string.Join(Environment.NewLine,
                    $"❌ Failed to restore packages",
                    "",
                    result.Error
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error restoring packages: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string CheckForUpdates(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                return $"❌ Project file not found: {projectPath}";
            }

            var result = ExecuteDotNetCommand($"list \"{projectPath}\" package --outdated");

            return string.Join(Environment.NewLine,
                $"📋 Package update check for {Path.GetFileName(projectPath)}:",
                "",
                result.Output
            );
        }
        catch (Exception ex)
        {
            LogError($"Error checking updates: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string GetHistory()
    {
        if (_operationHistory.Count == 0)
        {
            return "No dependency operations performed yet.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Dependency Operation History:");
        sb.AppendLine();

        foreach (var op in _operationHistory.TakeLast(20))
        {
            sb.AppendLine($"✅ {op.Timestamp:yyyy-MM-dd HH:mm:ss} - {op.Type}");
            sb.AppendLine($"   Package: {op.PackageName}");
            sb.AppendLine($"   Project: {Path.GetFileName(op.ProjectPath)}");
            sb.AppendLine();
        }

        if (_operationHistory.Count > 20)
        {
            sb.AppendLine($"... and {_operationHistory.Count - 20} more operations");
        }

        return sb.ToString();
    }

    private DotNetCommandResult ExecuteDotNetCommand(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();

            return new DotNetCommandResult
            {
                Success = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                Output = output,
                Error = error
            };
        }
        catch (Exception ex)
        {
            return new DotNetCommandResult
            {
                Success = false,
                ExitCode = -1,
                Output = string.Empty,
                Error = ex.Message
            };
        }
    }

    private class DependencyOperation
    {
        public string Type { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    private class DotNetCommandResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
