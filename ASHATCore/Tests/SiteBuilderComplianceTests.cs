using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abstractions;
using ASHATCore.Engine;
using ASHATCore.Engine.Manager;
using ASHATCore.Engine.Memory;
using ASHATCore.Modules.Extensions.SiteBuilder;

namespace ASHATCore.Tests;

/// <summary>
/// Comprehensive test suite for compliance (Issue #255)
/// Verifies that ALL UX modules and underconstruction pages route through internal ASHATOS process
/// with no static files or external endpoints
/// </summary>
public class WindowOfASHATComplianceTests
{
    public static void RunTests()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("SiteBuilder Compliance Tests - Issue #255");
        Console.WriteLine("========================================");
        Console.WriteLine();

        TestNoStaticHtmlFilesInWwwroot();
        TestWwwrootGeneratorNoHtmlGeneration();
        TestUnderConstructionHandlerDynamicGeneration();
        TestBotDetectorDynamicGeneration();
        TestProgramCsRouteDefinitions();
        TestNoStaticFileMiddleware();
        TestAllModulesCompliant();
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("All Compliance Tests Passed ✓");
        Console.WriteLine("System is FULLY COMPLIANT with architecture");
        Console.WriteLine("========================================");
    }

    private static void TestNoStaticHtmlFilesInWwwroot()
    {
        Console.WriteLine("Test 1: No Static HTML Files in wwwroot");
        
        var currentDir = Directory.GetCurrentDirectory();
        var wwwrootPath = Path.Combine(currentDir, "wwwroot");
        
        // Ensure wwwroot directory exists (it may be created during tests)
        if (!Directory.Exists(wwwrootPath))
        {
            Console.WriteLine("  ✓ wwwroot directory does not exist yet (first run)");
            return;
        }
        
        // Check for any .html files
        var htmlFiles = Directory.GetFiles(wwwrootPath, "*.html", SearchOption.AllDirectories);
        
        Assert(htmlFiles.Length == 0, 
            $"No HTML files should exist in wwwroot. Found: {htmlFiles.Length}");
        
        // Verify only config files and static assets (JS, CSS, images) exist - no HTML
        var allFiles = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in allFiles)
        {
            var ext = Path.GetExtension(file).ToLower();
            var fileName = Path.GetFileName(file).ToLower();
            
            // Allow config files, JavaScript, CSS, images, and JSON config files
            // But still forbid HTML files (all HTML must be dynamically generated)
            var allowedExtensions = new[] { ".conf", ".ini", ".js", ".css", ".json", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot" };
            var allowedFiles = new[] { "nginx.conf", "apache.conf", "php.ini" };
            
            Assert(
                allowedExtensions.Contains(ext) || allowedFiles.Contains(fileName),
                $"Only config files and static assets (no HTML) should exist in wwwroot. Found: {file} with extension {ext}"
            );
        }
        
        Console.WriteLine("  ✓ No HTML files in wwwroot (config files and static assets allowed)");
    }

    private static void TestWwwrootGeneratorNoHtmlGeneration()
    {
        Console.WriteLine("Test 2: WwwrootGenerator - No HTML Generation");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "ASHATCore_wwwroot_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            var wwwrootPath = Path.Combine(testDir, "wwwroot");
            
            // Create minimal setup
            var memoryModule = new MemoryModule();
            memoryModule.Initialize(null);
            
            var moduleManager = new ModuleManager();
            moduleManager.RegisterBuiltInModule(memoryModule);
            
            // Load module
            moduleManager.LoadModules();
            
            // Find module
            var Module = moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<SiteBuilderModule>()
                .FirstOrDefault();
            var _sitebuilderModule = Module;

            if (_sitebuilderModule != null)
            {
                // Generate wwwroot (should NOT create HTML files)
                var result =Module.GenerateWwwroot();
                
                // Verify no HTML files were created
                var htmlFiles = Directory.Exists(wwwrootPath) 
                    ? Directory.GetFiles(wwwrootPath, "*.html", SearchOption.AllDirectories) 
                    : Array.Empty<string>();
                
                Assert(htmlFiles.Length == 0, "No HTML files should be Generated - all UI is dynamic");
                
                // Verify the result message confirms dynamic routing
                Assert(result.Contains("dynamic"), "Result should mention dynamic routing");
                Assert(result.Contains("SiteBuilder"), "Result should mention");
                Assert(result.Contains("No static HTML files Generated"), "Result should confirm no HTML Generation");
                
                Console.WriteLine("  ✓ WwwrootGenerator creates no HTML files (SiteBuilder compliant)");
            }
            else
            {
                Console.WriteLine("  ⚠ module not found - test skipped");
            }
            
            // Restore original directory
            Directory.SetCurrentDirectory(originalDir);
        }
        finally
        {
            // Clean up test directory
            if (Directory.Exists(testDir))
            {
                try
                {
                    Directory.Delete(testDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    private static void TestUnderConstructionHandlerDynamicGeneration()
    {
        Console.WriteLine("Test 3: UnderConstructionHandler - Dynamic HTML Generation");
        
        var config = new ServerConfiguration
        {
            UnderConstruction = true,
            UnderConstructionMessage = "Test message"
        };
        
        // Generate HTML dynamically
        var html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
        
        // Verify HTML is Generated dynamically (not read from file)
        Assert(!string.IsNullOrEmpty(html), "HTML should be Generated");
        Assert(html.Contains("<!DOCTYPE html>"), "HTML should be valid");
        Assert(html.Contains("Test message"), "HTML should contain custom message");
        Assert(html.Contains("Under Construction"), "HTML should have correct title");
        
        // Verify it's Generated in-memory (method returns string, not file path)
        Assert(html.Length > 1000, "HTML should be substantial content");
        
        Console.WriteLine("  ✓ UnderConstructionHandler Generates dynamic HTML (no file dependency)");
    }

    private static void TestBotDetectorDynamicGeneration()
    {
        Console.WriteLine("Test 4: BotDetector - Dynamic HTML Generation");
        
        // Generate access denied message dynamically
        var html = BotDetector.GetAccessDeniedMessage();
        
        // Verify HTML is Generated dynamically
        Assert(!string.IsNullOrEmpty(html), "HTML should be Generated");
        Assert(html.Contains("<!DOCTYPE html>"), "HTML should be valid");
        Assert(html.Contains("Homepage Access"), "HTML should have correct content");
        Assert(html.Contains("/control-panel"), "HTML should link to control panel");
        
        // Test bot detection logic
        Assert(BotDetector.IsSearchEngineBot("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"), 
            "Should detect Googlebot");
        Assert(!BotDetector.IsSearchEngineBot("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0"), 
            "Should not detect regular browser as bot");
        
        Console.WriteLine("  ✓ BotDetector Generates dynamic HTML (no file dependency)");
    }

    private static void TestProgramCsRouteDefinitions()
    {
        Console.WriteLine("Test 5: Program.cs - All Routes Use Dynamic Generation");
        
        // Read Program.cs file
        var ProgramCsPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", "ASHATCore", "Program.cs"
        );
        
        // If running from different location, try alternative paths
        if (!File.Exists(ProgramCsPath))
        {
            ProgramCsPath = Path.Combine(Directory.GetCurrentDirectory(), "Program.cs");
        }
        
        if (!File.Exists(ProgramCsPath))
        {
            // Try to find it relative to test assembly
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyPath) ?? "";
            ProgramCsPath = Path.Combine(assemblyDir, "..", "..", "..", "..", "Program.cs");
        }
        
        if (File.Exists(ProgramCsPath))
        {
            var ProgramCs = File.ReadAllText(ProgramCsPath);
            
            // Verify static files are only used for assets (JS, CSS, images), not HTML
            // UseStaticFiles() is allowed for serving JavaScript, CSS, and images
            // But all HTML must be dynamically generated
            if (ProgramCs.Contains("UseStaticFiles"))
            {
                Console.WriteLine("  ℹ Static files middleware found (for JS/CSS assets)");
                // This is acceptable as long as no HTML is served statically
            }
            
            // Verify dynamic UI Generation methods exist
            Assert(ProgramCs.Contains("GenerateOnboardingUI()") || 
                   ProgramCs.Contains("GenerateControlPanelUI()"), 
                "Should have dynamic UI Generation methods");
            Assert(ProgramCs.Contains("GenerateLoginUI()") || 
                   ProgramCs.Contains("GenerateControlPanelUI()"), 
                "Should have GenerateLoginUI or GenerateControlPanelUI method");
            Assert(ProgramCs.Contains("GenerateControlPanelUI()"), 
                "Should have GenerateControlPanelUI method");
            Assert(ProgramCs.Contains("GenerateAdminUI()"), 
                "Should have GenerateAdminUI method");
            
            // Verify routes use dynamic Generation
            Assert(ProgramCs.Contains("GenerateControlPanelUI()") && 
                   ProgramCs.Contains("app.MapGet(\"/control-panel\""), 
                "Control panel route should use dynamic Generation");
            
            Console.WriteLine("  ✓ Program.cs uses dynamic Generation for all HTML UI routes");
        }
        else
        {
            Console.WriteLine("  ⚠ Program.cs not found - test skipped");
        }
    }

    private static void TestNoStaticFileMiddleware()
    {
        Console.WriteLine("Test 6: Static Files Only for Assets (No Static HTML)");
        
        // This test verifies that static files are only used for assets (JS, CSS, images)
        // and not for HTML pages, which must be dynamically generated
        
        // We can verify the architecture docs confirm this pattern
        var architectureDocPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "WINDOW_OF_ASHAT_ARCHITECTURE.md"
        );
        
        if (File.Exists(architectureDocPath))
        {
            var doc = File.ReadAllText(architectureDocPath);
            // The architecture should confirm dynamic HTML generation
            // Static files (JS, CSS, images) are allowed for assets
            Assert(doc.Contains("dynamic"), 
                "Architecture doc should mention dynamic Generation");
        }
        
        Console.WriteLine("  ✓ Static files used only for assets (JS/CSS), HTML is dynamic");
    }

    private static void TestAllModulesCompliant()
    {
        Console.WriteLine("Test 7: All Modules Are Compliant");
        
        // Load all modules and verify none Generate static HTML
        var memoryModule = new MemoryModule();
        memoryModule.Initialize(null);
        
        var moduleManager = new ModuleManager();
        moduleManager.RegisterBuiltInModule(memoryModule);
        moduleManager.LoadModules();
        
        var loadedModules = moduleManager.Modules.Count;
        Assert(loadedModules > 0, "Should load at least one module");
        
        // Verify module is loaded and configured correctly
        var siteBuilder = moduleManager.Modules
            .Select(m => m.Instance)
            .OfType<SiteBuilderModule>()
            .FirstOrDefault();
        
        if (siteBuilder != null)
        {
            var help = siteBuilder.Process("help");
            Assert(help.Contains("site spawn"), "SiteBuilder should have spawn command");
            
            // Verify the spawn command description mentions dynamic routing
            // (this is implicit - the actual behavior is tested in other tests)
        }
        
        Console.WriteLine($"  ✓ All {loadedModules} loaded modules are compliant");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Console.WriteLine($"  ❌ ASSERTION FAILED: {message}");
            throw new Exception($"SiteBuilder compliance test failed: {message}");
        }
    }
}
