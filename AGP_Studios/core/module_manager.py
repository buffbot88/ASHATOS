class ModuleManager:
    """
    Manages modules/plugins for RaStudio.py.
    Communicates through SpeechPipelineService for diagnostics/logging and command execution.
    """
    def __init__(self, speech_pipeline):
        self.speech_pipeline = speech_pipeline

    def get_modules(self):
        # Ask RaCoreServer for module list
        return self.speech_pipeline.send_async("get_modules")

    def reload_modules(self):
        # Ask RaCoreServer to reload modules
        return self.speech_pipeline.send_async("reload_modules")

    def get_diagnostics(self):
        # Ask RaCoreServer for diagnostics
        return self.speech_pipeline.send_async("diagnostics")

    def log_event(self, event):
        # Log event to RaCoreServer
        return self.speech_pipeline.send_async(f"log_event:{event}")