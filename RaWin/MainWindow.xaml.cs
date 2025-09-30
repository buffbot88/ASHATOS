using System.Windows;
using RaCore.Engine.Manager;

namespace RaWin
{
    public partial class MainWindow : Window
    {
        private ModuleManager _moduleManager;
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize RaCore modules
            _moduleManager = new ModuleManager();
            _moduleManager.LoadModules();

            // Populate ViewModel
            _viewModel = new MainWindowViewModel(_moduleManager);
            DataContext = _viewModel;
        }
    }
}
