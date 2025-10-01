using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for the Logs card in Monitoring.
    /// </summary>
    public class LogsPanelViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LogEntry> AllLogs { get; set; } = new();
        public ObservableCollection<LogEntry> FilteredLogs { get; set; } = new();

        private string _logSearchInput;
        public string LogSearchInput
        {
            get => _logSearchInput;
            set
            {
                if (_logSearchInput != value)
                {
                    _logSearchInput = value;
                    OnPropertyChanged();
                    FilterLogs();
                }
            }
        }

        public LogsPanelViewModel(ModuleManager manager)
        {
            // Attempt to fetch logs from modules that expose them
            foreach (var core in manager.CoreModules)
            {
                var instance = core.Instance;
                var logsProp = instance?.GetType().GetProperty("Logs");
                if (logsProp != null)
                {
                    var logList = logsProp.GetValue(instance) as IEnumerable<string>;
                    if (logList != null)
                    {
                        foreach (var log in logList)
                            AllLogs.Add(new LogEntry { Message = log, Type = "log" });
                    }
                }
            }
            FilterLogs();
        }

        private void FilterLogs()
        {
            FilteredLogs.Clear();
            var search = LogSearchInput?.Trim();
            foreach (var log in AllLogs)
            {
                if (string.IsNullOrEmpty(search) || log.Message.Contains(search, System.StringComparison.OrdinalIgnoreCase))
                    FilteredLogs.Add(log);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class LogEntry
    {
        public string Message { get; set; }
        public string Type { get; set; } // "log", "error", etc.
    }
}
