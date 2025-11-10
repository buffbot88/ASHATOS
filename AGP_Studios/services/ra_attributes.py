def ra_studio_plugin(name, description=None, version=None, category=None, metadata=None):
    def decorator(cls):
        cls._ra_plugin_info = {
            "name": name,
            "description": description,
            "version": version,
            "category": category,
            "metadata": metadata
        }
        return cls
    return decorator

def ra_studio_extension_point(extension_type, description=None, metadata=None):
    def decorator(cls):
        cls._ra_extension_info = {
            "extension_type": extension_type,
            "description": description,
            "metadata": metadata
        }
        return cls
    return decorator

def ra_studio_module(module_name, category=None, description=None, metadata=None):
    def decorator(cls):
        cls._ra_module_info = {
            "module_name": module_name,
            "category": category,
            "description": description,
            "metadata": metadata
        }
        return cls
    return decorator