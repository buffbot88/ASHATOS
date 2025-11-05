from PyQt6.QtWidgets import QWidget, QVBoxLayout
from panels.monitor_panel import MonitorPanel
from viewmodels.monitor_page_viewmodel import MonitorPageViewModel

class MonitorPage(QWidget):
    """
    Phase 3 ready: MonitorPage hosts the MonitorPanel, supports plugin metrics/logs/diagnostics, agentic extension.
    """
    def __init__(self, monitor_vm: MonitorPageViewModel):
        super().__init__()
        layout = QVBoxLayout()
        monitor_panel = MonitorPanel(monitor_vm)
        layout.addWidget(monitor_panel)
        self.setLayout(layout)
        # TODO: Inject plugin metric/log panels, diagnostics, agentic extension cards, etc.