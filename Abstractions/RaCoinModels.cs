namespace Abstractions;

/// <summary>
/// Represents a RaCoin wallet for a user.
/// </summary>
public class RaCoinWallet
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
/// Represents a RaCoin Transaction.
/// </summary>
public class RaCoinTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public RaCoinTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? ReferenceId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Types of RaCoin Transactions.
/// </summary>
public enum RaCoinTransactionType
{
    TopUp,
    Purchase,
    Refund,
    Transfer,
    Reward,
    Deduction
}

/// <summary>
/// Request model for topping up RaCoin balance.
/// </summary>
public class TopUpRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public Dictionary<string, string> PaymentDetails { get; set; } = new();
}

/// <summary>
/// Response model for RaCoin Operations.
/// </summary>
public class RaCoinResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? NewBalance { get; set; }
    public RaCoinTransaction? Transaction { get; set; }
}

/// <summary>
/// Represents a product in the SuperMarket.
/// </summary>
public class SuperMarketProduct
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PriceInRaCoins { get; set; }
    public ProductCategory Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Legendary Supermarket extensions
    public decimal? PriceInGold { get; set; }  // Optional Gold pricing
    public CurrencyType PrimaryCurrency { get; set; } = CurrencyType.RaCoin;
    public Guid? SellerId { get; set; }  // For user-listed items
}

/// <summary>
/// Product categories in the SuperMarket.
/// </summary>
public enum ProductCategory
{
    License,
    Module,
    Theme,
    Plugin,
    Feature,
    Service,
    Other
}

/// <summary>
/// Represents a purchase from the SuperMarket.
/// </summary>
public class SuperMarketPurchase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
    public DateTime PurchasedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid TransactionId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Completed;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Status of a purchase.
/// </summary>
public enum PurchaseStatus
{
    Pending,
    Completed,
    Refunded,
    Cancelled
}

/// <summary>
/// Currency type for marketplace Transactions in Legendary Supermarket.
/// </summary>
public enum CurrencyType
{
    RaCoin,  // Premium tier - subscriptions, licensing, premium items
    Gold     // Free tier - in-game items, player-to-player tASHATding
}

/// <summary>
/// Marketplace listing in the Legendary Supermarket (supports both RaCoin and Gold).
/// </summary>
public class MarketplaceListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CurrencyType CurrencyType { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime ListedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Seller information in the marketplace.
/// </summary>
public class SellerInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal ASHATting { get; set; }
    public int TotalSales { get; set; }
    public int ActiveListings { get; set; }
    public DateTime MemberSince { get; set; }
    public List<SellerReview> Reviews { get; set; } = new();
}

/// <summary>
/// Review for a seller in the marketplace.
/// </summary>
public class SellerReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public Guid PurchaseId { get; set; }
    public int ASHATting { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Marketplace Transaction (purchase from a listing).
/// </summary>
public class MarketplaceTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public CurrencyType CurrencyType { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime TransactionAtUtc { get; set; } = DateTime.UtcNow;
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Completed;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Search criteria for marketplace listings.
/// </summary>
public class MarketplaceSearchCriteria
{
    public string? SearchTerm { get; set; }
    public CurrencyType? CurrencyType { get; set; }
    public ProductCategory? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? SellerId { get; set; }
    public List<string>? Tags { get; set; }
    public MarketplaceSortBy SortBy { get; set; } = MarketplaceSortBy.DateListed;
    public bool Ascending { get; set; } = false;
}

/// <summary>
/// Sort options for marketplace search results.
/// </summary>
public enum MarketplaceSortBy
{
    DateListed,
    Price,
    Name,
    SellerASHATting,
    Popularity
}
