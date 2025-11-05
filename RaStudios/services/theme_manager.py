class ThemeManager:
    """
    Centralized theme manager for RaStudio.
    Handles theme switching, persistence, plugin theme loading.
    """
    def __init__(self, settings_manager):
        self._current_theme = settings_manager.theme
        self.settings_manager = settings_manager

    @property
    def current_theme(self):
        return self._current_theme

    def apply_theme(self, theme):
        if not theme:
            theme = "Light"
        self._current_theme = theme
        # Here, you would trigger the theme change in your UI framework
        self.settings_manager.theme = theme  # Persist to RaServer

    def load_theme_preference(self):
        self.apply_theme(self.settings_manager.theme)