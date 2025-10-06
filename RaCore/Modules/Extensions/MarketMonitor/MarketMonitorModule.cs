using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.MarketMonitor;

/// <summary>
/// AI Market Monitor Module - Continuously monitors RaCoin and Gold markets.
/// Detects manipulation, enforces fair exchange rates, and prevents exploitative behavior.
/// Aligns with "harm none, do what ye will" principle by deterring greed and maintaining economic balance.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class MarketMonitorModule : ModuleBase
{
    public override string Name => "MarketMonitor";

    private readonly ConcurrentDictionary<Guid, MarketAlert> _alerts = new();
    private readonly ConcurrentDictionary<Guid, CurrencyExchangeTransaction> _exchangeHistory = new();
    private readonly object _lock = new();
    private DateTime _lastAnalysisTime = DateTime.UtcNow;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    
    // Monitoring thresholds
    private const decimal MaxDeviationPercentage = 5.0m; // Max 5% deviation from standard rate
    private const decimal LargeTransactionThreshold = 100000m; // Alert on exchanges > 100k RaCoin
    private const int SuspiciousVolumeWindow = 60; // seconds
    private const decimal SuspiciousVolumeThreshold = 500000m; // 500k RaCoin in 60 seconds

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("Market Monitor module initialized - AI-powered market surveillance active");
        LogInfo($"Standard Exchange Rate: {CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold");
        LogInfo($"Maximum Deviation Allowed: {MaxDeviationPercentage}%");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("market stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetMarketStats();
        }

        if (text.Equals("market alerts", StringComparison.OrdinalIgnoreCase))
        {
            return GetActiveAlerts();
        }

        if (text.Equals("market analyze", StringComparison.OrdinalIgnoreCase))
        {
            return AnalyzeMarket();
        }

        if (text.StartsWith("market alert resolve ", StringComparison.OrdinalIgnoreCase))
        {
            var alertIdStr = text["market alert resolve ".Length..].Trim();
            if (Guid.TryParse(alertIdStr, out var alertId))
            {
                return ResolveAlert(alertId);
            }
            return "Invalid alert ID format";
        }

        return "Unknown market monitor command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Market Monitor commands:",
            "  market stats                  - Show current market statistics",
            "  market alerts                 - Show active market alerts",
            "  market analyze                - Perform market health analysis",
            "  market alert resolve <id>     - Resolve a specific alert",
            "  help                          - Show this help message",
            "",
            $"Standard Exchange Rate: {CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold",
            $"Rate Per Unit: 1 RaCoin = {CurrencyExchangeConstants.GoldPerRaCoin} Gold"
        );
    }

    /// <summary>
    /// Record an exchange transaction for monitoring.
    /// </summary>
    public void RecordExchange(CurrencyExchangeTransaction transaction)
    {
        _exchangeHistory.TryAdd(transaction.Id, transaction);
        
        // Check for large transaction
        if (transaction.RaCoinAmount >= LargeTransactionThreshold)
        {
            CreateAlert(
                MarketAlertType.LargeTransaction,
                MarketAlertSeverity.Medium,
                $"Large exchange detected: {transaction.RaCoinAmount} RaCoin ({transaction.ExchangeType})",
                new Dictionary<string, object>
                {
                    ["TransactionId"] = transaction.Id,
                    ["UserId"] = transaction.UserId,
                    ["RaCoinAmount"] = transaction.RaCoinAmount,
                    ["GoldAmount"] = transaction.GoldAmount,
                    ["ExchangeType"] = transaction.ExchangeType.ToString()
                }
            );
        }

        // Check for exchange rate deviation
        var expectedRate = transaction.ExchangeType == CurrencyExchangeType.RaCoinToGold
            ? CurrencyExchangeConstants.GoldPerRaCoin
            : CurrencyExchangeConstants.RaCoinPerGold;
        
        var deviation = Math.Abs((transaction.ExchangeRate - expectedRate) / expectedRate * 100);
        
        if (deviation > MaxDeviationPercentage)
        {
            CreateAlert(
                MarketAlertType.RapidPriceChange,
                MarketAlertSeverity.High,
                $"Exchange rate deviation detected: {deviation:F2}% from standard",
                new Dictionary<string, object>
                {
                    ["TransactionId"] = transaction.Id,
                    ["ExpectedRate"] = expectedRate,
                    ["ActualRate"] = transaction.ExchangeRate,
                    ["Deviation"] = deviation
                }
            );
        }

        // Check for suspicious volume
        CheckSuspiciousVolume();
    }

    private void CheckSuspiciousVolume()
    {
        var now = DateTime.UtcNow;
        var recentTransactions = _exchangeHistory.Values
            .Where(t => (now - t.TimestampUtc).TotalSeconds <= SuspiciousVolumeWindow)
            .ToList();

        var totalVolume = recentTransactions.Sum(t => t.RaCoinAmount);

        if (totalVolume >= SuspiciousVolumeThreshold)
        {
            CreateAlert(
                MarketAlertType.UnusualVolume,
                MarketAlertSeverity.High,
                $"Suspicious volume detected: {totalVolume} RaCoin in {SuspiciousVolumeWindow} seconds",
                new Dictionary<string, object>
                {
                    ["Volume"] = totalVolume,
                    ["TimeWindowSeconds"] = SuspiciousVolumeWindow,
                    ["TransactionCount"] = recentTransactions.Count
                }
            );
        }
    }

    /// <summary>
    /// Validate exchange rate against standard ratio.
    /// Returns true if rate is acceptable, false if it deviates too much.
    /// </summary>
    public bool ValidateExchangeRate(decimal proposedRate, CurrencyExchangeType exchangeType)
    {
        var standardRate = exchangeType == CurrencyExchangeType.RaCoinToGold
            ? CurrencyExchangeConstants.GoldPerRaCoin
            : CurrencyExchangeConstants.RaCoinPerGold;

        var deviation = Math.Abs((proposedRate - standardRate) / standardRate * 100);
        return deviation <= MaxDeviationPercentage;
    }

    /// <summary>
    /// Get the enforced exchange rate (always returns standard rate).
    /// </summary>
    public decimal GetEnforcedExchangeRate(CurrencyExchangeType exchangeType)
    {
        return exchangeType == CurrencyExchangeType.RaCoinToGold
            ? CurrencyExchangeConstants.GoldPerRaCoin
            : CurrencyExchangeConstants.RaCoinPerGold;
    }

    private void CreateAlert(MarketAlertType type, MarketAlertSeverity severity, string description, Dictionary<string, object> data)
    {
        // Check if similar alert already exists and is unresolved
        var existingAlert = _alerts.Values.FirstOrDefault(a => 
            !a.IsResolved && 
            a.AlertType == type && 
            (DateTime.UtcNow - a.DetectedAt).TotalMinutes < 5);

        if (existingAlert != null)
        {
            // Don't create duplicate alerts within 5 minutes
            return;
        }

        var alert = new MarketAlert
        {
            AlertType = type,
            Severity = severity,
            Description = description,
            DetectedAt = DateTime.UtcNow,
            Data = data
        };

        _alerts.TryAdd(alert.Id, alert);
        LogInfo($"MARKET ALERT [{severity}]: {description}");
    }

    private string GetMarketStats()
    {
        var now = DateTime.UtcNow;
        var last24h = now.AddHours(-24);

        var recentTransactions = _exchangeHistory.Values
            .Where(t => t.TimestampUtc >= last24h)
            .ToList();

        var stats = new
        {
            StandardExchangeRate = new
            {
                RaCoin = CurrencyExchangeConstants.StandardRaCoinAmount,
                Gold = CurrencyExchangeConstants.StandardGoldAmount,
                RatePerRaCoin = CurrencyExchangeConstants.GoldPerRaCoin
            },
            Last24Hours = new
            {
                TotalTransactions = recentTransactions.Count,
                TotalRaCoinExchanged = recentTransactions.Sum(t => t.RaCoinAmount),
                TotalGoldExchanged = recentTransactions.Sum(t => t.GoldAmount),
                AverageTransactionSize = recentTransactions.Any() 
                    ? recentTransactions.Average(t => t.RaCoinAmount) 
                    : 0m
            },
            Monitoring = new
            {
                ActiveAlerts = _alerts.Values.Count(a => !a.IsResolved),
                TotalAlertsGenerated = _alerts.Count,
                MaxDeviationAllowed = $"{MaxDeviationPercentage}%",
                LargeTransactionThreshold = LargeTransactionThreshold,
                LastAnalysis = _lastAnalysisTime
            }
        };

        return JsonSerializer.Serialize(stats, _jsonOptions);
    }

    private string GetActiveAlerts()
    {
        var activeAlerts = _alerts.Values
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.DetectedAt)
            .Take(50)
            .Select(a => new
            {
                a.Id,
                Type = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                a.Description,
                DetectedAt = a.DetectedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                a.Data
            })
            .ToList();

        if (activeAlerts.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                Message = "No active alerts - Market conditions are healthy",
                StandardRate = $"{CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold"
            }, _jsonOptions);
        }

        return JsonSerializer.Serialize(new
        {
            ActiveAlertCount = activeAlerts.Count,
            Alerts = activeAlerts
        }, _jsonOptions);
    }

    private string AnalyzeMarket()
    {
        _lastAnalysisTime = DateTime.UtcNow;
        var now = DateTime.UtcNow;

        var sb = new StringBuilder();
        sb.AppendLine("=== Market Health Analysis ===");
        sb.AppendLine();
        sb.AppendLine($"Analysis Time: {now:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine();
        sb.AppendLine("Standard Exchange Rate:");
        sb.AppendLine($"  {CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold");
        sb.AppendLine($"  1 RaCoin = {CurrencyExchangeConstants.GoldPerRaCoin} Gold");
        sb.AppendLine($"  1 Gold = {CurrencyExchangeConstants.RaCoinPerGold:F6} RaCoin");
        sb.AppendLine();

        // Analyze recent activity
        var last1h = now.AddHours(-1);
        var recentTransactions = _exchangeHistory.Values
            .Where(t => t.TimestampUtc >= last1h)
            .ToList();

        sb.AppendLine($"Recent Activity (Last Hour):");
        sb.AppendLine($"  Transactions: {recentTransactions.Count}");
        sb.AppendLine($"  Total Volume: {recentTransactions.Sum(t => t.RaCoinAmount):N2} RaCoin");
        sb.AppendLine();

        // Check for anomalies
        var activeAlerts = _alerts.Values.Where(a => !a.IsResolved).ToList();
        sb.AppendLine($"Active Alerts: {activeAlerts.Count}");
        
        if (activeAlerts.Count > 0)
        {
            var criticalCount = activeAlerts.Count(a => a.Severity == MarketAlertSeverity.Critical);
            var highCount = activeAlerts.Count(a => a.Severity == MarketAlertSeverity.High);
            
            sb.AppendLine($"  Critical: {criticalCount}");
            sb.AppendLine($"  High: {highCount}");
            sb.AppendLine($"  Medium/Low: {activeAlerts.Count - criticalCount - highCount}");
        }
        sb.AppendLine();

        // Market health status
        var healthStatus = activeAlerts.Count == 0 ? "HEALTHY" :
                          activeAlerts.Any(a => a.Severity == MarketAlertSeverity.Critical) ? "CRITICAL" :
                          activeAlerts.Any(a => a.Severity == MarketAlertSeverity.High) ? "WARNING" : "CAUTION";

        sb.AppendLine($"Overall Market Health: {healthStatus}");
        sb.AppendLine();
        sb.AppendLine("Principle Enforcement: \"Harm none, do what ye will\"");
        sb.AppendLine("  ✓ Standard ratio enforced across all exchanges");
        sb.AppendLine("  ✓ Monitoring for exploitative behavior");
        sb.AppendLine("  ✓ Automatic alerts for suspicious activity");

        return sb.ToString();
    }

    private string ResolveAlert(Guid alertId)
    {
        if (_alerts.TryGetValue(alertId, out var alert))
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.Resolution = "Manually resolved by administrator";
            
            LogInfo($"Market alert {alertId} resolved: {alert.Description}");
            
            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Alert resolved successfully",
                Alert = new
                {
                    alert.Id,
                    Type = alert.AlertType.ToString(),
                    alert.Description,
                    alert.ResolvedAt
                }
            }, _jsonOptions);
        }

        return JsonSerializer.Serialize(new
        {
            Success = false,
            Message = "Alert not found"
        }, _jsonOptions);
    }

    /// <summary>
    /// Get market statistics for reporting.
    /// </summary>
    public MarketStatistics GetMarketStatistics(decimal totalRaCoins, decimal totalGold)
    {
        var activeAlerts = _alerts.Values.Where(a => !a.IsResolved).ToList();
        var last24h = DateTime.UtcNow.AddHours(-24);
        var recent = _exchangeHistory.Values.Where(t => t.TimestampUtc >= last24h).ToList();

        return new MarketStatistics
        {
            TimestampUtc = DateTime.UtcNow,
            TotalRaCoinsInCirculation = totalRaCoins,
            TotalGoldInCirculation = totalGold,
            CurrentExchangeRate = CurrencyExchangeConstants.GoldPerRaCoin,
            StandardExchangeRate = CurrencyExchangeConstants.GoldPerRaCoin,
            DeviationPercentage = 0m, // Always 0 since we enforce standard rate
            TotalExchangeTransactions = _exchangeHistory.Count,
            ExchangeVolume24h = recent.Sum(t => t.RaCoinAmount),
            ActiveAlerts = activeAlerts
        };
    }
}
