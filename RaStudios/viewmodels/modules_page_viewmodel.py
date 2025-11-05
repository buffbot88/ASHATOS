from viewmodels.observable_object import ObservableObject

class ModulesPageViewModel(ObservableObject):
    """
    ViewModel for ModulesPage: manages modules list and extension interactions.
    """
    def __init__(self, module_manager):
        super().__init__()
        self.module_manager = module_manager
        self.modules = self.module_manager.get_modules()

    def reload_modules(self):
        self.module_manager.reload_modules()
        self.modules = self.module_manager.get_modules()
        self.notify_property_changed("modules")

    def get_modules(self):
        return self.modules