from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand

class DashboardPanelViewModel(ObservableObject):
    """
    ViewModel for DashboardPanel, wired to SpeechPipelineService for agentic input/output.
    """
    def __init__(self, speech_pipeline):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.user_input = ""
        self.log_output = ""
        self.send_user_input_command = RelayCommand(self.submit_input)

    def submit_input(self, _=None):
        if self.user_input.strip():
            response = self.speech_pipeline.send_async(self.user_input)
            self.log_output += f"You: {self.user_input}\nRaCore: {response}\n"
            self.user_input = ""
            self.notify_property_changed("log_output")
            self.notify_property_changed("user_input")