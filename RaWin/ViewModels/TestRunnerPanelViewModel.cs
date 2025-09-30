using System.Windows.Input;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    public class TestRunnerPanelViewModel : ObservableObject
    {
        private readonly IRaModule? _testRunner;

        public string TestResult { get; private set; }
        public ICommand RunIntegrationSuiteCommand { get; }

        public TestRunnerPanelViewModel(ModuleManager manager)
        {
            _testRunner = manager.GetModuleByName("TestRunner");
            RunIntegrationSuiteCommand = new RelayCommand(_ => RunIntegrationSuite());
        }

        private void RunIntegrationSuite()
        {
            if (_testRunner != null)
                TestResult = _testRunner.Process("start fast");
            else
                TestResult = "(TestRunner module not loaded)";
            OnPropertyChanged(nameof(TestResult));
        }
    }
}
