namespace Abstractions;

/// <summary>
/// Universal currency exchange constants for RaCore games.
/// Standardizes RaCoin to Gold conversion ratios.
/// </summary>
public static class CurrencyExchangeConstants
{
    /// <summary>
    /// Standard RaCoin amount for exchange operations.
    /// 1000 RaCoin = 400,000 Gold (Universal Ratio)
    /// </summary>
    public const decimal StandardRaCoinAmount = 1000m;
    
    /// <summary>
    /// Standard Gold amount for exchange operations.
    /// 1000 RaCoin = 400,000 Gold (Universal Ratio)
    /// </summary>
    public const decimal StandardGoldAmount = 400000m;
    
    /// <summary>
    /// Exchange rate: Gold per 1 RaCoin
    /// </summary>
    public const decimal GoldPerRaCoin = StandardGoldAmount / StandardRaCoinAmount;
    
    /// <summary>
    /// Exchange rate: RaCoin per 1 Gold
    /// </summary>
    public const decimal RaCoinPerGold = StandardRaCoinAmount / StandardGoldAmount;
    
    /// <summary>
    /// Convert RaCoin to Gold using the standard ratio.
    /// </summary>
    public static decimal RaCoinToGold(decimal racoinAmount)
    {
        return racoinAmount * GoldPerRaCoin;
    }
    
    /// <summary>
    /// Convert Gold to RaCoin using the standard ratio.
    /// </summary>
    public static decimal GoldToRaCoin(decimal goldAmount)
    {
        return goldAmount * RaCoinPerGold;
    }
}

/// <summary>
/// Represents a currency exchange transaction between RaCoin and Gold.
/// </summary>
public class CurrencyExchangeTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public CurrencyExchangeType ExchangeType { get; set; }
    public decimal RaCoinAmount { get; set; }
    public decimal GoldAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

/// <summary>
/// Type of currency exchange.
/// </summary>
public enum CurrencyExchangeType
{
    RaCoinToGold,
    GoldToRaCoin
}

/// <summary>
/// Response from a currency exchange operation.
/// </summary>
public class CurrencyExchangeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? NewRaCoinBalance { get; set; }
    public decimal? NewGoldBalance { get; set; }
    public CurrencyExchangeTransaction? Transaction { get; set; }
}

/// <summary>
/// Represents a Gold wallet for a user in game systems.
/// </summary>
public class GoldWallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0m;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Market monitoring alert for detecting unusual activity.
/// </summary>
public class MarketAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MarketAlertType AlertType { get; set; }
    public MarketAlertSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
}

/// <summary>
/// Types of market alerts.
/// </summary>
public enum MarketAlertType
{
    UnusualVolume,
    RapidPriceChange,
    SuspiciousActivity,
    LargeTransaction,
    MarketManipulation,
    SystemImbalance
}

/// <summary>
/// Severity levels for market alerts.
/// </summary>
public enum MarketAlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Market statistics snapshot.
/// </summary>
public class MarketStatistics
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalRaCoinsInCirculation { get; set; }
    public decimal TotalGoldInCirculation { get; set; }
    public decimal CurrentExchangeRate { get; set; }
    public decimal StandardExchangeRate { get; set; }
    public decimal DeviationPercentage { get; set; }
    public int TotalExchangeTransactions { get; set; }
    public decimal ExchangeVolume24h { get; set; }
    public List<MarketAlert> ActiveAlerts { get; set; } = new();
}
