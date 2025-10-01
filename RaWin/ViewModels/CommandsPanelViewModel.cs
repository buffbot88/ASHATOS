using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for the Commands card in Monitoring.
    /// </summary>
    public class CommandsPanelViewModel : INotifyPropertyChanged
    {
        private string _commandInput;
        public string CommandInput
        {
            get => _commandInput;
            set { _commandInput = value; OnPropertyChanged(); }
        }

        private string _commandResult;
        public string CommandResult
        {
            get => _commandResult;
            set { _commandResult = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public CommandsPanelViewModel()
        {
            SendCommand = new RelayCommand(_ => Execute());
        }

        private void Execute()
        {
            if (string.IsNullOrWhiteSpace(CommandInput))
            {
                ErrorMessage = "Command cannot be empty.";
                CommandResult = "";
            }
            else
            {
                // Placeholder for actual command execution logic. Extend as needed.
                CommandResult = $"Executed: {CommandInput}";
                ErrorMessage = "";
            }
            OnPropertyChanged(nameof(CommandResult));
            OnPropertyChanged(nameof(ErrorMessage));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
