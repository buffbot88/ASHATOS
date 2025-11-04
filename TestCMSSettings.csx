#!/usr/bin/env dotnet script

#r "/home/runner/work/ASHATOS/ASHATOS/LegendaryCMS/bin/Release/net9.0/LegendaryCMS.dll"

using System;
using System.IO;
using System.Threading.Tasks;
using LegendaryCMS.Configuration;

Console.WriteLine("=== CMS Settings Persistence Tests ===\n");

int passed = 0;
int failed = 0;

// Test 1: Save and Load Settings
Console.WriteLine("Test 1: Save and Load Settings");
try
{
    var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-config-{Guid.NewGuid()}.json");
    
    var config = new CMSConfiguration(testFile);
    config.SetValue("Site:Name", "Test CMS Site");
    config.SetValue("Site:BaseUrl", "https://test.example.com");
    config.SetValue("Security:MaxLoginAttempts", 10);
    config.SetValue("API:Enabled", false);
    
    await config.SaveAsync();
    
    if (!File.Exists(testFile))
    {
        Console.WriteLine("✗ FAILED: Configuration file was not created");
        failed++;
    }
    else
    {
        var config2 = new CMSConfiguration(testFile);
        await config2.ReloadAsync();
        
        var siteName = config2.GetValue<string>("Site:Name");
        var baseUrl = config2.GetValue<string>("Site:BaseUrl");
        var maxAttempts = config2.GetValue<int>("Security:MaxLoginAttempts", 0);
        var apiEnabled = config2.GetValue<bool>("API:Enabled", true);
        
        File.Delete(testFile);
        
        if (siteName == "Test CMS Site" && baseUrl == "https://test.example.com" 
            && maxAttempts == 10 && apiEnabled == false)
        {
            Console.WriteLine("✓ PASSED: Settings saved and loaded correctly");
            passed++;
        }
        else
        {
            Console.WriteLine($"✗ FAILED: Values don't match. Got: Name={siteName}, URL={baseUrl}, Attempts={maxAttempts}, API={apiEnabled}");
            failed++;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ FAILED: Exception - {ex.Message}");
    failed++;
}

// Test 2: Settings Persist Across Instances
Console.WriteLine("\nTest 2: Settings Persist Across Instances");
try
{
    var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-persist-{Guid.NewGuid()}.json");
    
    var config1 = new CMSConfiguration(testFile);
    config1.SetValue("Theme:Default", "dark-mode");
    config1.SetValue("Localization:DefaultLocale", "fr-FR");
    await config1.SaveAsync();
    
    var config2 = new CMSConfiguration(testFile);
    await config2.ReloadAsync();
    
    var theme = config2.GetValue<string>("Theme:Default");
    var locale = config2.GetValue<string>("Localization:DefaultLocale");
    
    File.Delete(testFile);
    
    if (theme == "dark-mode" && locale == "fr-FR")
    {
        Console.WriteLine("✓ PASSED: Settings persist across configuration instances");
        passed++;
    }
    else
    {
        Console.WriteLine($"✗ FAILED: Got theme={theme}, locale={locale}");
        failed++;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ FAILED: Exception - {ex.Message}");
    failed++;
}

// Test 3: Hierarchical Settings Keys
Console.WriteLine("\nTest 3: Hierarchical Settings Keys");
try
{
    var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-hierarchy-{Guid.NewGuid()}.json");
    
    var config = new CMSConfiguration(testFile);
    config.SetValue("Database:Type", "PostgreSQL");
    config.SetValue("Database:ConnectionString", "Host=localhost;Database=testdb");
    config.SetValue("Database:AutoMigrate", false);
    
    var dbSection = config.GetSection("Database");
    
    await config.SaveAsync();
    
    var config2 = new CMSConfiguration(testFile);
    await config2.ReloadAsync();
    
    var dbSection2 = config2.GetSection("Database");
    
    File.Delete(testFile);
    
    if (dbSection2.Count >= 3 && dbSection2["Type"]?.ToString() == "PostgreSQL")
    {
        Console.WriteLine("✓ PASSED: Hierarchical settings keys work correctly");
        passed++;
    }
    else
    {
        Console.WriteLine($"✗ FAILED: Section has {dbSection2.Count} items, Type={dbSection2.GetValueOrDefault("Type", "N/A")}");
        failed++;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ FAILED: Exception - {ex.Message}");
    failed++;
}

// Test 4: Settings Reload
Console.WriteLine("\nTest 4: Settings Reload");
try
{
    var testFile = Path.Combine(Path.GetTempPath(), $"test-cms-reload-{Guid.NewGuid()}.json");
    
    var config = new CMSConfiguration(testFile);
    config.SetValue("Performance:CacheDuration", 600);
    await config.SaveAsync();
    
    config.SetValue("Performance:CacheDuration", 1200);
    
    await config.ReloadAsync();
    
    var cacheDuration = config.GetValue<int>("Performance:CacheDuration", 0);
    
    File.Delete(testFile);
    
    if (cacheDuration == 600)
    {
        Console.WriteLine("✓ PASSED: Settings can be reloaded from file");
        passed++;
    }
    else
    {
        Console.WriteLine($"✗ FAILED: Expected 600, got {cacheDuration}");
        failed++;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ FAILED: Exception - {ex.Message}");
    failed++;
}

Console.WriteLine($"\n=== Test Results: {passed} passed, {failed} failed ===");
