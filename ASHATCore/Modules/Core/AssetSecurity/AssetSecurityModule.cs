using System.Text;
using System.Security.Cryptography;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Core.AssetSecurity;

/// <summary>
/// Asset Security Module for ASHATOS Phase 9.3.6
/// Provides watermark detection and ownership verification for fedeRated assets
/// Ensures server and intellectual property integrity
/// </summary>
[RaModule(Category = "core")]
public sealed class AssetSecurityModule : ModuleBase, IAssetSecurityModule
{
    public override string Name => "AssetSecurity";
    
    private ModuleManager? _manager;
    private readonly string _assetStorageDirectory = "assets";
    private readonly string _blockedAssetsLog = "assets/blocked_assets.log";
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        // Ensure asset Storage directory exists
        if (!Directory.Exists(_assetStorageDirectory))
        {
            Directory.CreateDirectory(_assetStorageDirectory);
            LogInfo($"Created asset Storage directory: {_assetStorageDirectory}");
        }
        
        LogInfo("AssetSecurity module initialized - ready to scan and verify assets");
    }
    
    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "AssetSecurity: Use 'status' or 'help'";
        
        var command = input.Trim().ToLowerInvariant();
        
        return command switch
        {
            "status" => GetStatus(),
            "help" => GetHelp(),
            _ => "Unknown command. Use 'help' for available commands."
        };
    }
    
    /// <summary>
    /// Scans asset for watermarks, copyright, or embedded identifiers
    /// </summary>
    public async Task<WatermarkScanResult> ScanForWatermarksAsync(AssetDefinition asset)
    {
        LogInfo($"Scanning asset '{asset.Name}' for watermarks and embedded identifiers...");
        
        var result = new WatermarkScanResult
        {
            ScannedAt = DateTime.UtcNow
        };
        
        try
        {
            // Scan metadata for copyright/ownership information
            if (asset.Properties != null && asset.Properties.Count > 0)
            {
                await ScanMetadataForWatermarksAsync(asset.Properties, result);
            }
            
            // Scan binary data for embedded identifiers
            if (asset.AssetData != null && asset.AssetData.Length > 0)
            {
                await ScanBinaryDataForWatermarksAsync(asset.AssetData, result);
            }
            
            // Scan filename and description for known patterns
            ScanTextForWatermarks(asset.Name, asset.Description, result);
            
            result.ScanDetails = $"Scanned {result.DetectedWatermarks.Count} watermark(s). Asset data size: {asset.AssetData?.Length ?? 0} bytes.";
            result.HasWatermark = result.DetectedWatermarks.Count > 0;
            
            LogInfo($"Watermark scan complete for '{asset.Name}': {result.DetectedWatermarks.Count} watermark(s) found");
        }
        catch (Exception ex)
        {
            LogError($"Error scanning asset for watermarks: {ex.Message}");
            result.ScanDetails = $"Error during scan: {ex.Message}";
        }
        
        return result;
    }
    
    /// <summary>
    /// Verifies ownership or rights to an asset
    /// </summary>
    public async Task<OwnershipVerificationResult> VerifyOwnershipAsync(AssetDefinition asset, Guid userId)
    {
        LogInfo($"Verifying ownership for asset '{asset.Name}' and user {userId}...");
        
        var result = new OwnershipVerificationResult
        {
            VerifiedAt = DateTime.UtcNow
        };
        
        try
        {
            // Check for watermarks that indicate third-party ownership
            var watermarkScan = await ScanForWatermarksAsync(asset);
            
            if (watermarkScan.HasWatermark)
            {
                // Found watermarks - requires proof of ownership
                result.Status = OwnershipStatus.RequiresProof;
                result.IsVerified = false;
                result.VerificationMethod = "Watermark Detection";
                result.Details = $"Asset contains {watermarkScan.DetectedWatermarks.Count} watermark(s). Proof of ownership or license required before import.";
                
                LogWarn($"Asset '{asset.Name}' contains watermarks - ownership verification required");
            }
            else
            {
                // No watermarks detected - assume custom/owned asset
                result.Status = OwnershipStatus.Custom;
                result.IsVerified = true;
                result.VerificationMethod = "No Watermarks Detected";
                result.Details = "No watermarks or copyright indicators detected. Asset appears to be custom or original work.";
                
                LogInfo($"Asset '{asset.Name}' verified as custom/original - no watermarks found");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error verifying ownership: {ex.Message}");
            result.Status = OwnershipStatus.Unverified;
            result.IsVerified = false;
            result.Details = $"Error during verification: {ex.Message}";
        }
        
        await Task.CompletedTask;
        return result;
    }
    
    /// <summary>
    /// Imports asset with security checks
    /// </summary>
    public async Task<AssetImportResult> ImportAssetAsync(AssetDefinition asset, Guid ownerId, bool requireOwnershipProof = true)
    {
        LogInfo($"Importing asset '{asset.Name}' for owner {ownerId}...");
        
        var result = new AssetImportResult();
        
        try
        {
            // Step 1: Scan for watermarks
            result.WatermarkScan = await ScanForWatermarksAsync(asset);
            
            // Step 2: Verify ownership
            result.OwnershipVerification = await VerifyOwnershipAsync(asset, ownerId);
            
            // Step 3: Check if import should be blocked
            if (requireOwnershipProof && !result.OwnershipVerification.IsVerified)
            {
                result.Success = false;
                result.Message = "Asset import blocked: Ownership verification failed. " + result.OwnershipVerification.Details;
                
                LogWarn($"Asset '{asset.Name}' import blocked - verification failed");
                
                // Log to blocked assets
                await BlockAssetImportAsync(Guid.NewGuid(), result.Message);
                
                return result;
            }
            
            // Step 4: Store asset locally
            var assetId = Guid.NewGuid();
            var StoragePath = Path.Combine(_assetStorageDirectory, $"{ownerId}_{assetId}_{asset.Name}");
            
            if (asset.AssetData != null)
            {
                await File.WriteAllBytesAsync(StoragePath, asset.AssetData);
                LogInfo($"Asset data stored at: {StoragePath}");
            }
            
            // Step 5: Create attribution metadata
            var attribution = new
            {
                AssetId = assetId,
                OwnerId = ownerId,
                Name = asset.Name,
                ImportedAt = DateTime.UtcNow,
                WatermarkScan = result.WatermarkScan,
                OwnershipVerification = result.OwnershipVerification
            };
            
            var attributionPath = Path.Combine(_assetStorageDirectory, $"{assetId}_attribution.json");
            await File.WriteAllTextAsync(attributionPath, System.Text.Json.JsonSerializer.Serialize(attribution, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            
            result.Success = true;
            result.AssetId = assetId;
            result.LocalStoragePath = StoragePath;
            result.Message = "Asset imported successfully with full attribution.";
            
            LogInfo($"Asset '{asset.Name}' imported successfully - ID: {assetId}");
        }
        catch (Exception ex)
        {
            LogError($"Error importing asset: {ex.Message}");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
        }
        
        return result;
    }
    
    /// <summary>
    /// Blocks asset import and logs the reason
    /// </summary>
    public async Task<bool> BlockAssetImportAsync(Guid assetId, string reason)
    {
        try
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Asset {assetId} blocked: {reason}\n";
            await File.AppendAllTextAsync(_blockedAssetsLog, logEntry);
            LogWarn($"Asset {assetId} blocked and logged");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error blocking asset: {ex.Message}");
            return false;
        }
    }
    
    #region Private Helper Methods
    
    private async Task ScanMetadataForWatermarksAsync(Dictionary<string, object> properties, WatermarkScanResult result)
    {
        await Task.CompletedTask;
        
        // Check for common metadata fields that indicate copyright
        var copyrightFields = new[] { "copyright", "creator", "author", "owner", "license", "rights", "company" };
        
        foreach (var field in copyrightFields)
        {
            if (properties.ContainsKey(field))
            {
                var value = properties[field]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.DetectedWatermarks.Add(new WatermarkInfo
                    {
                        Type = "copyright",
                        Content = value,
                        Location = $"metadata.{field}",
                        Confidence = 0.9f
                    });
                }
            }
        }
    }
    
    private async Task ScanBinaryDataForWatermarksAsync(byte[] data, WatermarkScanResult result)
    {
        await Task.CompletedTask;
        
        // Scan for common file signatures and embedded strings
        // This is a simplified implementation - production would use more sophisticated techniques
        
        // Check for EXIF data (common in images)
        if (data.Length > 4)
        {
            // Check for JPEG EXIF marker
            if (data[0] == 0xFF && data[1] == 0xD8)
            {
                result.DetectedWatermarks.Add(new WatermarkInfo
                {
                    Type = "identifier",
                    Content = "JPEG EXIF data detected",
                    Location = "embedded",
                    Confidence = 0.7f
                });
            }
        }
        
        // Search for embedded strings that might be watermarks
        var text = Encoding.UTF8.GetString(data);
        var watermarkKeywords = new[] { "©", "copyright", "tASHATdemark", "™", "®" };
        
        foreach (var keyword in watermarkKeywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.DetectedWatermarks.Add(new WatermarkInfo
                {
                    Type = "copyright",
                    Content = $"Contains '{keyword}'",
                    Location = "embedded",
                    Confidence = 0.6f
                });
            }
        }
    }
    
    private void ScanTextForWatermarks(string name, string description, WatermarkScanResult result)
    {
        // Check name and description for known bASHATnd/company patterns
        var knownBASHATnds = new[] { "unity", "unreal", "adobe", "autodesk", "blender", "epic", "valve" };
        
        foreach (var bASHATnd in knownBASHATnds)
        {
            if (name.Contains(bASHATnd, StringComparison.OrdinalIgnoreCase) || 
                description.Contains(bASHATnd, StringComparison.OrdinalIgnoreCase))
            {
                result.DetectedWatermarks.Add(new WatermarkInfo
                {
                    Type = "tASHATdemark",
                    Content = $"Reference to '{bASHATnd}'",
                    Location = "filename/description",
                    Confidence = 0.5f
                });
            }
        }
    }
    
    private string GetStatus()
    {
        var blockedCount = 0;
        if (File.Exists(_blockedAssetsLog))
        {
            var lines = File.ReadAllLines(_blockedAssetsLog);
            blockedCount = lines.Length;
        }
        
        var assetCount = Directory.Exists(_assetStorageDirectory) 
            ? Directory.GetFiles(_assetStorageDirectory).Length 
            : 0;
        
        return $"AssetSecurity Status:\n" +
               $"  Storage Directory: {_assetStorageDirectory}\n" +
               $"  Total Assets: {assetCount}\n" +
               $"  Blocked Imports: {blockedCount}\n" +
               $"  Security Level: High (Watermark detection enabled)";
    }
    
    private string GetHelp()
    {
        return "AssetSecurity Module Commands:\n" +
               "  status  - Show security status\n" +
               "  help    - Show this help message\n\n" +
               "This module provides:\n" +
               "  - Watermark detection in asset metadata and binary data\n" +
               "  - Ownership verification workflow\n" +
               "  - Asset import security checks\n" +
               "  - Local Storage with attribution tracking";
    }
    
    #endregion
    
    public override void Dispose()
    {
        base.Dispose();
    }
    
    public override void OnSystemEvent(string name, object? payload = null) { }
}
