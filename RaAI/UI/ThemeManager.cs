using System;
using System.Drawing;
using System.Windows.Forms;

namespace RaAI.UI
{
    public static class ThemeManager
    {
        public static Color PanelDark { get; private set; }
        public static Color PanelMid { get; private set; }
        public static Color Accent { get; private set; }
        public static Color ButtonText { get; private set; }
        public static Color Text { get; private set; }
        public static Font Monospace(int size) => new Font("Consolas", size, FontStyle.Regular);

        public static void Initialize()
        {
            // Dark theme colors
            PanelDark = Color.FromArgb(18, 18, 18);
            PanelMid = Color.FromArgb(25, 25, 25);
            Accent = Color.FromArgb(0, 123, 255);
            ButtonText = Color.White;
            Text = Color.White;
        }

        public static void ApplyFormTheme(Form form)
        {
            form.BackColor = PanelDark;
            form.ForeColor = Text;
        }

        public static void StyleButton(Button button)
        {
            button.BackColor = Accent;
            button.ForeColor = ButtonText;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        public static void StyleTextBox(TextBox textBox)
        {
            textBox.BackColor = PanelMid;
            textBox.ForeColor = Text;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void StyleRichTextBox(RichTextBox richTextBox)
        {
            richTextBox.BackColor = PanelDark;
            richTextBox.ForeColor = Text;
            richTextBox.BorderStyle = BorderStyle.None;
        }

        public static void StyleListBox(ListBox listBox)
        {
            listBox.BackColor = PanelMid;
            listBox.ForeColor = Text;
            listBox.BorderStyle = BorderStyle.None;
        }

        public static void StyleCheckedListBox(CheckedListBox checkedListBox)
        {
            checkedListBox.BackColor = PanelMid;
            checkedListBox.ForeColor = Text;
            checkedListBox.CheckOnClick = true;
        }

        public static void StyleLabel(Label label)
        {
            label.ForeColor = Text;
        }
    }
}