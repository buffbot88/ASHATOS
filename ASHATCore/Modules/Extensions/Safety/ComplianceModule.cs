using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Safety;

/// <summary>
/// Compliance and regulatory reporting module for ASHATCore.
/// Ensures adherence to COPPA, GDPR, CCPA, and other regulations.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ComplianceModule : ModuleBase, IComplianceModule
{
    public override string Name => "Compliance";

    private readonly ConcurrentDictionary<string, ComplianceIncident> _incidents = new();
    private readonly ConcurrentDictionary<string, AgeVerification> _ageVerifications = new();
    private readonly ConcurrentDictionary<string, ParentalConsent> _parentalConsents = new();
    private readonly Dictionary<string, RegulatoryFramework> _Frameworks = new();
    
    private ModuleManager? _manager;
    private IParentalControlModule? _parentalControlModule;
    private IContentmoderationModule? _moderationModule;
    private IAuthenticationModule? _authModule;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _parentalControlModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IParentalControlModule>()
                .FirstOrDefault();
            
            _moderationModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IContentmoderationModule>()
                .FirstOrDefault();
            
            _authModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IAuthenticationModule>()
                .FirstOrDefault();
        }
        
        // Initialize regulatory Frameworks
        InitializeFrameworks();
        
        Console.WriteLine($"[{Name}] Compliance Module initialized");
        Console.WriteLine($"[{Name}] Regulatory Frameworks: COPPA, GDPR, CCPA");
        Console.WriteLine($"[{Name}] Age verification enabled");
        Console.WriteLine($"[{Name}] Parental consent tracking active");
    }

    public override string Process(string input)
    {
        var text = (input ?? "").Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("compliance report ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: compliance report <reportType> [days]";
            }
            
            var reportType = parts[2];
            var days = parts.Length > 3 && int.TryParse(parts[3], out var d) ? d : 30;
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);
            
            var task = GenerateReportAsync(startDate, endDate, reportType);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.Equals("compliance incidents", StringComparison.OrdinalIgnoreCase))
        {
            var task = GetOpenIncidentsAsync();
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("compliance check ", StringComparison.OrdinalIgnoreCase))
        {
            var Framework = text["compliance check ".Length..].Trim();
            var task = CheckComplianceAsync(Framework);
            task.Wait();
            var result = task.Result;
            return $"Compliant: {result.compliant}\nIssues:\n- {string.Join("\n- ", result.issues)}";
        }

        if (text.StartsWith("compliance verify ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["compliance verify ".Length..].Trim();
            var task = VerifyAgeComplianceAsync(userId);
            task.Wait();
            var result = task.Result;
            return $"Verified: {result.verified}" + (result.reason != null ? $"\nReason: {result.reason}" : "");
        }

        if (text.Equals("compliance stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    public async Task<ComplianceReport> GenerateReportAsync(DateTime startDate, DateTime endDate, string reportType)
    {
        await Task.CompletedTask;
        
        var report = new ComplianceReport
        {
            ReportPeriodStart = startDate,
            ReportPeriodEnd = endDate,
            ReportType = reportType,
            GeneratedAtUtc = DateTime.UtcNow
        };

        // Gather statistics
        var stats = new ComplianceStatistics();

        // Get incidents in the period
        var incidentsInPeriod = _incidents.Values
            .Where(i => i.OccurredAtUtc >= startDate && i.OccurredAtUtc <= endDate)
            .ToList();

        report.Incidents = incidentsInPeriod;

        // Populate statistics
        stats.MinorUsers = _ageVerifications.Values.Count(v => v.DateOfBirth.HasValue && 
            CalculateAge(v.DateOfBirth.Value) < 13);
        stats.ParentalControlsActive = _parentalConsents.Values.Count(c => c.IsActive);
        
        // Get moderation stats if available
        if (_moderationModule != null)
        {
            var modStats = await _moderationModule.GetStatsAsync();
            stats.moderationActions = modStats.TotalScans - modStats.Approved;
            stats.UserSuspensions = modStats.ActiveSuspensions;
            stats.ContentViolations = modStats.Flagged + modStats.Blocked;
            stats.ViolationsByType = modStats.ViolationsByType;
        }

        report.Statistics = stats;

        Console.WriteLine($"[{Name}] Generated {reportType} compliance report for {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        Console.WriteLine($"[{Name}] - Total incidents: {report.Incidents.Count}");
        Console.WriteLine($"[{Name}] - Minor users: {stats.MinorUsers}");
        
        return report;
    }

    public async Task<ComplianceIncident> LogIncidentAsync(ComplianceIncident incident)
    {
        await Task.CompletedTask;
        
        _incidents[incident.Id.ToString()] = incident;
        
        Console.WriteLine($"[{Name}] Compliance incident logged: {incident.Type}");
        Console.WriteLine($"[{Name}] - Severity: {incident.Severity}");
        Console.WriteLine($"[{Name}] - Description: {incident.Description}");
        
        // Auto-escalate critical incidents
        if (incident.Severity == ComplianceIncidentSeverity.Critical)
        {
            incident.Status = ComplianceIncidentStatus.Escalated;
            Console.WriteLine($"[{Name}] CRITICAL INCIDENT - Auto-escalated for immediate review");
        }
        
        return incident;
    }

    public async Task<List<ComplianceIncident>> GetOpenIncidentsAsync()
    {
        await Task.CompletedTask;
        
        return _incidents.Values
            .Where(i => i.Status == ComplianceIncidentStatus.Open || 
                       i.Status == ComplianceIncidentStatus.InvestigationRequired ||
                       i.Status == ComplianceIncidentStatus.Escalated)
            .OrderByDescending(i => i.Severity)
            .ThenBy(i => i.OccurredAtUtc)
            .ToList();
    }

    public async Task<bool> ResolveIncidentAsync(Guid incidentId, string resolvedBy, string resolutionNotes)
    {
        await Task.CompletedTask;
        
        if (!_incidents.TryGetValue(incidentId.ToString(), out var incident))
        {
            return false;
        }

        incident.Status = ComplianceIncidentStatus.Resolved;
        incident.ResolvedAtUtc = DateTime.UtcNow;
        incident.ResolvedBy = resolvedBy;
        incident.ResolutionNotes = resolutionNotes;

        Console.WriteLine($"[{Name}] Incident {incidentId} resolved by {resolvedBy}");
        
        return true;
    }

    public async Task<(bool compliant, List<string> issues)> CheckComplianceAsync(string Framework)
    {
        await Task.CompletedTask;
        
        var issues = new List<string>();
        
        if (!_Frameworks.TryGetValue(Framework.ToUpperInvariant(), out var fw))
        {
            issues.Add($"Unknown regulatory Framework: {Framework}");
            return (false, issues);
        }

        // Check age verification for minors
        if (fw.RequireAgeVerification)
        {
            var unverifiedMinors = _ageVerifications.Values
                .Count(v => !v.IsVerified && v.DateOfBirth.HasValue && 
                       CalculateAge(v.DateOfBirth.Value) < fw.MinimumAge);
            
            if (unverifiedMinors > 0)
            {
                issues.Add($"{unverifiedMinors} minor user(s) without age verification");
            }
        }

        // Check parental consent for minors
        if (fw.RequireParentalConsent)
        {
            var minorsWithoutConsent = _ageVerifications.Values
                .Where(v => v.DateOfBirth.HasValue && 
                           CalculateAge(v.DateOfBirth.Value) < fw.MinimumAge)
                .Count(v => !_parentalConsents.Values.Any(c => 
                    c.MinorUserId == v.UserId && c.IsActive));
            
            if (minorsWithoutConsent > 0)
            {
                issues.Add($"{minorsWithoutConsent} minor user(s) without parental consent");
            }
        }

        // Check for open critical incidents
        var criticalIncidents = _incidents.Values
            .Count(i => i.Severity == ComplianceIncidentSeverity.Critical && 
                       i.Status != ComplianceIncidentStatus.Resolved &&
                       i.Status != ComplianceIncidentStatus.Closed);
        
        if (criticalIncidents > 0)
        {
            issues.Add($"{criticalIncidents} unresolved critical incident(s)");
        }

        var compliant = issues.Count == 0;
        
        if (compliant)
        {
            Console.WriteLine($"[{Name}] Compliance check passed for {Framework}");
        }
        else
        {
            Console.WriteLine($"[{Name}] Compliance check FAILED for {Framework}");
            Console.WriteLine($"[{Name}] - Issues found: {issues.Count}");
        }
        
        return (compliant, issues);
    }

    public async Task<RegulatoryFramework?> GetFrameworkAsync(string name)
    {
        await Task.CompletedTask;
        return _Frameworks.TryGetValue(name.ToUpperInvariant(), out var fw) ? fw : null;
    }

    public async Task<(bool verified, string? reason)> VerifyAgeComplianceAsync(string userId)
    {
        await Task.CompletedTask;
        
        // Check if user has age verification
        if (!_ageVerifications.TryGetValue(userId, out var verification))
        {
            return (false, "No age verification on file");
        }

        if (!verification.IsVerified)
        {
            return (false, "Age verification pending");
        }

        if (!verification.DateOfBirth.HasValue)
        {
            return (false, "Date of birth not provided");
        }

        var age = CalculateAge(verification.DateOfBirth.Value);
        
        // Check if minor requires parental consent (COPPA - under 13)
        if (age < 13)
        {
            var hasConsent = _parentalConsents.Values.Any(c => 
                c.MinorUserId == userId && c.IsActive);
            
            if (!hasConsent)
            {
                return (false, "Minor user requires parental consent (COPPA compliance)");
            }
        }

        return (true, null);
    }

    public async Task<AgeVerification> VerifyUseASHATgeAsync(string userId, DateTime? dateOfBirth, AgeVerificationMethod method)
    {
        await Task.CompletedTask;
        
        var verification = new AgeVerification
        {
            UserId = userId,
            DateOfBirth = dateOfBirth,
            Method = method,
            IsVerified = method != AgeVerificationMethod.SelfReported, // Self-reported requires additional verification
            VerifiedAtUtc = DateTime.UtcNow,
            VerifiedBy = method == AgeVerificationMethod.SelfReported ? userId : "System"
        };

        _ageVerifications[userId] = verification;
        
        if (dateOfBirth.HasValue)
        {
            var age = CalculateAge(dateOfBirth.Value);
            Console.WriteLine($"[{Name}] Age verification recorded for user {userId}: {age} years old");
            
            // Check if minor and log compliance requirement
            if (age < 13)
            {
                Console.WriteLine($"[{Name}] Minor user detected - COPPA compliance required");
                
                // Log incident for tracking
                await LogIncidentAsync(new ComplianceIncident
                {
                    Type = ComplianceIncidentType.ParentalConsentNotObtained,
                    Description = $"Minor user {userId} (age {age}) registered - parental consent required",
                    UserId = userId,
                    Severity = ComplianceIncidentSeverity.High,
                    Status = ComplianceIncidentStatus.Open
                });
            }
        }
        
        return verification;
    }

    public async Task<ParentalConsent> RecordParentalConsentAsync(ParentalConsent consent)
    {
        await Task.CompletedTask;
        
        _parentalConsents[consent.Id.ToString()] = consent;
        
        Console.WriteLine($"[{Name}] Parental consent recorded for minor user {consent.MinorUserId}");
        Console.WriteLine($"[{Name}] - Parent/Guardian: {consent.ParentGuardianName}");
        Console.WriteLine($"[{Name}] - Verification method: {consent.VerificationMethod}");
        
        // Resolve any open compliance incidents for this user
        var openIncidents = _incidents.Values
            .Where(i => i.UserId == consent.MinorUserId && 
                       i.Type == ComplianceIncidentType.ParentalConsentNotObtained &&
                       i.Status == ComplianceIncidentStatus.Open)
            .ToList();

        foreach (var incident in openIncidents)
        {
            await ResolveIncidentAsync(incident.Id, "System", "Parental consent obtained");
        }
        
        return consent;
    }

    private void InitializeFrameworks()
    {
        // COPPA (Children's Online Privacy Protection Act)
        _Frameworks["COPPA"] = new RegulatoryFramework
        {
            Name = "COPPA",
            Description = "Children's Online Privacy Protection Act (USA)",
            MinimumAge = 13,
            RequireParentalConsent = true,
            RequireAgeVerification = true,
            RequireDataDeletionOnRequest = true,
            RequireExplicitConsent = true,
            DataRetentionPeriod = TimeSpan.FromDays(365),
            RequiredDisclosures = new List<string>
            {
                "Data collection pASHATctices",
                "Parental consent requirements",
                "Data deletion rights",
                "Contact information for privacy concerns"
            }
        };

        // GDPR (General Data Protection Regulation)
        _Frameworks["GDPR"] = new RegulatoryFramework
        {
            Name = "GDPR",
            Description = "General Data Protection Regulation (EU)",
            MinimumAge = 16, // Can be lowered to 13 by member states
            RequireParentalConsent = true,
            RequireAgeVerification = true,
            RequireDataDeletionOnRequest = true,
            RequireExplicitConsent = true,
            DataRetentionPeriod = TimeSpan.FromDays(730),
            RequiredDisclosures = new List<string>
            {
                "Data processing purposes",
                "Legal basis for processing",
                "Data retention periods",
                "Rights of data subjects",
                "Data controller contact information"
            }
        };

        // CCPA (California Consumer Privacy Act)
        _Frameworks["CCPA"] = new RegulatoryFramework
        {
            Name = "CCPA",
            Description = "California Consumer Privacy Act (USA)",
            MinimumAge = 13,
            RequireParentalConsent = true,
            RequireAgeVerification = false,
            RequireDataDeletionOnRequest = true,
            RequireExplicitConsent = true,
            DataRetentionPeriod = TimeSpan.FromDays(365),
            RequiredDisclosures = new List<string>
            {
                "Categories of personal information collected",
                "Business purposes for collection",
                "Right to deletion",
                "Right to opt-out of sale",
                "Non-discrimination for exercising rights"
            }
        };

        Console.WriteLine($"[{Name}] Initialized regulatory Frameworks: {string.Join(", ", _Frameworks.Keys)}");
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    private string GetHelp()
    {
        return @"Compliance Module:
  - help                                    : Show this help
  - compliance report <type> [days]         : Generate compliance report (default: 30 days)
  - compliance incidents                    : List open compliance incidents
  - compliance check <Framework>            : Check compliance with regulatory Framework
  - compliance verify <userId>              : Verify age compliance for user
  - compliance stats                        : Show compliance statistics

Features:
  - COPPA, GDPR, CCPA compliance
  - Age verification and tracking
  - Parental consent management
  - Compliance incident logging and resolution
  - Regulatory reporting
  - Audit trail for compliance activities

Regulatory Frameworks:
  - COPPA: Children's Online Privacy Protection Act (USA, age 13)
  - GDPR: General Data Protection Regulation (EU, age 16)
  - CCPA: California Consumer Privacy Act (USA, age 13)

Compliance Requirements:
  - Age verification for all users
  - Parental consent for minors
  - Data deletion rights
  - Explicit consent for data processing
  - Comprehensive audit trails
  - Regular compliance reporting";
    }

    private string GetStats()
    {
        var stats = new
        {
            TotalIncidents = _incidents.Count,
            OpenIncidents = _incidents.Values.Count(i => 
                i.Status == ComplianceIncidentStatus.Open ||
                i.Status == ComplianceIncidentStatus.InvestigationRequired),
            CriticalIncidents = _incidents.Values.Count(i => 
                i.Severity == ComplianceIncidentSeverity.Critical),
            AgeVerifications = _ageVerifications.Count,
            VerifiedUsers = _ageVerifications.Values.Count(v => v.IsVerified),
            ParentalConsents = _parentalConsents.Count,
            ActiveConsents = _parentalConsents.Values.Count(c => c.IsActive),
            MinorUsers = _ageVerifications.Values.Count(v => v.DateOfBirth.HasValue && 
                CalculateAge(v.DateOfBirth.Value) < 13),
            IncidentsBySeverity = _incidents.Values
                .GroupBy(i => i.Severity)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            IncidentsByType = _incidents.Values
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return JsonSerializer.Serialize(stats, _jsonOptions);
    }
}
