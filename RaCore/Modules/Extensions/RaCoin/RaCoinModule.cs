using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.RaCoin;

/// <summary>
/// RaCoin Module - Handles cryptocurrency wallet management, transactions, and top-ups.
/// Provides site-wide virtual currency for purchasing licenses, modules, and features.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class RaCoinModule : ModuleBase, IRaCoinModule
{
    public override string Name => "RaCoin";

    private readonly Dictionary<Guid, RaCoinWallet> _wallets = new();
    private readonly Dictionary<Guid, RaCoinTransaction> _transactions = new();
    private readonly object _lock = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("RaCoin module initialized - Virtual currency system active");
    }

    public override string Process(string input)
    {
        return ProcessInternal(input);
    }

    private string ProcessInternal(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("racoin stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("racoin balance", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: racoin balance <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetBalanceInfo(userId);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("racoin topup", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: racoin topup <user-id> <amount>";
            }
            if (Guid.TryParse(parts[2], out var userId) && decimal.TryParse(parts[3], out var amount))
            {
                var task = TopUpAsync(userId, amount, "Manual top-up via console");
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("racoin history", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: racoin history <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetTransactionHistoryInfo(userId);
            }
            return "Invalid user ID format";
        }

        return "Unknown RaCoin command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "RaCoin Management commands:",
            "  racoin stats              - Show RaCoin system statistics",
            "  racoin balance <user-id>  - Check user wallet balance",
            "  racoin topup <user-id> <amount> - Top up user wallet",
            "  racoin history <user-id>  - Show transaction history",
            "  help                      - Show this help message"
        );
    }

    private string GetStats()
    {
        lock (_lock)
        {
            var totalWallets = _wallets.Count;
            var activeWallets = _wallets.Values.Count(w => w.IsActive);
            var totalBalance = _wallets.Values.Sum(w => w.Balance);
            var totalTransactions = _transactions.Count;
            var totalTopUps = _transactions.Values.Count(t => t.Type == RaCoinTransactionType.TopUp);
            var totalPurchases = _transactions.Values.Count(t => t.Type == RaCoinTransactionType.Purchase);

            var stats = new
            {
                TotalWallets = totalWallets,
                ActiveWallets = activeWallets,
                TotalRaCoinsInCirculation = totalBalance,
                TotalTransactions = totalTransactions,
                TopUps = totalTopUps,
                Purchases = totalPurchases,
                AverageWalletBalance = activeWallets > 0 ? totalBalance / activeWallets : 0m
            };

            return JsonSerializer.Serialize(stats, _jsonOptions);
        }
    }

    private string GetBalanceInfo(Guid userId)
    {
        var wallet = GetOrCreateWallet(userId);
        return JsonSerializer.Serialize(new
        {
            UserId = userId,
            Balance = wallet.Balance,
            WalletId = wallet.Id,
            CreatedAt = wallet.CreatedAtUtc,
            LastUpdated = wallet.LastUpdatedUtc,
            IsActive = wallet.IsActive
        }, _jsonOptions);
    }

    private string GetTransactionHistoryInfo(Guid userId)
    {
        var history = GetTransactionHistory(userId);
        return JsonSerializer.Serialize(new
        {
            UserId = userId,
            TransactionCount = history.Count,
            Transactions = history.Select(t => new
            {
                t.Id,
                t.Type,
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.TimestampUtc,
                t.ReferenceId
            })
        }, _jsonOptions);
    }

    public RaCoinWallet GetOrCreateWallet(Guid userId)
    {
        lock (_lock)
        {
            var wallet = _wallets.Values.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new RaCoinWallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0m,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow,
                    IsActive = true
                };
                _wallets[wallet.Id] = wallet;
                LogInfo($"Created new RaCoin wallet for user {userId}");
            }
            return wallet;
        }
    }

    public decimal GetBalance(Guid userId)
    {
        var wallet = GetOrCreateWallet(userId);
        return wallet.Balance;
    }

    public async Task<RaCoinResponse> TopUpAsync(Guid userId, decimal amount, string description)
    {
        if (amount <= 0)
        {
            return new RaCoinResponse
            {
                Success = false,
                Message = "Top-up amount must be positive"
            };
        }

        lock (_lock)
        {
            var wallet = GetOrCreateWallet(userId);
            wallet.Balance += amount;
            wallet.LastUpdatedUtc = DateTime.UtcNow;

            var transaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.TopUp,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Description = description,
                TimestampUtc = DateTime.UtcNow
            };

            _transactions[transaction.Id] = transaction;
            LogInfo($"Top-up successful: User {userId} added {amount} RaCoins, new balance: {wallet.Balance}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully topped up {amount} RaCoins",
                NewBalance = wallet.Balance,
                Transaction = transaction
            };
        }
    }

    public async Task<RaCoinResponse> DeductAsync(Guid userId, decimal amount, string description, string? referenceId = null)
    {
        if (amount <= 0)
        {
            return new RaCoinResponse
            {
                Success = false,
                Message = "Deduction amount must be positive"
            };
        }

        lock (_lock)
        {
            var wallet = GetOrCreateWallet(userId);
            
            if (wallet.Balance < amount)
            {
                return new RaCoinResponse
                {
                    Success = false,
                    Message = $"Insufficient balance. Current: {wallet.Balance} RaCoins, Required: {amount} RaCoins"
                };
            }

            wallet.Balance -= amount;
            wallet.LastUpdatedUtc = DateTime.UtcNow;

            var transaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.Purchase,
                Amount = -amount,
                BalanceAfter = wallet.Balance,
                Description = description,
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = referenceId
            };

            _transactions[transaction.Id] = transaction;
            LogInfo($"Purchase successful: User {userId} spent {amount} RaCoins on '{description}', new balance: {wallet.Balance}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully deducted {amount} RaCoins",
                NewBalance = wallet.Balance,
                Transaction = transaction
            };
        }
    }

    public async Task<RaCoinResponse> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description)
    {
        if (amount <= 0)
        {
            return new RaCoinResponse
            {
                Success = false,
                Message = "Transfer amount must be positive"
            };
        }

        lock (_lock)
        {
            var fromWallet = GetOrCreateWallet(fromUserId);
            
            if (fromWallet.Balance < amount)
            {
                return new RaCoinResponse
                {
                    Success = false,
                    Message = $"Insufficient balance for transfer"
                };
            }

            var toWallet = GetOrCreateWallet(toUserId);

            fromWallet.Balance -= amount;
            fromWallet.LastUpdatedUtc = DateTime.UtcNow;

            toWallet.Balance += amount;
            toWallet.LastUpdatedUtc = DateTime.UtcNow;

            var transactionId = Guid.NewGuid();
            
            var fromTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = fromWallet.Id,
                UserId = fromUserId,
                Type = RaCoinTransactionType.Transfer,
                Amount = -amount,
                BalanceAfter = fromWallet.Balance,
                Description = $"Transfer to user {toUserId}: {description}",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = transactionId.ToString()
            };

            var toTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = toWallet.Id,
                UserId = toUserId,
                Type = RaCoinTransactionType.Transfer,
                Amount = amount,
                BalanceAfter = toWallet.Balance,
                Description = $"Transfer from user {fromUserId}: {description}",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = transactionId.ToString()
            };

            _transactions[fromTransaction.Id] = fromTransaction;
            _transactions[toTransaction.Id] = toTransaction;

            LogInfo($"Transfer successful: {amount} RaCoins from {fromUserId} to {toUserId}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully transferred {amount} RaCoins",
                NewBalance = fromWallet.Balance,
                Transaction = fromTransaction
            };
        }
    }

    public List<RaCoinTransaction> GetTransactionHistory(Guid userId, int limit = 50)
    {
        lock (_lock)
        {
            return _transactions.Values
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TimestampUtc)
                .Take(limit)
                .ToList();
        }
    }

    public async Task<RaCoinResponse> RefundAsync(Guid transactionId)
    {
        lock (_lock)
        {
            if (!_transactions.TryGetValue(transactionId, out var originalTransaction))
            {
                return new RaCoinResponse
                {
                    Success = false,
                    Message = "Transaction not found"
                };
            }

            if (originalTransaction.Type != RaCoinTransactionType.Purchase)
            {
                return new RaCoinResponse
                {
                    Success = false,
                    Message = "Only purchase transactions can be refunded"
                };
            }

            var wallet = GetOrCreateWallet(originalTransaction.UserId);
            var refundAmount = Math.Abs(originalTransaction.Amount);
            wallet.Balance += refundAmount;
            wallet.LastUpdatedUtc = DateTime.UtcNow;

            var refundTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                UserId = originalTransaction.UserId,
                Type = RaCoinTransactionType.Refund,
                Amount = refundAmount,
                BalanceAfter = wallet.Balance,
                Description = $"Refund for: {originalTransaction.Description}",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = transactionId.ToString()
            };

            _transactions[refundTransaction.Id] = refundTransaction;
            LogInfo($"Refund successful: {refundAmount} RaCoins refunded to user {originalTransaction.UserId}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully refunded {refundAmount} RaCoins",
                NewBalance = wallet.Balance,
                Transaction = refundTransaction
            };
        }
    }
    
    /// <summary>
    /// Get total RaCoins in the system across all wallets.
    /// </summary>
    public decimal GetTotalSystemRaCoins()
    {
        lock (_lock)
        {
            return _wallets.Values.Sum(w => w.Balance);
        }
    }
    
    /// <summary>
    /// Get balance asynchronously for a user.
    /// </summary>
    public Task<decimal> GetBalanceAsync(Guid userId)
    {
        return Task.FromResult(GetBalance(userId));
    }
    
    /// <summary>
    /// Add RaCoins to a user's wallet (admin function).
    /// </summary>
    public async Task<RaCoinResponse> AddAsync(Guid userId, decimal amount, string reason)
    {
        return await TopUpAsync(userId, amount, reason);
    }
}
