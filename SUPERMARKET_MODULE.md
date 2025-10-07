# Legendary Supermarket Module

## 🏪 Overview

The Legendary Supermarket Module is RaCore's unified central marketplace with **dual currency support**:
- **RaCoin Side (Premium Tier)**: Official store for subscriptions, licensing, and premium items
- **Gold Side (Free Tier)**: User-to-user marketplace for in-game items and player trading

Features advanced search, seller profiles with ratings, and seamless integration with both currencies.

## ✅ Status: IMPLEMENTED (Phase 9.2)

Legendary Supermarket is fully operational with dual-currency support and advanced marketplace features.

## 🎯 Key Features

### Dual Currency System
- **RaCoin Market**: Premium subscriptions, official licenses, modules, and features
- **Gold Market**: Free-tier in-game items, player-to-player trading
- Seamless currency conversion via RaCoin module
- Separate catalogs and filtering by currency type

### Official Store (RaCoin Side)
- **Product Catalog**: Browse official products by category
- **Digital Products**: Instant delivery of licenses, modules, and features
- **Pricing in RaCoins**: All official items priced in native cryptocurrency
- **License Integration**: Automatic license creation and assignment
- **Purchase History**: Complete transaction tracking

### User Marketplace (Both Currencies)
- **Player-to-Player Trading**: List and sell items to other players
- **Dual Currency Listings**: Accept either RaCoins or Gold
- **3% Marketplace Fee**: Automatic fee deduction on sales (Phase 9.3.5 - Development Rate)
- **Stock Management**: Track inventory and availability
- **Transaction History**: Separate tracking for purchases and sales

### Product Categories
1. **License** - Software licenses (Standard, Professional, Enterprise)
2. **Module** - Extension modules (AI Language, Game Engine, etc.)
3. **Theme** - UI themes and customizations
4. **Plugin** - Additional functionality plugins
5. **Feature** - Individual feature unlocks
6. **Service** - Premium services and support
7. **Other** - Miscellaneous products

### Advanced Search & Discovery
- **Text Search**: Search by item name or description
- **Currency Filter**: Filter listings by RaCoin or Gold
- **Category Filter**: Browse by product category
- **Real-time Results**: Instant search capabilities

### Seller Profiles & Ratings
- **Seller Information**: Basic profile from UserProfile module
- **Display Name & Bio**: Rich seller information
- **Total Sales**: Transaction count and active listings
- **5-Star Reviews**: Standard 1-5 star rating system
- **Review Comments**: Written feedback on transactions
- **Average Rating**: Calculated seller reputation

## 🛒 Legendary Supermarket Commands

### Official Store (RaCoin Side)

```bash
# View official products catalog
market catalog
market products

# Purchase official product with RaCoins
market buy <user-id> <product-id>
```

### User Marketplace (RaCoin & Gold)

```bash
# List item for sale
market list <seller-id> <item-name> <currency> <price> <quantity> <category> <description>

# Example: List a sword for Gold
market list abc123 "Fire Sword" Gold 5000 1 Other "Legendary fire sword"

# Example: List premium mod for RaCoins
market list abc123 "Premium Mod Pack" RaCoin 50 10 Module "Advanced modding tools"

# Purchase from marketplace
market purchase <buyer-id> <listing-id> <quantity>

# Search marketplace
market search <search-term>

# Filter by currency and optionally category
market filter <currency-type> [category]
market filter Gold
market filter RaCoin License
```

### Seller Information & Reviews

```bash
# View seller profile and ratings
market seller <seller-id>

# Leave a review (rating 1-5)
market review <reviewer-id> <seller-id> <purchase-id> <rating> <comment>

# Example
market review user123 seller456 purchase789 5 "Great seller, fast delivery!"
```

### General Commands

```bash
# View marketplace statistics
market stats

# View purchase and sales history
market history <user-id>

# Get help
help
```

## 📦 Default Products

### License Products

| Product | Price | Duration | Features |
|---------|-------|----------|----------|
| Standard License | 100 RaCoins | 1 year | Basic features, 10 users |
| Professional License | 500 RaCoins | 1 year | Advanced features, 50 users |
| Enterprise License | 2000 RaCoins | 1 year | Unlimited features, 500 users |

