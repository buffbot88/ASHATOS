from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand
from viewmodels.modules_page_viewmodel import ModulesPageViewModel
from viewmodels.monitor_page_viewmodel import MonitorPageViewModel
from viewmodels.settings_panel_viewmodel import SettingsPanelViewModel

class MainWindowViewModel(ObservableObject):
    """
    Main window VM, initializes modular VMs and SpeechPipelineService for agentic input/output.
    """
    def __init__(self, module_manager, speech_pipeline, theme_manager, settings_manager, module_bus):
        super().__init__()
        self.module_manager = module_manager
        self.speech_pipeline = speech_pipeline
        self.theme_manager = theme_manager
        self.settings_manager = settings_manager
        self.module_bus = module_bus

        self.modules_vm = ModulesPageViewModel(module_manager, speech_pipeline, module_bus)
        self.monitor_vm = MonitorPageViewModel(module_manager, speech_pipeline, module_bus)
        self.settings_vm = SettingsPanelViewModel(speech_pipeline, theme_manager, settings_manager)

        self.diagnostics_errors = []
        self.theme = "Light"
        self.supported_languages = ["en", "ja", "de", "fr", "es"]
        self.selected_language = "en"

        self.reload_modules_command = RelayCommand(self.reload_modules)
        self.diagnostics_errors.append(f"Modules loaded: {len(self.modules_vm.core_modules)}")

    def on_theme_changed(self, value):
        self.theme_manager.apply_theme(value)
        self.theme = value
        self.notify_property_changed("theme")

    def reload_modules(self, _=None):
        self.module_manager.reload_modules()
        self.modules_vm.reload()
        self.monitor_vm.reload()
        self.diagnostics_errors.append(f"Modules reloaded, count: {len(self.modules_vm.core_modules)}")
        self.notify_property_changed("diagnostics_errors")
        self.module_bus.publish("Diagnostics", {
            "timestamp": "now",
            "message": f"Modules reloaded, count: {len(self.modules_vm.core_modules)}",
            "severity": "Info",
            "source": "MainWindowViewModel",
            "code": "MODULES_RELOAD",
            "metadata": {"ModulesCount": len(self.modules_vm.core_modules)}
        })