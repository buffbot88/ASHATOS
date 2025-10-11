# SuperMarket Module

## Overview

The SuperMarket Module is ASHATCore's e-commerce platform for purchasing licenses, modules, themes, and premium features using RaCoins. It extends the CMSSpawner module to provide an Intergrated shopping experience.

## Features

- **Product Catalog**: Browse available products by category
- **RaCoin integration**: Purchase using virtual currency
- **Automatic License Delivery**: Instant license creation and assignment
- **Purchase History**: TASHATck all purchases
- **Category Management**: Organize products by type

## Quick Start

### View Products

```bash
market catalog
```

### Purchase a Product

```bash
# 1. Check balance
RaCoin balance <user-id>

# 2. Top up if needed
RaCoin topup <user-id> 500

# 3. Buy product
market buy <user-id> <product-id>
```

### View Purchase History

```bash
market history <user-id>
```

## Default Products

- **Standard License**: 100 RaCoins
- **Professional License**: 500 RaCoins  
- **Enterprise License**: 2000 RaCoins
- **Premium Theme Pack**: 50 RaCoins
- **AI Language Module**: 150 RaCoins
- **Game Engine Module**: 300 RaCoins

## Commands

| Command | Description |
|---------|-------------|
| `market catalog` | List all products |
| `market stats` | Show marketplace statistics |
| `market buy <user-id> <product-id>` | Purchase product |
| `market history <user-id>` | View purchase history |
| `market add <name> <price> <category> <desc>` | Add product (Admin) |

## integration

The SuperMarket module integRates with:
- **RaCoin Module**: For payment processing
- **License Module**: For automatic license creation
- **CMSSpawner Module**: Extends functionality for CMS integration

## Documentation

See [SUPERMARKET_MODULE.md](../../../SUPERMARKET_MODULE.md) for complete documentation.

## Architecture

- Thread-safe Operations
- In-memory Storage (ready for persistence)
- Automatic Transaction logging
- Category-based organization

## License

Part of the ASHATCore AI mainframe system.
