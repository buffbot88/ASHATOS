# 🔒 RaOS v9.4.0 Security Recommendations - Implementation Guide

**Version:** 9.4.0  
**Purpose:** Detailed implementation guidance for production security hardening  
**Last Updated:** January 2025

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**

---

## 🎯 Overview

This document provides detailed implementation guidance for all security recommendations identified in [SECURITY_GATE_940.md](./SECURITY_GATE_940.md). Each recommendation includes code examples, configuration steps, and best practices.

---

## 1. Rate Limiting & Account Lockout

### Priority: 🔴 CRITICAL - Must implement before production

### Problem
Currently, there is no rate limiting on authentication endpoints, making the system vulnerable to:
- Brute force password attacks
- Credential stuffing attacks
- Account enumeration
- Denial of service via authentication endpoints

### Solution: IP-Based Rate Limiting

#### Implementation Steps

**Step 1: Add Rate Limiting Package**
```bash
cd RaCore
dotnet add package AspNetCoreRateLimit
```

**Step 2: Update Program.cs**
```csharp
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 5, // 5 attempts per minute
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1h",
            Limit = 20, // 20 attempts per hour
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/register",
            Period = "1h",
            Limit = 3, // 3 registrations per hour
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();

// Use rate limiting middleware (BEFORE other middleware)
app.UseIpRateLimiting();

// ... rest of middleware
app.UseCors();
app.UseWebSockets();
```

**Step 3: Add Configuration (appsettings.json)**
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1h",
        "Limit": 20
      },
      {
        "Endpoint": "POST:/api/auth/register",
        "Period": "1h",
        "Limit": 3
      }
    ]
  }
}
```

### Solution: Account Lockout

**Add to AuthenticationModule.cs:**
```csharp
private readonly Dictionary<string, (int failedAttempts, DateTime lockoutUntil)> _failedLoginAttempts = new();
private const int MAX_FAILED_ATTEMPTS = 5;
private static readonly TimeSpan LOCKOUT_DURATION = TimeSpan.FromMinutes(15);

public async Task<(bool success, string message, string? token)> LoginAsync(string username, string password, string ipAddress)
{
    // Check if account is locked out
    if (_failedLoginAttempts.TryGetValue(username, out var lockoutInfo))
    {
        if (lockoutInfo.lockoutUntil > DateTime.UtcNow)
        {
            var remainingTime = (lockoutInfo.lockoutUntil - DateTime.UtcNow).Minutes;
            await LogSecurityEventAsync("login_attempt_locked", username, null, ipAddress, 
                $"Account locked. {remainingTime} minutes remaining", false);
            return (false, $"Account is locked due to too many failed login attempts. Try again in {remainingTime} minutes.", null);
        }
        
        // Lockout expired, clear it
        _failedLoginAttempts.Remove(username);
    }

    var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    
    if (user == null || !VerifyPassword(password, user.PasswordHash, user.Salt))
    {
        // Increment failed attempts
        if (_failedLoginAttempts.ContainsKey(username))
        {
            var current = _failedLoginAttempts[username];
            var newAttempts = current.failedAttempts + 1;
            
            if (newAttempts >= MAX_FAILED_ATTEMPTS)
            {
                _failedLoginAttempts[username] = (newAttempts, DateTime.UtcNow.Add(LOCKOUT_DURATION));
                await LogSecurityEventAsync("account_locked", username, null, ipAddress, 
                    $"Account locked after {newAttempts} failed attempts", false);
                return (false, "Too many failed login attempts. Account locked for 15 minutes.", null);
            }
            else
            {
                _failedLoginAttempts[username] = (newAttempts, DateTime.MinValue);
                await LogSecurityEventAsync("login_failed", username, null, ipAddress, 
                    $"Failed attempt {newAttempts}/{MAX_FAILED_ATTEMPTS}", false);
            }
        }
        else
        {
            _failedLoginAttempts[username] = (1, DateTime.MinValue);
        }
        
        return (false, "Invalid username or password", null);
    }
    
    // Successful login - clear failed attempts
    _failedLoginAttempts.Remove(username);
    
    // ... rest of login logic
}
```

### Testing
```bash
# Test rate limiting
for i in {1..10}; do
  curl -X POST http://localhost:7077/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}'
  echo ""
done

