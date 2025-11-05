from PyQt6.QtWidgets import QWidget, QVBoxLayout, QListWidget, QListWidgetItem
from viewmodels.logs_panel_viewmodel import LogsPanelViewModel

class LogsPanel(QWidget):
    """
    A simple log viewer panel. Shows all logs, allows text selection in output.
    """
    def __init__(self, viewmodel: LogsPanelViewModel = None):
        super().__init__()
        self.viewmodel = viewmodel or LogsPanelViewModel()
        layout = QVBoxLayout()
        self.list_widget = QListWidget()
        layout.addWidget(self.list_widget)
        self.setLayout(layout)

        # Initial log population
        self._update_logs()

        # Bind property change signal if available
        if hasattr(self.viewmodel, "property_changed"):
            self.viewmodel.property_changed.connect(self._on_property_changed)

    def _update_logs(self):
        self.list_widget.clear()
        logs = self.viewmodel.get_logs()
        if logs is not None:
            for log_entry in logs:
                item = QListWidgetItem(str(log_entry))
                self.list_widget.addItem(item)

    def _on_property_changed(self, property_name):
        if property_name == "logs":
            self._update_logs()