using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.SuperMarket;

/// <summary>
/// SuperMarket Module - E-commerce system for purchasing licenses, modules, and features using RaCoins.
/// Extends CMSSpawner functionality to provide an integrated shopping experience.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class SuperMarketModule : ModuleBase
{
    public override string Name => "SuperMarket";

    private ModuleManager? _manager;
    private IRaCoinModule? _racoinModule;
    private ILicenseModule? _licenseModule;
    
    private readonly Dictionary<Guid, SuperMarketProduct> _products = new();
    private readonly Dictionary<Guid, SuperMarketPurchase> _purchases = new();
    private readonly object _lock = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _racoinModule = _manager.GetModuleByName("RaCoin") as IRaCoinModule;
            _licenseModule = _manager.GetModuleByName("License") as ILicenseModule;
        }
        
        InitializeDefaultProducts();
        LogInfo("SuperMarket module initialized - Virtual marketplace active");
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

        if (text.Equals("market catalog", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("market products", StringComparison.OrdinalIgnoreCase))
        {
            return GetCatalog();
        }

        if (text.Equals("market stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("market buy", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: market buy <user-id> <product-id>";
            }
            if (Guid.TryParse(parts[2], out var userId) && Guid.TryParse(parts[3], out var productId))
            {
                var task = PurchaseProductAsync(userId, productId);
                task.Wait();
                return task.Result;
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("market history", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: market history <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return GetPurchaseHistory(userId);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("market add", StringComparison.OrdinalIgnoreCase))
        {
            // market add <name> <price> <category> <description>
            var parts = text.Split(' ', 5, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: market add <name> <price> <category> <description>";
            }
            if (decimal.TryParse(parts[3], out var price) && 
                Enum.TryParse<ProductCategory>(parts[4], true, out var category))
            {
                return AddProduct(parts[2], price, category, parts.Length > 5 ? parts[5] : "");
            }
            return "Invalid parameters";
        }

        return "Unknown SuperMarket command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "SuperMarket commands:",
            "  market catalog           - List all available products",
            "  market products          - Same as 'market catalog'",
            "  market stats             - Show marketplace statistics",
            "  market buy <user-id> <product-id> - Purchase a product",
            "  market history <user-id> - View purchase history",
            "  market add <name> <price> <category> <description> - Add new product (Admin)",
            "  help                     - Show this help message"
        );
    }

    private void InitializeDefaultProducts()
    {
        lock (_lock)
        {
            // Add default products if none exist
            if (_products.Count == 0)
            {
                AddProductInternal("Standard License", 100m, ProductCategory.License, 
                    "1-year standard license with basic features");
                AddProductInternal("Professional License", 500m, ProductCategory.License, 
                    "1-year professional license with advanced features");
                AddProductInternal("Enterprise License", 2000m, ProductCategory.License, 
                    "1-year enterprise license with unlimited features");
                AddProductInternal("Premium Theme Pack", 50m, ProductCategory.Theme, 
                    "Collection of premium UI themes");
                AddProductInternal("AI Language Module", 150m, ProductCategory.Module, 
                    "Advanced AI language processing capabilities");
                AddProductInternal("Game Engine Module", 300m, ProductCategory.Module, 
                    "Complete game development engine");
                
                LogInfo("Initialized default SuperMarket products");
            }
        }
    }

    private string AddProduct(string name, decimal price, ProductCategory category, string description)
    {
        var product = AddProductInternal(name, price, category, description);
        return JsonSerializer.Serialize(new
        {
            Success = true,
            Message = "Product added successfully",
            Product = new
            {
                product.Id,
                product.Name,
                product.PriceInRaCoins,
                product.Category,
                product.Description
            }
        }, _jsonOptions);
    }

    private SuperMarketProduct AddProductInternal(string name, decimal price, ProductCategory category, string description)
    {
        lock (_lock)
        {
            var product = new SuperMarketProduct
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                PriceInRaCoins = price,
                Category = category,
                StockQuantity = 999999, // Unlimited for digital products
                IsAvailable = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _products[product.Id] = product;
            return product;
        }
    }

    private string GetCatalog()
    {
        lock (_lock)
        {
            var catalog = _products.Values
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.PriceInRaCoins)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    PriceInRaCoins = p.PriceInRaCoins,
                    p.Category,
                    p.IsAvailable
                });

            return JsonSerializer.Serialize(new
            {
                TotalProducts = _products.Count,
                AvailableProducts = _products.Values.Count(p => p.IsAvailable),
                Products = catalog
            }, _jsonOptions);
        }
    }

    private string GetStats()
    {
        lock (_lock)
        {
            var stats = new
            {
                TotalProducts = _products.Count,
                AvailableProducts = _products.Values.Count(p => p.IsAvailable),
                TotalPurchases = _purchases.Count,
                TotalRevenue = _purchases.Values.Sum(p => p.PricePaid),
                ProductsByCategory = _products.Values
                    .GroupBy(p => p.Category)
                    .Select(g => new { Category = g.Key.ToString(), Count = g.Count() }),
                TopSellingProducts = _purchases.Values
                    .GroupBy(p => p.ProductName)
                    .Select(g => new { ProductName = g.Key, Sales = g.Count() })
                    .OrderByDescending(x => x.Sales)
                    .Take(5)
            };

            return JsonSerializer.Serialize(stats, _jsonOptions);
        }
    }

    private async Task<string> PurchaseProductAsync(Guid userId, Guid productId)
    {
        if (_racoinModule == null)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "RaCoin module not available"
            }, _jsonOptions);
        }

        lock (_lock)
        {
            if (!_products.TryGetValue(productId, out var product))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Product not found"
                }, _jsonOptions);
            }

            if (!product.IsAvailable)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Product is not available"
                }, _jsonOptions);
            }

            // Check user balance
            var balance = _racoinModule.GetBalance(userId);
            if (balance < product.PriceInRaCoins)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = $"Insufficient RaCoins. Required: {product.PriceInRaCoins}, Available: {balance}",
                    RequiredAmount = product.PriceInRaCoins,
                    CurrentBalance = balance
                }, _jsonOptions);
            }

            // Deduct RaCoins
            var deductTask = _racoinModule.DeductAsync(userId, product.PriceInRaCoins, 
                $"Purchase: {product.Name}", productId.ToString());
            deductTask.Wait();
            
            if (!deductTask.Result.Success)
            {
                return JsonSerializer.Serialize(deductTask.Result, _jsonOptions);
            }

            // Create purchase record
            var purchase = new SuperMarketPurchase
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                ProductName = product.Name,
                PricePaid = product.PriceInRaCoins,
                PurchasedAtUtc = DateTime.UtcNow,
                TransactionId = deductTask.Result.Transaction!.Id,
                Status = PurchaseStatus.Completed
            };

            _purchases[purchase.Id] = purchase;

            // If it's a license, create and assign it
            if (product.Category == ProductCategory.License && _licenseModule != null)
            {
                try
                {
                    var licenseType = product.Name.Contains("Enterprise") ? LicenseType.Enterprise :
                                     product.Name.Contains("Professional") ? LicenseType.Professional :
                                     LicenseType.Standard;
                    
                    var license = _licenseModule.CreateAndAssignLicense(userId, 
                        $"Purchased License - {product.Name}", licenseType, 1);
                    
                    purchase.Metadata["LicenseId"] = license.Id.ToString();
                    purchase.Metadata["LicenseKey"] = license.LicenseKey;
                    
                    LogInfo($"License created and assigned for purchase {purchase.Id}");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to create license: {ex.Message}");
                }
            }

            LogInfo($"Purchase successful: User {userId} bought '{product.Name}' for {product.PriceInRaCoins} RaCoins");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = $"Successfully purchased {product.Name}",
                Purchase = new
                {
                    purchase.Id,
                    purchase.ProductName,
                    purchase.PricePaid,
                    purchase.PurchasedAtUtc,
                    NewBalance = deductTask.Result.NewBalance,
                    LicenseKey = purchase.Metadata.GetValueOrDefault("LicenseKey")
                }
            }, _jsonOptions);
        }
    }

    private string GetPurchaseHistory(Guid userId)
    {
        lock (_lock)
        {
            var userPurchases = _purchases.Values
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PurchasedAtUtc)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.PricePaid,
                    p.PurchasedAtUtc,
                    p.Status,
                    LicenseKey = p.Metadata.GetValueOrDefault("LicenseKey")
                });

            return JsonSerializer.Serialize(new
            {
                UserId = userId,
                TotalPurchases = userPurchases.Count(),
                TotalSpent = userPurchases.Sum(p => p.PricePaid),
                Purchases = userPurchases
            }, _jsonOptions);
        }
    }

    /// <summary>
    /// Get product by ID - for external module access.
    /// </summary>
    public SuperMarketProduct? GetProduct(Guid productId)
    {
        lock (_lock)
        {
            return _products.GetValueOrDefault(productId);
        }
    }

    /// <summary>
    /// Get all available products - for external module access.
    /// </summary>
    public List<SuperMarketProduct> GetAvailableProducts()
    {
        lock (_lock)
        {
            return _products.Values.Where(p => p.IsAvailable).ToList();
        }
    }
}
