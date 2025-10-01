using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for a single monitoring card/category.
    /// </summary>
    public class MonitorCategoryViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public StatusPanelViewModel StatusPanel { get; set; }
        public MetricsPanelViewModel MetricsPanel { get; set; }
        public LogsPanelViewModel LogsPanel { get; set; }
        public CommandsPanelViewModel CommandsPanel { get; set; }

        public bool IsStatusPanel { get; set; }
        public bool IsMetricsPanel { get; set; }
        public bool IsLogsPanel { get; set; }
        public bool IsCommandsPanel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
