using System;
using System.Threading.Tasks;
using RaAI.Handlers;
using RaAI.Handlers.Manager;
using Xunit;

namespace LocalModuleServer.IntegrationTests
{
    /// <summary>
    /// Test fixture that sets up a LocalModuleServer with real ModuleManager and ModuleManagerRegistry.
    /// Provides a shared server instance for all tests using the hard-coded port 5090.
    /// </summary>
    public class ServerFixture : IAsyncLifetime
    {
        public const int TestPort = 5090;
        public const string TestToken = "test-token-123";
        
        public RaAI.Handlers.LocalModuleServer Server { get; private set; } = null!;
        public ModuleManager ModuleManager { get; private set; } = null!;
        public ModuleManagerRegistry Registry { get; private set; } = null!;
        
        public async Task InitializeAsync()
        {
            // Create real ModuleManager
            ModuleManager = new ModuleManager();
            
            // Load built-in modules and any discovered modules
            ModuleManager.LoadBuiltInModules();
            ModuleManager.DiscoverAndLoadModules();
            
            // Create registry wrapper
            Registry = new ModuleManagerRegistry(ModuleManager);
            
            // Create server with test configuration
            Server = new RaAI.Handlers.LocalModuleServer(
                port: TestPort,
                token: TestToken,
                modules: Registry,
                externalNotifyCallback: (module, command) => 
                {
                    Console.WriteLine($"[ServerFixture] Notification: {module} -> {command}");
                });
            
            // Start the server
            Server.Start();
            
            // Brief warm-up wait for the server to be ready
            await Task.Delay(500);
        }

        public async Task DisposeAsync()
        {
            Server?.Stop();
            ModuleManager?.Dispose();
            
            // Small delay to ensure cleanup
            await Task.Delay(100);
        }
    }
}