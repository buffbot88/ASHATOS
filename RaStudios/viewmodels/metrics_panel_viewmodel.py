from viewmodels.observable_object import ObservableObject

class MetricsPanelViewModel(ObservableObject):
    """
    ViewModel for the Metrics card in Monitoring.
    Uses SpeechPipelineService for agentic metrics.
    """
    def __init__(self, speech_pipeline, module_manager, module_bus):
        super().__init__()
        self.speech_pipeline = speech_pipeline
        self.module_manager = module_manager
        self.module_bus = module_bus

        self.cpu_usage = 0.0
        self.ram_usage = 0.0
        self.thread_count = 0
        self.active_module_count = 0

        self.load_metrics()

    def load_metrics(self):
        # Try to get metrics via pipeline (SpeechModule)
        try:
            cpu = self.speech_pipeline.send_async("cpu")
            self.cpu_usage = float(cpu) if cpu else self._get_process_cpu_usage()

            ram = self.speech_pipeline.send_async("ram")
            self.ram_usage = float(ram) if ram else self._get_process_ram_usage_mb()

            threads = self.speech_pipeline.send_async("threads")
            self.thread_count = int(threads) if threads else self._get_thread_count()

            self.active_module_count = len(self.module_manager.core_modules)

            self.module_bus.publish("Diagnostics", {
                "timestamp": "now",
                "message": f"Metrics loaded: CPU={self.cpu_usage} RAM={self.ram_usage} Threads={self.thread_count}",
                "severity": "Info",
                "source": "MetricsPanelViewModel",
                "code": "METRICS_LOAD",
                "metadata": {
                    "CpuUsage": self.cpu_usage,
                    "RamUsage": self.ram_usage,
                    "ThreadCount": self.thread_count,
                    "ActiveModuleCount": self.active_module_count
                }
            })
        except Exception:
            # TODO: handle/report error
            pass

        self.notify_property_changed("cpu_usage")
        self.notify_property_changed("ram_usage")
        self.notify_property_changed("thread_count")
        self.notify_property_changed("active_module_count")

    def _get_process_cpu_usage(self):
        return 0.0  # TODO: platform-specific CPU usage

    def _get_process_ram_usage_mb(self):
        import psutil
        return psutil.Process().memory_info().rss / (1024.0 * 1024.0)

    def _get_thread_count(self):
        import threading
        return len(threading.enumerate())