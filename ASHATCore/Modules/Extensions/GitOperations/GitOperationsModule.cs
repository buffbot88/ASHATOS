using System.Diagnostics;
using System.Text;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.GitOperations;

/// <summary>
/// Git Operations Module - Enables ASHAT to perform Git operations
/// Provides version control capabilities for code management
/// </summary>
[RaModule(Category = "extensions")]
public sealed class GitOperationsModule : ModuleBase
{
    public override string Name => "GitOperations";

    private string _workingDirectory;
    private readonly List<GitOperation> _operationHistory = new();

    public GitOperationsModule()
    {
        _workingDirectory = AppContext.BaseDirectory;
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("Git Operations module initialized");
        LogInfo($"Working directory: {_workingDirectory}");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("git status", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteGitCommand("status");
        }

        if (text.StartsWith("git branch", StringComparison.OrdinalIgnoreCase))
        {
            var args = text["git branch".Length..].Trim();
            return ExecuteGitCommand($"branch {args}");
        }

        if (text.StartsWith("git checkout ", StringComparison.OrdinalIgnoreCase))
        {
            var branch = text["git checkout ".Length..].Trim();
            return ExecuteGitCommand($"checkout {branch}");
        }

        if (text.StartsWith("git add ", StringComparison.OrdinalIgnoreCase))
        {
            var files = text["git add ".Length..].Trim();
            return ExecuteGitCommand($"add {files}");
        }

        if (text.StartsWith("git commit ", StringComparison.OrdinalIgnoreCase))
        {
            var message = text["git commit ".Length..].Trim();
            return ExecuteGitCommand($"commit {message}");
        }

        if (text.StartsWith("git push", StringComparison.OrdinalIgnoreCase))
        {
            var args = text["git push".Length..].Trim();
            return ExecuteGitCommand($"push {args}");
        }

        if (text.StartsWith("git pull", StringComparison.OrdinalIgnoreCase))
        {
            var args = text["git pull".Length..].Trim();
            return ExecuteGitCommand($"pull {args}");
        }

        if (text.StartsWith("git diff", StringComparison.OrdinalIgnoreCase))
        {
            var args = text["git diff".Length..].Trim();
            return ExecuteGitCommand($"diff {args}");
        }

        if (text.StartsWith("git log", StringComparison.OrdinalIgnoreCase))
        {
            var args = text["git log".Length..].Trim();
            if (string.IsNullOrEmpty(args))
            {
                args = "--oneline -10";
            }
            return ExecuteGitCommand($"log {args}");
        }

        if (text.StartsWith("git setdir ", StringComparison.OrdinalIgnoreCase))
        {
            var dir = text["git setdir ".Length..].Trim();
            return SetWorkingDirectory(dir);
        }

        if (text.Equals("git history", StringComparison.OrdinalIgnoreCase))
        {
            return GetOperationHistory();
        }

        if (text.Equals("git info", StringComparison.OrdinalIgnoreCase))
        {
            return GetGitInfo();
        }

        return "Unknown git command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Git Operations commands:",
            "  git status                    - Show working tree status",
            "  git branch [options]          - List, create, or delete branches",
            "  git checkout <branch>         - Switch to a branch",
            "  git add <files>               - Add files to staging area",
            "  git commit -m \"message\"      - Commit staged changes",
            "  git push [remote] [branch]    - Push commits to remote",
            "  git pull [remote] [branch]    - Pull changes from remote",
            "  git diff [options]            - Show changes",
            "  git log [options]             - Show commit history",
            "  git setdir <path>             - Set working directory",
            "  git history                   - Show operation history",
            "  git info                      - Show repository information",
            "  help                          - Show this help message",
            "",
            "Examples:",
            "  git status",
            "  git branch feature/new-feature",
            "  git checkout feature/new-feature",
            "  git add .",
            "  git commit -m \"Add new feature\"",
            "  git push origin feature/new-feature"
        );
    }

    private string ExecuteGitCommand(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var output = new StringBuilder();
            var errors = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errors.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            var operation = new GitOperation
            {
                Command = arguments,
                Timestamp = DateTime.UtcNow,
                ExitCode = process.ExitCode,
                Success = process.ExitCode == 0
            };
            _operationHistory.Add(operation);

            var result = new StringBuilder();
            result.AppendLine($"Git command: {arguments}");
            result.AppendLine($"Exit code: {process.ExitCode}");
            result.AppendLine();

            if (output.Length > 0)
            {
                result.AppendLine("Output:");
                result.AppendLine(output.ToString());
            }

            if (errors.Length > 0)
            {
                result.AppendLine("Errors:");
                result.AppendLine(errors.ToString());
            }

            if (process.ExitCode == 0)
            {
                LogInfo($"Git command succeeded: {arguments}");
                return $"✅ {result}";
            }
            else
            {
                LogInfo($"Git command failed: {arguments} (exit code: {process.ExitCode})");
                return $"❌ {result}";
            }
        }
        catch (Exception ex)
        {
            LogError($"Error executing git command: {ex.Message}");
            return $"❌ Error executing git command: {ex.Message}";
        }
    }

    private string SetWorkingDirectory(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return $"❌ Directory not found: {directory}";
            }

            _workingDirectory = directory;
            LogInfo($"Working directory changed to: {directory}");
            
            return string.Join(Environment.NewLine,
                $"✅ Working directory set to:",
                $"   {directory}",
                "",
                "Git info:",
                GetGitInfo()
            );
        }
        catch (Exception ex)
        {
            LogError($"Error setting working directory: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string GetOperationHistory()
    {
        if (_operationHistory.Count == 0)
        {
            return "No git operations performed yet.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Git Operation History:");
        sb.AppendLine();

        foreach (var op in _operationHistory.TakeLast(20))
        {
            var status = op.Success ? "✅" : "❌";
            sb.AppendLine($"{status} {op.Timestamp:yyyy-MM-dd HH:mm:ss} UTC - git {op.Command}");
        }

        if (_operationHistory.Count > 20)
        {
            sb.AppendLine();
            sb.AppendLine($"... and {_operationHistory.Count - 20} more operations");
        }

        return sb.ToString();
    }

    private string GetGitInfo()
    {
        var sb = new StringBuilder();
        
        // Get current branch
        var branchResult = ExecuteGitCommandSilent("rev-parse --abbrev-ref HEAD");
        
        // Get remote URL
        var remoteResult = ExecuteGitCommandSilent("config --get remote.origin.url");
        
        // Get last commit
        var commitResult = ExecuteGitCommandSilent("log -1 --oneline");

        sb.AppendLine("Repository Information:");
        sb.AppendLine($"Working directory: {_workingDirectory}");
        
        if (!string.IsNullOrWhiteSpace(branchResult))
        {
            sb.AppendLine($"Current branch: {branchResult.Trim()}");
        }
        
        if (!string.IsNullOrWhiteSpace(remoteResult))
        {
            sb.AppendLine($"Remote: {remoteResult.Trim()}");
        }
        
        if (!string.IsNullOrWhiteSpace(commitResult))
        {
            sb.AppendLine($"Last commit: {commitResult.Trim()}");
        }

        return sb.ToString();
    }

    private string ExecuteGitCommandSilent(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null) return string.Empty;
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return process.ExitCode == 0 ? output : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private class GitOperation
    {
        public string Command { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int ExitCode { get; set; }
        public bool Success { get; set; }
    }
}
