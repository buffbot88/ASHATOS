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

        // Optional: allow Enter key to trigger command execution in the TextBox
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                var vm = textBox?.DataContext as RaWin.ViewModels.ModulePanelViewModel;
                if (vm != null && vm.RunCommand.CanExecute(null))
                {
                    vm.RunCommand.Execute(null);
                }
            }
        }
    }
}
