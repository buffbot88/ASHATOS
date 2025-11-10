from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel
from viewmodels.modules_page_viewmodel import ModulesPageViewModel

class ModulesPanel(QWidget):
    """
    ModulesPanel: Phase 3 ready, displays modular cards, plugin modules, diagnostics.
    """
    def __init__(self, viewmodel: ModulesPageViewModel):
        super().__init__()
        self.viewmodel = viewmodel
        layout = QVBoxLayout()
        layout.addWidget(QLabel("Modules"))
        # TODO: Dynamically add module cards from viewmodel.core_modules
        self.setLayout(layout)