### Module Products

| Product | Price | Description |
|---------|-------|-------------|
| AI Language Module | 150 RaCoins | Advanced AI language processing |
| Game Engine Module | 300 RaCoins | Complete game development engine |

### Theme Products

| Product | Price | Description |
|---------|-------|-------------|
| Premium Theme Pack | 50 RaCoins | Collection of premium UI themes |

## 🔄 Purchase Flow

### Step-by-Step Process

1. **Browse Catalog**
   ```bash
   market catalog
   ```
   - View all available products
   - Note product IDs and prices

2. **Check Balance**
   ```bash
   racoin balance <user-id>
   ```
   - Ensure sufficient RaCoins
   - Top up if needed

3. **Make Purchase**
   ```bash
   market buy <user-id> <product-id>
   ```
   - System validates balance
   - RaCoins deducted
   - Product delivered

4. **Verify Purchase**
   ```bash
   market history <user-id>
   ```
   - View purchase confirmation
   - Get license key (for licenses)

### Example Purchase Session

```bash
# 1. Check current balance
$ racoin balance 123e4567-e89b-12d3-a456-426614174000
{
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "Balance": 550.00,
  "IsActive": true
}

# 2. Browse catalog
$ market catalog
{
  "TotalProducts": 6,
  "AvailableProducts": 6,
  "Products": [
    {
      "Id": "product-id-1",
      "Name": "Professional License",
      "PriceInRaCoins": 500.00,
      "Category": "License"
    }
  ]
}

# 3. Purchase product
$ market buy 123e4567-e89b-12d3-a456-426614174000 product-id-1
{
  "Success": true,
  "Message": "Successfully purchased Professional License",
  "Purchase": {
    "Id": "purchase-id",
    "ProductName": "Professional License",
    "PricePaid": 500.00,
    "NewBalance": 50.00,
    "LicenseKey": "RACORE-ABC12345-DEF67890-GHI23456"
  }
}

# 4. Verify purchase history
$ market history 123e4567-e89b-12d3-a456-426614174000
{
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "TotalPurchases": 1,
  "TotalSpent": 500.00,
  "Purchases": [...]
}
```

## 📊 Data Models

### SuperMarketProduct (Extended)

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
    public DateTime CreatedAtUtc { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    
    // Legendary Supermarket extensions
    public decimal? PriceInGold { get; set; }  // Optional Gold pricing
    public CurrencyType PrimaryCurrency { get; set; } = CurrencyType.RaCoin;
    public Guid? SellerId { get; set; }  // For user-listed items
}
```

### MarketplaceListing

```csharp
public class MarketplaceListing
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; }
    public string ItemName { get; set; }
    public string Description { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime ListedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public List<string> Tags { get; set; }
}
```

### SellerInfo

```csharp
public class SellerInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal Rating { get; set; }
    public int TotalSales { get; set; }
    public int ActiveListings { get; set; }
    public DateTime MemberSince { get; set; }
    public List<SellerReview> Reviews { get; set; }
}
```

### SellerReview

```csharp
public class SellerReview
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; }
    public Guid SellerId { get; set; }
    public Guid PurchaseId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

### CurrencyType Enum

```csharp
public enum CurrencyType
{
    RaCoin,  // Premium tier - subscriptions, licensing, premium items
    Gold     // Free tier - in-game items, player-to-player trading
}
```

### ProductCategory Enum

```csharp
public enum ProductCategory
{
    License,    // Software licenses
    Module,     // Extension modules
    Theme,      // UI themes
    Plugin,     // Functionality plugins
    Feature,    // Feature unlocks
    Service,    // Premium services
    Other       // Miscellaneous
}
```

## 🌐 API Endpoints

### Official Store Endpoints

#### GET /api/market/catalog
Get all official RaCoin products

**Response:**
```json
{
  "Store": "Official RaCoin Store",
  "Currency": "RaCoin",
  "TotalProducts": 10,
  "AvailableProducts": 10,
  "Products": [...]
}
```

#### POST /api/market/purchase
Purchase official product or marketplace listing

