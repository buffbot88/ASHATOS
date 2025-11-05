from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand

class TestRunnerPanelViewModel(ObservableObject):
    """
    ViewModel for the test runner integration panel.
    Agentic diagnostics and integration via SpeechPipelineService.
    """
    def __init__(self, speech_pipeline, module_bus):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.module_bus = module_bus
        self.test_result = ""
        self.run_integration_suite_command = RelayCommand(self.run_integration_suite)

    def run_integration_suite(self, _=None):
        self.test_result = self.speech_pipeline.send_async("start fast")
        self.module_bus.publish("Diagnostics", {
            "timestamp": "now",
            "message": f"TestRunner executed: {self.test_result}",
            "severity": "Info",
            "source": "TestRunnerPanelViewModel",
            "code": "TEST_RUN",
            "metadata": {"TestResult": self.test_result}
        })
        self.notify_property_changed("test_result")