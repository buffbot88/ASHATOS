from viewmodels.observable_object import ObservableObject

class LogsPanelViewModel(ObservableObject):
    """
    ViewModel for LogsPanel, manages logs and diagnostics.
    """
    def __init__(self):
        super().__init__()
        self.logs = []

    def add_log(self, log_entry):
        self.logs.append(log_entry)
        self.notify_property_changed("logs")

    def get_logs(self):
        return self.logs