**Request Body:**
```json
{
  "BuyerId": "user-guid",
  "ProductId": "product-guid",
  "ListingId": "listing-guid",
  "Quantity": 1,
  "IsMarketplaceListing": false
}
```

### Marketplace Endpoints

#### GET /api/market/listings?currency=Gold&category=Other
Filter marketplace listings by currency and category

**Query Parameters:**
- `currency`: RaCoin or Gold
- `category`: Optional product category

#### GET /api/market/search?q=sword
Search marketplace by name or description

**Query Parameters:**
- `q`: Search term

#### POST /api/market/list
Create new marketplace listing

**Request Body:**
```json
{
  "SellerId": "user-guid",
  "ItemName": "Fire Sword",
  "CurrencyType": "Gold",
  "Price": 5000,
  "Quantity": 1,
  "Category": "Other",
  "Description": "Legendary fire sword"
}
```

### Seller Endpoints

#### GET /api/market/seller/{sellerId}
Get seller profile, ratings, and active listings

**Response:**
```json
{
  "Seller": {
    "UserId": "...",
    "Username": "...",
    "Rating": 4.8,
    "TotalSales": 42,
    "ActiveListings": 5
  },
  "ActiveListings": [...],
  "RecentReviews": [...],
  "AverageRating": 4.8
}
```

#### POST /api/market/review
Leave seller review

**Request Body:**
```json
{
  "ReviewerId": "user-guid",
  "SellerId": "seller-guid",
  "PurchaseId": "purchase-guid",
  "Rating": 5,
  "Comment": "Great seller!"
}
```

### General Endpoints

#### GET /api/market/stats
Get marketplace statistics

#### GET /api/market/history/{userId}
Get user's purchase and sales history

## 📊 Data Models

## 🔌 Integration with Other Modules

### RaCoin Module
- Balance checking before purchase
- Automatic deduction on purchase
- Transaction recording
- Refund processing

### License Module
- Automatic license creation for license products
- License key generation
- License assignment to user
- License metadata storage

### CMSSpawner Module
- SuperMarket extends CMSSpawner concepts
- Can be integrated into CMS pages
- Shared database structure
- Common authentication

## 💻 Programmatic Access

### Get Product Information

```csharp
var superMarket = moduleManager.GetModuleByName("SuperMarket") as SuperMarketModule;

// Get single product
var product = superMarket.GetProduct(productId);

// Get all available products
var products = superMarket.GetAvailableProducts();
```

### Process Purchase

```csharp
// Purchase through command
var result = superMarket.Process($"market buy {userId} {productId}");

// Parse result
var purchaseResult = JsonSerializer.Deserialize<PurchaseResponse>(result);
if (purchaseResult.Success)
{
    Console.WriteLine($"Purchase successful! License Key: {purchaseResult.LicenseKey}");
}
```

## 📈 Statistics & Reporting

### Marketplace Statistics

```bash
market stats
```

Returns:
- Total products in catalog
- Available products count
- Total purchases made
- Total revenue (in RaCoins)
- Products by category breakdown
- Top 5 selling products

### Example Stats Output

```json
{
  "TotalProducts": 6,
  "AvailableProducts": 6,
  "TotalPurchases": 42,
  "TotalRevenue": 12500.00,
  "ProductsByCategory": [
    { "Category": "License", "Count": 3 },
    { "Category": "Module", "Count": 2 },
    { "Category": "Theme", "Count": 1 }
  ],
  "TopSellingProducts": [
    { "ProductName": "Standard License", "Sales": 20 },
    { "ProductName": "Professional License", "Sales": 15 },
    { "ProductName": "Premium Theme Pack", "Sales": 7 }
  ]
}
```

## 🎨 Customization

### Adding Custom Products

Administrators can add custom products to the marketplace:

```bash
# Syntax
market add <name> <price> <category> <description>

# Examples
market add "VIP Support" 1000 Service "24/7 priority support"
market add "Custom Theme Designer" 200 Theme "Create custom themes"
market add "Advanced Analytics" 350 Feature "Real-time analytics dashboard"
```

### Product Categories

