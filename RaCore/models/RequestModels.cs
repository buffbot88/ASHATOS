using Abstractions;

namespace RaCore.Models;

// Game Engine Request Models
public record CreateSceneRequest(string Name);

// Server Setup Request Models
public record CreateAdminInstanceRequest(string LicenseNumber, string Username);
public record SetupConfigRequest(string LicenseNumber, string Username);

// Game Engine Update/Export Request Models
public record GameUpdateRequest(string Description);
public record GameExportRequest(string GameId, string Format);

// Server Mode Request Models
public record ServerModeChangeRequest(string Mode);
public record UnderConstructionRequest(bool Enabled, string? Message);

// Forum Request Models
public record ForumPostActionRequest(string PostId, string Action);
public record ForumLockRequest(string ThreadId, bool Locked, string? Reason);
public record ForumWarningRequest(string UserId, string Reason);
public record ForumBanRequest(string UserId, string Reason, int DurationDays);

// Blog Request Models
public record CreateBlogPostRequest(string Title, string Content, string[]? Tags);
public record AddCommentRequest(string PostId, string Content);

// Chat Request Models
public record CreateChatRoomRequest(string Name, bool IsPrivate);
public record SendMessageRequest(string Content);

// Social Request Models
public record AddFriendRequest(string FriendId);
public record CreateSocialPostRequest(string Content, string[]? Tags, string? MediaUrl);

// Market Request Models
public record MarketListingRequest(string SellerId, string ItemName, string CurrencyType, decimal Price, int Quantity, string Category, string Description);
public record MarketPurchaseRequest(bool IsMarketplaceListing, string BuyerId, string? ListingId, string? ProductId, int Quantity);
public record MarketReviewRequest(string ReviewerId, string SellerId, string PurchaseId, int Rating, string Comment);

// Distribution Request Models
public record CreateDistributionRequest(string LicenseKey, string Version);

// Client Builder Request Models
public record GenerateClientRequest(string LicenseKey, ClientPlatform Platform, ClientConfiguration? Configuration);
public record GenerateClientWithTemplateRequest(string? LicenseKey, ClientPlatform Platform, string? TemplateName, ClientConfiguration? Configuration);