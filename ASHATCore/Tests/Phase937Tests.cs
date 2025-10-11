using ASHATCore.Engine;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for Phase 9.3.7: Bot Detection and CloudFlare integration
/// </summary>
public static class Phase937Tests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Phase 9.3.7 Tests ===");
        Console.WriteLine();
        
        TestBotDetection();
        TestCloudFlareDetection();
        TestCloudFlareConfig();
        
        Console.WriteLine();
        Console.WriteLine("=== All Phase 9.3.7 Tests Completed ===");
    }

    private static void TestBotDetection()
    {
        Console.WriteLine("Test 1: Bot Detection");
        Console.WriteLine("---------------------");
        
        // Test search engine bots
        var testCases = new[]
        {
            ("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", true, "Googlebot"),
            ("Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)", true, "bingbot"),
            ("Mozilla/5.0 (compatible; Yahoo! Slurp; http://help.yahoo.com/help/us/ysearch/slurp)", true, "Slurp"),
            ("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)", true, "YandexBot"),
            ("Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)", true, "Baiduspider"),
            ("DuckDuckBot/1.0; (+http://duckduckgo.com/duckduckbot.html)", true, "DuckDuckBot"),
            ("facebookexternalhit/1.1 (+http://www.facebook.com/externalhit_uatext.php)", true, "facebookexternalhit"),
            ("Mozilla/5.0 (compatible; Applebot/0.1; +http://www.apple.com/go/applebot)", true, "Applebot"),
            
            // Regular browsers (should not be bots)
            ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", false, null),
            ("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", false, null),
            ("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0", false, null),
            ("", false, null),
            (null, false, null)
        };

        int passed = 0;
        int failed = 0;

        foreach (var (useASHATgent, expectedIsBot, expectedBotName) in testCases)
        {
            var isBot = BotDetector.IsSearchEngineBot(useASHATgent);
            var botName = BotDetector.GetBotName(useASHATgent);
            
            var testPassed = isBot == expectedIsBot && 
                             (expectedBotName == null || botName?.Contains(expectedBotName, StringComparison.OrdinalIgnoreCase) == true);
            
            if (testPassed)
            {
                passed++;
                Console.WriteLine($"✅ PASS: {(string.IsNullOrEmpty(useASHATgent) ? "[empty]" : useASHATgent.Substring(0, Math.Min(50, useASHATgent.Length)))}...");
                if (isBot)
                {
                    Console.WriteLine($"   Detected as: {botName}");
                }
            }
            else
            {
                failed++;
                Console.WriteLine($"❌ FAIL: {(string.IsNullOrEmpty(useASHATgent) ? "[empty]" : useASHATgent)}");
                Console.WriteLine($"   Expected: Bot={expectedIsBot}, BotName={expectedBotName}");
                Console.WriteLine($"   Got: Bot={isBot}, BotName={botName}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Bot Detection: {passed} passed, {failed} failed");
        Console.WriteLine();
    }

    private static void TestCloudFlareDetection()
    {
        Console.WriteLine("Test 2: CloudFlare Request Detection");
        Console.WriteLine("------------------------------------");
        
        // Test with CloudFlare headers
        var cfHeaders = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "CF-ASHATy", "1234567890abc-LAX" },
            { "CF-Connecting-IP", "203.0.113.45" },
            { "CF-IPCountry", "US" }
        };
        
        var isCfRequest = CloudFlareConfig.IsCloudFlareRequest(cfHeaders);
        var clientIp = CloudFlareConfig.GetRealClientIp(cfHeaders);
        var country = CloudFlareConfig.GetVisitorCountry(cfHeaders);
        
        if (isCfRequest && clientIp == "203.0.113.45" && country == "US")
        {
            Console.WriteLine("✅ PASS: CloudFlare request detected correctly");
            Console.WriteLine($"   Client IP: {clientIp}");
            Console.WriteLine($"   Country: {country}");
        }
        else
        {
            Console.WriteLine("❌ FAIL: CloudFlare request detection failed");
            Console.WriteLine($"   IsCF: {isCfRequest}, IP: {clientIp}, Country: {country}");
        }
        
        // Test without CloudFlare headers
        var normalHeaders = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "User-Agent", "Mozilla/5.0" }
        };
        
        var isNotCf = !CloudFlareConfig.IsCloudFlareRequest(normalHeaders);
        
        if (isNotCf)
        {
            Console.WriteLine("✅ PASS: Non-CloudFlare request handled correctly");
        }
        else
        {
            Console.WriteLine("❌ FAIL: Non-CloudFlare request detection failed");
        }
        
        Console.WriteLine();
    }

    private static void TestCloudFlareConfig()
    {
        Console.WriteLine("Test 3: CloudFlare Configuration Generation");
        Console.WriteLine("--------------------------------------------");
        
        try
        {
            // Test recommended settings
            var settings = CloudFlareConfig.GetRecommendedSettings();
            
            if (settings.ProxyEnabled && 
                settings.SslMode == "Full (Strict)" &&
                settings.Hsts.Enabled &&
                settings.MinimumTlsVersion == "1.2")
            {
                Console.WriteLine("✅ PASS: CloudFlare settings Generated correctly");
                Console.WriteLine($"   SSL Mode: {settings.SslMode}");
                Console.WriteLine($"   HSTS Enabled: {settings.Hsts.Enabled}");
                Console.WriteLine($"   Min TLS: {settings.MinimumTlsVersion}");
            }
            else
            {
                Console.WriteLine("❌ FAIL: CloudFlare settings validation failed");
            }
            
            // Test page rules
            var pageRules = CloudFlareConfig.GetRecommendedPageRules("example.com");
            
            if (pageRules.Count >= 4)
            {
                Console.WriteLine("✅ PASS: CloudFlare page rules Generated");
                Console.WriteLine($"   Rules count: {pageRules.Count}");
                foreach (var rule in pageRules)
                {
                    Console.WriteLine($"   - {rule.Url}");
                }
            }
            else
            {
                Console.WriteLine("❌ FAIL: CloudFlare page rules Generation failed");
            }
            
            // Test nginx config Generation
            var nginxConfig = CloudFlareConfig.GenerateNginxCloudFlareConfig("example.com");
            
            if (!string.IsNullOrEmpty(nginxConfig) && 
                nginxConfig.Contains("CF-Connecting-IP") &&
                nginxConfig.Contains("ssl_certificate"))
            {
                Console.WriteLine("✅ PASS: Nginx CloudFlare config Generated");
                Console.WriteLine($"   Config length: {nginxConfig.Length} characters");
            }
            else
            {
                Console.WriteLine("❌ FAIL: Nginx config Generation failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
        }
        
        Console.WriteLine();
    }
}
