"""
Authentication service for secure communication with RaOS server.
Handles token-based authentication, session management, and secure connections.
"""
import hashlib
import json
from datetime import datetime, timedelta
from typing import Optional, Dict

class AuthService:
    """
    Manages authentication and authorization with RaOS server.
    Supports token-based auth with TLS for secure communication.
    """
    
    def __init__(self, rcore_client):
        """
        Initialize AuthService with RaCore client connection.
        
        Args:
            rcore_client: RaCoreClient instance for server communication
        """
        self.rcore_client = rcore_client
        self.access_token: Optional[str] = None
        self.refresh_token: Optional[str] = None
        self.token_expiry: Optional[datetime] = None
        self.user_profile: Optional[Dict] = None
        self.user_roles: list = []
        
    def authenticate(self, username: str, password: str) -> bool:
        """
        Authenticate user with RaOS server.
        
        Args:
            username: User's username
            password: User's password
            
        Returns:
            bool: True if authentication successful, False otherwise
        """
        try:
            # Hash password before sending (basic security)
            password_hash = hashlib.sha256(password.encode()).hexdigest()
            
            # Send authentication request to RaOS server
            auth_request = json.dumps({
                "action": "authenticate",
                "username": username,
                "password_hash": password_hash
            })
            
            response = self.rcore_client.send(auth_request)
            auth_data = json.loads(response)
            
            if auth_data.get("success"):
                self.access_token = auth_data.get("access_token")
                self.refresh_token = auth_data.get("refresh_token")
                self.user_profile = auth_data.get("user_profile", {})
                self.user_roles = auth_data.get("roles", [])
                
                # Set token expiry (default 1 hour)
                expiry_minutes = auth_data.get("expires_in", 60)
                self.token_expiry = datetime.now() + timedelta(minutes=expiry_minutes)
                
                return True
            return False
            
        except Exception as e:
            print(f"Authentication error: {e}")
            return False
    
    def refresh_access_token(self) -> bool:
        """
        Refresh access token using refresh token.
        
        Returns:
            bool: True if refresh successful, False otherwise
        """
        if not self.refresh_token:
            return False
            
        try:
            refresh_request = json.dumps({
                "action": "refresh_token",
                "refresh_token": self.refresh_token
            })
            
            response = self.rcore_client.send(refresh_request)
            refresh_data = json.loads(response)
            
            if refresh_data.get("success"):
                self.access_token = refresh_data.get("access_token")
                expiry_minutes = refresh_data.get("expires_in", 60)
                self.token_expiry = datetime.now() + timedelta(minutes=expiry_minutes)
                return True
            return False
            
        except Exception as e:
            print(f"Token refresh error: {e}")
            return False
    
    def is_authenticated(self) -> bool:
        """
        Check if user is currently authenticated with valid token.
        
        Returns:
            bool: True if authenticated with valid token
        """
        if not self.access_token or not self.token_expiry:
            return False
        
        # Check if token is expired
        if datetime.now() >= self.token_expiry:
            # Try to refresh token
            return self.refresh_access_token()
        
        return True
    
    def logout(self):
        """
        Logout user and clear authentication data.
        """
        try:
            if self.access_token:
                logout_request = json.dumps({
                    "action": "logout",
                    "access_token": self.access_token
                })
                self.rcore_client.send(logout_request)
        except Exception as e:
            print(f"Logout error: {e}")
        finally:
            self.access_token = None
            self.refresh_token = None
            self.token_expiry = None
            self.user_profile = None
            self.user_roles = []
    
    def get_auth_header(self) -> Optional[str]:
        """
        Get authorization header for authenticated requests.
        
        Returns:
            str: Authorization header value, or None if not authenticated
        """
        if self.is_authenticated():
            return f"Bearer {self.access_token}"
        return None
    
    def has_role(self, role: str) -> bool:
        """
        Check if user has specific role.
        
        Args:
            role: Role to check (e.g., 'admin', 'developer', 'player')
            
        Returns:
            bool: True if user has the role
        """
        return role in self.user_roles
    
    def is_developer(self) -> bool:
        """Check if user has developer role."""
        return self.has_role('developer') or self.has_role('admin')
    
    def is_player(self) -> bool:
        """Check if user has player role."""
        return self.has_role('player')
