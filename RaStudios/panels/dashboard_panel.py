from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLineEdit, QPushButton, QTextEdit, QLabel
from viewmodels.dashboard_panel_viewmodel import DashboardPanelViewModel
from panels.logs_panel import LogsPanel

class DashboardPanel(QWidget):
    """
    DashboardPanel: Phase 3 ready, agentic input/output, diagnostics, plugin extension.
    Shows Dashboard input/output and logs panel.
    """
    def __init__(self, viewmodel: DashboardPanelViewModel, logs_viewmodel=None):
        super().__init__()
        self.viewmodel = viewmodel
        self.logs_panel = LogsPanel(logs_viewmodel) if logs_viewmodel else LogsPanel()

        layout = QVBoxLayout()

        self.input_box = QLineEdit()
        self.input_box.setPlaceholderText("Enter agentic command...")
        self.input_box.textChanged.connect(lambda text: setattr(self.viewmodel, 'user_input', text))

        self.send_button = QPushButton("Send")
        self.send_button.clicked.connect(lambda: self._handle_send())

        self.output_block = QTextEdit()
        self.output_block.setReadOnly(True)
        self.output_block.setPlainText(self.viewmodel.log_output)

        layout.addWidget(self.input_box)
        layout.addWidget(self.send_button)
        layout.addWidget(self.output_block)
        layout.addWidget(QLabel("Logs"))
        layout.addWidget(self.logs_panel)

        self.setLayout(layout)

        if hasattr(self.viewmodel, "property_changed"):
            self.viewmodel.property_changed.connect(self._on_property_changed)

    def _handle_send(self):
        self.viewmodel.submit_input()
        self.output_block.setPlainText(self.viewmodel.log_output)
        self.input_box.clear()

    def _on_property_changed(self, property_name):
        if property_name == "log_output":
            self.output_block.setPlainText(self.viewmodel.log_output)
        elif property_name == "user_input":
            self.input_box.setText(self.viewmodel.user_input)