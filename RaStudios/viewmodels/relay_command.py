class RelayCommand:
    def __init__(self, execute):
        self.execute = execute

    def __call__(self, *args, **kwargs):
        return self.execute(*args, **kwargs)