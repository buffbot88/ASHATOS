using System;
using System.IO;
using System.Threading.Tasks;
using LegendaryCMS.Configuration;

namespace ASHATCore.Tests;

/// <summary>
/// Tests to verify CMS settings persistence functionality
/// </summary>
public class CMSSettingsPersistenceTests
{
    public static async Task RunAllTests()
    {
        Console.WriteLine("=== CMS Settings Persistence Tests ===\n");
        
        int passed = 0;
        int failed = 0;
        
        // Test 1: Save and Load Settings
        if (await TestSaveAndLoadSettings())
        {
            Console.WriteLine("✓ Test 1 PASSED: Settings can be saved and loaded");
            passed++;
        }
        else
        {
            Console.WriteLine("✗ Test 1 FAILED: Settings persistence failed");
            failed++;
        }
        
        // Test 2: Settings Persist Across Instances
        if (await TestSettingsPersistAcrossInstances())
        {
            Console.WriteLine("✓ Test 2 PASSED: Settings persist across configuration instances");
            passed++;
        }
        else
        {
            Console.WriteLine("✗ Test 2 FAILED: Settings do not persist across instances");
            failed++;
        }
        
        // Test 3: Hierarchical Settings Keys
        if (await TestHierarchicalSettingsKeys())
        {
            Console.WriteLine("✓ Test 3 PASSED: Hierarchical settings keys work correctly");
            passed++;
        }
        else
        {
            Console.WriteLine("✗ Test 3 FAILED: Hierarchical settings keys not working");
            failed++;
        }
        
        // Test 4: Settings Reload
        if (await TestSettingsReload())
        {
            Console.WriteLine("✓ Test 4 PASSED: Settings can be reloaded from file");
            passed++;
        }
        else
        {
            Console.WriteLine("✗ Test 4 FAILED: Settings reload not working");
            failed++;
        }
        
        Console.WriteLine($"\n=== Test Results: {passed} passed, {failed} failed ===");
    }
    
    private static async Task<bool> TestSaveAndLoadSettings()
    {
        try
        {
            var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-config-{Guid.NewGuid()}.json");
            
            // Create configuration and set custom values
            var config = new CMSConfiguration(testFile);
            config.SetValue("Site:Name", "Test CMS Site");
            config.SetValue("Site:BaseUrl", "https://test.example.com");
            config.SetValue("Security:MaxLoginAttempts", 10);
            config.SetValue("API:Enabled", false);
            
            // Save to file
            await config.SaveAsync();
            
            // Verify file exists
            if (!File.Exists(testFile))
            {
                Console.WriteLine("  Error: Configuration file was not created");
                return false;
            }
            
            // Read file and verify JSON content
            var json = await File.ReadAllTextAsync(testFile);
            if (!json.Contains("Test CMS Site") || !json.Contains("test.example.com"))
            {
                Console.WriteLine("  Error: Configuration file does not contain expected values");
                return false;
            }
            
            // Create new instance and reload
            var config2 = new CMSConfiguration(testFile);
            await config2.ReloadAsync();
            
            // Verify values were loaded
            var siteName = config2.GetValue<string>("Site:Name");
            var baseUrl = config2.GetValue<string>("Site:BaseUrl");
            var maxAttempts = config2.GetValue<int>("Security:MaxLoginAttempts", 0);
            var apiEnabled = config2.GetValue<bool>("API:Enabled", true);
            
            // Clean up
            File.Delete(testFile);
            
            return siteName == "Test CMS Site" 
                && baseUrl == "https://test.example.com"
                && maxAttempts == 10
                && apiEnabled == false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.Message}");
            return false;
        }
    }
    
    private static async Task<bool> TestSettingsPersistAcrossInstances()
    {
        try
        {
            var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-persist-{Guid.NewGuid()}.json");
            
            // First instance - set and save
            var config1 = new CMSConfiguration(testFile);
            config1.SetValue("Theme:Default", "dark-mode");
            config1.SetValue("Localization:DefaultLocale", "fr-FR");
            await config1.SaveAsync();
            
            // Second instance - should load from file
            var config2 = new CMSConfiguration(testFile);
            await config2.ReloadAsync();
            
            var theme = config2.GetValue<string>("Theme:Default");
            var locale = config2.GetValue<string>("Localization:DefaultLocale");
            
            // Clean up
            File.Delete(testFile);
            
            return theme == "dark-mode" && locale == "fr-FR";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.Message}");
            return false;
        }
    }
    
    private static async Task<bool> TestHierarchicalSettingsKeys()
    {
        try
        {
            var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-hierarchy-{Guid.NewGuid()}.json");
            
            var config = new CMSConfiguration(testFile);
            config.SetValue("Database:Type", "PostgreSQL");
            config.SetValue("Database:ConnectionString", "Host=localhost;Database=testdb");
            config.SetValue("Database:AutoMigRate", false);
            
            // Get section
            var dbSection = config.GetSection("Database");
            
            bool hasCorrectValues = dbSection.Count >= 3
                && dbSection["Type"]?.ToString() == "PostgreSQL"
                && dbSection["ConnectionString"]?.ToString() == "Host=localhost;Database=testdb";
            
            // Save and reload
            await config.SaveAsync();
            
            var config2 = new CMSConfiguration(testFile);
            await config2.ReloadAsync();
            
            var dbSection2 = config2.GetSection("Database");
            bool persistedCorrectly = dbSection2.Count >= 3
                && dbSection2["Type"]?.ToString() == "PostgreSQL";
            
            // Clean up
            File.Delete(testFile);
            
            return hasCorrectValues && persistedCorrectly;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.Message}");
            return false;
        }
    }
    
    private static async Task<bool> TestSettingsReload()
    {
        try
        {
            var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-reload-{Guid.NewGuid()}.json");
            
            // Create and save initial config
            var config = new CMSConfiguration(testFile);
            config.SetValue("Performance:EnableCaching", true);
            config.SetValue("Performance:CacheDuration", 600);
            await config.SaveAsync();
            
            // Modify the setting in memory
            config.SetValue("Performance:CacheDuration", 1200);
            
            // Reload from file - should reset to saved value
            await config.ReloadAsync();
            
            var cacheDuration = config.GetValue<int>("Performance:CacheDuration", 0);
            
            // Clean up
            File.Delete(testFile);
            
            // Should be 600 (from file), not 1200 (from memory change)
            return cacheDuration == 600;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.Message}");
            return false;
        }
    }
}
