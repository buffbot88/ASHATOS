using RaAI.Handlers.Manager;
using RaAI;
using System;
using System.Windows.Forms;
using RaAI.Modules.DigitalFace;

namespace RaAI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create and show main form
            using var form = new RaAIForm();
            form.SetModuleManager(new ModuleManager()); // Initialize your module manager
            Application.Run(form);
        }
    }
}