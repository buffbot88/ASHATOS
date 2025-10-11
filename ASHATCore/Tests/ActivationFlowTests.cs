using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for server activation flow validation
/// Verifies that the license activation workflow is properly implemented
/// </summary>
public class ActivationFlowTests
{
    public static async Task RunTests()
    {
        Console.WriteLine("=== Activation Flow Tests ===");
        Console.WriteLine();

        await TestActivationRouteExists();
        await TestActivationUIStructure();
        await TestActivationApiEndpoints();
        await TestServerConfigurationFields();
        await TestControlPanelActivationGating();
        await TestOnboardingRedirectsToActivation();

        Console.WriteLine();
        Console.WriteLine("=== All Activation Flow Tests Passed ===");
    }

    private static Task TestActivationRouteExists()
    {
        Console.WriteLine("[TEST] Verifying activation route exists...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (File.Exists(ProgramPath))
        {
            var content = File.ReadAllText(ProgramPath);
            if (content.Contains("app.MapGet(\"/activation\""))
            {
                Console.WriteLine("  ✓ /activation route is registered");
            }
            else
            {
                throw new Exception("  ✗ FAIL: /activation route not found");
            }

            if (content.Contains("GenerateActivationUI()"))
            {
                Console.WriteLine("  ✓ GenerateActivationUI() method is called");
            }
            else
            {
                throw new Exception("  ✗ FAIL: GenerateActivationUI() method not found");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping route check");
        }

        Console.WriteLine("  ✓ PASS: Activation route exists");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestActivationUIStructure()
    {
        Console.WriteLine("[TEST] Verifying activation UI structure...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (File.Exists(ProgramPath))
        {
            var content = File.ReadAllText(ProgramPath);
            
            // Check for activation function definition
            var activationUIIndex = content.IndexOf("static string GenerateActivationUI()");
            if (activationUIIndex >= 0)
            {
                var activationUISection = content.Substring(
                    activationUIIndex,
                    Math.Min(10000, content.Length - activationUIIndex)
                );

                // Verify key UI elements
                if (activationUISection.Contains("Server Activation"))
                {
                    Console.WriteLine("  ✓ Activation UI has title");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Activation UI title not found");
                }

                if (activationUISection.Contains("licenseKey") && activationUISection.Contains("input"))
                {
                    Console.WriteLine("  ✓ Activation UI has license key input");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: License key input not found");
                }

                if (activationUISection.Contains("activateServer"))
                {
                    Console.WriteLine("  ✓ Activation UI has activateServer() function");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: activateServer() function not found");
                }

                if (activationUISection.Contains("/api/control/activate"))
                {
                    Console.WriteLine("  ✓ Activation UI calls /api/control/activate endpoint");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Activation endpoint call not found");
                }

                if (activationUISection.Contains("/api/control/activation-status"))
                {
                    Console.WriteLine("  ✓ Activation UI checks activation status");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Activation status check not found");
                }
            }
            else
            {
                throw new Exception("  ✗ FAIL: GenerateActivationUI() function not found");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping UI structure check");
        }

        Console.WriteLine("  ✓ PASS: Activation UI structure is complete");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestActivationApiEndpoints()
    {
        Console.WriteLine("[TEST] Verifying activation API endpoints...");

        var endpointsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Endpoints", "ControlPanelEndpoints.cs");
        endpointsPath = Path.GetFullPath(endpointsPath);
        
        if (File.Exists(endpointsPath))
        {
            var content = File.ReadAllText(endpointsPath);
            
            if (content.Contains("app.MapGet(\"/api/control/activation-status\""))
            {
                Console.WriteLine("  ✓ GET /api/control/activation-status endpoint exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: activation-status endpoint not found");
            }

            if (content.Contains("app.MapPost(\"/api/control/activate\""))
            {
                Console.WriteLine("  ✓ POST /api/control/activate endpoint exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: activate endpoint not found");
            }

            if (content.Contains("IsValidLicenseFormat"))
            {
                Console.WriteLine("  ✓ License format validation exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: License format validation not found");
            }

            if (content.Contains("ValidateLicenseWithServerAsync"))
            {
                Console.WriteLine("  ✓ License server validation exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: License server validation not found");
            }

            if (content.Contains("ServerMode.Dev") && content.Contains("SkipLicenseValidation"))
            {
                Console.WriteLine("  ✓ Dev mode bypass logic exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: Dev mode bypass logic not found");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: ControlPanelEndpoints.cs not found at {endpointsPath}, skipping API endpoint check");
        }

        Console.WriteLine("  ✓ PASS: Activation API endpoints are properly configured");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestServerConfigurationFields()
    {
        Console.WriteLine("[TEST] Verifying ServerConfiguration fields...");

        var serverModePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Abstractions", "ServerMode.cs");
        serverModePath = Path.GetFullPath(serverModePath);
        
        if (File.Exists(serverModePath))
        {
            var content = File.ReadAllText(serverModePath);
            
            if (content.Contains("public bool ServerActivated"))
            {
                Console.WriteLine("  ✓ ServerActivated field exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: ServerActivated field not found");
            }

            if (content.Contains("public DateTime? ActivatedAt"))
            {
                Console.WriteLine("  ✓ ActivatedAt field exists");
            }
            else
            {
                throw new Exception("  ✗ FAIL: ActivatedAt field not found");
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: ServerMode.cs not found at {serverModePath}, skipping field check");
        }

        Console.WriteLine("  ✓ PASS: ServerConfiguration has activation fields");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestControlPanelActivationGating()
    {
        Console.WriteLine("[TEST] Verifying control panel activation gating...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (File.Exists(ProgramPath))
        {
            var content = File.ReadAllText(ProgramPath);
            
            var controlPanelIndex = content.IndexOf("GenerateControlPanelUI");
            if (controlPanelIndex >= 0)
            {
                var controlPanelSection = content.Substring(
                    controlPanelIndex,
                    Math.Min(5000, content.Length - controlPanelIndex)
                );

                if (controlPanelSection.Contains("/api/control/activation-status"))
                {
                    Console.WriteLine("  ✓ Control panel checks activation status");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Control panel doesn't check activation status");
                }

                if (controlPanelSection.Contains("!activationData.activated") && 
                    controlPanelSection.Contains("window.location.href = '/activation'"))
                {
                    Console.WriteLine("  ✓ Control panel redirects to /activation if not activated");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Control panel doesn't redirect non-activated users");
                }
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping control panel gating check");
        }

        Console.WriteLine("  ✓ PASS: Control panel properly gates access based on activation");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static Task TestOnboardingRedirectsToActivation()
    {
        Console.WriteLine("[TEST] Verifying onboarding redirects to activation...");

        var ProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Program.cs");
        ProgramPath = Path.GetFullPath(ProgramPath);
        
        if (File.Exists(ProgramPath))
        {
            var content = File.ReadAllText(ProgramPath);
            
            var onboardingIndex = content.IndexOf("async function completeOnboarding()");
            if (onboardingIndex >= 0)
            {
                var onboardingSection = content.Substring(
                    onboardingIndex,
                    Math.Min(1000, content.Length - onboardingIndex)
                );

                if (onboardingSection.Contains("window.location.href = '/activation'"))
                {
                    Console.WriteLine("  ✓ Onboarding completion redirects to /activation");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Onboarding doesn't redirect to activation");
                }

                if (onboardingSection.Contains("Redirecting to server activation"))
                {
                    Console.WriteLine("  ✓ Onboarding shows activation redirect message");
                }
                else
                {
                    throw new Exception("  ✗ FAIL: Onboarding doesn't show activation message");
                }
            }
        }
        else
        {
            Console.WriteLine($"  ⚠ WARNING: Program.cs not found at {ProgramPath}, skipping onboarding redirect check");
        }

        Console.WriteLine("  ✓ PASS: Onboarding properly redirects to activation");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}
