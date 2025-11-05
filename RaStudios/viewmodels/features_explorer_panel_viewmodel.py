from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand
import re

class FeatureExplorerPanelViewModel(ObservableObject):
    """
    ViewModel for feature/module explorer panel.
    Supports plugin features, manifest metadata, and diagnostics via SpeechPipelineService.
    """
    def __init__(self, speech_pipeline, module_bus):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.module_bus = module_bus
        self.feature_report = "(No report yet)"
        self.modules = []
        self.error_message = ""
        self.reload_command = RelayCommand(self.load_feature_report)
        self.load_feature_report()

    def load_feature_report(self, _=None):
        try:
            self.modules.clear()
            self.feature_report = self.speech_pipeline.send_async("features full")
            lines = self.feature_report.splitlines()
            module_line_regex = re.compile(r"^\s*Module:\s*(\w+)")
            for line in lines:
                match = module_line_regex.match(line)
                if match:
                    self.modules.append(match.group(1))
            self.error_message = ""
        except Exception as ex:
            self.feature_report = "(Error loading report)"
            self.error_message = f"Error: {type(ex).__name__}: {ex}"
            self.module_bus.publish("Diagnostics", {
                "timestamp": "now",
                "message": self.error_message,
                "severity": "Error",
                "source": "FeatureExplorerPanelViewModel",
                "code": "FEATURE_REPORT_ERROR",
                "metadata": {"FeatureReport": self.feature_report}
            })
        self.notify_property_changed("feature_report")
        self.notify_property_changed("modules")
        self.notify_property_changed("error_message")