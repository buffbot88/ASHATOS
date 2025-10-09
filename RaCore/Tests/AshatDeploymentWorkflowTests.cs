using System;
using RaCore.Modules.Extensions.Ashat;
using RaCore.Engine.Manager;

namespace RaCore.Tests;

/// <summary>
/// Tests for ASHAT Deployment Workflow Module
/// Validates push-to-public-server workflow from ALPHA to OMEGA
/// </summary>
public class AshatDeploymentWorkflowTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    ASHAT Deployment Workflow Module Tests               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        TestModuleInitialization();
        TestHelpCommand();
        TestListServers();
        TestConfigurePublicServer();
        TestConfigureOmegaServer();
        TestPushToPublicServer();
        TestVerifyDeployment();
        TestApproveForOmega();
        TestRollbackDeployment();
        TestCancelDeployment();
        TestCompleteWorkflow();
        
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   ✨ All Deployment Workflow Tests Passed! ✨            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    }

    private static AshatDeploymentWorkflowModule CreateModule()
    {
        var module = new AshatDeploymentWorkflowModule();
        module.Initialize(null);
        return module;
    }

    private static void TestModuleInitialization()
    {
        Console.WriteLine("Test 1: Module Initialization");
        
        var module = CreateModule();
        
        if (module.Name != "AshatDeployment")
            throw new Exception($"Expected module name 'AshatDeployment', got '{module.Name}'");
        
        Console.WriteLine("  ✅ Module initialized successfully");
        Console.WriteLine($"  ✅ Module name: {module.Name}");
        Console.WriteLine();
    }

    private static void TestHelpCommand()
    {
        Console.WriteLine("Test 2: Help Command");
        
        var module = CreateModule();
        var result = module.Process("help");
        
        if (!result.Contains("ASHAT Deployment Workflow"))
            throw new Exception("Help text missing workflow title");
        if (!result.Contains("deploy push"))
            throw new Exception("Help text missing push command");
        if (!result.Contains("deploy verify"))
            throw new Exception("Help text missing verify command");
        if (!result.Contains("deploy approve"))
            throw new Exception("Help text missing approve command");
        
        Console.WriteLine("  ✅ Help command returns usage instructions");
        Console.WriteLine();
    }

    private static void TestListServers()
    {
        Console.WriteLine("Test 3: List Servers");
        
        var module = CreateModule();
        var result = module.Process("servers");
        
        if (!result.Contains("ALPHA"))
            throw new Exception("Servers list missing ALPHA");
        if (!result.Contains("OMEGA"))
            throw new Exception("Servers list missing OMEGA");
        if (!result.Contains("localhost"))
            throw new Exception("Servers list missing localhost");
        
        Console.WriteLine("  ✅ Default servers configured");
        Console.WriteLine();
    }

    private static void TestConfigurePublicServer()
    {
        Console.WriteLine("Test 4: Configure Public Server");
        
        var module = CreateModule();
        var result = module.Process("configure public http://staging.test.com PublicStaging");
        
        if (!result.Contains("✅ Server configured successfully"))
            throw new Exception("Server configuration failed");
        if (!result.Contains("PUBLIC"))
            throw new Exception("Server type not displayed");
        if (!result.Contains("http://staging.test.com"))
            throw new Exception("Server URL not displayed");
        
        Console.WriteLine("  ✅ Public Server configured successfully");
        Console.WriteLine();
    }

    private static void TestConfigureOmegaServer()
    {
        Console.WriteLine("Test 5: Configure OMEGA Server");
        
        var module = CreateModule();
        var result = module.Process("configure omega https://live.test.com OmegaLive");
        
        if (!result.Contains("✅ Server configured successfully"))
            throw new Exception("Server configuration failed");
        if (!result.Contains("OMEGA"))
            throw new Exception("Server type not displayed");
        if (!result.Contains("https://live.test.com"))
            throw new Exception("Server URL not displayed");
        
        Console.WriteLine("  ✅ OMEGA Server configured successfully");
        Console.WriteLine();
    }

    private static void TestPushToPublicServer()
    {
        Console.WriteLine("Test 6: Push to Public Server");
        
        var module = CreateModule();
        module.Process("configure public http://staging.test.com PublicStaging");
        var result = module.Process("push update-001 'Test update' 'Additional details'");
        
        if (!result.Contains("🚀 ASHAT Deployment Workflow Initiated"))
            throw new Exception("Deployment workflow not initiated");
        if (!result.Contains("update-001"))
            throw new Exception("Update ID not displayed");
        if (!result.Contains("📋 Deployment Plan"))
            throw new Exception("Deployment plan not displayed");
        
        Console.WriteLine("  ✅ Deployment session created");
        Console.WriteLine();
    }

    private static void TestVerifyDeployment()
    {
        Console.WriteLine("Test 7: Verify Deployment");
        
        var module = CreateModule();
        module.Process("configure public http://staging.test.com PublicStaging");
        module.Process("push update-002 'Test update'");
        var result = module.Process("verify update-002");
        
        if (!result.Contains("🔍 ASHAT Deployment Verification"))
            throw new Exception("Verification not started");
        if (!result.Contains("Health Check"))
            throw new Exception("Health check missing");
        if (!result.Contains("✅ Verification PASSED"))
            throw new Exception("Verification did not pass");
        
        Console.WriteLine("  ✅ Verification completed successfully");
        Console.WriteLine();
    }

    private static void TestApproveForOmega()
    {
        Console.WriteLine("Test 8: Approve for OMEGA");
        
        var module = CreateModule();
        module.Process("configure public http://staging.test.com PublicStaging");
        module.Process("configure omega https://live.test.com OmegaLive");
        module.Process("push update-003 'Test update'");
        module.Process("verify update-003");
        var result = module.Process("approve update-003");
        
        if (!result.Contains("🎉 ASHAT Deployment to OMEGA Approved"))
            throw new Exception("Approval not confirmed");
        if (!result.Contains("Distributing to Licensed Mainframes"))
            throw new Exception("Distribution message missing");
        if (!result.Contains("✅ Deployment completed successfully"))
            throw new Exception("Deployment completion not confirmed");
        
        Console.WriteLine("  ✅ Deployment approved and pushed to OMEGA");
        Console.WriteLine();
    }

    private static void TestRollbackDeployment()
    {
        Console.WriteLine("Test 9: Rollback Deployment");
        
        var module = CreateModule();
        module.Process("configure public http://staging.test.com PublicStaging");
        module.Process("push update-004 'Test update'");
        var result = module.Process("rollback update-004");
        
        if (!result.Contains("⏪ Deployment"))
            throw new Exception("Rollback not initiated");
        if (!result.Contains("rolled back successfully"))
            throw new Exception("Rollback not confirmed");
        
        Console.WriteLine("  ✅ Deployment rolled back successfully");
        Console.WriteLine();
    }

    private static void TestCancelDeployment()
    {
        Console.WriteLine("Test 10: Cancel Deployment");
        
        var module = CreateModule();
        module.Process("configure public http://staging.test.com PublicStaging");
        module.Process("push update-005 'Test update'");
        var result = module.Process("cancel update-005");
        
        if (!result.Contains("🚫 Deployment"))
            throw new Exception("Cancellation not initiated");
        if (!result.Contains("cancelled"))
            throw new Exception("Cancellation not confirmed");
        
        Console.WriteLine("  ✅ Deployment cancelled successfully");
        Console.WriteLine();
    }

    private static void TestCompleteWorkflow()
    {
        Console.WriteLine("Test 11: Complete ALPHA -> OMEGA Workflow");
        
        var module = CreateModule();
        
        // Step 1: Configure servers
        var configPublic = module.Process("configure public http://staging.raos.io PublicStaging");
        if (!configPublic.Contains("✅"))
            throw new Exception("Public server configuration failed");
        
        var configOmega = module.Process("configure omega https://omega.raos.io OmegaProduction");
        if (!configOmega.Contains("✅"))
            throw new Exception("OMEGA server configuration failed");
        
        // Step 2: Push to public server
        var push = module.Process("push patch-v1.2.3 'Security fixes and performance improvements'");
        if (!push.Contains("🚀 ASHAT Deployment Workflow Initiated"))
            throw new Exception("Push failed");
        
        // Step 3: Verify on public server
        var verify = module.Process("verify patch-v1.2.3");
        if (!verify.Contains("✅ Verification PASSED"))
            throw new Exception("Verification failed");
        
        // Step 4: Approve for OMEGA
        var approve = module.Process("approve patch-v1.2.3");
        if (!approve.Contains("🎉 ASHAT Deployment to OMEGA Approved"))
            throw new Exception("Approval failed");
        if (!approve.Contains("Distributing to Licensed Mainframes"))
            throw new Exception("Distribution not initiated");
        
        // Step 5: Check history
        var history = module.Process("history");
        if (!history.Contains("patch-v1.2.3"))
            throw new Exception("Deployment not recorded in history");
        
        Console.WriteLine("  ✅ Complete workflow executed successfully");
        Console.WriteLine("  ✅ ALPHA -> Public Server -> OMEGA pipeline verified");
        Console.WriteLine();
    }
}
