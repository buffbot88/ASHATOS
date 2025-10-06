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
/// Represents a RaCoin transaction.
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
/// Types of RaCoin transactions.
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
/// Response model for RaCoin operations.
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
