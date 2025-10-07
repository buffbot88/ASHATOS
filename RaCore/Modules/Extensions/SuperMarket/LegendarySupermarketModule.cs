using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.UserProfiles;

namespace RaCore.Modules.Extensions.SuperMarket;

/// <summary>
/// Legendary Supermarket Module - Unified central marketplace with dual currency support.
/// RaCoin side: Premium tier for subscriptions, licensing, and premium items.
/// Gold side: Free tier for in-game items and player-to-player trading.
/// Features: Advanced search, seller profiles, ratings, and dual-currency support.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class LegendarySupermarketModule : ModuleBase
{
    public override string Name => "LegendarySupermarket";

    private ModuleManager? _manager;
    private IRaCoinModule? _racoinModule;
    private ILicenseModule? _licenseModule;
    private UserProfileModule? _profileModule;
    
    // Official store products (RaCoin side - licenses, modules, features)
    private readonly Dictionary<Guid, SuperMarketProduct> _officialProducts = new();
    
    // User marketplace listings (both RaCoin and Gold)
    private readonly Dictionary<Guid, MarketplaceListing> _marketplaceListings = new();
    
    // Purchases and transactions
    private readonly Dictionary<Guid, SuperMarketPurchase> _purchases = new();
    private readonly Dictionary<Guid, MarketplaceTransaction> _marketplaceTransactions = new();
    
    // Seller information and ratings
    private readonly Dictionary<Guid, SellerInfo> _sellers = new();
    private readonly Dictionary<Guid, List<SellerReview>> _sellerReviews = new();
    
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
            _profileModule = _manager.GetModuleByName("UserProfile") as UserProfileModule;
        }
        
        InitializeDefaultProducts();
        LogInfo("Legendary Supermarket module initialized - Dual-currency marketplace active");
        LogInfo("RaCoin side: Premium subscriptions, licensing, and premium items");
        LogInfo("Gold side: Free-tier in-game items and player trading");
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

        // Official Store Commands (RaCoin side)
        if (text.Equals("market catalog", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("market products", StringComparison.OrdinalIgnoreCase))
        {
            return GetOfficialCatalog();
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
                var task = PurchaseOfficialProductAsync(userId, productId);
                task.Wait();
                return task.Result;
            }
            return "Invalid parameters";
        }

        // Marketplace Commands (dual currency)
        if (text.StartsWith("market list", StringComparison.OrdinalIgnoreCase))
        {
            // market list <seller-id> <item-name> <currency-type> <price> <quantity> <category> <description>
            var parts = text.Split(' ', 8, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8)
            {
                return "Usage: market list <seller-id> <item-name> <currency-type> <price> <quantity> <category> <description>";
            }
            if (Guid.TryParse(parts[2], out var sellerId) &&
                Enum.TryParse<CurrencyType>(parts[4], true, out var currencyType) &&
                decimal.TryParse(parts[5], out var price) &&
                int.TryParse(parts[6], out var quantity) &&
                Enum.TryParse<ProductCategory>(parts[7], true, out var category))
            {
                return CreateListing(sellerId, parts[3], currencyType, price, quantity, category, 
                    parts.Length > 8 ? parts[8] : "");
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("market search", StringComparison.OrdinalIgnoreCase))
        {
            var searchTerm = text.Substring("market search".Length).Trim();
            return SearchMarketplace(searchTerm);
        }

        if (text.StartsWith("market filter", StringComparison.OrdinalIgnoreCase))
        {
            // market filter <currency-type> [category]
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: market filter <currency-type> [category]";
            }
            if (Enum.TryParse<CurrencyType>(parts[2], true, out var currencyType))
            {
                ProductCategory? category = null;
                if (parts.Length > 3 && Enum.TryParse<ProductCategory>(parts[3], true, out var cat))
                {
                    category = cat;
                }
                return FilterMarketplace(currencyType, category);
            }
            return "Invalid currency type. Use: RaCoin or Gold";
        }

        if (text.StartsWith("market purchase", StringComparison.OrdinalIgnoreCase))
        {
            // market purchase <buyer-id> <listing-id> <quantity>
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: market purchase <buyer-id> <listing-id> <quantity>";
            }
            if (Guid.TryParse(parts[2], out var buyerId) &&
                Guid.TryParse(parts[3], out var listingId) &&
                int.TryParse(parts[4], out var quantity))
            {
                var task = PurchaseFromMarketplaceAsync(buyerId, listingId, quantity);
                task.Wait();
                return task.Result;
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("market seller", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: market seller <seller-id>";
            }
            if (Guid.TryParse(parts[2], out var sellerId))
            {
                return GetSellerInfo(sellerId);
            }
            return "Invalid seller ID format";
        }

        if (text.StartsWith("market review", StringComparison.OrdinalIgnoreCase))
        {
            // market review <reviewer-id> <seller-id> <purchase-id> <rating> <comment>
            var parts = text.Split(' ', 7, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 7)
            {
                return "Usage: market review <reviewer-id> <seller-id> <purchase-id> <rating> <comment>";
            }
            if (Guid.TryParse(parts[2], out var reviewerId) &&
                Guid.TryParse(parts[3], out var sellerId) &&
                Guid.TryParse(parts[4], out var purchaseId) &&
                int.TryParse(parts[5], out var rating))
            {
                return AddReview(reviewerId, sellerId, purchaseId, rating, parts[6]);
            }
            return "Invalid parameters";
        }

        if (text.Equals("market stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetMarketStats();
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

        return "Unknown Legendary Supermarket command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "=== LEGENDARY SUPERMARKET COMMANDS ===",
            "",
            "Official Store (RaCoin - Premium Tier):",
            "  market catalog                     - View official products (licenses, modules, features)",
            "  market buy <user-id> <product-id>  - Purchase official product with RaCoins",
            "",
            "User Marketplace (RaCoin & Gold):",
            "  market list <seller-id> <item-name> <currency> <price> <qty> <category> <desc>",
            "                                     - List item for sale (currency: RaCoin or Gold)",
            "  market search <term>               - Search marketplace by name/description",
            "  market filter <currency> [category]- Filter by currency and optional category",
            "  market purchase <buyer-id> <listing-id> <quantity>",
            "                                     - Purchase from marketplace",
            "",
            "Seller Information:",
            "  market seller <seller-id>          - View seller profile and ratings",
            "  market review <reviewer-id> <seller-id> <purchase-id> <rating> <comment>",
            "                                     - Leave review (rating: 1-5)",
            "",
            "General:",
            "  market stats                       - Marketplace statistics",
            "  market history <user-id>           - View purchase history",
            "  help                               - Show this help message",
            "",
            "Currency Types:",
            "  RaCoin - Premium tier (subscriptions, licensing, premium items)",
            "  Gold   - Free tier (in-game items, player trading)"
        );
    }

    private void InitializeDefaultProducts()
    {
        lock (_lock)
        {
            // Add default official products if none exist
            // USD to RaCoin Ratio: 1:1000 (e.g., $20 USD = 20,000 RaCoins)
            if (_officialProducts.Count == 0)
            {
                // Reseller Pricing (AGPStudios Licensed Products)
                AddOfficialProductInternal("Forum Script License", 20000m, ProductCategory.License, 
                    "1-year Forum Script license - Full-featured forum system ($20 USD)");
                AddOfficialProductInternal("CMS Script License", 20000m, ProductCategory.License, 
                    "1-year CMS Script license - Complete content management system ($20 USD)");
                AddOfficialProductInternal("Custom Game Server License", 1000000m, ProductCategory.License, 
                    "1-year Custom Game Server license - Unified client with server-side assets ($1000 USD)");
                
                // Standard License Tiers
                AddOfficialProductInternal("Standard License", 100m, ProductCategory.License, 
                    "1-year standard license with basic features");
                AddOfficialProductInternal("Professional License", 500m, ProductCategory.License, 
                    "1-year professional license with advanced features");
                AddOfficialProductInternal("Enterprise License", 2000m, ProductCategory.License, 
                    "1-year enterprise license with unlimited features");
                
                // Additional Products
                AddOfficialProductInternal("Premium Theme Pack", 50m, ProductCategory.Theme, 
                    "Collection of premium UI themes");
                AddOfficialProductInternal("AI Language Module", 150m, ProductCategory.Module, 
                    "Advanced AI language processing capabilities");
                AddOfficialProductInternal("Game Engine Module", 300m, ProductCategory.Module, 
                    "Complete game development engine");
                AddOfficialProductInternal("AI Content Generation", 500m, ProductCategory.Module,
                    "AI-powered game asset and content generation system");
                
                LogInfo("Initialized Legendary Supermarket with official products (RaCoin side)");
            }
        }
    }

    private SuperMarketProduct AddOfficialProductInternal(string name, decimal price, ProductCategory category, string description)
    {
        lock (_lock)
        {
            var product = new SuperMarketProduct
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                PriceInRaCoins = price,
                PrimaryCurrency = CurrencyType.RaCoin,
                Category = category,
                StockQuantity = 999999, // Unlimited for digital products
                IsAvailable = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _officialProducts[product.Id] = product;
            return product;
        }
    }

    private string GetOfficialCatalog()
    {
        lock (_lock)
        {
            var catalog = _officialProducts.Values
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
                Store = "Official RaCoin Store",
                Currency = "RaCoin",
                TotalProducts = _officialProducts.Count,
                AvailableProducts = _officialProducts.Values.Count(p => p.IsAvailable),
                Products = catalog
            }, _jsonOptions);
        }
    }

    private async Task<string> PurchaseOfficialProductAsync(Guid userId, Guid productId)
    {
        await Task.CompletedTask;
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
            if (!_officialProducts.TryGetValue(productId, out var product))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Product not found in official store"
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

            LogInfo($"Official store purchase: User {userId} bought '{product.Name}' for {product.PriceInRaCoins} RaCoins");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = $"Successfully purchased {product.Name} from official store",
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

    private string CreateListing(Guid sellerId, string itemName, CurrencyType currencyType, decimal price, 
        int quantity, ProductCategory category, string description)
    {
        lock (_lock)
        {
            // Get or create seller info
            if (!_sellers.ContainsKey(sellerId))
            {
                var sellerTask = GetOrCreateSellerInfoAsync(sellerId);
                sellerTask.Wait();
            }

            var listing = new MarketplaceListing
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                SellerName = _sellers.GetValueOrDefault(sellerId)?.Username ?? "Unknown",
                ItemName = itemName,
                Description = description,
                CurrencyType = currencyType,
                Price = price,
                Quantity = quantity,
                Category = category,
                IsAvailable = true,
                ListedAtUtc = DateTime.UtcNow
            };

            _marketplaceListings[listing.Id] = listing;
            
            // Update seller active listings count
            if (_sellers.TryGetValue(sellerId, out var seller))
            {
                seller.ActiveListings++;
            }

            LogInfo($"New marketplace listing: {itemName} by seller {sellerId} for {price} {currencyType}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Listing created successfully",
                Listing = new
                {
                    listing.Id,
                    listing.ItemName,
                    listing.CurrencyType,
                    listing.Price,
                    listing.Quantity,
                    listing.Category,
                    listing.ListedAtUtc
                }
            }, _jsonOptions);
        }
    }

    private async Task<SellerInfo> GetOrCreateSellerInfoAsync(Guid sellerId)
    {
        await Task.CompletedTask;
        
        lock (_lock)
        {
            if (_sellers.TryGetValue(sellerId, out var existing))
            {
                return existing;
            }

            // Try to get profile from UserProfile module
            string username = $"User-{sellerId.ToString()[..8]}";
            string? displayName = null;
            string? bio = null;
            string? avatarUrl = null;

            if (_profileModule != null)
            {
                var profileTask = _profileModule.GetProfileAsync(sellerId.ToString());
                profileTask.Wait();
                var profile = profileTask.Result;
                
                if (profile != null)
                {
                    username = profile.UserId;
                    displayName = profile.DisplayName;
                    bio = profile.Bio;
                    avatarUrl = profile.AvatarUrl;
                }
            }

            var seller = new SellerInfo
            {
                UserId = sellerId,
                Username = username,
                DisplayName = displayName,
                Bio = bio,
                AvatarUrl = avatarUrl,
                Rating = 0,
                TotalSales = 0,
                ActiveListings = 0,
                MemberSince = DateTime.UtcNow
            };

            _sellers[sellerId] = seller;
            return seller;
        }
    }

    private string SearchMarketplace(string searchTerm)
    {
        lock (_lock)
        {
            var results = _marketplaceListings.Values
                .Where(l => l.IsAvailable &&
                           (l.ItemName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(l => l.ListedAtUtc)
                .Select(l => new
                {
                    l.Id,
                    l.ItemName,
                    l.Description,
                    l.CurrencyType,
                    l.Price,
                    l.Quantity,
                    l.Category,
                    l.SellerName,
                    SellerRating = _sellers.GetValueOrDefault(l.SellerId)?.Rating ?? 0,
                    l.ListedAtUtc
                })
                .ToList();

            return JsonSerializer.Serialize(new
            {
                SearchTerm = searchTerm,
                ResultCount = results.Count,
                Results = results
            }, _jsonOptions);
        }
    }

    private string FilterMarketplace(CurrencyType currencyType, ProductCategory? category)
    {
        lock (_lock)
        {
            var query = _marketplaceListings.Values
                .Where(l => l.IsAvailable && l.CurrencyType == currencyType);

            if (category.HasValue)
            {
                query = query.Where(l => l.Category == category.Value);
            }

            var results = query
                .OrderByDescending(l => l.ListedAtUtc)
                .Select(l => new
                {
                    l.Id,
                    l.ItemName,
                    l.Description,
                    l.CurrencyType,
                    l.Price,
                    l.Quantity,
                    l.Category,
                    l.SellerName,
                    SellerRating = _sellers.GetValueOrDefault(l.SellerId)?.Rating ?? 0,
                    l.ListedAtUtc
                })
                .ToList();

            return JsonSerializer.Serialize(new
            {
                Filter = new { CurrencyType = currencyType.ToString(), Category = category?.ToString() ?? "All" },
                ResultCount = results.Count,
                Results = results
            }, _jsonOptions);
        }
    }

    private async Task<string> PurchaseFromMarketplaceAsync(Guid buyerId, Guid listingId, int quantity)
    {
        await Task.CompletedTask;

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
            if (!_marketplaceListings.TryGetValue(listingId, out var listing))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Listing not found"
                }, _jsonOptions);
            }

            if (!listing.IsAvailable)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Listing is no longer available"
                }, _jsonOptions);
            }

            if (listing.Quantity < quantity)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = $"Insufficient quantity. Available: {listing.Quantity}, Requested: {quantity}"
                }, _jsonOptions);
            }

            var totalPrice = listing.Price * quantity;

            // Process payment based on currency type
            if (listing.CurrencyType == CurrencyType.RaCoin)
            {
                var balance = _racoinModule.GetBalance(buyerId);
                if (balance < totalPrice)
                {
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = $"Insufficient RaCoins. Required: {totalPrice}, Available: {balance}"
                    }, _jsonOptions);
                }

                // Deduct from buyer
                var deductTask = _racoinModule.DeductAsync(buyerId, totalPrice, 
                    $"Marketplace purchase: {listing.ItemName}", listingId.ToString());
                deductTask.Wait();

                if (!deductTask.Result.Success)
                {
                    return JsonSerializer.Serialize(deductTask.Result, _jsonOptions);
                }

                // Credit seller (85% after 15% marketplace fee)
                var sellerAmount = totalPrice * 0.85m;
                var creditTask = _racoinModule.TopUpAsync(listing.SellerId, sellerAmount, 
                    $"Marketplace sale: {listing.ItemName}");
                creditTask.Wait();
            }
            else // Gold
            {
                // Check Gold balance
                var goldWallets = _racoinModule.GetType().GetField("_goldWallets", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (goldWallets != null)
                {
                    var wallets = goldWallets.GetValue(_racoinModule) as Dictionary<Guid, GoldWallet>;
                    if (wallets != null)
                    {
                        if (!wallets.TryGetValue(buyerId, out var buyerWallet) || buyerWallet.Balance < totalPrice)
                        {
                            return JsonSerializer.Serialize(new
                            {
                                Success = false,
                                Message = $"Insufficient Gold. Required: {totalPrice}"
                            }, _jsonOptions);
                        }

                        // Deduct from buyer
                        buyerWallet.Balance -= totalPrice;
                        buyerWallet.LastUpdatedUtc = DateTime.UtcNow;

                        // Credit seller (85% after 15% marketplace fee)
                        var sellerAmount = totalPrice * 0.85m;
                        if (!wallets.TryGetValue(listing.SellerId, out var sellerWallet))
                        {
                            sellerWallet = new GoldWallet { UserId = listing.SellerId };
                            wallets[listing.SellerId] = sellerWallet;
                        }
                        sellerWallet.Balance += sellerAmount;
                        sellerWallet.LastUpdatedUtc = DateTime.UtcNow;
                    }
                }
            }

            // Update listing quantity
            listing.Quantity -= quantity;
            if (listing.Quantity == 0)
            {
                listing.IsAvailable = false;
            }

            // Create transaction record
            var transaction = new MarketplaceTransaction
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                BuyerId = buyerId,
                SellerId = listing.SellerId,
                ItemName = listing.ItemName,
                CurrencyType = listing.CurrencyType,
                Price = totalPrice,
                Quantity = quantity,
                TransactionAtUtc = DateTime.UtcNow,
                Status = PurchaseStatus.Completed
            };

            _marketplaceTransactions[transaction.Id] = transaction;

            // Update seller stats
            if (_sellers.TryGetValue(listing.SellerId, out var seller))
            {
                seller.TotalSales++;
                if (listing.Quantity == 0)
                {
                    seller.ActiveListings--;
                }
            }

            LogInfo($"Marketplace transaction: {buyerId} purchased {quantity}x {listing.ItemName} for {totalPrice} {listing.CurrencyType}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = $"Successfully purchased {quantity}x {listing.ItemName}",
                Transaction = new
                {
                    transaction.Id,
                    transaction.ItemName,
                    transaction.Quantity,
                    transaction.Price,
                    transaction.CurrencyType,
                    transaction.TransactionAtUtc,
                    SellerFee = "15%"
                }
            }, _jsonOptions);
        }
    }

    private string GetSellerInfo(Guid sellerId)
    {
        lock (_lock)
        {
            if (!_sellers.TryGetValue(sellerId, out var seller))
            {
                var task = GetOrCreateSellerInfoAsync(sellerId);
                task.Wait();
                seller = task.Result;
            }

            var listings = _marketplaceListings.Values
                .Where(l => l.SellerId == sellerId && l.IsAvailable)
                .Select(l => new
                {
                    l.Id,
                    l.ItemName,
                    l.CurrencyType,
                    l.Price,
                    l.Quantity,
                    l.Category
                })
                .ToList();

            var reviews = _sellerReviews.GetValueOrDefault(sellerId) ?? new List<SellerReview>();

            return JsonSerializer.Serialize(new
            {
                Seller = new
                {
                    seller.UserId,
                    seller.Username,
                    seller.DisplayName,
                    seller.Bio,
                    seller.AvatarUrl,
                    seller.Rating,
                    seller.TotalSales,
                    seller.ActiveListings,
                    seller.MemberSince
                },
                ActiveListings = listings,
                RecentReviews = reviews.OrderByDescending(r => r.CreatedAtUtc).Take(10),
                ReviewCount = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0
            }, _jsonOptions);
        }
    }

    private string AddReview(Guid reviewerId, Guid sellerId, Guid purchaseId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "Rating must be between 1 and 5"
            }, _jsonOptions);
        }

        lock (_lock)
        {
            // Verify the purchase exists
            var transaction = _marketplaceTransactions.Values
                .FirstOrDefault(t => t.Id == purchaseId && t.BuyerId == reviewerId && t.SellerId == sellerId);

            if (transaction == null)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Purchase not found or you are not authorized to review this seller"
                }, _jsonOptions);
            }

            // Get reviewer name
            string reviewerName = $"User-{reviewerId.ToString()[..8]}";
            if (_profileModule != null)
            {
                var profileTask = _profileModule.GetProfileAsync(reviewerId.ToString());
                profileTask.Wait();
                var profile = profileTask.Result;
                if (profile != null)
                {
                    reviewerName = profile.DisplayName ?? profile.UserId;
                }
            }

            var review = new SellerReview
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewerName = reviewerName,
                SellerId = sellerId,
                PurchaseId = purchaseId,
                Rating = rating,
                Comment = comment,
                CreatedAtUtc = DateTime.UtcNow
            };

            if (!_sellerReviews.ContainsKey(sellerId))
            {
                _sellerReviews[sellerId] = new List<SellerReview>();
            }

            _sellerReviews[sellerId].Add(review);

            // Update seller rating
            if (_sellers.TryGetValue(sellerId, out var seller))
            {
                seller.Reviews = _sellerReviews[sellerId];
                seller.Rating = (decimal)seller.Reviews.Average(r => r.Rating);
            }

            LogInfo($"New review for seller {sellerId}: {rating} stars from {reviewerId}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Review submitted successfully",
                Review = new
                {
                    review.Id,
                    review.Rating,
                    review.Comment,
                    review.CreatedAtUtc,
                    NewSellerRating = _sellers.GetValueOrDefault(sellerId)?.Rating ?? 0
                }
            }, _jsonOptions);
        }
    }

    private string GetMarketStats()
    {
        lock (_lock)
        {
            var stats = new
            {
                OfficialStore = new
                {
                    TotalProducts = _officialProducts.Count,
                    AvailableProducts = _officialProducts.Values.Count(p => p.IsAvailable),
                    Currency = "RaCoin"
                },
                Marketplace = new
                {
                    TotalListings = _marketplaceListings.Count,
                    ActiveListings = _marketplaceListings.Values.Count(l => l.IsAvailable),
                    RaCoinListings = _marketplaceListings.Values.Count(l => l.IsAvailable && l.CurrencyType == CurrencyType.RaCoin),
                    GoldListings = _marketplaceListings.Values.Count(l => l.IsAvailable && l.CurrencyType == CurrencyType.Gold),
                    TotalTransactions = _marketplaceTransactions.Count,
                    TotalSellers = _sellers.Count,
                    TopSellers = _sellers.Values
                        .OrderByDescending(s => s.TotalSales)
                        .Take(5)
                        .Select(s => new { s.Username, s.TotalSales, s.Rating })
                },
                OfficialPurchases = new
                {
                    TotalPurchases = _purchases.Count,
                    TotalRevenue = _purchases.Values.Sum(p => p.PricePaid)
                },
                TopCategories = _marketplaceListings.Values
                    .GroupBy(l => l.Category)
                    .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
            };

            return JsonSerializer.Serialize(stats, _jsonOptions);
        }
    }

    private string GetPurchaseHistory(Guid userId)
    {
        lock (_lock)
        {
            var officialPurchases = _purchases.Values
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PurchasedAtUtc)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    PricePaid = p.PricePaid,
                    Currency = "RaCoin",
                    p.PurchasedAtUtc,
                    p.Status,
                    Source = "Official Store",
                    LicenseKey = p.Metadata.GetValueOrDefault("LicenseKey")
                });

            var marketplacePurchases = _marketplaceTransactions.Values
                .Where(t => t.BuyerId == userId)
                .OrderByDescending(t => t.TransactionAtUtc)
                .Select(t => new
                {
                    t.Id,
                    ProductName = t.ItemName,
                    PricePaid = t.Price,
                    Currency = t.CurrencyType.ToString(),
                    PurchasedAtUtc = t.TransactionAtUtc,
                    Status = t.Status.ToString(),
                    Source = "User Marketplace",
                    Quantity = t.Quantity
                });

            var marketplaceSales = _marketplaceTransactions.Values
                .Where(t => t.SellerId == userId)
                .OrderByDescending(t => t.TransactionAtUtc)
                .Select(t => new
                {
                    t.Id,
                    ItemName = t.ItemName,
                    SaleAmount = t.Price * 0.85m, // After 15% fee
                    Currency = t.CurrencyType.ToString(),
                    SoldAtUtc = t.TransactionAtUtc,
                    Quantity = t.Quantity,
                    MarketplaceFee = "15%"
                });

            return JsonSerializer.Serialize(new
            {
                UserId = userId,
                OfficialPurchases = new
                {
                    Count = officialPurchases.Count(),
                    TotalSpent = officialPurchases.Sum(p => p.PricePaid),
                    Purchases = officialPurchases.ToList()
                },
                MarketplacePurchases = new
                {
                    Count = marketplacePurchases.Count(),
                    Purchases = marketplacePurchases.ToList()
                },
                MarketplaceSales = new
                {
                    Count = marketplaceSales.Count(),
                    TotalEarned = marketplaceSales.Sum(s => s.SaleAmount),
                    Sales = marketplaceSales.ToList()
                }
            }, _jsonOptions);
        }
    }

    /// <summary>
    /// Get official product by ID - for external module access.
    /// </summary>
    public SuperMarketProduct? GetOfficialProduct(Guid productId)
    {
        lock (_lock)
        {
            return _officialProducts.GetValueOrDefault(productId);
        }
    }

    /// <summary>
    /// Get all available official products - for external module access.
    /// </summary>
    public List<SuperMarketProduct> GetAvailableOfficialProducts()
    {
        lock (_lock)
        {
            return _officialProducts.Values.Where(p => p.IsAvailable).ToList();
        }
    }

    /// <summary>
    /// Get marketplace listing by ID - for external module access.
    /// </summary>
    public MarketplaceListing? GetListing(Guid listingId)
    {
        lock (_lock)
        {
            return _marketplaceListings.GetValueOrDefault(listingId);
        }
    }

    /// <summary>
    /// Get all available marketplace listings - for external module access.
    /// </summary>
    public List<MarketplaceListing> GetAvailableListings()
    {
        lock (_lock)
        {
            return _marketplaceListings.Values.Where(l => l.IsAvailable).ToList();
        }
    }
}
