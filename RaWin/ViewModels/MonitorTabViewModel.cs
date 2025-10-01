using System.Collections.ObjectModel;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for the Monitoring tab; provides categories as cards.
    /// </summary>
    public class MonitorTabViewModel
    {
        public ObservableCollection<MonitorCategoryViewModel> MonitorCategories { get; }

        public MonitorTabViewModel(ModuleManager manager)
        {
            MonitorCategories = new ObservableCollection<MonitorCategoryViewModel>
            {
                new MonitorCategoryViewModel
                {
                    Name = "Status",
                    StatusPanel = new StatusPanelViewModel(manager),
                    IsStatusPanel = true
                },
                new MonitorCategoryViewModel
                {
                    Name = "Metrics",
                    MetricsPanel = new MetricsPanelViewModel(manager),
                    IsMetricsPanel = true
                },
                new MonitorCategoryViewModel
                {
                    Name = "Logs",
                    LogsPanel = new LogsPanelViewModel(manager),
                    IsLogsPanel = true
                },
                new MonitorCategoryViewModel
                {
                    Name = "Commands",
                    CommandsPanel = new CommandsPanelViewModel(),
                    IsCommandsPanel = true
                }
            };
        }
    }
}
