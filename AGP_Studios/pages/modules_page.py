from PyQt6.QtWidgets import QWidget, QVBoxLayout
from panels.modules_panel import ModulesPanel
from viewmodels.modules_page_viewmodel import ModulesPageViewModel

class ModulesPage(QWidget):
    """
    Phase 3 ready: ModulesPage hosts the ModulesPanel, supports plugin module cards and extension points.
    """
    def __init__(self, modules_vm: ModulesPageViewModel):
        super().__init__()
        layout = QVBoxLayout()
        modules_panel = ModulesPanel(modules_vm)
        layout.addWidget(modules_panel)
        self.setLayout(layout)
        # TODO: Inject plugin module cards, extension panels, etc.