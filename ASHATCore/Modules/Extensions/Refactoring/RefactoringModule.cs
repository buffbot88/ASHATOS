using System.Text;
using System.Text.RegularExpressions;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Refactoring;

/// <summary>
/// Refactoring Module - Provides code refactoring operations
/// Helps improve code quality, structure, and maintainability
/// </summary>
[RaModule(Category = "extensions")]
public sealed class RefactoringModule : ModuleBase
{
    public override string Name => "Refactoring";

    private readonly List<RefactoringOperation> _operationHistory = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("Refactoring module initialized");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("refactor rename ", StringComparison.OrdinalIgnoreCase))
        {
            // refactor rename <oldName> to <newName> in <path>
            var parts = text["refactor rename ".Length..].Split(new[] { " to ", " in " }, StringSplitOptions.None);
            if (parts.Length != 3)
            {
                return "Usage: refactor rename <oldName> to <newName> in <path>";
            }
            return RenameSymbol(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
        }

        if (text.StartsWith("refactor extract ", StringComparison.OrdinalIgnoreCase))
        {
            // refactor extract method from <path> lines <start>-<end>
            return "Extract method refactoring - Coming soon!";
        }

        if (text.StartsWith("refactor format ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["refactor format ".Length..].Trim();
            return FormatCode(path);
        }

        if (text.StartsWith("refactor simplify ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["refactor simplify ".Length..].Trim();
            return SimplifyCode(path);
        }

        if (text.StartsWith("refactor organize ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["refactor organize ".Length..].Trim();
            return OrganizeCode(path);
        }

        if (text.Equals("refactor history", StringComparison.OrdinalIgnoreCase))
        {
            return GetHistory();
        }

        return "Unknown refactor command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Refactoring commands:",
            "  refactor rename <old> to <new> in <path>  - Rename a symbol (class/method/variable)",
            "  refactor format <path>                    - Format and standardize code style",
            "  refactor simplify <path>                  - Simplify complex code patterns",
            "  refactor organize <path>                  - Organize using statements and methods",
            "  refactor history                          - Show refactoring history",
            "  help                                      - Show this help message",
            "",
            "Examples:",
            "  refactor rename OldClass to NewClass in /path/to/file.cs",
            "  refactor format /path/to/file.cs",
            "  refactor simplify /path/to/file.cs"
        );
    }

    private string RenameSymbol(string oldName, string newName, string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            
            // Create word boundary regex to match whole words only
            var pattern = $@"\b{Regex.Escape(oldName)}\b";
            var regex = new Regex(pattern);
            var matches = regex.Matches(content);

            if (matches.Count == 0)
            {
                return $"⚠️ Symbol '{oldName}' not found in file";
            }

            // Create backup first (using FileOperations if available)
            var backupPath = $"{path}.refactor.backup";
            File.Copy(path, backupPath, true);

            // Perform replacement
            var newContent = regex.Replace(content, newName);
            File.WriteAllText(path, newContent);

            // Record operation
            var operation = new RefactoringOperation
            {
                Type = "Rename",
                FilePath = path,
                Details = $"{oldName} → {newName}",
                OccurrencesChanged = matches.Count,
                Timestamp = DateTime.UtcNow
            };
            _operationHistory.Add(operation);

            LogInfo($"Renamed '{oldName}' to '{newName}' in {path} ({matches.Count} occurrences)");

            return string.Join(Environment.NewLine,
                $"✅ Rename refactoring completed",
                $"   File: {path}",
                $"   {oldName} → {newName}",
                $"   Occurrences: {matches.Count}",
                $"   Backup: {backupPath}"
            );
        }
        catch (Exception ex)
        {
            LogError($"Error renaming symbol: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string FormatCode(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            var originalLength = content.Length;

            // Create backup
            var backupPath = $"{path}.format.backup";
            File.Copy(path, backupPath, true);

            // Apply basic formatting rules
            var formatted = content;

            // Fix indentation (basic - proper indentation would need parsing)
            formatted = Regex.Replace(formatted, @"^\s+", match => 
            {
                var spaces = match.Value.Replace("\t", "    ");
                return spaces;
            }, RegexOptions.Multiline);

            // Remove trailing whitespace
            formatted = Regex.Replace(formatted, @"[ \t]+$", "", RegexOptions.Multiline);

            // Ensure single blank line between methods/classes
            formatted = Regex.Replace(formatted, @"\n{3,}", "\n\n");

            // Add space after commas
            formatted = Regex.Replace(formatted, @",(\S)", ", $1");

            // Add space around operators
            formatted = Regex.Replace(formatted, @"(\S)(==|!=|<=|>=|&&|\|\|)(\S)", "$1 $2 $3");

            File.WriteAllText(path, formatted);

            var operation = new RefactoringOperation
            {
                Type = "Format",
                FilePath = path,
                Details = "Code formatting applied",
                OccurrencesChanged = 1,
                Timestamp = DateTime.UtcNow
            };
            _operationHistory.Add(operation);

            LogInfo($"Formatted code in {path}");

            return string.Join(Environment.NewLine,
                $"✅ Code formatting completed",
                $"   File: {path}",
                $"   Original size: {originalLength} chars",
                $"   Formatted size: {formatted.Length} chars",
                $"   Backup: {backupPath}"
            );
        }
        catch (Exception ex)
        {
            LogError($"Error formatting code: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string SimplifyCode(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            var suggestions = new List<string>();

            // Detect simplification opportunities
            
            // 1. String.Empty vs ""
            if (content.Contains("\"\"") && content.Contains("String.Empty"))
            {
                suggestions.Add("Consider consistent use of String.Empty or \"\" for empty strings");
            }

            // 2. Unnecessary ToString()
            if (Regex.IsMatch(content, @"\.ToString\(\)\.ToString\(\)"))
            {
                suggestions.Add("Remove redundant ToString() calls");
            }

            // 3. Complex null checks
            if (Regex.IsMatch(content, @"if\s*\(\s*\w+\s*!=\s*null\s*&&"))
            {
                suggestions.Add("Consider using null-conditional operator (?.)");
            }

            // 4. Nested ifs
            var nestedIfCount = Regex.Matches(content, @"if\s*\([^)]+\)\s*\{\s*if").Count;
            if (nestedIfCount > 3)
            {
                suggestions.Add($"Consider simplifying {nestedIfCount} nested if statements");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"📊 Code Simplification Analysis: {path}");
            sb.AppendLine();

            if (suggestions.Count > 0)
            {
                sb.AppendLine($"Found {suggestions.Count} simplification opportunities:");
                foreach (var suggestion in suggestions)
                {
                    sb.AppendLine($"  💡 {suggestion}");
                }
            }
            else
            {
                sb.AppendLine("✅ No obvious simplification opportunities found");
            }

            LogInfo($"Analyzed code simplification for {path}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error analyzing code: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string OrganizeCode(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            
            // Create backup
            var backupPath = $"{path}.organize.backup";
            File.Copy(path, backupPath, true);

            var organized = content;
            var changes = new List<string>();

            // Sort using statements (basic implementation)
            var usingPattern = @"^using\s+[\w.]+;$";
            var usingMatches = Regex.Matches(content, usingPattern, RegexOptions.Multiline);
            
            if (usingMatches.Count > 1)
            {
                var usings = usingMatches.Select(m => m.Value).OrderBy(u => u).ToList();
                var firstUsing = usingMatches[0];
                var lastUsing = usingMatches[usingMatches.Count - 1];
                
                var beforeUsings = content[..firstUsing.Index];
                var afterUsings = content[(lastUsing.Index + lastUsing.Length)..];
                
                organized = beforeUsings + string.Join(Environment.NewLine, usings) + afterUsings;
                changes.Add("Sorted using statements");
            }

            // Remove duplicate blank lines
            organized = Regex.Replace(organized, @"\n{3,}", "\n\n");
            changes.Add("Removed excessive blank lines");

            File.WriteAllText(path, organized);

            var operation = new RefactoringOperation
            {
                Type = "Organize",
                FilePath = path,
                Details = string.Join(", ", changes),
                OccurrencesChanged = changes.Count,
                Timestamp = DateTime.UtcNow
            };
            _operationHistory.Add(operation);

            LogInfo($"Organized code in {path}");

            return string.Join(Environment.NewLine,
                $"✅ Code organization completed",
                $"   File: {path}",
                $"   Changes: {string.Join(", ", changes)}",
                $"   Backup: {backupPath}"
            );
        }
        catch (Exception ex)
        {
            LogError($"Error organizing code: {ex.Message}");
            return $"❌ Error: {ex.Message}";
        }
    }

    private string GetHistory()
    {
        if (_operationHistory.Count == 0)
        {
            return "No refactoring operations performed yet.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Refactoring History:");
        sb.AppendLine();

        foreach (var op in _operationHistory.TakeLast(20))
        {
            sb.AppendLine($"✅ {op.Timestamp:yyyy-MM-dd HH:mm:ss} - {op.Type}");
            sb.AppendLine($"   File: {Path.GetFileName(op.FilePath)}");
            sb.AppendLine($"   {op.Details}");
            sb.AppendLine();
        }

        if (_operationHistory.Count > 20)
        {
            sb.AppendLine($"... and {_operationHistory.Count - 20} more operations");
        }

        return sb.ToString();
    }

    private class RefactoringOperation
    {
        public string Type { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int OccurrencesChanged { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
