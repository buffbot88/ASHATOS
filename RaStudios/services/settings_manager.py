class SettingsManager:
    """
    Manages user and application settings for RaStudio.
    Phase 3: persists theme, language, plugin settings, diagnostics, and user/config preferences on the RaServer.
    """
    def __init__(self, server_api):
        self.server_api = server_api

    def get(self, key, default=""):
        return self.server_api.get_setting(key) or default

    def set(self, key, value):
        self.server_api.set_setting(key, value)

    @property
    def theme(self):
        return self.get("UserTheme", "Light")

    @theme.setter
    def theme(self, value):
        self.set("UserTheme", value)

    @property
    def language(self):
        return self.get("UserLanguage", "en")

    @language.setter
    def language(self, value):
        self.set("UserLanguage", value)

    @property
    def theme_options(self):
        return self.server_api.get_setting("ThemeOptions") or ["Auto", "Light", "Dark", "Blue", "SkyBlue"]

    @property
    def language_options(self):
        return self.server_api.get_setting("LanguageOptions") or ["en", "ja", "de", "fr", "es"]

    def get_plugin_setting(self, plugin_name, key, default=""):
        return self.get(f"{plugin_name}_{key}", default)

    def set_plugin_setting(self, plugin_name, key, value):
        self.set(f"{plugin_name}_{key}", value)

    @property
    def agentic_mode_enabled(self):
        return self.get("AgenticModeEnabled", "false") == "true"

    @agentic_mode_enabled.setter
    def agentic_mode_enabled(self, value):
        self.set("AgenticModeEnabled", "true" if value else "false")