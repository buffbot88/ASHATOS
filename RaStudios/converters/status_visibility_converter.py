from converters.visibility_converter import VisibilityConverter

class StatusVisibilityConverter(VisibilityConverter):
    def __init__(self):
        super().__init__("Status")