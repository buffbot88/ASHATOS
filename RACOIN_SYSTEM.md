# RaCoin Cryptocurrency System

## 🪙 Overview

RaCoin is the native cryptocurrency of the RaCore ecosystem. It provides a unified virtual currency system for purchasing licenses, modules, themes, and other premium features throughout the platform.

## ✅ Status: IMPLEMENTED

RaCoin system is fully operational and integrated with the SuperMarket module.

## 🎯 Key Features

### Virtual Currency Management
- **Wallet System**: Each user automatically gets a RaCoin wallet
- **Balance Tracking**: Real-time balance management with transaction history
- **Top-Up System**: Users can add RaCoins to their wallet
- **Secure Transactions**: Thread-safe operations with full audit trail

### Transaction Types
1. **TopUp** - Add RaCoins to wallet
2. **Purchase** - Buy products from SuperMarket
3. **Refund** - Return RaCoins for cancelled purchases
4. **Transfer** - Send RaCoins between users
5. **Reward** - Receive RaCoins as rewards
6. **Deduction** - Administrative deductions

### Integration Points
- **SuperMarket Module**: Purchase licenses, modules, and features
- **License Module**: Automatic license creation on purchase
- **Authentication Module**: User wallet management
- **User Profiles**: Balance displayed in profile

## 💰 RaCoin Commands

### Balance Management

```bash
# Check user balance
racoin balance <user-id>

# Top up user wallet
racoin topup <user-id> <amount>

# View transaction history
racoin history <user-id>

# System statistics
racoin stats
```

### Example Usage

```bash
# Create test user wallet and top up
racoin topup 123e4567-e89b-12d3-a456-426614174000 500

# Check balance
racoin balance 123e4567-e89b-12d3-a456-426614174000

# View transaction history
racoin history 123e4567-e89b-12d3-a456-426614174000
```

## 🏪 SuperMarket Integration

### Product Catalog

Default products available:
- **Standard License**: 100 RaCoins (1-year, basic features)
- **Professional License**: 500 RaCoins (1-year, advanced features)
- **Enterprise License**: 2000 RaCoins (1-year, unlimited features)
- **Premium Theme Pack**: 50 RaCoins
- **AI Language Module**: 150 RaCoins
- **Game Engine Module**: 300 RaCoins

### Purchasing Products

```bash
# View catalog
market catalog

# Purchase a product
market buy <user-id> <product-id>

# View purchase history
market history <user-id>

# Add new product (Admin only)
market add "Custom Module" 250 Module "Description of the module"
```

### Purchase Flow

1. User views product catalog
2. User checks balance: `racoin balance <user-id>`
3. User purchases product: `market buy <user-id> <product-id>`
4. System validates balance
5. RaCoins deducted from wallet
6. Purchase recorded
7. License automatically created (for license products)
8. Transaction logged

## 📊 Data Models

### RaCoinWallet

```csharp
public class RaCoinWallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public bool IsActive { get; set; }
}
```

### RaCoinTransaction

```csharp
public class RaCoinTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public RaCoinTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? ReferenceId { get; set; }
}
```

### SuperMarketProduct

```csharp
public class SuperMarketProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal PriceInRaCoins { get; set; }
    public ProductCategory Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
```

## 🔄 API Integration (Programmatic Access)

### RaCoin Module Interface

```csharp
public interface IRaCoinModule
{
    RaCoinWallet GetOrCreateWallet(Guid userId);
    decimal GetBalance(Guid userId);
    Task<RaCoinResponse> TopUpAsync(Guid userId, decimal amount, string description);
    Task<RaCoinResponse> DeductAsync(Guid userId, decimal amount, string description, string? referenceId = null);
    Task<RaCoinResponse> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description);
    List<RaCoinTransaction> GetTransactionHistory(Guid userId, int limit = 50);
    Task<RaCoinResponse> RefundAsync(Guid transactionId);
}
```

### Example Code

```csharp
// Get RaCoin module
var racoinModule = moduleManager.GetModuleByName("RaCoin") as IRaCoinModule;

// Check balance
var balance = racoinModule.GetBalance(userId);

// Top up wallet
var response = await racoinModule.TopUpAsync(userId, 100m, "Welcome bonus");

// Make purchase
var deductResponse = await racoinModule.DeductAsync(userId, 50m, "Purchase: Premium Theme");

// Transfer between users
var transferResponse = await racoinModule.TransferAsync(fromUserId, toUserId, 25m, "Gift");
```

## 🏗️ Architecture

