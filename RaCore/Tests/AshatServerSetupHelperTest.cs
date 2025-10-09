using System;
using System.Threading.Tasks;
using RaCore.Modules.Extensions.Ashat;
using RaCore.Modules.Extensions.ServerSetup;

namespace RaCore.Tests
{
    /// <summary>
    /// Test for ASHAT Server Setup Helper functionality
    /// </summary>
    public class AshatServerSetupHelperTest
    {
        public static async Task RunTests()
        {
            Console.WriteLine("=== ASHAT Server Setup Helper Test ===\n");

            var helper = new AshatServerSetupHelperModule();
            var serverSetup = new ServerSetupModule();
            
            // Initialize modules
            serverSetup.Initialize(null);
            helper.Initialize(null);

            // Test 1: Get Help
            Console.WriteLine("\n--- Test 1: Get Help ---");
            TestHelp(helper);

            // Test 2: Get Server Setup Guide
            Console.WriteLine("\n--- Test 2: Get Server Setup Guide ---");
            TestServerSetupGuide(helper);

            // Test 3: FTP Setup Guidance
            Console.WriteLine("\n--- Test 3: FTP Setup Guidance ---");
            TestFtpSetupGuide(helper);

            // Test 4: Server Health with Guidance
            Console.WriteLine("\n--- Test 4: Server Health Check ---");
            TestServerHealth(helper);

            // Test 5: List Training Courses
            Console.WriteLine("\n--- Test 5: List Training Courses ---");
            TestListCourses(helper);

            // Test 6: View Course Content
            Console.WriteLine("\n--- Test 6: View Course Content ---");
            TestViewCourse(helper);

            // Test 7: Launch Guided Workflow
            Console.WriteLine("\n--- Test 7: Launch Guided Workflow ---");
            TestLaunchWorkflow(helper);

            // Test 8: Setup Checklist
            Console.WriteLine("\n--- Test 8: Setup Checklist ---");
            TestSetupChecklist(helper);

            await Task.CompletedTask;

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        private static void TestHelp(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("help");
                Console.WriteLine("✓ Help Retrieved Successfully");
                Console.WriteLine($"  Output preview: {result.Substring(0, Math.Min(100, result.Length))}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Help Test Failed: {ex.Message}");
            }
        }

        private static void TestServerSetupGuide(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("guide");
                Console.WriteLine("✓ Server Setup Guide Retrieved");
                Console.WriteLine($"  Contains 'Prerequisites': {result.Contains("Prerequisites")}");
                Console.WriteLine($"  Contains 'Setup Steps': {result.Contains("Setup Steps")}");
                Console.WriteLine($"  Contains 'Server Health': {result.Contains("Check Server Health")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Server Setup Guide Test Failed: {ex.Message}");
            }
        }

        private static void TestFtpSetupGuide(AshatServerSetupHelperModule helper)
        {
            try
            {
                // Test each FTP setup step
                string[] steps = { "check", "install", "configure", "secure" };
                
                foreach (var step in steps)
                {
                    var result = helper.Process($"ftp {step}");
                    Console.WriteLine($"✓ FTP Setup Guide - {step.ToUpper()}");
                    Console.WriteLine($"  Length: {result.Length} characters");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FTP Setup Guide Test Failed: {ex.Message}");
            }
        }

        private static void TestServerHealth(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("health");
                Console.WriteLine("✓ Server Health Check with Guidance");
                Console.WriteLine($"  Contains 'Server Health': {result.Contains("Server Health")}");
                Console.WriteLine($"  Contains recommendations: {result.Contains("Recommendations") || result.Contains("Next steps")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Server Health Test Failed: {ex.Message}");
            }
        }

        private static void TestListCourses(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("courses");
                Console.WriteLine("✓ Training Courses Listed");
                Console.WriteLine($"  Contains 'ftp-basics': {result.Contains("ftp-basics")}");
                Console.WriteLine($"  Contains 'ftp-security': {result.Contains("ftp-security")}");
                Console.WriteLine($"  Contains 'server-health': {result.Contains("server-health")}");
                Console.WriteLine($"  Contains 'public-server-launch': {result.Contains("public-server-launch")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ List Courses Test Failed: {ex.Message}");
            }
        }

        private static void TestViewCourse(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("course ftp-security");
                Console.WriteLine("✓ Course Content Retrieved");
                Console.WriteLine($"  Title present: {result.Contains("Secure FTP Configuration")}");
                Console.WriteLine($"  Lessons listed: {result.Contains("Lessons:")}");
                Console.WriteLine($"  Related commands shown: {result.Contains("Related Commands:")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ View Course Test Failed: {ex.Message}");
            }
        }

        private static void TestLaunchWorkflow(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("launch");
                Console.WriteLine("✓ Launch Workflow Retrieved");
                Console.WriteLine($"  Contains 'Guided Workflow': {result.Contains("Guided Workflow")}");
                Console.WriteLine($"  Contains steps: {result.Contains("Step 1") && result.Contains("Step 2")}");
                Console.WriteLine($"  Server health check: {result.Contains("Server Health Check")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Launch Workflow Test Failed: {ex.Message}");
            }
        }

        private static void TestSetupChecklist(AshatServerSetupHelperModule helper)
        {
            try
            {
                var result = helper.Process("checklist");
                Console.WriteLine("✓ Setup Checklist Retrieved");
                Console.WriteLine($"  Contains 'Prerequisites': {result.Contains("Prerequisites")}");
                Console.WriteLine($"  Contains 'Server Health': {result.Contains("Server Health")}");
                Console.WriteLine($"  Contains 'FTP Installation': {result.Contains("FTP Installation")}");
                Console.WriteLine($"  Contains checkboxes: {result.Contains("[ ]")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Setup Checklist Test Failed: {ex.Message}");
            }
        }
    }
}
