# Generic visibility converter for any category.
# Usage: VisibilityConverter(target="Commands").convert(value)

class VisibilityConverter:
    def __init__(self, target):
        self.target = target

    def convert(self, value):
        # Returns True (visible) or False (hidden)
        return str(value).lower() == self.target.lower()