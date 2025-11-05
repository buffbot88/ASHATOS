"""
Content manager for RaOS content processing.
Handles fetching, editing, and uploading RaOS content assets (blogs, posts, images, etc.).
"""
import json
from typing import List, Dict, Optional
from datetime import datetime

class ContentAsset:
    """Represents a content asset in RaOS."""
    
    def __init__(self, asset_id: str, asset_type: str, title: str, content: str = ""):
        self.asset_id = asset_id
        self.asset_type = asset_type  # blog, post, image, video, code, etc.
        self.title = title
        self.content = content
        self.metadata: Dict = {}
        self.created_date = datetime.now()
        self.modified_date = datetime.now()
        
    def to_dict(self) -> Dict:
        """Convert asset to dictionary for serialization."""
        return {
            "asset_id": self.asset_id,
            "asset_type": self.asset_type,
            "title": self.title,
            "content": self.content,
            "metadata": self.metadata,
            "created_date": self.created_date.isoformat(),
            "modified_date": self.modified_date.isoformat()
        }

class ContentManager:
    """
    Manages RaOS content assets.
    Provides content fetching, editing, uploading, and asset pipeline integration.
    """
    
    def __init__(self, rcore_client, auth_service):
        """
        Initialize ContentManager.
        
        Args:
            rcore_client: RaCoreClient instance for server communication
            auth_service: AuthService instance for authenticated requests
        """
        self.rcore_client = rcore_client
        self.auth_service = auth_service
        self.current_asset: Optional[ContentAsset] = None
        
    def fetch_content(self, asset_id: str) -> Optional[ContentAsset]:
        """
        Fetch content asset from RaOS server.
        
        Args:
            asset_id: ID of asset to fetch
            
        Returns:
            ContentAsset: Fetched asset, or None if fetch failed
        """
        if not self.auth_service.is_authenticated():
            print("Error: Authentication required")
            return None
            
        try:
            request = json.dumps({
                "action": "fetch_content",
                "auth_token": self.auth_service.access_token,
                "asset_id": asset_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                asset_data = data.get("asset", {})
                asset = ContentAsset(
                    asset_data.get("asset_id"),
                    asset_data.get("asset_type"),
                    asset_data.get("title"),
                    asset_data.get("content", "")
                )
                asset.metadata = asset_data.get("metadata", {})
                self.current_asset = asset
                return asset
                
            print(f"Content fetch failed: {data.get('error', 'Unknown error')}")
            return None
            
        except Exception as e:
            print(f"Error fetching content: {e}")
            return None
    
    def list_content(self, content_type: Optional[str] = None) -> List[Dict]:
        """
        List available content from RaOS server.
        
        Args:
            content_type: Optional filter by content type
            
        Returns:
            List[Dict]: List of content metadata
        """
        if not self.auth_service.is_authenticated():
            return []
            
        try:
            request = json.dumps({
                "action": "list_content",
                "auth_token": self.auth_service.access_token,
                "content_type": content_type
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("content_list", [])
            return []
            
        except Exception as e:
            print(f"Error listing content: {e}")
            return []
    
    def create_content(self, asset_type: str, title: str, content: str = "") -> Optional[ContentAsset]:
        """
        Create new content asset.
        
        Args:
            asset_type: Type of content (blog, post, image, etc.)
            title: Content title
            content: Content body
            
        Returns:
            ContentAsset: Created asset, or None if creation failed
        """
        if not self.auth_service.is_authenticated():
            return None
            
        try:
            request = json.dumps({
                "action": "create_content",
                "auth_token": self.auth_service.access_token,
                "asset_type": asset_type,
                "title": title,
                "content": content
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                asset_id = data.get("asset_id")
                asset = ContentAsset(asset_id, asset_type, title, content)
                self.current_asset = asset
                return asset
                
            print(f"Content creation failed: {data.get('error', 'Unknown error')}")
            return None
            
        except Exception as e:
            print(f"Error creating content: {e}")
            return None
    
    def update_content(self, asset: ContentAsset) -> bool:
        """
        Update content asset on RaOS server.
        
        Args:
            asset: ContentAsset to update
            
        Returns:
            bool: True if update successful
        """
        if not self.auth_service.is_authenticated():
            return False
            
        try:
            asset.modified_date = datetime.now()
            
            request = json.dumps({
                "action": "update_content",
                "auth_token": self.auth_service.access_token,
                "asset": asset.to_dict()
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            return data.get("success", False)
            
        except Exception as e:
            print(f"Error updating content: {e}")
            return False
    
    def delete_content(self, asset_id: str) -> bool:
        """
        Delete content asset from RaOS server.
        
        Args:
            asset_id: ID of asset to delete
            
        Returns:
            bool: True if deletion successful
        """
        if not self.auth_service.is_authenticated():
            return False
            
        try:
            request = json.dumps({
                "action": "delete_content",
                "auth_token": self.auth_service.access_token,
                "asset_id": asset_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            return data.get("success", False)
            
        except Exception as e:
            print(f"Error deleting content: {e}")
            return False
    
    def upload_binary_asset(self, asset_type: str, filename: str, file_data: bytes, 
                           compress: bool = True, convert_format: Optional[str] = None) -> Optional[str]:
        """
        Upload binary asset with optional compression and conversion.
        
        Args:
            asset_type: Type of asset (image, audio, video, etc.)
            filename: Original filename
            file_data: Binary file data
            compress: Whether to compress the asset
            convert_format: Optional target format for conversion
            
        Returns:
            str: Asset ID if upload successful, None otherwise
        """
        if not self.auth_service.is_authenticated():
            return None
            
        try:
            import base64
            
            # Apply compression if requested
            if compress:
                file_data = self._compress_data(file_data)
            
            # Apply format conversion if requested
            if convert_format:
                file_data = self._convert_format(file_data, asset_type, convert_format)
            
            file_data_b64 = base64.b64encode(file_data).decode('utf-8')
            
            request = json.dumps({
                "action": "upload_binary_asset",
                "auth_token": self.auth_service.access_token,
                "asset_type": asset_type,
                "filename": filename,
                "file_data": file_data_b64,
                "compressed": compress,
                "format": convert_format
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("asset_id")
                
            print(f"Binary asset upload failed: {data.get('error', 'Unknown error')}")
            return None
            
        except Exception as e:
            print(f"Error uploading binary asset: {e}")
            return None
    
    def _compress_data(self, data: bytes) -> bytes:
        """
        Compress binary data using gzip.
        
        Args:
            data: Binary data to compress
            
        Returns:
            bytes: Compressed data
        """
        import gzip
        return gzip.compress(data)
    
    def _convert_format(self, data: bytes, asset_type: str, target_format: str) -> bytes:
        """
        Convert asset to different format (placeholder for actual implementation).
        
        Args:
            data: Binary asset data
            asset_type: Type of asset
            target_format: Target format
            
        Returns:
            bytes: Converted data
        """
        # TODO: Implement actual format conversion based on asset type
        # This would use libraries like PIL for images, ffmpeg for video/audio, etc.
        print(f"Format conversion from {asset_type} to {target_format} - not yet implemented")
        return data
    
    def analyze_asset(self, asset_id: str) -> Optional[Dict]:
        """
        Analyze asset and return metadata/statistics.
        
        Args:
            asset_id: ID of asset to analyze
            
        Returns:
            Dict: Analysis results
        """
        if not self.auth_service.is_authenticated():
            return None
            
        try:
            request = json.dumps({
                "action": "analyze_asset",
                "auth_token": self.auth_service.access_token,
                "asset_id": asset_id
            })
            
            response = self.rcore_client.send(request)
            data = json.loads(response)
            
            if data.get("success"):
                return data.get("analysis", {})
            return None
            
        except Exception as e:
            print(f"Error analyzing asset: {e}")
            return None
