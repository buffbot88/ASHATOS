from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel
from panels.pluginpanels.iplugin_panel import IPluginPanel

class HelloWorldPluginPanel(QWidget, IPluginPanel):
    """
    Example plugin panel for HelloWorldDemo. Phase 3 ready: agentic actions, diagnostics, extension UI.
    """
    def __init__(self):
        QWidget.__init__(self)
        IPluginPanel.__init__(self)
        layout = QVBoxLayout()
        layout.addWidget(QLabel("Hello World Plugin Panel"))
        self.setLayout(layout)

    def get_diagnostics(self):
        # Example: Return diagnostic info or plugin status
        return "No issues detected."

    def initialize(self, manifest_metadata=None):
        self.metadata = manifest_metadata