### Design Patterns
- **Module Pattern**: Extends ModuleBase
- **Repository Pattern**: In-memory storage (ready for persistence)
- **Transaction Pattern**: Atomic balance updates
- **Interface Segregation**: IRaCoinModule for external access

### Thread Safety
- All operations protected with lock
- Atomic balance updates
- Consistent transaction history

### Future Enhancements
- Database persistence (SQLite)
- External payment gateway integration
- Multi-currency support
- Exchange rate management
- Wallet export/import

## 💳 Payment Methods (Future)

### Planned Payment Integrations
- Credit/Debit Cards
- PayPal
- Cryptocurrency (BTC, ETH)
- Bank Transfer
- Gift Cards

### Top-Up Flow (Future)
1. User initiates top-up
2. Selects payment method
3. External payment processed
4. RaCoins credited on success
5. Receipt generated

## 📈 Statistics & Analytics

### System Stats

```bash
racoin stats
```

Output includes:
- Total wallets created
- Active wallets
- Total RaCoins in circulation
- Total transactions
- Top-ups vs purchases
- Average wallet balance

### SuperMarket Stats

```bash
market stats
```

Output includes:
- Total products
- Available products
- Total purchases
- Total revenue
- Products by category
- Top-selling products

## 🔒 Security Features

### Transaction Security
- All transactions logged with timestamps
- Balance validation before deductions
- Insufficient balance protection
- Transaction reference IDs for tracking
- Refund capability for purchases

### Wallet Security
- User-specific wallets
- Balance cannot go negative
- Audit trail for all operations
- Thread-safe operations

## 🎓 Best Practices

### For Users
1. Always check balance before purchasing
2. Keep track of transaction history
3. Top up in reasonable amounts
4. Review purchase history regularly

### For Administrators
1. Monitor system statistics regularly
2. Review large transactions
3. Add products with appropriate pricing
4. Process refunds promptly when needed

### For Developers
1. Always check operation success
2. Handle insufficient balance gracefully
3. Log all RaCoin operations
4. Use reference IDs for tracking
5. Validate amounts before operations

## 🐛 Troubleshooting

### Common Issues

**Issue**: Insufficient balance for purchase  
**Solution**: Top up wallet with required amount

**Issue**: Transaction not appearing in history  
**Solution**: Check user ID is correct, transactions are immediate

**Issue**: Balance not updating  
**Solution**: Verify module is initialized, check logs for errors

## 📝 Example Workflows

### New User Onboarding
1. User registers
2. Wallet automatically created
3. Welcome bonus credited (optional)
4. User browses SuperMarket
5. User tops up wallet
6. User makes first purchase

### License Purchase
1. User checks license prices in catalog
2. User ensures sufficient balance
3. User purchases license product
4. RaCoins deducted
5. License automatically created and assigned
6. User receives license key

### Admin Product Management
1. Admin adds new product to catalog
2. Sets price in RaCoins
3. Product becomes available
4. Users can purchase
5. Admin monitors sales statistics

## 🔗 Related Documentation

- [SUPERMARKET_MODULE.md](SUPERMARKET_MODULE.md) - SuperMarket details
- [LICENSE_MANAGEMENT.md](LICENSE_MANAGEMENT.md) - License system
- [AUTHENTICATION_QUICKSTART.md](AUTHENTICATION_QUICKSTART.md) - User authentication

## 📦 File Structure

```
RaCore/
├── Modules/Extensions/
│   ├── RaCoin/
│   │   └── RaCoinModule.cs       # Core RaCoin implementation
│   └── SuperMarket/
│       └── SuperMarketModule.cs  # SuperMarket implementation

Abstractions/
├── IRaCoinModule.cs               # RaCoin interface
└── RaCoinModels.cs                # Data models
```

## 🚀 Getting Started

### Quick Start

```bash
# 1. Start RaCore server
dotnet run --project RaCore

# 2. Create a user (via Authentication module)
# 3. Top up wallet
racoin topup <user-id> 1000

# 4. Browse catalog
market catalog

# 5. Make a purchase
market buy <user-id> <product-id>

# 6. Check balance
racoin balance <user-id>
```

## 🎉 Conclusion

RaCoin provides a complete virtual currency ecosystem for RaCore, enabling seamless purchases of licenses, modules, and features. The integration with SuperMarket creates a unified commerce experience for all users.

---

**Module**: RaCoin  
**Status**: ✅ IMPLEMENTED  
**Version**: 1.0  
**Last Updated**: 2025-10-06
