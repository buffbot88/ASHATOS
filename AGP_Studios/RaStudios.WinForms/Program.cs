using System;
using System.Windows.Forms;
using System.IO;

namespace RaStudios.WinForms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set up global exception handlers for unhandled exceptions
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
        }

        private static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception, "UI Thread Exception");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "Unhandled Exception");
            }
        }

        private static void HandleException(Exception ex, string title)
        {
            try
            {
                // Log the exception to a file
                LogException(ex);

                // Show user-friendly error message
                var message = $"An error occurred:\n\n{ex.Message}\n\n" +
                             $"Technical details:\n{ex.GetType().Name}\n\n" +
                             $"Error logs saved to:\n" +
                             $"• Current directory: RaStudios_Error.log\n" +
                             $"• AppData: %AppData%\\RaStudios\\Logs\\RaStudios_Error.log\n\n" +
                             $"Please check if:\n" +
                             "• All dependencies are installed\n" +
                             "• Your graphics drivers are up to date\n" +
                             "• You have the required .NET runtime installed";

                MessageBox.Show(message, $"RaStudios - {title}", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // If we can't even show a message box, write to a fallback error file
                try
                {
                    File.WriteAllText("RaStudios_Critical_Error.txt", 
                        $"Critical error at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}:\n{ex}");
                }
                catch
                {
                    // Absolute last resort - nothing we can do
                }
            }
        }

        private static void HandleFatalException(Exception ex)
        {
            try
            {
                // Log the exception to a file
                LogException(ex);

                // Show user-friendly error message
                var message = $"RaStudios failed to start:\n\n{ex.Message}\n\n" +
                             $"Technical details:\n{ex.GetType().Name}\n" +
                             $"At: {ex.TargetSite?.DeclaringType?.Name ?? "Unknown"}\n\n" +
                             $"The error has been logged to 'RaStudios_Error.log'.\n\n" +
                             $"Common solutions:\n" +
                             "• Update your graphics drivers (especially for DirectX 11)\n" +
                             "• Install the latest .NET 9.0 Windows Desktop Runtime\n" +
                             "• Run the application as Administrator\n" +
                             "• Check if antivirus software is blocking the application";

                MessageBox.Show(message, "RaStudios - Startup Failed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // If we can't even show a message box, create a basic error file
                try
                {
                    File.WriteAllText("RaStudios_Critical_Error.txt", 
                        $"Fatal error occurred at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}:\n{ex}");
                }
                catch
                {
                    // Absolute last resort - do nothing
                }
            }
        }

        private static void LogException(Exception ex)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RaStudios",
                    "Logs"
                );

                Directory.CreateDirectory(logPath);

                var logFile = Path.Combine(logPath, "RaStudios_Error.log");
                var logEntry = $"\n=== Error at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC} ===\n" +
                              $"Message: {ex.Message}\n" +
                              $"Type: {ex.GetType().FullName}\n" +
                              $"Stack Trace:\n{ex.StackTrace}\n";

                if (ex.InnerException != null)
                {
                    logEntry += $"\nInner Exception: {ex.InnerException.Message}\n" +
                               $"Inner Stack Trace:\n{ex.InnerException.StackTrace}\n";
                }

                File.AppendAllText(logFile, logEntry);

                // Also create/append to a simple log in the current directory for easy access
                var currentDirLog = "RaStudios_Error.log";
                File.AppendAllText(currentDirLog, logEntry);
            }
            catch
            {
                // If logging fails, we can't do much about it
            }
        }
    }
}
