from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand

class ModulePanelViewModel(ObservableObject):
    """
    ViewModel for a single module card (used in Modules tab).
    Supports plugin modules, agentic diagnostics, manifest extension, and command via SpeechPipelineService.
    """
    def __init__(self, module_wrapper, module_manager, speech_pipeline, module_bus):
        super().__init__()
        self.module = module_wrapper
        self.module_manager = module_manager
        self.speech_pipeline = speech_pipeline
        self.module_bus = module_bus

        self.status = ""
        self.help = ""
        self.logs = []
        self.command_input = ""
        self.command_result = ""
        self.error_message = ""
        self.run_command = RelayCommand(self.run_custom_command)

        self.load_properties()

    def load_properties(self):
        mod = getattr(self.module, "module_instance", None)
        if mod:
            try:
                self.status = mod.process("status")
            except Exception as ex:
                self.status = f"ERROR: status ({type(ex).__name__}: {ex})"

            try:
                self.help = mod.process("help")
            except Exception as ex:
                self.help = f"ERROR: help ({type(ex).__name__}: {ex})"

            try:
                logs_prop = getattr(mod, "logs", None)
                if logs_prop is not None:
                    self.logs = list(logs_prop)
            except Exception as ex:
                self.logs = [f"ERROR loading logs: {type(ex).__name__}: {ex}"]

            self.error_message = ""
        else:
            self.status = "(module not loaded)"
            self.help = "(module not loaded)"
            self.logs = ["(module not loaded)"]
            self.error_message = "Module instance is null (not loaded)"

        self.notify_property_changed("status")
        self.notify_property_changed("help")
        self.notify_property_changed("logs")
        self.notify_property_changed("error_message")

    def run_custom_command(self, _=None):
        if not self.command_input.strip():
            return
        try:
            self.command_result = self.speech_pipeline.send_async(self.command_input)
            self.error_message = ""
        except Exception as ex:
            self.command_result = "(command error)"
            self.error_message = f"Error running command: {type(ex).__name__}: {ex}"
            self.module_bus.publish("Diagnostics", {
                "timestamp": "now",
                "message": self.error_message,
                "severity": "Error",
                "source": "ModulePanelViewModel",
                "code": "MODULE_COMMAND_ERROR",
                "metadata": {"CommandInput": self.command_input}
            })
        self.notify_property_changed("command_result")
        self.notify_property_changed("error_message")