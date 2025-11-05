import importlib
import os

class PanelLoader:
    """
    Phase 3 ready: discovers and loads built-in and plugin panels, supports manifest/metadata extension and agentic context.
    """
    @staticmethod
    def load_panels():
        panels = [
            # You would instantiate these with their viewmodels
            # DashboardPanel(dashboard_vm), ModulesPanel(modules_vm), etc.
        ]

        plugin_dir = os.path.join(os.getcwd(), "plugins")
        if os.path.exists(plugin_dir):
            for fname in os.listdir(plugin_dir):
                if fname.endswith(".py"):
                    try:
                        mod = importlib.import_module(f"plugins.{fname[:-3]}")
                        if hasattr(mod, "register_panel"):
                            panel = mod.register_panel()
                            panels.append(panel)
                    except Exception:
                        pass  # Ignore bad plugins for now

        # Future: load panels via manifest metadata for advanced discovery

        return panels