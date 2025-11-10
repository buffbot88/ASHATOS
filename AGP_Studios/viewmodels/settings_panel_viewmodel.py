from viewmodels.observable_object import ObservableObject
from viewmodels.relay_command import RelayCommand

class SettingsPanelViewModel(ObservableObject):
    """
    SettingsPanelViewModel for theme, language, agentic mode, and plugin settings.
    Wired to SpeechPipelineService for backend agentic integration.
    """
    def __init__(self, speech_pipeline, theme_manager, settings_manager):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.theme_manager = theme_manager
        self.settings_manager = settings_manager

        self.theme_options = settings_manager.theme_options
        self.selected_theme = settings_manager.theme

        self.language_options = settings_manager.language_options
        self.selected_language = settings_manager.language

        self.agentic_mode_enabled = settings_manager.agentic_mode_enabled

        self.plugin_settings = []  # List of PluginSettingViewModel

        self.apply_theme_command = RelayCommand(self.apply_theme)
        self.apply_language_command = RelayCommand(self.apply_language)
        self.apply_agentic_mode_command = RelayCommand(self.apply_agentic_mode)

    def apply_theme(self, _=None):
        self.theme_manager.apply_theme(self.selected_theme)
        self.settings_manager.theme = self.selected_theme
        self.speech_pipeline.send_async(f"theme set {self.selected_theme}")

    def apply_language(self, _=None):
        self.settings_manager.language = self.selected_language
        self.speech_pipeline.send_async(f"language set {self.selected_language}")

    def apply_agentic_mode(self, _=None):
        self.settings_manager.agentic_mode_enabled = self.agentic_mode_enabled
        self.speech_pipeline.send_async(f"agentic mode {'enable' if self.agentic_mode_enabled else 'disable'}")

class PluginSettingViewModel(ObservableObject):
    def __init__(self, name=None, value=None):
        super().__init__()
        self.name = name
        self.value = value