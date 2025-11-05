from viewmodels.observable_object import ObservableObject

class DiagnosticsPanelViewModel(ObservableObject):
    """
    DiagnosticsPanelViewModel: subscribes to diagnostics events for agentic, plugin, and extension diagnostics.
    Phase 3 ready: subscribes to diagnostics published from SpeechPipelineService.
    """
    def __init__(self, module_bus):
        super().__init__()
        self.diagnostics = []
        self.module_bus = module_bus
        self.module_bus.subscribe("Diagnostics", self.handle_event)

    def handle_event(self, evt):
        entry = evt.get("payload")
        if entry:
            self.diagnostics.append(entry)
            self.notify_property_changed("diagnostics")

    def add_diagnostic(self, entry):
        self.diagnostics.append(entry)
        self.notify_property_changed("diagnostics")