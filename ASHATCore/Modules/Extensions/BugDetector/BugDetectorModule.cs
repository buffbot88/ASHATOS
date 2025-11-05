using System.Text;
using System.Text.RegularExpressions;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.BugDetector;

/// <summary>
/// Bug Detector Module - Analyzes code for common bugs and issues
/// Provides automated detection and fix suggestions
/// </summary>
[RaModule(Category = "extensions")]
public sealed class BugDetectorModule : ModuleBase
{
    public override string Name => "BugDetector";

    private readonly List<BugPattern> _patterns = new();
    private readonly List<DetectedIssue> _detectedIssues = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        InitializePatterns();
        LogInfo("Bug Detector module initialized");
        LogInfo($"Loaded {_patterns.Count} bug detection patterns");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("bugdetect scan ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["bugdetect scan ".Length..].Trim();
            return ScanFile(path);
        }

        if (text.StartsWith("bugdetect scandir ", StringComparison.OrdinalIgnoreCase))
        {
            var path = text["bugdetect scandir ".Length..].Trim();
            return ScanDirectory(path);
        }

        if (text.Equals("bugdetect list", StringComparison.OrdinalIgnoreCase))
        {
            return ListDetectedIssues();
        }

        if (text.Equals("bugdetect clear", StringComparison.OrdinalIgnoreCase))
        {
            _detectedIssues.Clear();
            return "✅ Cleared detected issues";
        }

        if (text.Equals("bugdetect patterns", StringComparison.OrdinalIgnoreCase))
        {
            return ListPatterns();
        }

        if (text.StartsWith("bugdetect fix ", StringComparison.OrdinalIgnoreCase))
        {
            var issueId = text["bugdetect fix ".Length..].Trim();
            return SuggestFix(issueId);
        }

