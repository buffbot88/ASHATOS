from viewmodels.observable_object import ObservableObject

class MonitorPageViewModel(ObservableObject):
    """
    ViewModel for MonitorPage: handles diagnostics and monitoring via ModuleManager.
    """
    def __init__(self, module_manager):
        super().__init__()
        self.module_manager = module_manager
        self.diagnostics = self.module_manager.get_diagnostics()

    def refresh_diagnostics(self):
        self.diagnostics = self.module_manager.get_diagnostics()
        self.notify_property_changed("diagnostics")

    def get_diagnostics(self):
        return self.diagnostics