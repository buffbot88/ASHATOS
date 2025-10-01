using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<ModulePanelViewModel> CoreModules { get; }
        public ObservableCollection<ModulePanelViewModel> AllModules { get; }
        public MonitorTabViewModel MonitorTab { get; }

        private string _logOutput = "";
        public string LogOutput
        {
            get => _logOutput;
            set { _logOutput = value; OnPropertyChanged(); }
        }

        private string _chatOutput = "";
        public string ChatOutput
        {
            get => _chatOutput;
            set { _chatOutput = value; OnPropertyChanged(); }
        }

        private string _userInput = "";
        public string UserInput
        {
            get => _userInput;
            set { _userInput = value; OnPropertyChanged(); }
        }

        private string _digitalScreenOutput = "";
        public string DigitalScreenOutput
        {
            get => _digitalScreenOutput;
            set { _digitalScreenOutput = value; OnPropertyChanged(); }
        }

        public ICommand SendUserInputCommand { get; }

        private readonly ModuleManager _manager;

        public MainWindowViewModel(ModuleManager manager)
        {
            _manager = manager;

            CoreModules = new ObservableCollection<ModulePanelViewModel>(
                manager.CoreModules.Select(m => new ModulePanelViewModel(new ModuleWrapperView(m), manager))
            );

            AllModules = new ObservableCollection<ModulePanelViewModel>(
                manager.Modules
                    .Where(m => m.Instance != null)
                    .Select(m => new ModulePanelViewModel(new ModuleWrapperView(m), manager))
            );

            MonitorTab = new MonitorTabViewModel(manager);

            LogOutput = "System started. No errors detected.\nModuleX loaded successfully.";
            ChatOutput = "Welcome to RaWin!\nHow can I assist you?";
            DigitalScreenOutput = "";

            SendUserInputCommand = new RelayCommand(_ => SendUserInput());
        }

        private void SendUserInput()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
                return;

            // Send the input to the SpeechModule
            var speechResult = _manager.SafeInvokeModuleByName("Speech", UserInput)
                               ?? _manager.SafeInvokeModuleByName("SpeechModule", UserInput);

            // Show the command and response in the chat
            ChatOutput += "\n> " + UserInput;
            ChatOutput += "\n" + (speechResult ?? "(no response)");

            // Also set the output for the DigitalScreen card
            DigitalScreenOutput = speechResult ?? "(no response)";

            UserInput = ""; // Clear the input box
            OnPropertyChanged(nameof(ChatOutput));
            OnPropertyChanged(nameof(DigitalScreenOutput));
            OnPropertyChanged(nameof(UserInput));
        }
    }
}
