from ui.main_window import start_ui
from services.rapi_client import RaCoreClient

def main():
    # Initialize RaCore client (WebSocket)
    rcore = RaCoreClient("ws://localhost:7077/ws")
    # Start the Python UI and pass RaCore client for live comms
    start_ui(rcore)

if __name__ == "__main__":
    main()