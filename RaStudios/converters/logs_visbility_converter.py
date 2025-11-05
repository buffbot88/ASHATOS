from converters.visibility_converter import VisibilityConverter

class LogsVisibilityConverter(VisibilityConverter):
    def __init__(self):
        super().__init__("Logs")