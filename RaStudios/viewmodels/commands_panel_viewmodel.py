from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand

class CommandsPanelViewModel(ObservableObject):
    """
    ViewModel for the Commands card in Monitoring.
    Wired to SpeechPipelineService for async agentic command execution.
    """
    def __init__(self, speech_pipeline, module_bus):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.module_bus = module_bus

        self.command_input = ""
        self.command_result = ""
        self.error_message = ""

        self.send_command = RelayCommand(self.execute)

    def execute(self, _=None):
        if not self.command_input.strip():
            self.error_message = "Command cannot be empty."
            self.command_result = ""
        else:
            try:
                self.command_result = self.speech_pipeline.send_async(self.command_input)
                self.error_message = ""
            except Exception as ex:
                self.error_message = f"Error: {type(ex).__name__}: {ex}"
                self.command_result = ""
                self.module_bus.publish("Diagnostics", {
                    "timestamp": "now",
                    "message": self.error_message,
                    "severity": "Error",
                    "source": "CommandsPanelViewModel",
                    "code": "COMMAND_ERROR",
                    "metadata": {"CommandInput": self.command_input}
                })
        self.notify_property_changed("command_result")
        self.notify_property_changed("error_message")