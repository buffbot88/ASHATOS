using System.Text;
using System.Text.RegularExpressions;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.FileOperations;

/// <summary>
/// File Operations Module - Enables ASHAT to read, write, and edit files safely
/// Provides backup/restore capabilities and comprehensive file management
/// </summary>
[RaModule(Category = "extensions")]
public sealed class FileOperationsModule : ModuleBase
{
    public override string Name => "FileOperations";

    private readonly string _backupPath;
    private readonly Dictionary<string, FileBackup> _backups = new();
    private readonly object _lockObj = new();

    public FileOperationsModule()
    {
        _backupPath = Path.Combine(AppContext.BaseDirectory, "Backups", "FileOperations");
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }

        LogInfo("File Operations module initialized");
        LogInfo($"Backup path: {_backupPath}");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("fileops read ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["fileops read ".Length..].Trim();
            return ReadFile(path);
        }

        if (text.StartsWith("fileops write ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text["fileops write ".Length..].Split(new[] { " >> " }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return "Usage: fileops write <path> >> <content>";
            }
            return WriteFile(parts[0].Trim(), parts[1]);
        }

        if (text.StartsWith("fileops append ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text["fileops append ".Length..].Split(new[] { " >> " }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return "Usage: fileops append <path> >> <content>";
            }
            return AppendToFile(parts[0].Trim(), parts[1]);
        }

        if (text.StartsWith("fileops replace ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text["fileops replace ".Length..].Split(new[] { " in " }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return "Usage: fileops replace <old_text> in <path>";
            }
            var oldNew = parts[0].Split(new[] { " with " }, StringSplitOptions.None);
            if (oldNew.Length != 2)
            {
                return "Usage: fileops replace <old_text> with <new_text> in <path>";
            }
            return ReplaceInFile(parts[1].Trim(), oldNew[0].Trim(), oldNew[1].Trim());
        }

