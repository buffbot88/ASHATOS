class PluginRegistry:
    """
    Phase 3 ready: Registry for plugin panels, enables manifest-based discovery, agentic context, and metadata.
    """
    _registered_panels = []

    @classmethod
    def register_panel(cls, panel, metadata=None):
        if panel not in cls._registered_panels:
            panel.metadata = metadata
            cls._registered_panels.append(panel)

    @classmethod
    def registered_panels(cls):
        return cls._registered_panels

    @classmethod
    def find_panel_by_metadata(cls, metadata):
        for panel in cls._registered_panels:
            if panel.metadata == metadata:
                return panel
        return None

    @classmethod
    def find_panels(cls, match):
        return [panel for panel in cls._registered_panels if match(panel.metadata)]