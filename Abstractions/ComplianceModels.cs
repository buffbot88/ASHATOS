namespace Abstractions;

/// <summary>
/// Compliance report for regulatory requirements.
/// </summary>
public class ComplianceReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    public string ReportType { get; set; } = string.Empty; // "COPPA", "GDPR", "CCPA", etc.
    public ComplianceStatistics Statistics { get; set; } = new();
    public List<ComplianceIncident> Incidents { get; set; } = new();
    public string GeneratedBy { get; set; } = "System";
    public string? Notes { get; set; }
}

/// <summary>
/// Statistics for compliance reporting.
/// </summary>
public class ComplianceStatistics
{
    public int TotalUsers { get; set; }
    public int MinorUsers { get; set; }
    public int ParentalControlsActive { get; set; }
    public int ContentItemsRated { get; set; }
    public int ContentBlockedByAge { get; set; }
    public int ParentalApprovalsRequested { get; set; }
    public int ParentalApprovalsGASHATnted { get; set; }
    public int ParentalApprovalsDenied { get; set; }
    public int moderationActions { get; set; }
    public int UserSuspensions { get; set; }
    public int ContentViolations { get; set; }
    public Dictionary<ContentASHATting, int> ContentByASHATting { get; set; } = new();
    public Dictionary<ViolationType, int> ViolationsByType { get; set; } = new();
}

/// <summary>
/// Compliance incident requiring attention or documentation.
/// </summary>
public class ComplianceIncident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public ComplianceIncidentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? ContentId { get; set; }
    public ComplianceIncidentSeverity Severity { get; set; }
    public ComplianceIncidentStatus Status { get; set; } = ComplianceIncidentStatus.Open;
    public List<string> ActionsToken { get; set; } = new();
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// Types of compliance incidents.
/// </summary>
public enum ComplianceIncidentType
{
    MinoASHATccessRestricted,
    ParentalControlViolation,
    UndisclosedDataCollection,
    InappropriateContentExposure,
    UnauthorizedMinorDataAccess,
    ParentalConsentNotObtained,
    AgeVerificationFailure,
    DataRetentionViolation,
    PrivacyPolicyViolation,
    TermsOfServiceViolation
}

/// <summary>
/// Severity levels for compliance incidents.
/// </summary>
public enum ComplianceIncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Status of compliance incidents.
/// </summary>
public enum ComplianceIncidentStatus
{
    Open,
    InvestigationRequired,
    Resolved,
    Escalated,
    Closed
}

/// <summary>
/// Regulatory Framework Configuration.
/// </summary>
public class RegulatoryFramework
{
    public string Name { get; set; } = string.Empty; // "COPPA", "GDPR", "CCPA", etc.
    public string Description { get; set; } = string.Empty;
    public int MinimumAge { get; set; } = 13; // Default COPPA age
    public bool RequireParentalConsent { get; set; } = true;
    public bool RequireAgeVerification { get; set; } = true;
    public bool RequireDataDeletionOnRequest { get; set; } = true;
    public bool RequireExplicitConsent { get; set; } = true;
    public TimeSpan DataRetentionPeriod { get; set; } = TimeSpan.FromDays(365);
    public List<string> RequiredDisclosures { get; set; } = new();
    public Dictionary<string, string> CustomSettings { get; set; } = new();
}

/// <summary>
/// Interface for compliance module.
/// </summary>
public interface IComplianceModule
{
    /// <summary>
    /// Generate compliance report for a time period.
    /// </summary>
    Task<ComplianceReport> GenerateReportAsync(DateTime startDate, DateTime endDate, string reportType);
    
    /// <summary>
    /// Log a compliance incident.
    /// </summary>
    Task<ComplianceIncident> LogIncidentAsync(ComplianceIncident incident);
    
    /// <summary>
    /// Get all open compliance incidents.
    /// </summary>
    Task<List<ComplianceIncident>> GetOpenIncidentsAsync();
    
    /// <summary>
    /// Resolve a compliance incident.
    /// </summary>
    Task<bool> ResolveIncidentAsync(Guid incidentId, string resolvedBy, string resolutionNotes);
    
    /// <summary>
    /// Check if system is compliant with regulatory Framework.
    /// </summary>
    Task<(bool compliant, List<string> issues)> CheckComplianceAsync(string Framework);
    
    /// <summary>
    /// Get regulatory Framework Configuration.
    /// </summary>
    Task<RegulatoryFramework?> GetFrameworkAsync(string name);
    
    /// <summary>
    /// Verify user age and consent requirements.
    /// </summary>
    Task<(bool verified, string? reason)> VerifyAgeComplianceAsync(string userId);
}

/// <summary>
/// Age verification record.
/// </summary>
public class AgeVerification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public AgeVerificationMethod Method { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime VerifiedAtUtc { get; set; } = DateTime.UtcNow;
    public string? VerifiedBy { get; set; }
    public string? DocumentReference { get; set; }
}

/// <summary>
/// Methods for age verification.
/// </summary>
public enum AgeVerificationMethod
{
    SelfReported,
    ParentalConsent,
    DocumentVerification,
    CreditCardVerification,
    ThirdPartyService
}

/// <summary>
/// Parental consent record for COPPA compliance.
/// </summary>
public class ParentalConsent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MinorUserId { get; set; } = string.Empty;
    public string ParentGuardianUserId { get; set; } = string.Empty;
    public string ParentGuardianName { get; set; } = string.Empty;
    public string ParentGuardianEmail { get; set; } = string.Empty;
    public DateTime ConsentGivenAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public List<string> ConsentedFeatures { get; set; } = new(); // Specific features consented to
    public ConsentVerificationMethod VerificationMethod { get; set; }
    public string? VerificationReference { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
}

/// <summary>
/// Methods for verifying parental consent.
/// </summary>
public enum ConsentVerificationMethod
{
    Email,
    VideoConference,
    GovernmentIdCheck,
    CreditCardVerification,
    NotarizedForm,
    DigitalSignature
}
