public class MonitorLogsPanelViewModel : INotifyPropertyChanged
{
    public ObservableCollection<LogEntry> AllLogs { get; } = new();
    public ObservableCollection<LogEntry> FilteredLogs { get; } = new();

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

    public void LoadLogs(IEnumerable<string> errorLogs, IEnumerable<string> infoLogs)
    {
        AllLogs.Clear();
        foreach (var err in errorLogs)
            AllLogs.Add(new LogEntry { Message = err, Type = "error" });
        foreach (var info in infoLogs)
            AllLogs.Add(new LogEntry { Message = info, Type = "log" });
        FilterLogs();
    }

    private void FilterLogs()
    {
        FilteredLogs.Clear();
        var search = LogSearchInput?.Trim();
        foreach (var log in AllLogs)
        {
            if (string.IsNullOrEmpty(search) || log.Message.Contains(search, StringComparison.OrdinalIgnoreCase))
                FilteredLogs.Add(log);
        }
    }

    // ...INotifyPropertyChanged implementation...
}

public class LogEntry
{
    public string Message { get; set; }
    public string Type { get; set; } // "log" or "error"
}
