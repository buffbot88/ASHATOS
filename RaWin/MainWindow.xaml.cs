using System.Windows;
using System.Windows.Input;
using RaCore.Engine.Manager;
using RaWin.ViewModels;

namespace RaWin
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            var manager = new ModuleManager();
            manager.LoadModules();

            ViewModel = new MainWindowViewModel(manager);
            DataContext = ViewModel;
        }

        // Allow Enter key to trigger command execution in a TextBox (Modules and Monitor tabs)
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as System.Windows.Controls.TextBox;

                // For Modules tab
                if (textBox?.DataContext is ModulePanelViewModel moduleVm && moduleVm.RunCommand?.CanExecute(null) == true)
                {
                    moduleVm.RunCommand.Execute(null);
                    return;
                }

                // For Monitor tab (Commands panel)
                if (textBox?.DataContext is CommandsPanelViewModel commandsVm && commandsVm.SendCommand?.CanExecute(null) == true)
                {
                    commandsVm.SendCommand.Execute(null);
                    return;
                }
            }
        }
    }
}
