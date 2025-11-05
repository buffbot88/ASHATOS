from PyQt6.QtCore import QObject, pyqtSignal

class ObservableObject(QObject):
    property_changed = pyqtSignal(str)

    def __init__(self):
        super().__init__()

    def notify_property_changed(self, property_name):
        self.property_changed.emit(property_name)