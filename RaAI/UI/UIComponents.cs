using System;
using System.Drawing;
using System.Windows.Forms;

namespace RaAI.UI
{
    public static class UIComponents
    {
        public static Panel CreatePanel(DockStyle dock, Color backColor)
        {
            return new Panel { Dock = dock, BackColor = backColor };
        }

        public static Label CreateLabel(string text, int width, ContentAlignment align)
        {
            return new Label { Text = text, Width = width, TextAlign = align };
        }

        public static Button CreateButton(string text, int width)
        {
            return new Button { Text = text, Width = width };
        }

        public static TextBox CreateTextBox()
        {
            return new TextBox { Height = 34, Font = ThemeManager.Monospace(10) };
        }

        public static RichTextBox CreateRichTextBox()
        {
            return new RichTextBox { ReadOnly = true, Font = ThemeManager.Monospace(10) };
        }

        public static CheckedListBox CreateCheckedListBox()
        {
            return new CheckedListBox { Height = 110, Font = ThemeManager.Monospace(9) };
        }
    }
}