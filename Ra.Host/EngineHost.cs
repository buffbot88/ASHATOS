using System;
using System.Collections.Generic;
using System.Linq;
using RaAI.Handlers.Manager;

namespace Ra.Host
{
    // Thin wrapper around ModuleManager for the headless host
    public sealed class EngineHost : IDisposable
    {
        public ModuleManager Manager { get; }

        public EngineHost()
        {
            Manager = new ModuleManager();

            // Ensure loader sees modules from the host's bin (so WebFaceRenderer is discovered)
            try { Manager.AddSearchPath(AppContext.BaseDirectory); } catch { }

            var load = Manager.LoadModules(requireAttributeOrNamespace: true);
            foreach (var err in load.Errors)
                Console.WriteLine($"[ModuleLoader] {err}");

            // Bind any renderers and wake the face
            try
            {
                Manager.SafeInvokeModuleByName("Face", "face bind");
                Manager.SafeInvokeModuleByName("Face", "face wake");
            }
            catch { }

            // Optional warmup
            try { Manager.RaiseSystemEvent("Warmup"); } catch { }
        }

        public string Invoke(string moduleName, string input)
        {
            return Manager.SafeInvokeModuleByName(moduleName, input)
                ?? Manager.SafeInvokeModuleByName(moduleName + "Module", input)
                ?? "(no response)";
        }

        public IEnumerable<string> ListModules()
        {
            return Manager.Modules
                .Select(w => $"{w.Instance?.Name} ({w.Instance?.GetType().Name})");
        }

        public void Dispose()
        {
            try { Manager?.Dispose(); } catch { }
        }
    }
}