# RaCoin Module

## Overview

RaCoin is the native cryptocurrency of ASHATCore. It provides a unified virtual currency system for purchasing licenses, modules, and features throughout the platform.

## Features

- **Wallet Management**: Automatic wallet creation for users
- **Transaction History**: Complete audit trail
- **Top-Up System**: Add RaCoins to wallet
- **Transfer System**: Send RaCoins between users
- **Refund Support**: Process refunds for purchases

## Quick Start

### Check Balance

```bash
RaCoin balance <user-id>
```

### Top Up Wallet

```bash
RaCoin topup <user-id> 500
```

### View Transaction History

```bash
RaCoin history <user-id>
```

### System Statistics

```bash
RaCoin stats
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
| `RaCoin balance <user-id>` | Check wallet balance |
| `RaCoin topup <user-id> <amount>` | Add RaCoins |
| `RaCoin history <user-id>` | View Transactions |
| `RaCoin stats` | System statistics |

## integration

RaCoin integRates with:
- **SuperMarket Module**: Payment processing
- **License Module**: License purchases
- **User Profiles**: Balance tracking

## API Access

```csharp
var RaCoinModule = moduleManager.GetModuleByName("RaCoin") as IRaCoinModule;

// Get balance
var balance = RaCoinModule.GetBalance(userId);

// Top up
var response = await RaCoinModule.TopUpAsync(userId, 100m, "Welcome bonus");

// Deduct for purchase
var deductResponse = await RaCoinModule.DeductAsync(userId, 50m, "Purchase");
```

## Documentation

See [RaCoin_SYSTEM.md](../../../RaCoin_SYSTEM.md) for complete documentation.

## Architecture

- Thread-safe wallet Operations
- Atomic balance updates
- Complete Transaction logging
- Immutable Transaction history

## License

Part of the ASHATCore AI mainframe system.
