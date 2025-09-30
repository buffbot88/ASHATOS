using System;
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

        // Error diagnostics property
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
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
            try
            {
                var mod = Module.ModuleInstance;
                Status = mod != null ? mod.Process("status") : "";
                Help = mod != null ? mod.Process("help") : "";

                var logsProp = mod?.GetType().GetProperty("Logs");
                if (logsProp != null)
                {
                    var logList = logsProp.GetValue(mod) as IEnumerable<string>;
                    if (logList != null)
                    {
                        Logs.Clear();
                        foreach (var log in logList)
                            Logs.Add(log);
                    }
                }
                ErrorMessage = "";
            }
            catch (Exception ex)
            {
                Status = "";
                Help = "";
                Logs.Clear();
                ErrorMessage = $"Error loading module properties: {ex.GetType().Name}: {ex.Message}";
            }
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Help));
            OnPropertyChanged(nameof(Logs));
            OnPropertyChanged(nameof(ErrorMessage));
        }

        private void RunCustomCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandInput)) return;
            try
            {
                var mod = Module.ModuleInstance;
                if (mod != null)
                {
                    CommandResult = mod.Process(CommandInput) ?? "(no result)";
                    ErrorMessage = "";
                }
                else
                {
                    CommandResult = "(module not loaded)";
                    ErrorMessage = "Module instance is null (not loaded)";
                }
            }
            catch (Exception ex)
            {
                CommandResult = "(command error)";
                ErrorMessage = $"Error running command: {ex.GetType().Name}: {ex.Message}";
            }
            OnPropertyChanged(nameof(CommandResult));
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
}
