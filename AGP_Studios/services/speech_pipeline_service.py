class SpeechPipelineService:
    """
    Handles agentic communication with RaCoreServer via RaCoreClient.
    """
    def __init__(self, rcore_client):
        self.rcore_client = rcore_client

    def send_async(self, message):
        # Forward message to RaCoreServer
        return self.rcore_client.send(message)