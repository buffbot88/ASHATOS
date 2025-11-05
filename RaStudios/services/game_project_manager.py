"""
Game project manager for RaOS game development.
Handles game project creation, loading, asset management, and synchronization with RaOS server.
"""
import json
from typing import List, Dict, Optional
from datetime import datetime

class GameProject:
    """Represents a game project in RaOS."""
    
    def __init__(self, project_id: str, name: str, description: str = ""):
        self.project_id = project_id
        self.name = name
        self.description = description
        self.created_date = datetime.now()
        self.modified_date = datetime.now()
        self.assets: List[Dict] = []
        self.scenes: List[Dict] = []
        self.scripts: List[Dict] = []
        
    def to_dict(self) -> Dict:
        """Convert project to dictionary for serialization."""
        return {
            "project_id": self.project_id,
            "name": self.name,
            "description": self.description,
            "created_date": self.created_date.isoformat(),
            "modified_date": self.modified_date.isoformat(),
            "assets": self.assets,
            "scenes": self.scenes,
            "scripts": self.scripts
        }

class GameProjectManager:
    """
    Manages game projects for the LegendaryGameEngine.
    Provides IDE functionality for game development with RaOS server integration.
    """
    
    def __init__(self, rcore_client, auth_service):
        """
        Initialize GameProjectManager.
        
        Args:
            rcore_client: RaCoreClient instance for server communication
            auth_service: AuthService instance for authenticated requests
        """
        self.rcore_client = rcore_client
        self.auth_service = auth_service
        self.current_project: Optional[GameProject] = None
        
    def create_project(self, name: str, description: str = "") -> Optional[GameProject]:
        """
        Create a new game project on RaOS server.
        
        Args:
            name: Project name
            description: Project description
            
        Returns:
            GameProject: Created project, or None if creation failed
        """
        if not self.auth_service.is_developer():
            print("Error: Developer role required to create projects")
            return None
            
        try:
            request = json.dumps({
                "action": "create_game_project",
                "auth_token": self.auth_service.access_token,
                "name": name,
                "description": description
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                project_id = data.get("project_id")
                project = GameProject(project_id, name, description)
                self.current_project = project
                return project
                
            print(f"Project creation failed: {data.get('error', 'Unknown error')}")
            return None
            
        except Exception as e:
            print(f"Error creating project: {e}")
            return None
    
    def load_project(self, project_id: str) -> Optional[GameProject]:
        """
        Load existing game project from RaOS server.
        
        Args:
            project_id: Project ID to load
            
        Returns:
            GameProject: Loaded project, or None if load failed
        """
        if not self.auth_service.is_authenticated():
            print("Error: Authentication required")
            return None
            
        try:
            request = json.dumps({
                "action": "load_game_project",
                "auth_token": self.auth_service.access_token,
                "project_id": project_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                project_data = data.get("project", {})
                project = GameProject(
                    project_data.get("project_id"),
                    project_data.get("name"),
                    project_data.get("description", "")
                )
                project.assets = project_data.get("assets", [])
                project.scenes = project_data.get("scenes", [])
                project.scripts = project_data.get("scripts", [])
                self.current_project = project
                return project
                
            print(f"Project load failed: {data.get('error', 'Unknown error')}")
            return None
            
        except Exception as e:
            print(f"Error loading project: {e}")
            return None
    
    def save_project(self) -> bool:
        """
        Save current project to RaOS server.
        
        Returns:
            bool: True if save successful
        """
        if not self.current_project:
            print("Error: No project loaded")
            return False
            
        if not self.auth_service.is_developer():
            print("Error: Developer role required to save projects")
            return False
            
        try:
            self.current_project.modified_date = datetime.now()
            
            request = json.dumps({
                "action": "save_game_project",
                "auth_token": self.auth_service.access_token,
                "project": self.current_project.to_dict()
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            return data.get("success", False)
            
        except Exception as e:
            print(f"Error saving project: {e}")
            return False
    
    def list_projects(self) -> List[Dict]:
        """
        Get list of available game projects from RaOS server.
        
        Returns:
            List[Dict]: List of project metadata
        """
        if not self.auth_service.is_authenticated():
            return []
            
        try:
            request = json.dumps({
                "action": "list_game_projects",
                "auth_token": self.auth_service.access_token
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("projects", [])
            return []
            
        except Exception as e:
            print(f"Error listing projects: {e}")
            return []
    
    def sync_assets(self) -> bool:
        """
        Synchronize project assets with RaOS server.
        
        Returns:
            bool: True if sync successful
        """
        if not self.current_project:
            return False
            
        try:
            request = json.dumps({
                "action": "sync_assets",
                "auth_token": self.auth_service.access_token,
                "project_id": self.current_project.project_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                self.current_project.assets = data.get("assets", [])
                return True
            return False
            
        except Exception as e:
            print(f"Error syncing assets: {e}")
            return False
    
    def add_asset(self, asset_name: str, asset_type: str, asset_data: bytes) -> bool:
        """
        Add asset to current project and upload to RaOS server.
        
        Args:
            asset_name: Name of the asset
            asset_type: Type of asset (image, audio, model, etc.)
            asset_data: Binary asset data
            
        Returns:
            bool: True if asset added successfully
        """
        if not self.current_project or not self.auth_service.is_developer():
            return False
            
        try:
            import base64
            asset_data_b64 = base64.b64encode(asset_data).decode('utf-8')
            
            request = json.dumps({
                "action": "add_asset",
                "auth_token": self.auth_service.access_token,
                "project_id": self.current_project.project_id,
                "asset_name": asset_name,
                "asset_type": asset_type,
                "asset_data": asset_data_b64
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                asset_info = {
                    "name": asset_name,
                    "type": asset_type,
                    "asset_id": data.get("asset_id"),
                    "url": data.get("asset_url")
                }
                self.current_project.assets.append(asset_info)
                return True
            return False
            
        except Exception as e:
            print(f"Error adding asset: {e}")
            return False
