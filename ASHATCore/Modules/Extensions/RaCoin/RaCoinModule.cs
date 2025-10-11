using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.RaCoin;

/// <summary>
/// RaCoin Module - Handles cryptocurrency wallet management, Transactions, and top-ups.
/// Provides site-wide virtual currency for purchasing licenses, modules, and features.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class RaCoinModule : ModuleBase, IRaCoinModule
{
    public override string Name => "RaCoin";

    private readonly Dictionary<Guid, RaCoinWallet> _wallets = new();
    private readonly Dictionary<Guid, RaCoinTransaction> _Transactions = new();
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

        if (text.Equals("RaCoin stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("RaCoin balance", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: RaCoin balance <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetBalanceInfo(userId);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("RaCoin topup", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: RaCoin topup <user-id> <amount>";
            }
            if (Guid.TryParse(parts[2], out var userId) && decimal.TryParse(parts[3], out var amount))
            {
                var task = TopUpAsync(userId, amount, "Manual top-up via console");
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid Parameters";
        }

        if (text.StartsWith("RaCoin history", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: RaCoin history <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetTransactionHistoryInfo(userId);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("RaCoin exchange RaCoin-to-gold", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: RaCoin exchange RaCoin-to-gold <user-id> <RaCoin-amount>";
            }
            if (Guid.TryParse(parts[3], out var userId) && decimal.TryParse(parts[4], out var amount))
            {
                var task = ExchangeRaCoinToGoldAsync(userId, amount);
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid Parameters";
        }

        if (text.StartsWith("RaCoin exchange gold-to-RaCoin", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: RaCoin exchange gold-to-RaCoin <user-id> <gold-amount>";
            }
            if (Guid.TryParse(parts[3], out var userId) && decimal.TryParse(parts[4], out var amount))
            {
                var task = ExchangeGoldToRaCoinAsync(userId, amount);
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid Parameters";
        }

        if (text.StartsWith("RaCoin gold balance", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: RaCoin gold balance <user-id>";
            }
            if (Guid.TryParse(parts[3], out var userId))
            {
                return GetGoldBalanceInfo(userId);
            }
            return "Invalid user ID format";
        }

        if (text.Equals("RaCoin exchange Rates", StringComparison.OrdinalIgnoreCase))
        {
            return GetExchangeRates();
        }

        return "Unknown RaCoin command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "RaCoin Management commands:",
            "  RaCoin stats                          - Show RaCoin system statistics",
            "  RaCoin balance <user-id>              - Check user wallet balance",
            "  RaCoin topup <user-id> <amount>       - Top up user wallet",
            "  RaCoin history <user-id>              - Show Transaction history",
            "  RaCoin exchange Rates                 - Show currency exchange Rates",
            "  RaCoin exchange RaCoin-to-gold <user-id> <amount> - Exchange RaCoin to Gold",
            "  RaCoin exchange gold-to-RaCoin <user-id> <amount> - Exchange Gold to RaCoin",
            "  RaCoin gold balance <user-id>         - Check user Gold balance",
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
            var totalTransactions = _Transactions.Count;
            var totalTopUps = _Transactions.Values.Count(t => t.Type == RaCoinTransactionType.TopUp);
            var totalPurchases = _Transactions.Values.Count(t => t.Type == RaCoinTransactionType.Purchase);

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

            var Transaction = new RaCoinTransaction
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

            _Transactions[Transaction.Id] = Transaction;
            LogInfo($"Top-up successful: User {userId} added {amount} RaCoins, new balance: {wallet.Balance}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully topped up {amount} RaCoins",
                NewBalance = wallet.Balance,
                Transaction = Transaction
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

            var Transaction = new RaCoinTransaction
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

            _Transactions[Transaction.Id] = Transaction;
            LogInfo($"Purchase successful: User {userId} spent {amount} RaCoins on '{description}', new balance: {wallet.Balance}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully deducted {amount} RaCoins",
                NewBalance = wallet.Balance,
                Transaction = Transaction
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
                    Message = $"Insufficient balance for Transfer"
                };
            }

            var toWallet = GetOrCreateWallet(toUserId);

            fromWallet.Balance -= amount;
            fromWallet.LastUpdatedUtc = DateTime.UtcNow;

            toWallet.Balance += amount;
            toWallet.LastUpdatedUtc = DateTime.UtcNow;

            var TransactionId = Guid.NewGuid();
            
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
                ReferenceId = TransactionId.ToString()
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
                ReferenceId = TransactionId.ToString()
            };

            _Transactions[fromTransaction.Id] = fromTransaction;
            _Transactions[toTransaction.Id] = toTransaction;

            LogInfo($"Transfer successful: {amount} RaCoins from {fromUserId} to {toUserId}");

            return new RaCoinResponse
            {
                Success = true,
                Message = $"Successfully Transferred {amount} RaCoins",
                NewBalance = fromWallet.Balance,
                Transaction = fromTransaction
            };
        }
    }

    public List<RaCoinTransaction> GetTransactionHistory(Guid userId, int limit = 50)
    {
        lock (_lock)
        {
            return _Transactions.Values
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TimestampUtc)
                .Take(limit)
                .ToList();
        }
    }

    public async Task<RaCoinResponse> RefundAsync(Guid TransactionId)
    {
        await Task.CompletedTask;
        lock (_lock)
        {
            if (!_Transactions.TryGetValue(TransactionId, out var originalTransaction))
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
                    Message = "Only purchase Transactions can be refunded"
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
                ReferenceId = TransactionId.ToString()
            };

            _Transactions[refundTransaction.Id] = refundTransaction;
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
    /// Exchange RaCoin to Gold using standard ASHATtio.
    /// </summary>
    public async Task<CurrencyExchangeResponse> ExchangeRaCoinToGoldAsync(Guid userId, decimal RaCoinAmount)
    {
        await Task.CompletedTask;
        
        if (RaCoinAmount <= 0)
        {
            return new CurrencyExchangeResponse
            {
                Success = false,
                Message = "Exchange amount must be positive"
            };
        }

        lock (_lock)
        {
            var RaCoinWallet = GetOrCreateWallet(userId);
            
            if (RaCoinWallet.Balance < RaCoinAmount)
            {
                return new CurrencyExchangeResponse
                {
                    Success = false,
                    Message = $"Insufficient RaCoin balance. Current: {RaCoinWallet.Balance}, Required: {RaCoinAmount}"
                };
            }

            var goldWallet = GetOrCreateGoldWallet(userId);
            var goldAmount = CurrencyExchangeConstants.RaCoinToGold(RaCoinAmount);
            var exchangeRate = CurrencyExchangeConstants.GoldPerRaCoin;

            // Deduct RaCoin
            RaCoinWallet.Balance -= RaCoinAmount;
            RaCoinWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Add Gold
            goldWallet.Balance += goldAmount;
            goldWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Record Transaction
            var Transaction = new CurrencyExchangeTransaction
            {
                UserId = userId,
                ExchangeType = CurrencyExchangeType.RaCoinToGold,
                RaCoinAmount = RaCoinAmount,
                GoldAmount = goldAmount,
                ExchangeRate = exchangeRate,
                TimestampUtc = DateTime.UtcNow,
                Notes = "Standard Rate exchange"
            };

            // Record in RaCoin Transaction log
            var RaCoinTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = RaCoinWallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.Deduction,
                Amount = -RaCoinAmount,
                BalanceAfter = RaCoinWallet.Balance,
                Description = $"Exchanged to {goldAmount} Gold",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = Transaction.Id.ToString()
            };
            _Transactions[RaCoinTransaction.Id] = RaCoinTransaction;

            // Notify market monitor if available
            NotifyMarketMonitor(Transaction);

            LogInfo($"Currency exchange: User {userId} exchanged {RaCoinAmount} RaCoin to {goldAmount} Gold");

            return new CurrencyExchangeResponse
            {
                Success = true,
                Message = $"Successfully exchanged {RaCoinAmount} RaCoin to {goldAmount} Gold",
                NewRaCoinBalance = RaCoinWallet.Balance,
                NewGoldBalance = goldWallet.Balance,
                Transaction = Transaction
            };
        }
    }

    /// <summary>
    /// Exchange Gold to RaCoin using standard ASHATtio.
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

            var RaCoinWallet = GetOrCreateWallet(userId);
            var RaCoinAmount = CurrencyExchangeConstants.GoldToRaCoin(goldAmount);
            var exchangeRate = CurrencyExchangeConstants.RaCoinPerGold;

            // Deduct Gold
            goldWallet.Balance -= goldAmount;
            goldWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Add RaCoin
            RaCoinWallet.Balance += RaCoinAmount;
            RaCoinWallet.LastUpdatedUtc = DateTime.UtcNow;

            // Record Transaction
            var Transaction = new CurrencyExchangeTransaction
            {
                UserId = userId,
                ExchangeType = CurrencyExchangeType.GoldToRaCoin,
                RaCoinAmount = RaCoinAmount,
                GoldAmount = goldAmount,
                ExchangeRate = exchangeRate,
                TimestampUtc = DateTime.UtcNow,
                Notes = "Standard Rate exchange"
            };

            // Record in RaCoin Transaction log
            var RaCoinTransaction = new RaCoinTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = RaCoinWallet.Id,
                UserId = userId,
                Type = RaCoinTransactionType.TopUp,
                Amount = RaCoinAmount,
                BalanceAfter = RaCoinWallet.Balance,
                Description = $"Exchanged from {goldAmount} Gold",
                TimestampUtc = DateTime.UtcNow,
                ReferenceId = Transaction.Id.ToString()
            };
            _Transactions[RaCoinTransaction.Id] = RaCoinTransaction;

            // Notify market monitor if available
            NotifyMarketMonitor(Transaction);

            LogInfo($"Currency exchange: User {userId} exchanged {goldAmount} Gold to {RaCoinAmount} RaCoin");

            return new CurrencyExchangeResponse
            {
                Success = true,
                Message = $"Successfully exchanged {goldAmount} Gold to {RaCoinAmount} RaCoin",
                NewRaCoinBalance = RaCoinWallet.Balance,
                NewGoldBalance = goldWallet.Balance,
                Transaction = Transaction
            };
        }
    }

    private void NotifyMarketMonitor(CurrencyExchangeTransaction Transaction)
    {
        try
        {
            // Try to get MarketMonitor module and notify it
            var marketMonitor = _manager?.Modules
                .FirstOrDefault(m => m.Instance?.Name == "MarketMonitor")?.Instance;
            
            if (marketMonitor != null)
            {
                var method = marketMonitor.GetType().GetMethod("RecordExchange");
                method?.Invoke(marketMonitor, new object[] { Transaction });
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
