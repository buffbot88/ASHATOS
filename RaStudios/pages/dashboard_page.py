from PyQt6.QtWidgets import QWidget, QVBoxLayout
from panels.dashboard_panel import DashboardPanel
from services import module_bus
from viewmodels.dashboard_panel_viewmodel import DashboardPanelViewModel
from viewmodels.logs_panel_viewmodel import LogsPanelViewModel
from services.speech_pipeline_service import SpeechPipelineService
from services.module_bus import ModuleBus

class DashboardPage(QWidget):
    """
    DashboardPage hosts DashboardPanel and LogsPanel, supports plugin extension cards and agentic context.
    """
    def __init__(self, rcore_client, plugin_panels=None):
        super().__init__()
        layout = QVBoxLayout()

        # Create the speech pipeline and viewmodels
        speech_pipeline = SpeechPipelineService(rcore_client)
        module_bus = ModuleBus()
        dashboard_vm = DashboardPanelViewModel(speech_pipeline)
        logs_vm = LogsPanelViewModel()

        dashboard_panel = DashboardPanel(dashboard_vm, logs_vm)
        layout.addWidget(dashboard_panel)