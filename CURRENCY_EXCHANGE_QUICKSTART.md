# Currency Exchange Quick Start Guide

## Overview

RaCore now includes a universal currency exchange system with AI-powered market monitoring. This guide will help you get started with exchanging RaCoin and Gold in your games.

## Standard Exchange Ratio

**1000 RaCoin = 400,000 Gold**

- 1 RaCoin = 400 Gold
- 1 Gold = 0.0025 RaCoin

This ratio is enforced across all RaCore-powered games to ensure fairness and prevent exploitation.

## For Players

### Check Your Balance

Check your RaCoin balance:
```
racoin balance <your-user-id>
```

Check your Gold balance:
```
racoin gold balance <your-user-id>
```

### Exchange RaCoin to Gold

Convert RaCoin to Gold for in-game purchases:
```
racoin exchange racoin-to-gold <your-user-id> <amount>
```

Example: Exchange 1000 RaCoin for 400,000 Gold:
```
racoin exchange racoin-to-gold <your-user-id> 1000
```

### Exchange Gold to RaCoin

Convert Gold back to RaCoin:
```
racoin exchange gold-to-racoin <your-user-id> <amount>
```

Example: Exchange 400,000 Gold for 1000 RaCoin:
```
racoin exchange gold-to-racoin <your-user-id> 400000
```

### View Exchange Rates

See current exchange rates and system statistics:
```
racoin exchange rates
```

## For Game Developers

### Quick Integration Example

```csharp
// Get the RaCoin module
var racoinModule = moduleManager.GetModuleByName("RaCoin") as IRaCoinModule;

// Player wants to buy an item that costs 1,000,000 Gold
decimal itemPrice = 1000000m;
Guid playerId = GetCurrentPlayerId();

// Check if player has enough Gold
decimal goldBalance = racoinModule.GetGoldBalance(playerId);

if (goldBalance < itemPrice)
{
    // Calculate how much RaCoin they need to exchange
    decimal goldNeeded = itemPrice - goldBalance;
    decimal racoinNeeded = CurrencyExchangeConstants.GoldToRaCoin(goldNeeded);
    
    // Prompt player to exchange
    Console.WriteLine($"You need {goldNeeded} more Gold.");
    Console.WriteLine($"Exchange {racoinNeeded} RaCoin to continue.");
    
    // Perform the exchange (with player consent)
    var result = await racoinModule.ExchangeRaCoinToGoldAsync(playerId, racoinNeeded);
    
    if (result.Success)
    {
        // Now complete the purchase
        ProcessItemPurchase(playerId, itemPrice);
    }
}
else
{
    // Player has enough Gold, proceed with purchase
    ProcessItemPurchase(playerId, itemPrice);
}
```

### Currency Conversion Helper Functions

```csharp
using Abstractions;

// Convert RaCoin to Gold
decimal gold = CurrencyExchangeConstants.RaCoinToGold(1000m);
// Result: 400000

// Convert Gold to RaCoin
decimal racoin = CurrencyExchangeConstants.GoldToRaCoin(400000m);
// Result: 1000

// Get exchange rate
decimal goldPerRacoin = CurrencyExchangeConstants.GoldPerRaCoin;
// Result: 400

decimal racoinPerGold = CurrencyExchangeConstants.RaCoinPerGold;
// Result: 0.0025
```

### Quest Rewards with Gold

```csharp
// Define a quest that rewards Gold
var quest = new Quest
{
    Name = "Dragon Slayer",
    Description = "Defeat the ancient dragon",
    Rewards = new List<QuestReward>
    {
        new() { RewardType = "Gold", Value = 500000 },
        new() { RewardType = "Experience", Value = 1000 }
    }
};

// When quest completes
foreach (var reward in quest.Rewards)
{
    if (reward.RewardType == "Gold")
    {
        // Add Gold to player's wallet
        var goldWallet = racoinModule.GetOrCreateGoldWallet(playerId);
        goldWallet.Balance += reward.Value;
        
        // Optionally show RaCoin equivalent
        var racoinValue = CurrencyExchangeConstants.GoldToRaCoin(reward.Value);
        Console.WriteLine($"Earned {reward.Value} Gold (≈ {racoinValue} RaCoin)");
    }
}
```

### NPC Merchant Shop

```csharp
public class MerchantShop
{
    private IRaCoinModule _racoinModule;
    
    public async Task<bool> SellItem(Guid playerId, string itemName, decimal goldPrice)
    {
        Console.WriteLine($"{itemName}: {goldPrice} Gold");
        
        var goldBalance = _racoinModule.GetGoldBalance(playerId);
        
        if (goldBalance >= goldPrice)
        {
            // Deduct Gold directly
            var goldWallet = _racoinModule.GetOrCreateGoldWallet(playerId);
            goldWallet.Balance -= goldPrice;
            return true;
        }
        else
        {
            // Offer to pay with RaCoin
            var racoinNeeded = CurrencyExchangeConstants.GoldToRaCoin(goldPrice);
            Console.WriteLine($"Not enough Gold. Pay {racoinNeeded} RaCoin instead?");
            
            var racoinBalance = _racoinModule.GetBalance(playerId);
            if (racoinBalance >= racoinNeeded)
            {
                var result = await _racoinModule.DeductAsync(
                    playerId, 
                    racoinNeeded, 
                    $"Purchase: {itemName}"
                );
                return result.Success;
            }
        }
        
        Console.WriteLine("Insufficient funds.");
        return false;
    }
}
```