# Expected: First 5 succeed (with 401), next 5 get 429 Too Many Requests
```

---

## 2. Log Rotation & Data Retention

### Priority: 🟠 HIGH - Implement before production

### Problem
- Logs grow indefinitely, consuming disk space
- No automatic cleanup of old logs
- No size limits on log files
- Risk of disk space exhaustion

### Solution: Serilog with File Size Limits

**Step 1: Add Serilog**
```bash
cd RaCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

**Step 2: Configure Logging (Program.cs)**
```csharp
using Serilog;
using Serilog.Events;

// Configure Serilog BEFORE builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/racore-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30, // Keep 30 days
        fileSizeLimitBytes: 104_857_600, // 100 MB per file
        rollOnFileSizeLimit: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/security-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90, // Keep 90 days for security logs
        fileSizeLimitBytes: 52_428_800, // 50 MB per file
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: LogEventLevel.Warning, // Security-relevant events
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
```

**Step 3: Add to .gitignore**
```
# Application logs
logs/
*.log
```

### Session Cleanup Job

**Add to Program.cs or create SessionCleanupService.cs:**
```csharp
public class SessionCleanupService : BackgroundService
{
    private readonly IAuthenticationModule _authModule;
    private readonly ILogger<SessionCleanupService> _logger;
    private static readonly TimeSpan CLEANUP_INTERVAL = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CLEANUP_INTERVAL, stoppingToken);
                
                var expiredCount = await _authModule.CleanupExpiredSessionsAsync();
                if (expiredCount > 0)
                {
                    _logger.LogInformation($"Cleaned up {expiredCount} expired sessions");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in session cleanup job");
            }
        }
    }
}

// Register in Program.cs
builder.Services.AddHostedService<SessionCleanupService>();
```

**Add to AuthenticationModule.cs:**
```csharp
public async Task<int> CleanupExpiredSessionsAsync()
{
    var now = DateTime.UtcNow;
    var expiredSessions = _sessions.Where(kvp => kvp.Value.ExpiresAtUtc < now).ToList();
    
    foreach (var session in expiredSessions)
    {
        _sessions.TryRemove(session.Key, out _);
        await LogSecurityEventAsync("session_expired", null, session.Value.UserId, null, 
            "Session expired and removed", true);
    }
    
    return expiredSessions.Count;
}
```

---

## 3. CI/CD Security Pipeline

### Priority: 🔴 CRITICAL - Must implement before production

### Problem
- No automated security checks
- No vulnerability scanning
- No secret detection
- Manual security review burden

### Solution: GitHub Actions Security Workflow

**Create .github/workflows/security-scan.yml:**
```yaml
name: Security Scan

on:
  push:
    branches: [ main, 'release/*' ]
  pull_request:
    branches: [ main ]
  schedule:
    # Run weekly on Sundays at 2 AM UTC
    - cron: '0 2 * * 0'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run tests
      run: dotnet test --no-build --configuration Release --verbosity normal

  dependency-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Check for vulnerable packages
      run: |
        dotnet list package --vulnerable --include-transitive 2>&1 | tee vulnerability-report.txt
        if grep -q "has the following vulnerable packages" vulnerability-report.txt; then
          echo "::error::Vulnerable packages detected!"
          exit 1
        fi
    
    - name: Upload vulnerability report
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: vulnerability-report
        path: vulnerability-report.txt

  codeql-analysis:
    runs-on: ubuntu-latest
    permissions:
      actions: read
      contents: read
      security-events: write
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Initialize CodeQL
      uses: github/codeql-action/init@v3
      with:
        languages: csharp
        queries: security-and-quality
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v3

  secret-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
    
    - name: Run Gitleaks
      uses: gitleaks/gitleaks-action@v2
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

  security-audit:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'RaOS'
        path: '.'
        format: 'HTML'
    
    - name: Upload OWASP report
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: owasp-report
        path: reports/
```

**Create .github/workflows/codeowners-check.yml:**
```yaml
name: CODEOWNERS Validation

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  check-codeowners:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Verify CODEOWNERS approval
      uses: actions/github-script@v7
      with:
        script: |
          const { data: reviews } = await github.rest.pulls.listReviews({
            owner: context.repo.owner,
            repo: context.repo.repo,
            pull_number: context.issue.number
          });
          
          const approvals = reviews.filter(r => r.state === 'APPROVED');
          
          if (approvals.length === 0) {
            core.setFailed('No CODEOWNERS approval found. At least one CODEOWNER must approve.');
          }
```

---

## 4. Production HTTPS Enforcement

### Priority: 🔴 CRITICAL - Must implement in production

### Configuration

