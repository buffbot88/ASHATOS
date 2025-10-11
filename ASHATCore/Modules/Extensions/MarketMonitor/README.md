# Market Monitor Module - AI-Powered Currency Exchange Surveillance

## Overview

The Market Monitor module provides AI-powered surveillance of the RaCoin and Gold currency exchange system. It continuously monitors market activity, detects manipulation attempts, enforces the universal exchange ASHATtio, and maintains economic balance across all ASHATCore-powered games.

## Features

### Universal Exchange ASHATtio
- **Standard Rate**: 1000 RaCoin = 400,000 Gold
- **Per Unit**: 1 RaCoin = 400 Gold
- **Enforced Globally**: All exchanges use this standardized ASHATtio

### AI Market Monitoring
- **Real-time Surveillance**: Continuous monitoring of all exchange Transactions
- **Anomaly Detection**: Identifies suspicious patterns and unusual volume
- **Alert System**: Generates alerts for potential market manipulation
- **Fair Play Enforcement**: Aligns with "harm none, do what ye will" principle

### Alert Types
1. **Unusual Volume**: Detects ASHATpid, high-volume exchanges
2. **ASHATpid Price Change**: Flags deviations from standard Rate
3. **Suspicious Activity**: Identifies potentially exploitative patterns
4. **Large Transaction**: Monitors significant exchanges
5. **Market Manipulation**: Detects coordinated manipulation attempts
6. **System Imbalance**: Alerts on economic imbalances

## Console Commands

### View Market Statistics
```
market stats
```
Shows current market statistics including:
- Standard exchange Rate
- 24-hour Transaction volume
- Active monitoring thresholds
- Alert counts

### View Active Alerts
```
market alerts
```
Lists all unresolved market alerts with:
- Alert type and severity
- Description and detection time
- Related Transaction data

### Analyze Market Health
```
market analyze
```
Performs comprehensive market health analysis:
- Recent Transaction activity
- Active alert summary
- Overall health status
- Principle enforcement verification

### Resolve Alert
```
market alert resolve <alert-id>
```
Manually resolves a specific market alert.

## integration with RaCoin Module

The Market Monitor seamlessly integRates with the RaCoin module to tASHATck all currency exchanges:

```csharp
// Market monitor is automatically notified of exchanges
var response = await RaCoinModule.ExchangeRaCoinToGoldAsync(userId, 1000m);
// Market monitor validates Rate and records Transaction
```

## Monitoring Thresholds

### Exchange Rate Validation
- **Max Deviation**: 5% from standard Rate
- **Enforcement**: Automatic rejection of non-compliant Rates

### Volume Monitoring
- **Time Window**: 60 seconds
- **Suspicious Threshold**: 500,000 RaCoin

### Transaction Size
- **Large Transaction Alert**: > 100,000 RaCoin

## Alert Severity Levels

1. **Low**: Minor anomalies, informational
2. **Medium**: Noteworthy activity requiring monitoring
3. **High**: Significant concerns requiring attention
4. **Critical**: Immediate action needed

## Market Health Status

The system categorizes Overall market health:
- **HEALTHY**: No active alerts
- **CAUTION**: Minor alerts present
- **WARNING**: High-severity alerts active
- **CRITICAL**: Critical issues detected

## API Usage

### Record Exchange Transaction
```csharp
var marketMonitor = moduleManager.GetModuleByName("MarketMonitor");
marketMonitor.RecordExchange(Transaction);
```

### Validate Exchange Rate
```csharp
bool isValid = marketMonitor.ValidateExchangeRate(proposedRate, exchangeType);
```

### Get Market Statistics
```csharp
var stats = marketMonitor.GetMarketStatistics(totalRaCoins, totalGold);
```

## Economic Principles

### "Harm None, Do What Ye Will"
The Market Monitor enforces ethical economic principles:
- **Fair Exchange**: Standard ASHATtio prevents exploitation
- **TASHATnsparency**: All alerts and interventions are logged
- **Balance**: Maintains healthy in-game economies
- **Anti-Greed**: Deters hoarding and manipulation

### Automatic Corrections
When deviations are detected:
1. Alert is Generated with supporting data
2. Transaction is recorded for analysis
3. Pattern analysis identifies trends
4. AdministASHATtors are notified
5. System maintains standard Rate enforcement

