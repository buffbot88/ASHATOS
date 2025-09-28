using RaAI.Handlers.Manager;
using RaAI;
using System;
using System.Windows.Forms;
using RaAI.Modules;

namespace RaAI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize module manager and eagerly load modules before showing the form
            var manager = new ModuleManager();

            // Optional: add additional search paths for external modules (keeps defaults too)
            // manager.AddSearchPath(Path.Combine(AppContext.BaseDirectory, "Modules"));

            var loadResult = manager.LoadModules(requireAttributeOrNamespace: true);
            foreach (var err in loadResult.Errors)
                Console.WriteLine($"[ModuleLoader] {err}");

            using var form = new RaAIForm();
            form.SetModuleManager(manager);
            Application.Run(form);
        }
    }
}