**Add to Program.cs (Production mode only):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Force HTTPS in production
if (builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
    
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

var app = builder.Build();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    if (builder.Environment.IsProduction())
    {
        context.Response.Headers.Add("Strict-Transport-Security", 
            "max-age=31536000; includeSubDomains; preload");
    }
    
    await next();
});

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

**Nginx Configuration (Production):**
```nginx
server {
    listen 80;
    server_name agpstudios.online;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name agpstudios.online;
    
    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/agpstudios.online/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/agpstudios.online/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    
    # HSTS
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
    
    # Security Headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    location / {
        proxy_pass http://localhost:7077;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## 5. Monitoring & Alerting

### Priority: 🟠 HIGH - Implement for production observability

### Application Insights Integration

**Add Package:**
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

**Configure (Program.cs):**
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

**Add to appsettings.Production.json:**
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning"
      }
    }
  }
}
```

### Custom Metrics

**Create MetricsService.cs:**
```csharp
public class MetricsService
{
    private readonly TelemetryClient _telemetry;
    
    public void TrackLoginAttempt(string username, bool success, string ipAddress)
    {
        _telemetry.TrackEvent("LoginAttempt", new Dictionary<string, string>
        {
            { "username", username },
            { "success", success.ToString() },
            { "ipAddress", ipAddress }
        });
        
        _telemetry.TrackMetric("LoginAttempts", 1, new Dictionary<string, string>
        {
            { "result", success ? "success" : "failure" }
        });
    }
    
    public void TrackDiskUsage(long bytesUsed, long bytesAvailable)
    {
        var usagePercent = (double)bytesUsed / (bytesUsed + bytesAvailable) * 100;
        _telemetry.TrackMetric("DiskUsagePercent", usagePercent);
        
        if (usagePercent > 85)
        {
            _telemetry.TrackEvent("HighDiskUsage", new Dictionary<string, string>
            {
                { "percentUsed", usagePercent.ToString("F2") },
                { "severity", usagePercent > 95 ? "critical" : "warning" }
            });
        }
    }
}
```

---

## 6. Incident Response Plan

### Priority: 🟡 MEDIUM - Document before production

### Create INCIDENT_RESPONSE_PLAN.md

**Structure:**
1. **Incident Classification**
   - P0: Critical security breach (data leak, complete system compromise)
   - P1: High severity (authentication bypass, unauthorized access)
   - P2: Medium severity (denial of service, information disclosure)
   - P3: Low severity (security misconfiguration, minor vulnerability)

2. **Response Procedures**
   - Detection & Triage (15 min)
   - Containment (30 min for P0/P1)
   - Investigation & Analysis
   - Remediation
   - Recovery
   - Post-Incident Review

3. **Emergency Contacts**
   - On-Call Engineer: [Contact]
   - Security Team: [Email/Slack]
   - CTO/VP Engineering: [Phone]
   - Legal Counsel: [Contact]
   - PR/Communications: [Contact]

4. **Communication Templates**
   - Internal notification
   - External customer notification (if applicable)
   - Post-mortem report

---

## 7. Branch Protection & CODEOWNERS

### Priority: 🟠 HIGH - Configure before production

### Create .github/CODEOWNERS
```
# RaCore Security & MainFrame Code Owners

# Default owners for everything
* @buffbot88

# Security-sensitive files require security team approval
SECURITY*.md @security-team @buffbot88
/RaCore/Modules/Extensions/Authentication/ @security-team @buffbot88
/RaCore/Engine/FirstRunManager.cs @security-team @buffbot88

# CI/CD and workflows
/.github/workflows/ @devops-team @buffbot88

# Configuration files
*.json @buffbot88
server-config.json @security-team @buffbot88
```

### GitHub Branch Protection Settings

**Configure for `main` branch:**
1. Require pull request before merging
2. Require 1+ approvals
3. Dismiss stale approvals when new commits are pushed
4. Require review from CODEOWNERS
5. Require status checks to pass:
   - `build-and-test`
   - `dependency-scan`
   - `codeql-analysis`
   - `secret-scan`
6. Require branches to be up to date
7. Require linear history
8. Do not allow force pushes
9. Do not allow deletions

---

## 8. Production Deployment Checklist

### Pre-Deployment

