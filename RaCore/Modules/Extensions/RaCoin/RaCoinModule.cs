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
    private readonly Dictionary<Guid, GoldWallet> _goldWallets = new();
    private readonly object _lock = new();
    private ModuleManager? _manager;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        LogInfo("RaCoin module initialized - Virtual currency system active");
        LogInfo($"Currency Exchange System: {CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold");
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

        if (text.StartsWith("racoin exchange racoin-to-gold", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: racoin exchange racoin-to-gold <user-id> <racoin-amount>";
            }
            if (Guid.TryParse(parts[3], out var userId) && decimal.TryParse(parts[4], out var amount))
            {
                var task = ExchangeRaCoinToGoldAsync(userId, amount);
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("racoin exchange gold-to-racoin", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: racoin exchange gold-to-racoin <user-id> <gold-amount>";
            }
            if (Guid.TryParse(parts[3], out var userId) && decimal.TryParse(parts[4], out var amount))
            {
                var task = ExchangeGoldToRaCoinAsync(userId, amount);
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("racoin gold balance", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: racoin gold balance <user-id>";
            }
            if (Guid.TryParse(parts[3], out var userId))
            {
                return GetGoldBalanceInfo(userId);
            }
            return "Invalid user ID format";
        }

        if (text.Equals("racoin exchange rates", StringComparison.OrdinalIgnoreCase))
        {
            return GetExchangeRates();
        }

        return "Unknown RaCoin command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "RaCoin Management commands:",
            "  racoin stats                          - Show RaCoin system statistics",
            "  racoin balance <user-id>              - Check user wallet balance",
            "  racoin topup <user-id> <amount>       - Top up user wallet",
            "  racoin history <user-id>              - Show transaction history",
            "  racoin exchange rates                 - Show currency exchange rates",
            "  racoin exchange racoin-to-gold <user-id> <amount> - Exchange RaCoin to Gold",
            "  racoin exchange gold-to-racoin <user-id> <amount> - Exchange Gold to RaCoin",
            "  racoin gold balance <user-id>         - Check user Gold balance",
            "  help                                  - Show this help message",
            "",
            $"Standard Exchange Rate: {CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold"
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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

    /// <summary>
    /// Get or create a Gold wallet for a user.
    /// </summary>
    public GoldWallet GetOrCreateGoldWallet(Guid userId)
    {
        lock (_lock)
        {
            var wallet = _goldWallets.Values.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new GoldWallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0m,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow,
                    IsActive = true
                };
                _goldWallets[wallet.Id] = wallet;
                LogInfo($"Created new Gold wallet for user {userId}");
            }
            return wallet;
        }
    }

    /// <summary>
    /// Get Gold balance for a user.
    /// </summary>
    public decimal GetGoldBalance(Guid userId)
    {
        var wallet = GetOrCreateGoldWallet(userId);
        return wallet.Balance;
    }

    /// <summary>
    /// Exchange RaCoin to Gold using standard ratio.
    /// </summary>
    public async Task<CurrencyExchangeResponse> ExchangeRaCoinToGoldAsync(Guid userId, decimal racoinAmount)
    {
        await Task.CompletedTask;
        
        if (racoinAmount <= 0)
        {
            return new CurrencyExchangeResponse
            {
                Success = false,
                Message = "Exchange amount must be positive"
            };
        }

        lock (_lock)
        {
            var racoinWallet = GetOrCreateWallet(userId);
            
            if (racoinWallet.Balance < racoinAmount)
            {
                return new CurrencyExchangeResponse
                {
                    Success = false,
                    Message = $"Insufficient RaCoin balance. Current: {racoinWallet.Balance}, Required: {racoinAmount}"
                };
            }

            var goldWallet = GetOrCreateGoldWallet(userId);
            var goldAmount = CurrencyExchangeConstants.RaCoinToGold(racoinAmount);
            var exchangeRate = CurrencyExchangeConstants.GoldPerRaCoin;

            // Deduct RaCoin
            racoinWallet.Balance -= racoinAmount;
            racoinWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Add Gold
            goldWallet.Balance += goldAmount;
            goldWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Record transaction
            var transaction = new CurrencyExchangeTransaction
            {
                UserId = userId,
                ExchangeType = CurrencyExchangeType.RaCoinToGold,
                RaCoinAmount = racoinAmount,
                GoldAmount = goldAmount,
                ExchangeRate = exchangeRate,
                TimestampUtc = DateTime.UtcNow,
                Notes = "Standard rate exchange"
            };

            // Record in RaCoin transaction log
            var racoinTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = racoinWallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.Deduction,
                Amount = -racoinAmount,
                BalanceAfter = racoinWallet.Balance,
                Description = $"Exchanged to {goldAmount} Gold",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = transaction.Id.ToString()
            };
            _transactions[racoinTransaction.Id] = racoinTransaction;

            // Notify market monitor if available
            NotifyMarketMonitor(transaction);

            LogInfo($"Currency exchange: User {userId} exchanged {racoinAmount} RaCoin to {goldAmount} Gold");

            return new CurrencyExchangeResponse
            {
                Success = true,
                Message = $"Successfully exchanged {racoinAmount} RaCoin to {goldAmount} Gold",
                NewRaCoinBalance = racoinWallet.Balance,
                NewGoldBalance = goldWallet.Balance,
                Transaction = transaction
            };
        }
    }

    /// <summary>
    /// Exchange Gold to RaCoin using standard ratio.
    /// </summary>
    public async Task<CurrencyExchangeResponse> ExchangeGoldToRaCoinAsync(Guid userId, decimal goldAmount)
    {
        await Task.CompletedTask;
        
        if (goldAmount <= 0)
        {
            return new CurrencyExchangeResponse
            {
                Success = false,
                Message = "Exchange amount must be positive"
            };
        }

        lock (_lock)
        {
            var goldWallet = GetOrCreateGoldWallet(userId);
            
            if (goldWallet.Balance < goldAmount)
            {
                return new CurrencyExchangeResponse
                {
                    Success = false,
                    Message = $"Insufficient Gold balance. Current: {goldWallet.Balance}, Required: {goldAmount}"
                };
            }

            var racoinWallet = GetOrCreateWallet(userId);
            var racoinAmount = CurrencyExchangeConstants.GoldToRaCoin(goldAmount);
            var exchangeRate = CurrencyExchangeConstants.RaCoinPerGold;

            // Deduct Gold
            goldWallet.Balance -= goldAmount;
            goldWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Add RaCoin
            racoinWallet.Balance += racoinAmount;
            racoinWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Record transaction
            var transaction = new CurrencyExchangeTransaction
            {
                UserId = userId,
                ExchangeType = CurrencyExchangeType.GoldToRaCoin,
                RaCoinAmount = racoinAmount,
                GoldAmount = goldAmount,
                ExchangeRate = exchangeRate,
                TimestampUtc = DateTime.UtcNow,
                Notes = "Standard rate exchange"
            };

            // Record in RaCoin transaction log
            var racoinTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = racoinWallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.TopUp,
                Amount = racoinAmount,
                BalanceAfter = racoinWallet.Balance,
                Description = $"Exchanged from {goldAmount} Gold",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = transaction.Id.ToString()
            };
            _transactions[racoinTransaction.Id] = racoinTransaction;

            // Notify market monitor if available
            NotifyMarketMonitor(transaction);

            LogInfo($"Currency exchange: User {userId} exchanged {goldAmount} Gold to {racoinAmount} RaCoin");

            return new CurrencyExchangeResponse
            {
                Success = true,
                Message = $"Successfully exchanged {goldAmount} Gold to {racoinAmount} RaCoin",
                NewRaCoinBalance = racoinWallet.Balance,
                NewGoldBalance = goldWallet.Balance,
                Transaction = transaction
            };
        }
    }

    private void NotifyMarketMonitor(CurrencyExchangeTransaction transaction)
    {
        try
        {
            // Try to get MarketMonitor module and notify it
            var marketMonitor = _manager?.Modules
                .FirstOrDefault(m => m.Instance?.Name == "MarketMonitor")?.Instance;
            
            if (marketMonitor != null)
            {
                var method = marketMonitor.GetType().GetMethod("RecordExchange");
                method?.Invoke(marketMonitor, new object[] { transaction });
            }
        }
        catch (Exception ex)
        {
            LogInfo($"Failed to notify market monitor: {ex.Message}");
        }
    }

    private string GetGoldBalanceInfo(Guid userId)
    {
        var wallet = GetOrCreateGoldWallet(userId);
        return JsonSerializer.Serialize(new
        {
            UserId = userId,
            GoldBalance = wallet.Balance,
            WalletId = wallet.Id,
            CreatedAt = wallet.CreatedAtUtc,
            LastUpdated = wallet.LastUpdatedUtc,
            IsActive = wallet.IsActive
        }, _jsonOptions);
    }

    private string GetExchangeRates()
    {
        lock (_lock)
        {
            var totalGold = _goldWallets.Values.Sum(w => w.Balance);
            
            return JsonSerializer.Serialize(new
            {
                StandardExchangeRate = new
                {
                    RaCoin = CurrencyExchangeConstants.StandardRaCoinAmount,
                    Gold = CurrencyExchangeConstants.StandardGoldAmount,
                    Formula = $"{CurrencyExchangeConstants.StandardRaCoinAmount} RaCoin = {CurrencyExchangeConstants.StandardGoldAmount} Gold"
                },
                ConversionRates = new
                {
                    GoldPerRaCoin = CurrencyExchangeConstants.GoldPerRaCoin,
                    RaCoinPerGold = CurrencyExchangeConstants.RaCoinPerGold
                },
                SystemStatistics = new
                {
                    TotalRaCoinsInCirculation = GetTotalSystemRaCoins(),
                    TotalGoldInCirculation = totalGold,
                    TotalGoldWallets = _goldWallets.Count,
                    ActiveGoldWallets = _goldWallets.Values.Count(w => w.IsActive)
                },
                ExampleConversions = new
                {
                    Example1 = $"1,000 RaCoin = {CurrencyExchangeConstants.RaCoinToGold(1000)} Gold",
                    Example2 = $"10,000 RaCoin = {CurrencyExchangeConstants.RaCoinToGold(10000)} Gold",
                    Example3 = $"100,000 Gold = {CurrencyExchangeConstants.GoldToRaCoin(100000)} RaCoin"
                }
            }, _jsonOptions);
        }
    }
}
