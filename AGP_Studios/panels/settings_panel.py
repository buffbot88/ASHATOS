from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel
from viewmodels.settings_panel_viewmodel import SettingsPanelViewModel

class SettingsPanel(QWidget):
    """
    SettingsPanel: Phase 3 ready, theme/language/agentic mode, plugin/extensible settings.
    """
    def __init__(self, viewmodel: SettingsPanelViewModel):
        super().__init__()
        self.viewmodel = viewmodel
        layout = QVBoxLayout()
        layout.addWidget(QLabel("Settings"))
        # TODO: Add controls for theme/language/agentic mode, bound to viewmodel
        self.setLayout(layout)