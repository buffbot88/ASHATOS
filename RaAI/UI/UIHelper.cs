using System;
using System.Text;
using System.Windows.Forms;

namespace RaAI.UI
{
    public static class UIHelper
    {
        public static void AppendLog(RichTextBox logBox, string moduleName, string input, string response)
        {
            string logMessage = FormatLogMessage(moduleName, input, response);

            void appendAction()
            {
                logBox.AppendText(logMessage);
                AutoScrollIfNecessary(logBox);
            }

            SafeInvoke(logBox, appendAction);
        }

        private static string FormatLogMessage(string moduleName, string input, string response)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            var sb = new StringBuilder();
            sb.AppendLine($"[{ts}] [{moduleName}] > {input}");
            sb.AppendLine(response);
            sb.AppendLine();
            return sb.ToString();
        }

        private static void AutoScrollIfNecessary(RichTextBox logBox)
        {
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        }

        public static void SafeInvoke(Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        public static string GetSelectedModule(CheckedListBox clb)
        {
            return clb.CheckedItems.Cast<string>().FirstOrDefault() ?? string.Empty;
        }
    }
}