using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.LegendaryPay;

/// <summary>
/// LegendaryPay Module - Next-generation payment system for RaOS and generated content platforms.
/// Phase 9.3.5: Initial implementation with Dev Mode for testing and development.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class LegendaryPayModule : ModuleBase
{
    public override string Name => "LegendaryPay";

    private ModuleManager? _manager;
    private IRaCoinModule? _racoinModule;
    private readonly object _lock = new();
    
    // Dev Mode settings - now tied to server-wide mode
    private bool _isDevMode = false; // Will be set from server configuration
    private readonly decimal _devModeRewardAmount = 1m; // 1 Gold per approved action in Dev Mode
    
    // Payment action tracking
    private readonly Dictionary<Guid, PaymentAction> _paymentActions = new();
    private readonly Dictionary<Guid, List<PaymentAction>> _userActions = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _racoinModule = _manager.GetModuleByName("RaCoin") as IRaCoinModule;
        }
        
        LogInfo("LegendaryPay module initialized - Next-generation payment system");
        LogInfo($"Mode: {(_isDevMode ? "DEV MODE" : "Production")} (will sync with server mode)");
        
        if (_isDevMode)
        {
            LogInfo($"Dev Mode Active: All approved actions generate {_devModeRewardAmount} Gold per user");
            LogInfo("Applies to: User Activity on Forums, Chat Rooms, Blogs, and Game Servers");
        }
    }
    
    /// <summary>
    /// Set Dev mode from server configuration (called by FirstRunManager or ServerConfigModule)
    /// </summary>
    public void SetDevModeFromServer(bool isDevMode)
    {
        lock (_lock)
        {
            var oldMode = _isDevMode ? "Development" : "Production";
            _isDevMode = isDevMode;
            var newMode = _isDevMode ? "Development" : "Production";
            
            if (oldMode != newMode)
            {
                LogInfo($"LegendaryPay mode synced with server: {oldMode} -> {newMode}");
            }
        }
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

        if (text.Equals("pay status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatus();
        }

        if (text.StartsWith("pay action", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: pay action <user-id> <action-description>";
            }
            
            if (Guid.TryParse(parts[2], out var userId))
            {
                var description = string.Join(" ", parts.Skip(3));
                return ProcessPaymentAction(userId, description);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("pay approve", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: pay approve <action-id>";
            }
            
            if (Guid.TryParse(parts[2], out var actionId))
            {
                return ApprovePaymentAction(actionId);
            }
            return "Invalid action ID format";
        }

        if (text.StartsWith("pay history", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: pay history <user-id>";
            }
            
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetUserPaymentHistory(userId);
            }
            return "Invalid user ID format";
        }

        if (text.Equals("pay stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetPaymentStats();
        }

        return "Unknown LegendaryPay command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== LegendaryPay Module - Help ===");
        sb.AppendLine();
        sb.AppendLine("Next-generation payment system for RaOS platforms");
        sb.AppendLine($"Current Mode: {(_isDevMode ? "DEV MODE" : "Production")}");
        sb.AppendLine();
        sb.AppendLine("Commands:");
        sb.AppendLine("  pay status                          - Show module status and mode");
        sb.AppendLine("  pay action <user-id> <description>  - Create payment action");
        sb.AppendLine("  pay approve <action-id>             - Approve payment action (triggers reward in Dev Mode)");
        sb.AppendLine("  pay history <user-id>               - View user's payment action history");
        sb.AppendLine("  pay stats                           - Show payment system statistics");
        sb.AppendLine();
        sb.AppendLine("Dev Mode Features:");
        sb.AppendLine($"  - All approved actions generate {_devModeRewardAmount} Gold per user");
        sb.AppendLine("  - Applies to User Activity on Forums, Chat Rooms, Blogs, and Game Servers");
        sb.AppendLine("  - Does NOT apply to site-wide homepages");
        sb.AppendLine();
        sb.AppendLine("Note: Dev mode is controlled by server configuration (use 'serverconfig mode Dev')");
        sb.AppendLine();
        sb.AppendLine("Module Category: extensions");
        sb.AppendLine("Status: Active (Phase 9.3.5)");
        
        return sb.ToString();
    }

    private string GetStatus()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(new
            {
                Module = "LegendaryPay",
                Version = "1.0.0",
                Phase = "9.3.5",
                Mode = _isDevMode ? "Development" : "Production",
                DevModeSettings = new
                {
                    Enabled = _isDevMode,
                    RewardPerAction = $"{_devModeRewardAmount} Gold",
                    AppliesTo = new[]
                    {
                        "User Activity on Forums",
                        "User Activity on Chat Rooms",
                        "User Activity on Blogs",
                        "User Activity on Game Servers"
                    },
                    DoesNotApplyTo = new[]
                    {
                        "Site-Wide Homepages",
                        "LegendarySiteBuilder Homepage",
                        "AGP Studios INC Homepage"
                    }
                },
                Statistics = new
                {
                    TotalActions = _paymentActions.Count,
                    ApprovedActions = _paymentActions.Values.Count(a => a.IsApproved),
                    PendingActions = _paymentActions.Values.Count(a => !a.IsApproved && !a.IsRejected),
                    TotalUsers = _userActions.Count
                },
                RaCoinModuleConnected = _racoinModule != null
            }, _jsonOptions);
        }
    }

    private string ProcessPaymentAction(Guid userId, string description)
    {
        lock (_lock)
        {
            var action = new PaymentAction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false,
                IsRejected = false
            };

            _paymentActions[action.Id] = action;
            
            if (!_userActions.ContainsKey(userId))
            {
                _userActions[userId] = new List<PaymentAction>();
            }
            _userActions[userId].Add(action);

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Payment action created",
                ActionId = action.Id,
                UserId = userId,
                Description = description,
                Status = "Pending Approval",
                DevModeNote = _isDevMode ? $"Will generate {_devModeRewardAmount} Gold when approved" : null
            }, _jsonOptions);
        }
    }

    private string ApprovePaymentAction(Guid actionId)
    {
        lock (_lock)
        {
            if (!_paymentActions.TryGetValue(actionId, out var action))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Payment action not found"
                }, _jsonOptions);
            }

            if (action.IsApproved)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Payment action already approved"
                }, _jsonOptions);
            }

            action.IsApproved = true;
            action.ApprovedAt = DateTime.UtcNow;

            // In Dev Mode, generate Gold reward
            if (_isDevMode && _racoinModule != null)
            {
                // Access Gold wallets via reflection (similar to SuperMarket module)
                var goldWalletsField = _racoinModule.GetType().GetField("_goldWallets",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (goldWalletsField != null)
                {
                    var goldWallets = goldWalletsField.GetValue(_racoinModule) as Dictionary<Guid, GoldWallet>;
                    
                    if (goldWallets != null)
                    {
                        if (!goldWallets.TryGetValue(action.UserId, out var wallet))
                        {
                            wallet = new GoldWallet { UserId = action.UserId };
                            goldWallets[action.UserId] = wallet;
                        }

                        wallet.Balance += _devModeRewardAmount;
                        wallet.LastUpdatedUtc = DateTime.UtcNow;
                        action.RewardAmount = _devModeRewardAmount;
                    }
                }
            }

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Payment action approved",
                ActionId = actionId,
                UserId = action.UserId,
                Description = action.Description,
                ApprovedAt = action.ApprovedAt,
                DevModeReward = _isDevMode ? new
                {
                    Amount = $"{_devModeRewardAmount} Gold",
                    Note = "Dev Mode reward generated"
                } : null
            }, _jsonOptions);
        }
    }

    private string GetUserPaymentHistory(Guid userId)
    {
        lock (_lock)
        {
            if (!_userActions.TryGetValue(userId, out var actions))
            {
                return JsonSerializer.Serialize(new
                {
                    UserId = userId,
                    Message = "No payment actions found for this user",
                    TotalActions = 0
                }, _jsonOptions);
            }

            var history = actions.Select(a => new
            {
                a.Id,
                a.Description,
                a.CreatedAt,
                Status = a.IsApproved ? "Approved" : (a.IsRejected ? "Rejected" : "Pending"),
                a.ApprovedAt,
                RewardAmount = a.RewardAmount > 0 ? $"{a.RewardAmount} Gold" : null
            }).OrderByDescending(a => a.CreatedAt);

            return JsonSerializer.Serialize(new
            {
                UserId = userId,
                TotalActions = actions.Count,
                ApprovedActions = actions.Count(a => a.IsApproved),
                PendingActions = actions.Count(a => !a.IsApproved && !a.IsRejected),
                History = history
            }, _jsonOptions);
        }
    }

    private string GetPaymentStats()
    {
        lock (_lock)
        {
            var totalRewards = _paymentActions.Values
                .Where(a => a.IsApproved)
                .Sum(a => a.RewardAmount);

            return JsonSerializer.Serialize(new
            {
                Module = "LegendaryPay",
                Mode = _isDevMode ? "Development" : "Production",
                Statistics = new
                {
                    TotalActions = _paymentActions.Count,
                    ApprovedActions = _paymentActions.Values.Count(a => a.IsApproved),
                    RejectedActions = _paymentActions.Values.Count(a => a.IsRejected),
                    PendingActions = _paymentActions.Values.Count(a => !a.IsApproved && !a.IsRejected),
                    TotalUsers = _userActions.Count,
                    TotalRewardsGenerated = _isDevMode ? $"{totalRewards} Gold" : "N/A (Production Mode)",
                    AverageRewardPerAction = _isDevMode && totalRewards > 0 
                        ? $"{totalRewards / _paymentActions.Values.Count(a => a.IsApproved)} Gold" 
                        : "N/A"
                },
                TopUsers = _userActions
                    .Select(kvp => new
                    {
                        UserId = kvp.Key,
                        TotalActions = kvp.Value.Count,
                        ApprovedActions = kvp.Value.Count(a => a.IsApproved),
                        TotalRewards = kvp.Value.Where(a => a.IsApproved).Sum(a => a.RewardAmount)
                    })
                    .OrderByDescending(u => u.TotalRewards)
                    .Take(10)
            }, _jsonOptions);
        }
    }
}

/// <summary>
/// Represents a payment action in the LegendaryPay system.
/// </summary>
public class PaymentAction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public decimal RewardAmount { get; set; } = 0m;
}