Choose appropriate category when adding products:
- `License` - For software licenses
- `Module` - For extension modules
- `Theme` - For UI customizations
- `Plugin` - For add-on functionality
- `Feature` - For feature unlocks
- `Service` - For services
- `Other` - For miscellaneous items

## 🏗️ Architecture

### Design Patterns
- **Module Pattern**: Extends ModuleBase
- **Repository Pattern**: In-memory product catalog
- **Transaction Pattern**: Atomic purchase operations
- **Integration Pattern**: Seamless RaCoin and License integration

### Module Dependencies
- **Required**: RaCoin Module (for payments)
- **Optional**: License Module (for license products)
- **Extends**: CMSSpawner concepts

### Thread Safety
- Thread-safe product catalog
- Atomic purchase operations
- Concurrent purchase support
- Race condition protection

## 🔒 Security Features

### Purchase Validation
- Balance verification before purchase
- Product availability check
- User authentication required
- Transaction logging

### Anti-Fraud Measures
- Purchase limits (future)
- Refund policies
- Transaction monitoring
- Audit trail

## 🎓 Best Practices

### For Users
1. Always check product details before purchasing
2. Ensure sufficient balance
3. Save license keys immediately
4. Review purchase history regularly
5. Contact support for issues

### For Administrators
1. Price products appropriately
2. Update product descriptions clearly
3. Monitor sales statistics
4. Process refunds promptly
5. Add new products based on demand

### For Developers
1. Validate purchases before processing
2. Handle insufficient balance gracefully
3. Log all purchase attempts
4. Provide clear error messages
5. Test purchase flow thoroughly

## 🚀 Future Enhancements

### Phase 1 (Planned)
- [ ] Product images and screenshots
- [ ] Product reviews and ratings
- [ ] Wishlist functionality
- [ ] Bundle pricing
- [ ] Discount codes and promotions

### Phase 2 (Advanced)
- [ ] Subscription-based products
- [ ] Free trial periods
- [ ] Product recommendations
- [ ] Gift purchases
- [ ] Marketplace for user-created products

### Phase 3 (Enterprise)
- [ ] Volume discounts
- [ ] Enterprise pricing tiers
- [ ] Bulk purchases
- [ ] Quote system
- [ ] Purchase orders

## 🐛 Troubleshooting

### Common Issues

**Issue**: "Product not found"  
**Solution**: Verify product ID from catalog, products may have been removed

**Issue**: "Insufficient RaCoins"  
**Solution**: Top up wallet before purchasing

**Issue**: "Product is not available"  
**Solution**: Product may be temporarily unavailable, check back later

**Issue**: "License not created"  
**Solution**: License module may not be available, contact administrator

## 📝 Use Cases

### Small Business
- Purchase Standard License (100 RaCoins)
- Add Premium Theme Pack (50 RaCoins)
- Total: 150 RaCoins

### Developer
- Purchase Professional License (500 RaCoins)
- Add AI Language Module (150 RaCoins)
- Add Game Engine Module (300 RaCoins)
- Total: 950 RaCoins

### Enterprise
- Purchase Enterprise License (2000 RaCoins)
- Add all modules and themes
- Custom support services
- Total: 2500+ RaCoins

## 🔗 Related Documentation

- [RACOIN_SYSTEM.md](RACOIN_SYSTEM.md) - RaCoin cryptocurrency
- [LICENSE_MANAGEMENT.md](LICENSE_MANAGEMENT.md) - License system
- [CMS_QUICKSTART.md](CMS_QUICKSTART.md) - CMS integration

## 📦 File Structure

```
RaCore/
└── Modules/Extensions/
    └── SuperMarket/
        └── SuperMarketModule.cs  # Core implementation

Abstractions/
└── RaCoinModels.cs               # SuperMarket models
```

## 🎉 Conclusion

The SuperMarket Module provides a complete e-commerce solution for RaCore, enabling users to purchase licenses, modules, and features using RaCoins. The seamless integration with RaCoin and License modules creates a unified commerce experience.

---

**Module**: Legendary Supermarket  
**Status**: ✅ IMPLEMENTED  
**Version**: v9.2.0  
**Category**: Unified Marketplace with Dual Currency Support  
**Last Updated**: Phase 9.2
**Previous Version**: SuperMarket v4.8.9
