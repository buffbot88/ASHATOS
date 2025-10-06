# RaCoin & SuperMarket Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Check Your Balance

```bash
racoin balance <your-user-id>
```

If you don't have a wallet yet, one will be automatically created for you!

### Step 2: Top Up Your Wallet

```bash
racoin topup <your-user-id> 500
```

This adds 500 RaCoins to your wallet.

### Step 3: Browse Products

```bash
market catalog
```

You'll see all available products with their IDs and prices.

### Step 4: Make a Purchase

```bash
market buy <your-user-id> <product-id>
```

The system will:
- Check your balance
- Deduct the RaCoins
- Deliver the product instantly
- Create a license (if buying a license product)

### Step 5: View Your Purchases

```bash
market history <your-user-id>
```

See all your purchases and license keys!

## 💰 Quick Commands Reference

| Command | What It Does |
|---------|--------------|
| `racoin balance <user-id>` | Check wallet balance |
| `racoin topup <user-id> <amount>` | Add RaCoins |
| `racoin history <user-id>` | View transactions |
| `racoin stats` | System statistics |
| `market catalog` | Browse products |
| `market buy <user-id> <product-id>` | Purchase product |
| `market history <user-id>` | Purchase history |
| `market stats` | Marketplace stats |
| `license prices` | View license prices |

## 🏪 Available Products

### Licenses
- **Standard License**: 100 RaCoins (1 year, 10 users)
- **Professional License**: 500 RaCoins (1 year, 50 users)
- **Enterprise License**: 2000 RaCoins (1 year, 500 users)

### Modules
- **AI Language Module**: 150 RaCoins
- **Game Engine Module**: 300 RaCoins

### Themes
- **Premium Theme Pack**: 50 RaCoins

## 💡 Tips

1. **Always check your balance first** before attempting a purchase
2. **Top up more than you need** to avoid multiple small top-ups
3. **Save your license keys** immediately after purchase
4. **Check transaction history** to track your spending
5. **Use `market stats`** to see popular products

## 🔗 Need More Help?

- Full RaCoin Documentation: [RACOIN_SYSTEM.md](RACOIN_SYSTEM.md)
- Full SuperMarket Documentation: [SUPERMARKET_MODULE.md](SUPERMARKET_MODULE.md)
- Phase 4.2 Summary: [PHASE4_2_RACOIN_SUMMARY.md](PHASE4_2_RACOIN_SUMMARY.md)

## 🎯 Example Session

```bash
# 1. Check balance
$ racoin balance 123e4567-e89b-12d3-a456-426614174000
{"Balance": 0.00}

# 2. Top up
$ racoin topup 123e4567-e89b-12d3-a456-426614174000 1000
{"Success": true, "NewBalance": 1000.00}

# 3. Browse catalog
$ market catalog
{"Products": [{"Name": "Standard License", "Price": 100.00, "Id": "..."}]}

# 4. Purchase
$ market buy 123e4567-e89b-12d3-a456-426614174000 <product-id>
{"Success": true, "LicenseKey": "RACORE-ABC12345-..."}

# 5. Verify
$ racoin balance 123e4567-e89b-12d3-a456-426614174000
{"Balance": 900.00}
```

## 🎉 That's It!

You're ready to use RaCoin and SuperMarket. Happy shopping! 🛒

---

**Quick Reference Guide**  
**Version:** 1.0  
**Last Updated:** 2025-01-05
