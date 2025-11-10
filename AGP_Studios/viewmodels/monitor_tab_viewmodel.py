from viewmodels.monitor_category_viewmodel import MonitorCategoryViewModel
from viewmodels.status_panel_viewmodel import StatusPanelViewModel
from viewmodels.metrics_panel_viewmodel import MetricsPanelViewModel
from viewmodels.logs_panel_viewmodel import LogsPanelViewModel
from viewmodels.commands_panel_viewmodel import CommandsPanelViewModel

class MonitorTabViewModel:
    """
    DEPRECATED: Use MonitorPageViewModel for modular Monitoring tab!
    Phase 3 ready: For legacy compatibility only.
    """
    def __init__(self, module_manager, speech_pipeline, module_bus):
        self.monitor_categories = [
            MonitorCategoryViewModel("Status"),
            MonitorCategoryViewModel("Metrics"),
            MonitorCategoryViewModel("Logs"),
            MonitorCategoryViewModel("Commands")
        ]
        self.monitor_categories[0].status_panel = StatusPanelViewModel(speech_pipeline, module_bus)
        self.monitor_categories[0].is_status_panel = True

        self.monitor_categories[1].metrics_panel = MetricsPanelViewModel(speech_pipeline, module_manager, module_bus)
        self.monitor_categories[1].is_metrics_panel = True

        self.monitor_categories[2].logs_panel = LogsPanelViewModel(module_bus)
        self.monitor_categories[2].is_logs_panel = True

        self.monitor_categories[3].commands_panel = CommandsPanelViewModel(speech_pipeline, module_bus)
        self.monitor_categories[3].is_commands_panel = True