        if (text.StartsWith("fileops backup ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["fileops backup ".Length..].Trim();
            return BackupFile(path);
        }

        if (text.StartsWith("fileops restore ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["fileops restore ".Length..].Trim();
            return RestoreFile(path);
        }

        if (text.StartsWith("fileops exists ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["fileops exists ".Length..].Trim();
            return FileExists(path);
        }

        if (text.StartsWith("fileops list ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["fileops list ".Length..].Trim();
            return ListFiles(path);
        }

        if (text.Equals("fileops status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatus();
        }

        return "Unknown fileops command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "File Operations commands:",
            "  fileops read <path>                           - Read file contents",
            "  fileops write <path> >> <content>             - Write content to file (overwrites)",
            "  fileops append <path> >> <content>            - Append content to file",
            "  fileops replace <old> with <new> in <path>    - Replace text in file",
            "  fileops backup <path>                         - Create backup of file",
            "  fileops restore <path>                        - Restore file from backup",
            "  fileops exists <path>                         - Check if file exists",
            "  fileops list <directory>                      - List files in directory",
            "  fileops status                                - Show module status",
            "  help                                          - Show this help message",
            "",
            "Examples:",
            "  fileops read /path/to/file.cs",
            "  fileops write /path/to/file.txt >> Hello World",
            "  fileops replace oldMethod with newMethod in /path/to/code.cs"
        );
    }

    private string ReadFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            var lines = content.Split('\n').Length;
            var size = new FileInfo(path).Length;

            return string.Join(Environment.NewLine,
                $"📄 File: {path}",
                $"   Lines: {lines}",
                $"   Size: {FormatBytes(size)}",
                "",
                "Content:",
                "─────────────────────────────────────",
                content,
                "─────────────────────────────────────"
            );
        }
        catch (Exception ex)
        {
            LogError($"Error reading file: {ex.Message}");
            return $"❌ Error reading file: {ex.Message}";
        }
    }

    private string WriteFile(string path, string content)
    {
        try
        {
            lock (_lockObj)
            {
                // Backup existing file if it exists
                if (File.Exists(path))
                {
                    BackupFile(path);
                }

                File.WriteAllText(path, content);
                var size = new FileInfo(path).Length;

                LogInfo($"File written: {path}");
                return string.Join(Environment.NewLine,
                    $"✅ File written: {path}",
                    $"   Size: {FormatBytes(size)}",
                    $"   Backup created: {_backups.ContainsKey(path)}"
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error writing file: {ex.Message}");
            return $"❌ Error writing file: {ex.Message}";
        }
    }

    private string AppendToFile(string path, string content)
    {
        try
        {
            lock (_lockObj)
            {
                // Backup existing file if it exists
                if (File.Exists(path))
                {
                    BackupFile(path);
                }

                File.AppendAllText(path, content);
                var size = new FileInfo(path).Length;

                LogInfo($"Content appended to file: {path}");
                return string.Join(Environment.NewLine,
                    $"✅ Content appended: {path}",
                    $"   New size: {FormatBytes(size)}"
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error appending to file: {ex.Message}");
            return $"❌ Error appending to file: {ex.Message}";
        }
    }

    private string ReplaceInFile(string path, string oldText, string newText)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            lock (_lockObj)
            {
                // Backup before modification
                BackupFile(path);

                var content = File.ReadAllText(path);
                var occurrences = Regex.Matches(content, Regex.Escape(oldText)).Count;

                if (occurrences == 0)
                {
                    return $"⚠️ Text not found in file: '{oldText}'";
                }

                var newContent = content.Replace(oldText, newText);
                File.WriteAllText(path, newContent);

                LogInfo($"Replaced {occurrences} occurrences in: {path}");
                return string.Join(Environment.NewLine,
                    $"✅ Replacement successful: {path}",
                    $"   Occurrences replaced: {occurrences}",
                    $"   Old text: '{oldText}'",
                    $"   New text: '{newText}'"
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error replacing in file: {ex.Message}");
            return $"❌ Error replacing in file: {ex.Message}";
        }
    }

    private string BackupFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            lock (_lockObj)
            {
                var content = File.ReadAllText(path);
                var timestamp = DateTime.UtcNow;
                var backupId = Guid.NewGuid().ToString("N")[..8];
                
                var backup = new FileBackup
                {
                    OriginalPath = path,
                    Content = content,
                    Timestamp = timestamp,
                    BackupId = backupId
                };

                _backups[path] = backup;

                // Also save to disk
                var backupFileName = $"{Path.GetFileName(path)}.{backupId}.backup";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);
                File.WriteAllText(backupFilePath, content);

                LogInfo($"File backed up: {path}");
                return string.Join(Environment.NewLine,
                    $"✅ Backup created: {path}",
                    $"   Backup ID: {backupId}",
                    $"   Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss} UTC"
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error backing up file: {ex.Message}");
            return $"❌ Error backing up file: {ex.Message}";
        }
    }

    private string RestoreFile(string path)
    {
        try
        {
            lock (_lockObj)
            {
                if (!_backups.ContainsKey(path))
                {
                    return $"❌ No backup found for: {path}";
                }

                var backup = _backups[path];
                File.WriteAllText(path, backup.Content);

                LogInfo($"File restored from backup: {path}");
                return string.Join(Environment.NewLine,
                    $"✅ File restored: {path}",
                    $"   Backup ID: {backup.BackupId}",
                    $"   Backup time: {backup.Timestamp:yyyy-MM-dd HH:mm:ss} UTC"
                );
            }
        }
        catch (Exception ex)
        {
            LogError($"Error restoring file: {ex.Message}");
            return $"❌ Error restoring file: {ex.Message}";
        }
    }

    private string FileExists(string path)
    {
        var exists = File.Exists(path);
        if (exists)
        {
            var info = new FileInfo(path);
            return string.Join(Environment.NewLine,
                $"✅ File exists: {path}",
                $"   Size: {FormatBytes(info.Length)}",
                $"   Last modified: {info.LastWriteTimeUtc:yyyy-MM-dd HH:mm:ss} UTC"
            );
        }
        return $"❌ File does not exist: {path}";
    }

    private string ListFiles(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return $"❌ Directory not found: {directory}";
            }

            var files = Directory.GetFiles(directory);
            var dirs = Directory.GetDirectories(directory);

            var sb = new StringBuilder();
            sb.AppendLine($"📁 Directory: {directory}");
            sb.AppendLine($"   Files: {files.Length}, Subdirectories: {dirs.Length}");
            sb.AppendLine();

            if (dirs.Length > 0)
            {
                sb.AppendLine("Subdirectories:");
                foreach (var dir in dirs.Take(20))
                {
                    sb.AppendLine($"  📁 {Path.GetFileName(dir)}");
                }
                if (dirs.Length > 20)
                {
                    sb.AppendLine($"  ... and {dirs.Length - 20} more");
                }
                sb.AppendLine();
            }

            if (files.Length > 0)
            {
                sb.AppendLine("Files:");
                foreach (var file in files.Take(50))
                {
                    var info = new FileInfo(file);
                    sb.AppendLine($"  📄 {Path.GetFileName(file)} ({FormatBytes(info.Length)})");
                }
                if (files.Length > 50)
                {
                    sb.AppendLine($"  ... and {files.Length - 50} more");
                }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error listing files: {ex.Message}");
            return $"❌ Error listing files: {ex.Message}";
        }
    }

    private string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("File Operations Status:");
        sb.AppendLine();
        sb.AppendLine($"Backup path: {_backupPath}");
        sb.AppendLine($"Active backups: {_backups.Count}");
        
        if (_backups.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Recent backups:");
            foreach (var backup in _backups.Values.OrderByDescending(b => b.Timestamp).Take(10))
            {
                sb.AppendLine($"  • {Path.GetFileName(backup.OriginalPath)} - {backup.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
            }
        }

        return sb.ToString();
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private class FileBackup
    {
        public string OriginalPath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string BackupId { get; set; } = string.Empty;
    }
}
