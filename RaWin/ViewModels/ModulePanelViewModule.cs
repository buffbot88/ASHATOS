using System.Collections.ObjectModel;
using System.Windows.Input;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    public class ModulePanelViewModel : ObservableObject
    {
        public ModuleWrapperView Module { get; }
        private readonly ModuleManager _manager;

        public string Status { get; private set; }
        public string Help { get; private set; }
        public ObservableCollection<string> Logs { get; } = new();

        private string _commandInput = "";
        public string CommandInput
        {
            get => _commandInput;
            set { _commandInput = value; OnPropertyChanged(); }
        }

        private string _commandResult = "";
        public string CommandResult
        {
            get => _commandResult;
            set { _commandResult = value; OnPropertyChanged(); }
        }

        public ICommand RunCommand { get; }

        public ModulePanelViewModel(ModuleWrapperView module, ModuleManager manager)
        {
            Module = module;
            _manager = manager;

            RunCommand = new RelayCommand(_ => RunCustomCommand());

            LoadProperties();
        }

        private void LoadProperties()
        {
            Status = Module.Raw is IRaModule mod ? mod.Process("status") : "";
            Help = Module.Raw is IRaModule mod2 ? mod2.Process("help") : "";

            var logsProp = Module.Raw.GetType().GetProperty("Logs");
            if (logsProp != null)
            {
                var logList = logsProp.GetValue(Module.Raw) as IEnumerable<string>;
                if (logList != null)
                {
                    Logs.Clear();
                    foreach (var log in logList)
                        Logs.Add(log);
                }
            }
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Help));
            OnPropertyChanged(nameof(Logs));
        }

        private void RunCustomCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandInput)) return;
            if (Module.Raw is IRaModule mod)
                CommandResult = mod.Process(CommandInput) ?? "(no result)";
            else
                CommandResult = "(module not loaded)";
        }
    }
}
