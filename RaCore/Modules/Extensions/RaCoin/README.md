# RaCoin Module

## Overview

RaCoin is the native cryptocurrency of RaCore. It provides a unified virtual currency system for purchasing licenses, modules, and features throughout the platform.

## Features

- **Wallet Management**: Automatic wallet creation for users
- **Transaction History**: Complete audit trail
- **Top-Up System**: Add RaCoins to wallet
- **Transfer System**: Send RaCoins between users
- **Refund Support**: Process refunds for purchases

## Quick Start

### Check Balance

```bash
racoin balance <user-id>
```

### Top Up Wallet

```bash
racoin topup <user-id> 500
```

### View Transaction History

```bash
racoin history <user-id>
```

### System Statistics

```bash
racoin stats
```

## Transaction Types

- **TopUp**: Add RaCoins
- **Purchase**: Spend RaCoins
- **Refund**: Return RaCoins
- **Transfer**: Send to another user
- **Reward**: Receive as reward
- **Deduction**: Admin deduction

## Commands

| Command | Description |
|---------|-------------|
| `racoin balance <user-id>` | Check wallet balance |
| `racoin topup <user-id> <amount>` | Add RaCoins |
| `racoin history <user-id>` | View transactions |
| `racoin stats` | System statistics |

## Integration

RaCoin integrates with:
- **SuperMarket Module**: Payment processing
- **License Module**: License purchases
- **User Profiles**: Balance tracking

## API Access

```csharp
var racoinModule = moduleManager.GetModuleByName("RaCoin") as IRaCoinModule;

// Get balance
var balance = racoinModule.GetBalance(userId);

// Top up
var response = await racoinModule.TopUpAsync(userId, 100m, "Welcome bonus");

// Deduct for purchase
var deductResponse = await racoinModule.DeductAsync(userId, 50m, "Purchase");
```

## Documentation

See [RACOIN_SYSTEM.md](../../../RACOIN_SYSTEM.md) for complete documentation.

## Architecture

- Thread-safe wallet operations
- Atomic balance updates
- Complete transaction logging
- Immutable transaction history

## License

Part of the RaCore AI Mainframe system.