- [ ] Change default admin credentials (admin/admin123)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure production database path
- [ ] Set up HTTPS certificates (Let's Encrypt)
- [ ] Configure production CORS origins
- [ ] Enable HSTS
- [ ] Set up monitoring (Application Insights or equivalent)
- [ ] Configure log rotation
- [ ] Set up automated backups
- [ ] Document database connection strings (use environment variables)
- [ ] Configure rate limiting
- [ ] Set up firewall rules
- [ ] Disable development features
- [ ] Enable security headers
- [ ] Configure session timeout
- [ ] Set up alert notifications

### Post-Deployment

- [ ] Verify HTTPS is working
- [ ] Test rate limiting
- [ ] Verify authentication flows
- [ ] Check security headers
- [ ] Monitor application logs
- [ ] Verify backups are running
- [ ] Test failsafe recovery
- [ ] Monitor resource usage (CPU, memory, disk)
- [ ] Verify monitoring/alerting
- [ ] Document any production issues
- [ ] Schedule security review (30 days)

---

## 9. Security Testing

### Automated Tests

**Create SecurityTests.cs:**
```csharp
[TestClass]
public class SecurityTests
{
    [TestMethod]
    public async Task RateLimiting_ShouldBlock_AfterTooManyRequests()
    {
        var client = new HttpClient();
        var successCount = 0;
        var blockedCount = 0;
        
        for (int i = 0; i < 10; i++)
        {
            var response = await client.PostAsync("http://localhost:7077/api/auth/login", 
                new StringContent("{\"username\":\"test\",\"password\":\"test\"}", 
                Encoding.UTF8, "application/json"));
            
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                blockedCount++;
            else
                successCount++;
        }
        
        Assert.IsTrue(blockedCount > 0, "Rate limiting should block some requests");
    }
    
    [TestMethod]
    public void PasswordHashing_ShouldUse_SecureAlgorithm()
    {
        var authModule = new AuthenticationModule();
        var (hash, salt) = authModule.HashPassword("SecurePassword123!");
        
        Assert.IsTrue(hash.Length >= 64, "Hash should be at least 64 bytes");
        Assert.IsTrue(salt.Length >= 32, "Salt should be at least 32 bytes");
        Assert.IsFalse(hash.Contains("SecurePassword"), "Hash should not contain plain password");
    }
    
    [TestMethod]
    public async Task SessionExpiry_ShouldInvalidate_OldTokens()
    {
        var authModule = new AuthenticationModule();
        var (_, token) = await authModule.LoginAsync("testuser", "password", "127.0.0.1");
        
        // Fast-forward time (mock)
        var session = await authModule.ValidateTokenAsync(token);
        session.ExpiresAtUtc = DateTime.UtcNow.AddHours(-1);
        
        var validSession = await authModule.ValidateTokenAsync(token);
        Assert.IsNull(validSession, "Expired session should be invalid");
    }
}
```

---

## 10. Compliance & Auditing

### GDPR Compliance

**User Data Export (Add to UserProfilesModule):**
```csharp
public async Task<string> ExportUserDataAsync(string userId)
{
    var user = await GetUserAsync(userId);
    var sessions = await GetUserSessionsAsync(userId);
    var auditLog = await GetUserAuditLogAsync(userId);
    
    var exportData = new
    {
        User = new
        {
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAtUtc,
            user.Role
        },
        Sessions = sessions.Select(s => new { s.CreatedAtUtc, s.ExpiresAtUtc }),
        AuditLog = auditLog.Select(a => new { a.Timestamp, a.Action, a.Details })
    };
    
    return JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
}
```

**User Data Deletion:**
```csharp
public async Task<bool> DeleteUserDataAsync(string userId)
{
    // Delete user account
    _users.RemoveAll(u => u.Id == userId);
    
    // Delete sessions
    foreach (var session in _sessions.Where(s => s.Value.UserId == userId))
    {
        _sessions.TryRemove(session.Key, out _);
    }
    
    // Anonymize audit logs (keep for compliance, remove PII)
    var userLogs = _securityEvents.Where(e => e.UserId == userId);
    foreach (var log in userLogs)
    {
        log.UserId = "[DELETED]";
        log.Username = "[DELETED]";
        log.Details = "[User data deleted per GDPR request]";
    }
    
    await LogSecurityEventAsync("user_deleted", null, userId, null, 
        "User data deleted per GDPR request", true);
    
    return true;
}
```

---

## 📚 Additional Resources

- [OWASP Top 10](https://owasp.org/Top10/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [CIS Controls](https://www.cisecurity.org/controls)
- [Azure Security Best Practices](https://learn.microsoft.com/en-us/azure/security/fundamentals/best-practices-and-patterns)
- [.NET Security Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/)

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**Maintained By:** RaOS Security Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
