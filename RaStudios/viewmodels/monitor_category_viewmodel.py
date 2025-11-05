from viewmodels.observable_object import ObservableObject

class MonitorCategoryViewModel(ObservableObject):
    """
    ViewModel for a single monitoring card/category.
    Supports plugin/extension category panels and agentic diagnostics.
    """
    def __init__(self, name=None):
        super().__init__()
        self.name = name
        self.status_panel = None
        self.metrics_panel = None
        self.logs_panel = None
        self.commands_panel = None
        self.is_status_panel = False
        self.is_metrics_panel = False
        self.is_logs_panel = False
        self.is_commands_panel = False
        # Add plugin/extension category support and diagnostics context as needed