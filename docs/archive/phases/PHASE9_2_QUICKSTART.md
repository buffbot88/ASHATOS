# Phase 9.2 Quickstart: Legendary Supermarket Module

## Overview

Phase 9.2 implements the **Legendary Supermarket Module** - a complete rebuild of the SuperMarket into a unified central marketplace with dual currency support.

### Key Features
- **Dual Currency System**: RaCoin (premium) and Gold (free tier)
- **User-to-User Marketplace**: Player trading with both currencies
- **Advanced Search**: Text search with currency and category filtering
- **Seller Profiles**: Integration with UserProfile module
- **Rating System**: 5-star reviews with comments
- **15% Marketplace Fee**: Automatic fee on marketplace sales

## Quick Start

### 1. Official Store (RaCoin Side)

```bash
# View official products
market catalog

# Purchase a license with RaCoins
market buy <user-id> <product-id>
```

### 2. User Marketplace (Gold Side)

```bash
# List an item for Gold
market list <seller-id> "Fire Sword" Gold 5000 1 Other "Legendary sword"

# Search marketplace
market search sword

# Filter by currency
market filter Gold

# Purchase from marketplace
market purchase <buyer-id> <listing-id> 1
```

### 3. Seller Profiles & Reviews

```bash
# View seller information
market seller <seller-id>

# Leave a review
market review <reviewer-id> <seller-id> <purchase-id> 5 "Great seller!"
```

## API Endpoints

### Official Store
- `GET /api/market/catalog` - Official RaCoin products
- `POST /api/market/purchase` - Purchase official product

### Marketplace
- `GET /api/market/listings?currency=Gold&category=Other` - Filter listings
- `GET /api/market/search?q=sword` - Search marketplace
- `POST /api/market/list` - Create listing
- `POST /api/market/purchase` - Purchase from marketplace

### Sellers
- `GET /api/market/seller/{sellerId}` - Seller profile
- `POST /api/market/review` - Leave review

### General
- `GET /api/market/stats` - Marketplace statistics
- `GET /api/market/history/{userId}` - Transaction history

## Currency Types

### RaCoin (Premium Tier)
- Official licenses and modules
- Premium subscriptions
- Professional features
- Enterprise services

### Gold (Free Tier)
- In-game items
- Player-crafted goods
- Cosmetics and consumables
- Community marketplace

## Example Usage

### Scenario 1: Buy Official License

```bash
# Check RaCoin balance
racoin balance abc-123

# Browse official store
market catalog

# Purchase Professional License
market buy abc-123 product-456

# Verify purchase
market history abc-123
```

### Scenario 2: Trade In-Game Item

```bash
# List sword for Gold
market list seller-123 "Legendary Sword" Gold 10000 1 Other "Max level sword"

# Buyer searches marketplace
market search legendary

# Buyer purchases
market purchase buyer-456 listing-789 1

# Seller gets 8500 Gold (85% after 15% fee)
```

### Scenario 3: Build Reputation

```bash
# Seller completes transaction
market purchase buyer-123 listing-456 1

# Buyer leaves positive review
market review buyer-123 seller-456 purchase-789 5 "Fast delivery, great item!"

# Check updated seller profile
market seller seller-456
```

## Data Flow

### Official Purchase (RaCoin)
1. User browses official catalog
2. User has sufficient RaCoins
3. RaCoins deducted from wallet
4. License automatically created and assigned
5. Purchase recorded in history

### Marketplace Transaction (Gold)
1. Seller creates listing with Gold price
2. Buyer searches and finds listing
3. Buyer has sufficient Gold
4. Gold transferred (85% to seller, 15% fee)
5. Transaction recorded
6. Buyer can review seller

## Seller Fee Structure

All marketplace sales include a **15% marketplace fee**:
- **Buyer pays**: Listed price
- **Seller receives**: 85% of price
- **Marketplace fee**: 15% of price

Example:
- Item price: 10,000 Gold
- Buyer pays: 10,000 Gold
- Seller receives: 8,500 Gold
- Marketplace fee: 1,500 Gold

