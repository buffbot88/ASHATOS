using RaAI.Handlers;
using RaAI.UI;
using System;
using System.Windows.Forms;

namespace RaAI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize theme
            ThemeManager.Initialize();

            // Create and show main form
            using var form = new RaAIForm();
            form.SetModuleManager(new ModuleManager()); // Initialize your module manager
            Application.Run(form);
        }
    }
}