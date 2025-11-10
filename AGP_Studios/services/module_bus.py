import datetime

class ModuleEvent:
    def __init__(self, name, payload=None, sender=None, diagnostics_info=None):
        self.name = name
        self.payload = payload
        self.sender = sender
        self.diagnostics_info = diagnostics_info
        self.timestamp = datetime.datetime.utcnow()

class ModuleBus:
    """
    Event bus for UI <-> RaCore module communication and extension wiring.
    Phase 3: supports agentic events, plugin hooks, diagnostics, async panel/module notifications, and plugin context.
    """
    _listeners = {}

    @staticmethod
    def subscribe(event_name, handler):
        if event_name not in ModuleBus._listeners:
            ModuleBus._listeners[event_name] = []
        ModuleBus._listeners[event_name].append(handler)

    @staticmethod
    def unsubscribe(event_name, handler):
        if event_name in ModuleBus._listeners:
            ModuleBus._listeners[event_name].remove(handler)

    @staticmethod
    def publish(event_name, payload=None, sender=None, diagnostics_info=None):
        event = ModuleEvent(event_name, payload, sender, diagnostics_info)
        if event_name in ModuleBus._listeners:
            for handler in ModuleBus._listeners[event_name]:
                handler(event)
        # Optionally, publish to RaServer for global diagnostics/events

    @staticmethod
    async def publish_async(event_name, payload=None, sender=None, diagnostics_info=None):
        event = ModuleEvent(event_name, payload, sender, diagnostics_info)
        if event_name in ModuleBus._listeners:
            for handler in ModuleBus._listeners[event_name]:
                await handler(event)

    @staticmethod
    def reset():
        ModuleBus._listeners.clear()