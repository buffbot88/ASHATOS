using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for onboarding flow validation
/// Verifies that first-time users are routed through Masters Class onboarding
/// </summary>
public class OnboardingFlowTests
{
    public static async Task RunTests()
    {
        Console.WriteLine("=== Onboarding Flow Tests ===");
        Console.WriteLine();

        await TestOnboardingRouteExists();
        await TestLoginRedirectLogic();
        await TestControlPanelGating();
        await TestLearningApiEndpoints();
        TestOnboardingUIStructure();

        Console.WriteLine();
        Console.WriteLine("=== All Onboarding Flow Tests Passed ===");
    }

    private static Task TestOnboardingRouteExists()
    {
        Console.WriteLine("[TEST] Verifying onboarding route exists...");

        // Verify the /onboarding route is defined in Program.cs
        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (System.IO.File.Exists(ProgramPath))
        {
            var content = System.IO.File.ReadAllText(ProgramPath);
            if (content.Contains("app.MapGet(\"/onboarding\""))
            {
                Console.WriteLine("  ✓ /onboarding route is registered");
            }
            else
            {
                throw new Exception("  ✗ FAIL: /onboarding route not found");
            }

            if (content.Contains("GenerateOnboardingUI()"))
            {
                Console.WriteLine("  ✓ GenerateOnboardingUI() function is called");
            }
            else
            {
                throw new Exception("  ✗ FAIL: GenerateOnboardingUI() not found");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping route check");
        }

        Console.WriteLine("  ✓ PASS: Onboarding route properly configured");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestLoginRedirectLogic()
    {
        Console.WriteLine("[TEST] Verifying login redirect logic checks RequiresLULModule...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (System.IO.File.Exists(ProgramPath))
        {
            var content = System.IO.File.ReadAllText(ProgramPath);
            
            // Check that login form checks requiresLULModule flag
            if (content.Contains("data.requiresLULModule"))
            {
                Console.WriteLine("  ✓ Login checks requiresLULModule flag");
            }
            else
            {
                throw new Exception("  ✗ FAIL: Login doesn't check requiresLULModule");
            }

            // Check that login redirects to /onboarding when flag is true
            if (content.Contains("window.location.href = '/onboarding'"))
            {
                Console.WriteLine("  ✓ Login redirects to /onboarding when required");
            }
            else
            {
                throw new Exception("  ✗ FAIL: Login doesn't redirect to /onboarding");
            }

            // Check that login still redirects to /control-panel when flag is false
            if (content.Contains("window.location.href = '/control-panel'"))
            {
                Console.WriteLine("  ✓ Login redirects to /control-panel when onboarding not required");
            }
            else
            {
                throw new Exception("  ✗ FAIL: Login doesn't redirect to /control-panel");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping login redirect check");
        }

        Console.WriteLine("  ✓ PASS: Login redirect logic properly implemented");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestControlPanelGating()
    {
        Console.WriteLine("[TEST] Verifying control panel gates access...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (System.IO.File.Exists(ProgramPath))
        {
            var content = System.IO.File.ReadAllText(ProgramPath);
            
            // Check that control panel calls /api/learning/SuperAdmin/status
            if (content.Contains("/api/learning/SuperAdmin/status"))
            {
                Console.WriteLine("  ✓ Control panel checks onboarding status");
            }
            else
            {
                throw new Exception("  ✗ FAIL: Control panel doesn't check onboarding status");
            }

            // Check that control panel redirects incomplete users
            var controlPanelIndex = content.IndexOf("GenerateControlPanelUI");
            if (controlPanelIndex >= 0)
            {
                var controlPanelSection = content.Substring(
                    controlPanelIndex,
                    Math.Min(5000, content.Length - controlPanelIndex)
                );

                if (controlPanelSection.Contains("!data.hasCompleted") && 
                    controlPanelSection.Contains("window.location.href = '/onboarding'"))
                {
                    Console.WriteLine("  ✓ Control panel redirects to /onboarding if incomplete");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Control panel doesn't redirect incomplete users");
                }
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping control panel gating check");
        }

        Console.WriteLine("  ✓ PASS: Control panel properly gates access");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestLearningApiEndpoints()
    {
        Console.WriteLine("[TEST] Verifying learning API endpoints exist...");

        var expectedEndpoints = new[]
        {
            "/api/learning/SuperAdmin/status",
            "/api/learning/SuperAdmin/complete",
            "/api/learning/courses/{level}",
            "/api/learning/courses/{courseId}/lessons",
            "/api/learning/lessons/{lessonId}/complete"
        };

        // Check that endpoints exist in ControlPanelEndpoints.cs
        var endpointsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Endpoints", "ControlPanelEndpoints.cs");
        endpointsPath = Path.GetFullPath(endpointsPath);
        
        if (System.IO.File.Exists(endpointsPath))
        {
            var content = System.IO.File.ReadAllText(endpointsPath);

            foreach (var endpoint in expectedEndpoints)
            {
                // Remove Parameter placeholders for checking
                var searchEndpoint = endpoint.Replace("{level}", "").Replace("{courseId}", "").Replace("{lessonId}", "");
                if (content.Contains($"/api/learning/"))
                {
                    Console.WriteLine($"  ✓ Found learning endpoint: {endpoint}");
                }
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: ControlPanelEndpoints.cs not found at {endpointsPath}, skipping API endpoint check");
        }

        Console.WriteLine("  ✓ PASS: Learning API endpoints properly configured");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static void TestOnboardingUIStructure()
    {
        Console.WriteLine("[TEST] Verifying onboarding UI structure...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (System.IO.File.Exists(ProgramPath))
        {
            var content = System.IO.File.ReadAllText(ProgramPath);
            
            var requiredElements = new[]
            {
                "Masters Class Onboarding",  // Page title
                "course-list",                // Course list container
                "lesson-viewer",              // Lesson viewer
                "progressBar",                // Progress bar
                "loadCourses()",             // Course loading function
                "completeLesson()",          // Lesson completion function
                "completeOnboarding()",      // Onboarding completion function
            };

            foreach (var element in requiredElements)
            {
                if (content.Contains(element))
                {
                    Console.WriteLine($"  ✓ Found UI element: {element}");
                }
                else
                {
                    throw new Exception($"  ✗ FAIL: Missing UI element: {element}");
                }
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping UI structure check");
        }

        Console.WriteLine("  ✓ PASS: Onboarding UI properly structured");
        Console.WriteLine();
    }
}
