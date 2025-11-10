class IPluginPanel:
    """
    Defines the base interface for plugin panels in Phase 3 agentic UI.
    Supports initialization, diagnostics context, and manifest metadata.
    """
    def __init__(self):
        self.metadata = None

    def initialize(self, manifest_metadata=None):
        """
        Initialize the plugin panel with manifest metadata or agentic context.
        """
        self.metadata = manifest_metadata

    def get_diagnostics(self):
        """
        Optional: Provide diagnostics or plugin status.
        """
        return None