        return "Unknown bugdetect command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Bug Detector commands:",
            "  bugdetect scan <file>         - Scan a file for potential bugs",
            "  bugdetect scandir <directory> - Scan all files in a directory",
            "  bugdetect list                - List all detected issues",
            "  bugdetect clear               - Clear detected issues list",
            "  bugdetect patterns            - List detection patterns",
            "  bugdetect fix <issueId>       - Get fix suggestion for an issue",
            "  help                          - Show this help message",
            "",
            "Examples:",
            "  bugdetect scan /path/to/file.cs",
            "  bugdetect scandir /path/to/project",
            "  bugdetect list",
            "  bugdetect fix 1"
        );
    }

    private void InitializePatterns()
    {
        // Null reference patterns
        _patterns.Add(new BugPattern
        {
            Id = "NULL_REF_001",
            Name = "Potential Null Reference",
            Severity = BugSeverity.High,
            Pattern = new Regex(@"\.(?:Process|ToString|GetType|Equals)\((?!\S)", RegexOptions.Compiled),
            Description = "Method call on potentially null object",
            FixSuggestion = "Add null check before method call (e.g., obj?.Method() or if (obj != null))"
        });

        // String comparison patterns
        _patterns.Add(new BugPattern
        {
            Id = "STR_CMP_001",
            Name = "Case-Sensitive String Comparison",
            Severity = BugSeverity.Medium,
            Pattern = new Regex(@"==\s*[""']|[""']\s*==", RegexOptions.Compiled),
            Description = "Direct string comparison without case consideration",
            FixSuggestion = "Use StringComparison.OrdinalIgnoreCase for case-insensitive comparison"
        });

        // Exception handling patterns
        _patterns.Add(new BugPattern
        {
            Id = "EXC_001",
            Name = "Empty Catch Block",
            Severity = BugSeverity.High,
            Pattern = new Regex(@"catch\s*\([^)]+\)\s*\{\s*\}", RegexOptions.Compiled),
            Description = "Empty catch block swallows exceptions",
            FixSuggestion = "Add logging or proper error handling in catch block"
        });

        // Resource management patterns
        _patterns.Add(new BugPattern
        {
            Id = "RES_001",
            Name = "Missing Using Statement",
            Severity = BugSeverity.Medium,
            Pattern = new Regex(@"new\s+(File|Stream|SqlConnection|HttpClient)\s*\(", RegexOptions.Compiled),
            Description = "IDisposable object created without using statement",
            FixSuggestion = "Wrap in using statement or call Dispose() explicitly"
        });

        // Async/await patterns
        _patterns.Add(new BugPattern
        {
            Id = "ASYNC_001",
            Name = "Missing Await",
            Severity = BugSeverity.High,
            Pattern = new Regex(@"(?<!await\s+)\w+Async\(", RegexOptions.Compiled),
            Description = "Async method called without await",
            FixSuggestion = "Add await keyword or use .Wait()/.Result with caution"
        });

        // SQL injection patterns
        _patterns.Add(new BugPattern
        {
            Id = "SQL_001",
            Name = "Potential SQL Injection",
            Severity = BugSeverity.Critical,
            Pattern = new Regex(@"(ExecuteQuery|ExecuteNonQuery|ExecuteScalar)\s*\(\s*[""'].*?\+.*?[""']", RegexOptions.Compiled),
            Description = "String concatenation in SQL query",
            FixSuggestion = "Use parameterized queries with SqlParameter"
        });

        // Hard-coded credentials
        _patterns.Add(new BugPattern
        {
            Id = "SEC_001",
            Name = "Hard-Coded Credentials",
            Severity = BugSeverity.Critical,
            Pattern = new Regex(@"(password|apikey|secret|token)\s*=\s*[""'][^""']+[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            Description = "Hard-coded sensitive information",
            FixSuggestion = "Use configuration files or environment variables"
        });

        // Division by zero (only in assignments and returns, not URLs or paths)
        _patterns.Add(new BugPattern
        {
            Id = "MATH_001",
            Name = "Potential Division by Zero",
            Severity = BugSeverity.High,
            Pattern = new Regex(@"(=|return)\s+\w+\s*/\s*\w+\s*[;\)]", RegexOptions.Compiled),
            Description = "Division operation in assignment or return without zero check",
            FixSuggestion = "Add check for zero before division (e.g., if (divisor != 0))"
        });
    }

    private string ScanFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return $"❌ File not found: {path}";
            }

            var content = File.ReadAllText(path);
            var lines = content.Split('\n');
            var issuesFound = new List<DetectedIssue>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                foreach (var pattern in _patterns)
                {
                    var matches = pattern.Pattern.Matches(line);
                    foreach (Match match in matches)
                    {
                        var issue = new DetectedIssue
                        {
                            Id = Guid.NewGuid().ToString("N")[..8],
                            PatternId = pattern.Id,
                            PatternName = pattern.Name,
                            Severity = pattern.Severity,
                            FilePath = path,
                            LineNumber = i + 1,
                            LineContent = line.Trim(),
                            Description = pattern.Description,
                            FixSuggestion = pattern.FixSuggestion,
                            DetectedAt = DateTime.UtcNow
                        };
                        issuesFound.Add(issue);
                        _detectedIssues.Add(issue);
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"📄 Scanned: {path}");
            sb.AppendLine($"   Lines: {lines.Length}");
            sb.AppendLine($"   Issues found: {issuesFound.Count}");
            sb.AppendLine();

            if (issuesFound.Count > 0)
            {
                sb.AppendLine("Detected Issues:");
                foreach (var issue in issuesFound.OrderByDescending(i => i.Severity))
                {
                    var icon = issue.Severity switch
                    {
                        BugSeverity.Critical => "🔴",
                        BugSeverity.High => "🟠",
                        BugSeverity.Medium => "🟡",
                        _ => "🟢"
                    };
                    sb.AppendLine($"{icon} [{issue.Id}] Line {issue.LineNumber}: {issue.PatternName}");
                    sb.AppendLine($"   Severity: {issue.Severity}");
                    sb.AppendLine($"   Code: {issue.LineContent}");
                    sb.AppendLine($"   Issue: {issue.Description}");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("✅ No issues detected!");
            }

            LogInfo($"Scanned file: {path}, found {issuesFound.Count} issues");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error scanning file: {ex.Message}");
            return $"❌ Error scanning file: {ex.Message}";
        }
    }

    private string ScanDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return $"❌ Directory not found: {path}";
            }

            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            var totalIssues = 0;
            var sb = new StringBuilder();

            sb.AppendLine($"📁 Scanning directory: {path}");
            sb.AppendLine($"   Files to scan: {files.Length}");
            sb.AppendLine();

            foreach (var file in files.Take(100)) // Limit to 100 files
            {
                var beforeCount = _detectedIssues.Count;
                ScanFile(file);
                var issuesInFile = _detectedIssues.Count - beforeCount;
                totalIssues += issuesInFile;

                if (issuesInFile > 0)
                {
                    sb.AppendLine($"  • {Path.GetFileName(file)}: {issuesInFile} issues");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"✅ Scan complete!");
            sb.AppendLine($"   Total issues: {totalIssues}");
            sb.AppendLine($"   Use 'bugdetect list' to see all issues");

            LogInfo($"Scanned directory: {path}, found {totalIssues} issues in {files.Length} files");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Error scanning directory: {ex.Message}");
            return $"❌ Error scanning directory: {ex.Message}";
        }
    }

    private string ListDetectedIssues()
    {
        if (_detectedIssues.Count == 0)
        {
            return "No issues detected. Run 'bugdetect scan' first.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Detected Issues ({_detectedIssues.Count} total):");
        sb.AppendLine();

        var grouped = _detectedIssues
            .GroupBy(i => i.Severity)
            .OrderByDescending(g => g.Key);

        foreach (var group in grouped)
        {
            var icon = group.Key switch
            {
                BugSeverity.Critical => "🔴",
                BugSeverity.High => "🟠",
                BugSeverity.Medium => "🟡",
                _ => "🟢"
            };

            sb.AppendLine($"{icon} {group.Key} ({group.Count()}):");
            foreach (var issue in group.Take(20))
            {
                sb.AppendLine($"  [{issue.Id}] {Path.GetFileName(issue.FilePath)}:{issue.LineNumber} - {issue.PatternName}");
            }
            if (group.Count() > 20)
            {
                sb.AppendLine($"  ... and {group.Count() - 20} more");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ListPatterns()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Bug Detection Patterns ({_patterns.Count} total):");
        sb.AppendLine();

        var grouped = _patterns.GroupBy(p => p.Severity).OrderByDescending(g => g.Key);

        foreach (var group in grouped)
        {
            sb.AppendLine($"{group.Key}:");
            foreach (var pattern in group)
            {
                sb.AppendLine($"  [{pattern.Id}] {pattern.Name}");
                sb.AppendLine($"      {pattern.Description}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string SuggestFix(string issueId)
    {
        var issue = _detectedIssues.FirstOrDefault(i => i.Id == issueId);
        if (issue == null)
        {
            return $"❌ Issue not found: {issueId}";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Fix Suggestion for Issue {issueId}:");
        sb.AppendLine();
        sb.AppendLine($"Pattern: {issue.PatternName}");
        sb.AppendLine($"Severity: {issue.Severity}");
        sb.AppendLine($"File: {issue.FilePath}");
        sb.AppendLine($"Line: {issue.LineNumber}");
        sb.AppendLine();
        sb.AppendLine($"Issue: {issue.Description}");
        sb.AppendLine();
        sb.AppendLine($"Code:");
        sb.AppendLine($"  {issue.LineContent}");
        sb.AppendLine();
        sb.AppendLine($"💡 Suggested Fix:");
        sb.AppendLine($"  {issue.FixSuggestion}");

        return sb.ToString();
    }

    private class BugPattern
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public BugSeverity Severity { get; set; }
        public Regex Pattern { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string FixSuggestion { get; set; } = string.Empty;
    }

    private class DetectedIssue
    {
        public string Id { get; set; } = string.Empty;
        public string PatternId { get; set; } = string.Empty;
        public string PatternName { get; set; } = string.Empty;
        public BugSeverity Severity { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string LineContent { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FixSuggestion { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
    }

    private enum BugSeverity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
