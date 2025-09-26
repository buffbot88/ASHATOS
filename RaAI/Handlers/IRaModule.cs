using System;

namespace RaAI.Handlers
{
    // Minimal module contract
    public interface IRaModule : IDisposable
    {
        string Name { get; }

        // Called once after the module is instantiated. Manager passes itself for legacy compatibility.
        void Initialize(ModuleManager manager);

        // Simple text processing entry point for interactive testing
        string Process(string input);
    }
}