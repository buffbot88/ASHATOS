using System;
using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Core.Ashat;
using RaCore.Modules.Core.Ashat.GameIntegration;
using RaCore.Modules.Core.Ashat.RuntimeMonitoring;

namespace RaCore.Tests;

/// <summary>
/// Tests for ASHAT Core Module v9.4.0
/// Verifies AI consciousness, Guardian Angel, and self-healing capabilities
/// </summary>
public class AshatCoreTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         ASHAT Core Module v9.4.0 Tests                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        TestModuleInitialization();
        TestConsciousnessState();
        TestGuardianAngelService();
        TestPlayerInteraction();
        TestHealthMonitoring();
        TestSelfHealing();
        TestAutonomousCapabilities();
        
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           ✨ All ASHAT Tests Passed! ✨                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    }
    
    private static void TestModuleInitialization()
    {
        Console.WriteLine("Test 1: Module Initialization");
        
        var module = new AshatModule();
        var moduleManager = new ModuleManager();
        
        module.Initialize(moduleManager);
        
        if (module.Name != "Ashat")
            throw new Exception($"Expected module name 'Ashat', got '{module.Name}'");
        
        Console.WriteLine("  ✅ Module initialized successfully");
        Console.WriteLine($"  ✅ Module name: {module.Name}");
        Console.WriteLine();
    }
    
    private static void TestConsciousnessState()
    {
        Console.WriteLine("Test 2: AI Consciousness State");
        
        var module = new AshatModule();
        module.Initialize(null);
        
        var response = module.Process("consciousness");
        
        if (string.IsNullOrWhiteSpace(response))
            throw new Exception("Consciousness state returned empty response");
        
        if (!response.Contains("ASHAT") && !response.Contains("consciousness"))
            throw new Exception("Consciousness state response missing expected content");
        
        Console.WriteLine("  ✅ Consciousness state accessible");
        Console.WriteLine("  ✅ Response contains expected information");
        Console.WriteLine();
    }
    
    private static void TestGuardianAngelService()
    {
        Console.WriteLine("Test 3: Guardian Angel Service");
        
        var service = new GuardianAngelService();
        var playerId = "test_player_123";
        
        // Test Guardian initialization
        var state = service.InitializeGuardian(playerId);
        
        if (state.PlayerId != playerId)
            throw new Exception($"Expected PlayerId '{playerId}', got '{state.PlayerId}'");
        
        if (state.GuardianName != "ASHAT")
            throw new Exception($"Expected GuardianName 'ASHAT', got '{state.GuardianName}'");
        
        Console.WriteLine("  ✅ Guardian Angel initialized for player");
        Console.WriteLine($"  ✅ Guardian Name: {state.GuardianName}");
        Console.WriteLine($"  ✅ Protection Level: {state.ProtectionLevel}");
        
        // Test guidance provision
        var situation = new PlayerSituation
        {
            IsInDanger = true,
            PlayerLevel = 5
        };
        
        var guidance = service.ProvideGuidance(playerId, situation);
        
        if (guidance.Priority != GuidancePriority.Critical)
            throw new Exception($"Expected Critical priority for danger, got {guidance.Priority}");
        
        if (string.IsNullOrWhiteSpace(guidance.Message))
            throw new Exception("Guidance message is empty");
        
        Console.WriteLine("  ✅ Guidance provided for dangerous situation");
        Console.WriteLine($"  ✅ Priority: {guidance.Priority}");
        
        // Test protection assessment
        var threat = new ThreatContext
        {
            ImmediateDanger = true,
            ProximityToPlayer = 3.0f
        };
        
        var protection = service.AssessProtection(playerId, threat);
        
        if (protection.ThreatLevel != GuardianThreatLevel.Critical)
            throw new Exception($"Expected Critical threat level, got {protection.ThreatLevel}");
        
        if (!protection.GuardianIntervention)
            throw new Exception("Expected Guardian intervention for critical threat");
        
        Console.WriteLine("  ✅ Protection assessment working");
        Console.WriteLine($"  ✅ Threat Level: {protection.ThreatLevel}");
        Console.WriteLine($"  ✅ Shield Strength: {protection.ShieldStrength}");
        Console.WriteLine();
    }
    
    private static void TestPlayerInteraction()
    {
        Console.WriteLine("Test 4: Player Interaction Handler");
        
        var guardianService = new GuardianAngelService();
        var handler = new PlayerInteractionHandler(guardianService);
        var playerId = "test_player_456";
        
        // Test query processing
        var response = handler.ProcessPlayerQuery(playerId, "I need help");
        
        if (response.PlayerId != playerId)
            throw new Exception($"Expected PlayerId '{playerId}', got '{response.PlayerId}'");
        
        if (string.IsNullOrWhiteSpace(response.ResponseText))
            throw new Exception("Response text is empty");
        
        Console.WriteLine("  ✅ Query processing works");
        Console.WriteLine($"  ✅ Response Type: {response.ResponseType}");
        
        // Test help request
        var helpRequest = new HelpRequest
        {
            RequestType = HelpType.Emergency,
            IsUrgent = true
        };
        
        var helpResponse = handler.ProvideHelp(playerId, helpRequest);
        
        if (helpResponse.Priority != ResponsePriority.Critical)
            throw new Exception($"Expected Critical priority for emergency, got {helpResponse.Priority}");
        
        Console.WriteLine("  ✅ Emergency help response working");
        Console.WriteLine($"  ✅ Priority: {helpResponse.Priority}");
        
        // Test statistics
        var stats = handler.GetStatistics(playerId);
        
        if (stats.TotalQueries < 1)
            throw new Exception("Expected at least 1 query in statistics");
        
        if (stats.HelpRequests < 1)
            throw new Exception("Expected at least 1 help request in statistics");
        
        Console.WriteLine("  ✅ Interaction statistics tracking");
        Console.WriteLine($"  ✅ Total Queries: {stats.TotalQueries}");
        Console.WriteLine($"  ✅ Help Requests: {stats.HelpRequests}");
        Console.WriteLine($"  ✅ Relationship Level: {stats.RelationshipLevel}");
        Console.WriteLine();
    }
    
    private static void TestHealthMonitoring()
    {
        Console.WriteLine("Test 5: Health Monitoring");
        
        var monitor = new AshatHealthMonitor();
        
        // Perform health check
        var report = monitor.PerformFullHealthCheckAsync().Result;
        
        if (report.Components.Count == 0)
            throw new Exception("Health report has no components");
        
        Console.WriteLine("  ✅ Health check completed");
        Console.WriteLine($"  ✅ Overall Health: {report.OverallHealth}");
        Console.WriteLine($"  ✅ Components Checked: {report.Components.Count}");
        
        // Verify expected components
        var expectedComponents = new[] 
        { 
            "AI Consciousness", 
            "Guardian Angel Systems", 
            "Core Module Integration",
            "Runtime Resources",
            "Decision Making"
        };
        
        foreach (var expectedComponent in expectedComponents)
        {
            if (!report.Components.ContainsKey(expectedComponent))
                throw new Exception($"Expected component '{expectedComponent}' not found in health report");
        }
        
        Console.WriteLine("  ✅ All expected components checked");
        
        // Test health trends
        var trend = monitor.GetHealthTrend();
        
        Console.WriteLine($"  ✅ Health Trend: {trend.Direction}");
        Console.WriteLine($"  ✅ Confidence: {trend.Confidence:P0}");
        Console.WriteLine();
    }
    
    private static void TestSelfHealing()
    {
        Console.WriteLine("Test 6: Self-Healing Capabilities");
        
        var monitor = new AshatHealthMonitor();
        var selfHealing = new AshatSelfHealing(monitor);
        
        // Enable auto-recovery
        selfHealing.SetAutoRecovery(true);
        
        // Test component recovery
        var success = selfHealing.RecoverComponentAsync("Test Component").Result;
        
        Console.WriteLine("  ✅ Self-healing initialized");
        Console.WriteLine($"  ✅ Auto-recovery: Enabled");
        
        // Get recovery history
        var history = selfHealing.GetRecoveryHistory(10);
        
        if (history.Count == 0)
            throw new Exception("Expected recovery history to contain at least one entry");
        
        Console.WriteLine($"  ✅ Recovery History: {history.Count} entries");
        Console.WriteLine($"  ✅ Last Recovery: {history[0].ActionType}");
        Console.WriteLine();
    }
    
    private static void TestAutonomousCapabilities()
    {
        Console.WriteLine("Test 7: Autonomous Decision-Making");
        
        var module = new AshatModule();
        module.Initialize(null);
        
        // Test recommendation generation
        var recommendation = module.AnalyzeAndRecommendAsync(
            "Player needs guidance on quest progression",
            new Dictionary<string, object> { { "questId", "quest_001" } }
        ).Result;
        
        if (recommendation.FromModule != "Ashat")
            throw new Exception($"Expected FromModule 'Ashat', got '{recommendation.FromModule}'");
        
        if (string.IsNullOrWhiteSpace(recommendation.Description))
            throw new Exception("Recommendation description is empty");
        
        Console.WriteLine("  ✅ Recommendation generated");
        Console.WriteLine($"  ✅ Confidence: {recommendation.Confidence:P0}");
        Console.WriteLine($"  ✅ Action Type: {recommendation.ActionType}");
        
        // Test decision execution
        var result = module.ExecuteDecisionAsync(recommendation, true).Result;
        
        if (!result.Approved)
            throw new Exception("Expected decision to be approved");
        
        if (!result.Executed)
            throw new Exception("Expected decision to be executed");
        
        Console.WriteLine("  ✅ Decision execution successful");
        Console.WriteLine($"  ✅ Result: {result.Result}");
        
        // Test self-check capability
        var healthStatus = module.PerformSelfCheckAsync().Result;
        
        if (healthStatus.ModuleName != "Ashat")
            throw new Exception($"Expected ModuleName 'Ashat', got '{healthStatus.ModuleName}'");
        
        Console.WriteLine("  ✅ Self-check capability working");
        Console.WriteLine($"  ✅ Health State: {healthStatus.State}");
        Console.WriteLine($"  ✅ Metrics: {healthStatus.Metrics.Count}");
        Console.WriteLine();
    }
}
