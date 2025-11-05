# Phase 3: Supports custom colors, theming, and plugin extensibility.

class MessageTypeToBrushConverter:
    def __init__(self, theme=None):
        # Default color mapping (can be extended at runtime)
        self.colors = {
            "error": "#FF0000",      # Red
            "info": "#1E90FF",       # Dodger Blue
            "warning": "#FF8C00",    # Dark Orange
            "success": "#008000",    # Green
            "debug": "#9370DB",      # Medium Purple
            "default": "#000000",    # Black
        }
        if theme:
            self.colors.update(theme)

    def convert(self, value):
        # Returns a hex color string for the message type
        t = str(value).lower() if value else "default"
        return self.colors.get(t, self.colors["default"])

    def register_type(self, type_name, color_str):
        # Allows plugins to register a new type/color
        self.colors[type_name.lower()] = color_str