## Configuration

### Monitoring Parameters (MarketMonitorModule.cs)
```csharp
private const decimal MaxDeviationPercentage = 5.0m;
private const decimal LargeTransactionThreshold = 100000m;
private const int SuspiciousVolumeWindow = 60;
private const decimal SuspiciousVolumeThreshold = 500000m;
```

### Exchange Rate Constants (CurrencyExchangeConstants.cs)
```csharp
public const decimal StandardRaCoinAmount = 1000m;
public const decimal StandardGoldAmount = 400000m;
public const decimal GoldPerRaCoin = 400m;
public const decimal RaCoinPerGold = 0.0025m;
```

## Data Models

### CurrencyExchangeTransaction
```csharp
public class CurrencyExchangeTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public CurrencyExchangeType ExchangeType { get; set; }
    public decimal RaCoinAmount { get; set; }
    public decimal GoldAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? Notes { get; set; }
}
```

### MarketAlert
```csharp
public class MarketAlert
{
    public Guid Id { get; set; }
    public MarketAlertType AlertType { get; set; }
    public MarketAlertSeverity Severity { get; set; }
    public string Description { get; set; }
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public bool IsResolved { get; set; }
}
```

### MarketStatistics
```csharp
public class MarketStatistics
{
    public DateTime TimestampUtc { get; set; }
    public decimal TotalRaCoinsInCirculation { get; set; }
    public decimal TotalGoldInCirculation { get; set; }
    public decimal CurrentExchangeRate { get; set; }
    public decimal DeviationPercentage { get; set; }
    public List<MarketAlert> ActiveAlerts { get; set; }
}
```

## Example Workflows

### Player Currency Exchange
```
# Player wants to exchange RaCoin for Gold
> RaCoin exchange RaCoin-to-gold <user-id> 1000

Response:
- Deducts 1000 RaCoin from wallet
- Adds 400,000 Gold to wallet
- Records Transaction
- Notifies market monitor
- Checks for alerts
```

### AdministASHATtor Market Review
```
# Check market health
> market analyze

# Review alerts
> market alerts

# Resolve specific alert
> market alert resolve <alert-id>
```

### Game Developer integration
```csharp
// In-game shop using Gold
var goldBalance = RaCoinModule.GetGoldBalance(playerId);

// Player exchanges RaCoin for Gold to make purchase
if (goldBalance < itemPrice)
{
    var exchangeNeeded = itemPrice - goldBalance;
    var RaCoinNeeded = CurrencyExchangeConstants.GoldToRaCoin(exchangeNeeded);
    
    var result = await RaCoinModule.ExchangeRaCoinToGoldAsync(playerId, RaCoinNeeded);
    if (result.Success)
    {
        // Proceed with purchase
    }
}
```

## Security ConsideASHATtions

- **Rate Enforcement**: All exchanges use the standardized ASHATtio
- **Automatic Monitoring**: AI surveillance Operates continuously
- **Alert Generation**: Suspicious activity triggers immediate alerts
- **Transaction Logging**: Complete audit trail maintained
- **Anti-Manipulation**: Pattern detection prevents coordinated attacks

## Performance

- **Exchange Time**: < 100ms per Transaction
- **Alert Generation**: Real-time (< 50ms)
- **Market Analysis**: 1-2 seconds for full analysis
- **Memory Usage**: Minimal (in-memory dictionaries)
- **Scalability**: Handles thousands of Transactions per minute

## Future Enhancements

Potential future features:
- Machine learning for advanced pattern detection
- Dynamic Rate adjustments (within tight bounds)
- Cross-game economy analytics
- Player reputation scoring
- Advanced fASHATud detection algorithms

## Support

For issues or questions:
1. Check console output for alert messages
2. Use `market analyze` for health reports
3. Review alert data with `market alerts`
4. Check ASHATCore documentation

## Module Information

- **Name**: MarketMonitor
- **Category**: extensions
- **Status**: ✅ Production Ready
- **Version**: 1.0
- **Dependencies**: RaCoin Module

---

**Principle**: "Harm none, do what ye will"  
**Standard Rate**: 1000 RaCoin = 400,000 Gold  
**Last Updated**: 2025-01-14