## Search & Discovery

### Text Search
Search across item names and descriptions:
```bash
market search "fire sword"
```

### Currency Filter
Filter by payment type:
```bash
market filter RaCoin    # Premium items only
market filter Gold      # Free tier items only
```

### Category Filter
Narrow down by category:
```bash
market filter Gold Module     # Gold-priced modules
market filter RaCoin License  # RaCoin licenses
```

## Seller Profiles

Each seller has a profile showing:
- **Username & Display Name**: From UserProfile module
- **Bio & Avatar**: Rich seller information
- **Total Sales**: Number of completed transactions
- **Active Listings**: Current items for sale
- **Average Rating**: Calculated from reviews
- **Recent Reviews**: Latest buyer feedback
- **Member Since**: Account creation date

## Rating System

### How It Works
1. Only verified buyers can leave reviews
2. Ratings are 1-5 stars
3. Comments are optional but encouraged
4. Average rating is automatically calculated
5. Reviews are visible on seller profiles

### Review Guidelines
- Rate based on transaction experience
- Be honest and constructive
- Mention delivery speed and item quality
- Help other buyers make informed decisions

## Technical Details

### Module Name
`LegendarySupermarketModule`

### Dependencies
- **RaCoin Module**: Currency and wallet management
- **License Module**: Automatic license creation
- **UserProfile Module**: Seller profile information

### File Locations
- Module: `RaCore/Modules/Extensions/SuperMarket/LegendarySupermarketModule.cs`
- Models: `Abstractions/RaCoinModels.cs`
- Docs: `SUPERMARKET_MODULE.md`

## Migration from SuperMarket

### Breaking Changes
- Module renamed from `SuperMarket` to `LegendarySupermarket`
- New command structure for marketplace features
- Added dual currency support

### Backward Compatibility
- Official store commands remain compatible
- Existing purchase history preserved
- `market catalog` and `market buy` unchanged

### New Features
- User marketplace listings
- Gold currency support
- Search and filter capabilities
- Seller profiles and ratings

## Best Practices

### For Buyers
1. Check seller ratings before purchasing
2. Read recent reviews
3. Verify item details and price
4. Leave honest reviews after purchase

### For Sellers
1. Provide accurate item descriptions
2. Price items competitively
3. Maintain good reputation
4. Respond to buyer feedback
5. Keep active listings updated

### For Administrators
1. Monitor marketplace statistics
2. Review flagged transactions
3. Manage official store inventory
4. Set appropriate marketplace fees

## Troubleshooting

**Issue**: Cannot list item in marketplace
**Solution**: Verify you have the item to sell and correct currency type

**Issue**: Search returns no results
**Solution**: Try broader search terms or different filters

**Issue**: Rating not updating
**Solution**: Ensure review is for a verified purchase

**Issue**: Marketplace fee seems high
**Solution**: 15% fee is standard and helps maintain the marketplace

## Future Enhancements

### Planned Features
- Product images and screenshots
- Wishlist functionality
- Bulk listing management
- Advanced analytics for sellers
- Dispute resolution system
- Gift purchases
- Auction system

### Community Features
- Featured sellers
- Top-rated items
- Trending searches
- Seller badges and achievements

## Related Documentation

- [SUPERMARKET_MODULE.md](SUPERMARKET_MODULE.md) - Complete module documentation
- [RACOIN_SYSTEM.md](RACOIN_SYSTEM.md) - RaCoin cryptocurrency system
- [CURRENCY_EXCHANGE_SYSTEM.md](CURRENCY_EXCHANGE_SYSTEM.md) - Currency conversion
- [LICENSE_MANAGEMENT.md](LICENSE_MANAGEMENT.md) - License system

## Support

For issues or questions:
1. Check this quickstart guide
2. Review full documentation
3. Check marketplace statistics
4. Contact system administrator

---

**Phase**: 9.2  
**Status**: ✅ COMPLETED  
**Module**: Legendary Supermarket  
**Version**: v9.2.0
