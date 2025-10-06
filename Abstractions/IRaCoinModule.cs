namespace Abstractions;

/// <summary>
/// Interface for RaCoin cryptocurrency management functionality.
/// </summary>
public interface IRaCoinModule : IDisposable
{
    /// <summary>
    /// Get or create a wallet for a user.
    /// </summary>
    RaCoinWallet GetOrCreateWallet(Guid userId);
    
    /// <summary>
    /// Get wallet balance for a user.
    /// </summary>
    decimal GetBalance(Guid userId);
    
    /// <summary>
    /// Top up a user's RaCoin wallet.
    /// </summary>
    Task<RaCoinResponse> TopUpAsync(Guid userId, decimal amount, string description);
    
    /// <summary>
    /// Deduct RaCoins from a user's wallet (for purchases).
    /// </summary>
    Task<RaCoinResponse> DeductAsync(Guid userId, decimal amount, string description, string? referenceId = null);
    
    /// <summary>
    /// Transfer RaCoins between users.
    /// </summary>
    Task<RaCoinResponse> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description);
    
    /// <summary>
    /// Get transaction history for a user.
    /// </summary>
    List<RaCoinTransaction> GetTransactionHistory(Guid userId, int limit = 50);
    
    /// <summary>
    /// Refund a transaction.
    /// </summary>
    Task<RaCoinResponse> RefundAsync(Guid transactionId);
}
