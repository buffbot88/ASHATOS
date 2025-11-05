from viewmodels.observable_object import ObservableObject

class StatusPanelViewModel(ObservableObject):
    """
    ViewModel for the Status card in Monitoring.
    Uses SpeechPipelineService for agentic status and uptime retrieval.
    """
    def __init__(self, speech_pipeline, module_bus):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.module_bus = module_bus
        self.status = ""
        self.uptime = ""
        self.load_status()

    def load_status(self):
        self.status = self.speech_pipeline.send_async("status")
        self.uptime = self.speech_pipeline.send_async("uptime")
        self.module_bus.publish("Diagnostics", {
            "timestamp": "now",
            "message": f"Status loaded: {self.status}, Uptime: {self.uptime}",
            "severity": "Info",
            "source": "StatusPanelViewModel",
            "code": "STATUS_LOAD",
            "metadata": {"Status": self.status, "Uptime": self.uptime}
        })
        self.notify_property_changed("status")
        self.notify_property_changed("uptime")