## For Administrators

### Monitor Market Health

Check market statistics:
```
market stats
```

View active alerts:
```
market alerts
```

Perform market analysis:
```
market analyze
```

### Resolve Alerts

When an alert is generated, review and resolve it:
```
market alert resolve <alert-id>
```

### Add RaCoin to User Wallet

Top up a user's RaCoin balance:
```
racoin topup <user-id> <amount>
```

## Market Monitoring

The AI Market Monitor automatically:
- ✓ Tracks all exchange transactions
- ✓ Detects unusual volume patterns
- ✓ Flags large transactions (>100k RaCoin)
- ✓ Monitors for market manipulation
- ✓ Enforces the standard exchange ratio
- ✓ Generates alerts for suspicious activity

### Alert Types

1. **Unusual Volume**: High-volume exchanges in short time
2. **Large Transaction**: Individual exchanges over 100k RaCoin
3. **Rate Deviation**: Attempts to use non-standard rates (automatically blocked)
4. **Suspicious Activity**: Patterns indicating potential exploitation
5. **Market Manipulation**: Coordinated manipulation attempts

### Alert Severity Levels

- **Low**: Minor anomalies, informational
- **Medium**: Noteworthy activity requiring monitoring
- **High**: Significant concerns requiring attention
- **Critical**: Immediate action needed

## Common Conversions

| RaCoin | Gold |
|--------|------|
| 100 | 40,000 |
| 500 | 200,000 |
| 1,000 | 400,000 |
| 2,500 | 1,000,000 |
| 5,000 | 2,000,000 |
| 10,000 | 4,000,000 |

## Principle: "Harm None, Do What Ye Will"

The currency exchange system enforces ethical economic principles:
- **Fair Exchange**: Standard ratio prevents exploitation
- **Transparency**: All exchanges are logged and monitored
- **Balance**: Maintains healthy in-game economies
- **Anti-Greed**: Deters hoarding and manipulation
- **Protection**: Automated monitoring prevents exploitation

## Troubleshooting

### Insufficient Balance

If you get an "insufficient balance" error:
1. Check your balance: `racoin balance <user-id>`
2. Top up your wallet if needed
3. Try exchanging a smaller amount

### Exchange Failed

If an exchange fails:
1. Verify you have sufficient balance
2. Check the exchange rate: `racoin exchange rates`
3. Ensure you're using the correct command format
4. Review any active market alerts: `market alerts`

### Large Transaction Alert

If you see a large transaction alert:
- This is normal for exchanges over 100k RaCoin
- The exchange still completes successfully
- Alerts help administrators monitor market health
- No action needed unless flagged as suspicious

## API Reference

### Currency Conversion

```csharp
// Static conversion functions
decimal gold = CurrencyExchangeConstants.RaCoinToGold(racoinAmount);
decimal racoin = CurrencyExchangeConstants.GoldToRaCoin(goldAmount);

// Exchange rates
decimal rate1 = CurrencyExchangeConstants.GoldPerRaCoin; // 400
decimal rate2 = CurrencyExchangeConstants.RaCoinPerGold; // 0.0025
```

### Wallet Management

```csharp
// RaCoin wallet
var racoinWallet = racoinModule.GetOrCreateWallet(userId);
decimal racoinBalance = racoinModule.GetBalance(userId);

// Gold wallet
var goldWallet = racoinModule.GetOrCreateGoldWallet(userId);
decimal goldBalance = racoinModule.GetGoldBalance(userId);
```

### Currency Exchange

```csharp
// Exchange RaCoin to Gold
var result = await racoinModule.ExchangeRaCoinToGoldAsync(userId, racoinAmount);

// Exchange Gold to RaCoin
var result = await racoinModule.ExchangeGoldToRaCoinAsync(userId, goldAmount);

// Check result
if (result.Success)
{
    Console.WriteLine($"New RaCoin Balance: {result.NewRaCoinBalance}");
    Console.WriteLine($"New Gold Balance: {result.NewGoldBalance}");
}
```

## Next Steps

1. **Test the System**: Try exchanging small amounts first
2. **Integrate into Your Game**: Use the examples above
3. **Monitor Market Health**: Review stats and alerts regularly
4. **Read Full Documentation**: See `CURRENCY_EXCHANGE_SYSTEM.md`

## Support

For questions or issues:
1. Check this guide and the full documentation
2. Review the examples in `CURRENCY_EXCHANGE_SYSTEM.md`
3. Test with the provided test program
4. Monitor console output for errors

---

**Standard Rate**: 1000 RaCoin = 400,000 Gold  
**Principle**: "Harm none, do what ye will"  
**Status**: ✅ Production Ready  
**Last Updated**: 2025-01-14
