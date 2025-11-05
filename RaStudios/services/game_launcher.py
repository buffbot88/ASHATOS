"""
Game launcher service for playing RaOS games.
Handles game discovery, authentication, downloading/streaming, and launching games.
"""
import json
from typing import List, Dict, Optional

class GameLauncher:
    """
    Manages game player functionality for RaOS.
    Supports game discovery, launching, profile management, achievements, and leaderboards.
    """
    
    def __init__(self, rcore_client, auth_service):
        """
        Initialize GameLauncher.
        
        Args:
            rcore_client: RaCoreClient instance for server communication
            auth_service: AuthService instance for authenticated requests
        """
        self.rcore_client = rcore_client
        self.auth_service = auth_service
        self.current_game: Optional[Dict] = None
        self.player_profile: Optional[Dict] = None
        
    def get_available_games(self) -> List[Dict]:
        """
        Get list of available games from RaOS server.
        
        Returns:
            List[Dict]: List of game metadata (id, name, description, etc.)
        """
        if not self.auth_service.is_authenticated():
            return []
            
        try:
            request = json.dumps({
                "action": "list_games",
                "auth_token": self.auth_service.access_token
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("games", [])
            return []
            
        except Exception as e:
            print(f"Error fetching games: {e}")
            return []
    
    def launch_game(self, game_id: str, mode: str = "stream") -> bool:
        """
        Launch a game from RaOS server.
        
        Args:
            game_id: ID of the game to launch
            mode: Launch mode - 'stream' for streaming, 'download' for local play
            
        Returns:
            bool: True if game launch successful
        """
        if not self.auth_service.is_player():
            print("Error: Player role required to launch games")
            return False
            
        try:
            request = json.dumps({
                "action": "launch_game",
                "auth_token": self.auth_service.access_token,
                "game_id": game_id,
                "mode": mode
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                self.current_game = {
                    "game_id": game_id,
                    "session_id": data.get("session_id"),
                    "stream_url": data.get("stream_url"),
                    "mode": mode
                }
                return True
                
            print(f"Game launch failed: {data.get('error', 'Unknown error')}")
            return False
            
        except Exception as e:
            print(f"Error launching game: {e}")
            return False
    
    def stop_game(self) -> bool:
        """
        Stop currently running game.
        
        Returns:
            bool: True if game stopped successfully
        """
        if not self.current_game:
            return False
            
        try:
            request = json.dumps({
                "action": "stop_game",
                "auth_token": self.auth_service.access_token,
                "session_id": self.current_game.get("session_id")
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                self.current_game = None
                return True
            return False
            
        except Exception as e:
            print(f"Error stopping game: {e}")
            return False
    
    def get_player_profile(self) -> Optional[Dict]:
        """
        Get player profile from RaOS server.
        
        Returns:
            Dict: Player profile data (username, level, stats, etc.)
        """
        if not self.auth_service.is_authenticated():
            return None
            
        try:
            request = json.dumps({
                "action": "get_player_profile",
                "auth_token": self.auth_service.access_token
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                self.player_profile = data.get("profile", {})
                return self.player_profile
            return None
            
        except Exception as e:
            print(f"Error fetching player profile: {e}")
            return None
    
    def get_achievements(self, game_id: Optional[str] = None) -> List[Dict]:
        """
        Get player achievements from RaOS server.
        
        Args:
            game_id: Optional game ID to filter achievements
            
        Returns:
            List[Dict]: List of achievements
        """
        if not self.auth_service.is_authenticated():
            return []
            
        try:
            request = json.dumps({
                "action": "get_achievements",
                "auth_token": self.auth_service.access_token,
                "game_id": game_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("achievements", [])
            return []
            
        except Exception as e:
            print(f"Error fetching achievements: {e}")
            return []
    
    def get_leaderboard(self, game_id: str, category: str = "global") -> List[Dict]:
        """
        Get leaderboard data from RaOS server.
        
        Args:
            game_id: Game ID for leaderboard
            category: Leaderboard category (global, friends, regional, etc.)
            
        Returns:
            List[Dict]: Leaderboard entries
        """
        if not self.auth_service.is_authenticated():
            return []
            
        try:
            request = json.dumps({
                "action": "get_leaderboard",
                "auth_token": self.auth_service.access_token,
                "game_id": game_id,
                "category": category
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("leaderboard", [])
            return []
            
        except Exception as e:
            print(f"Error fetching leaderboard: {e}")
            return []
    
    def download_game(self, game_id: str, destination_path: str) -> bool:
        """
        Download game for offline play.
        
        Args:
            game_id: ID of game to download
            destination_path: Local path to save game
            
        Returns:
            bool: True if download successful
        """
        if not self.auth_service.is_player():
            return False
            
        try:
            request = json.dumps({
                "action": "download_game",
                "auth_token": self.auth_service.access_token,
                "game_id": game_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                # In a real implementation, this would handle chunked download
                download_url = data.get("download_url")
                # TODO: Implement actual file download logic
                print(f"Game download URL: {download_url}")
                return True
                
            return False
            
        except Exception as e:
            print(f"Error downloading game: {e}")
            return False
