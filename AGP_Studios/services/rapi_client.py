import websocket

class RaCoreClient:
    """
    Handles WebSocket/REST connection to RaCoreServer.
    """
    def __init__(self, url):
        self.url = url
        self.ws = websocket.create_connection(url)

    def send(self, message):
        self.ws.send(message)
        return self.ws.recv()