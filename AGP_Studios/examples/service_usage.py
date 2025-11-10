"""
Example usage of RaStudios services for RaOS integration.

This script demonstrates how to use the RaStudios services programmatically
without the UI. Useful for automation, testing, and scripting.
"""

from services.rapi_client import RaCoreClient
from services.auth_service import AuthService
from services.game_project_manager import GameProjectManager
from services.game_launcher import GameLauncher
from services.content_manager import ContentManager

def example_authentication():
    """Example: Authenticate with RaOS server."""
    print("\n=== Authentication Example ===")
    
    # Connect to RaOS server
    rcore = RaCoreClient("ws://localhost:7077/ws")
    auth = AuthService(rcore)
    
    # Authenticate
    if auth.authenticate("demo_user", "demo_password"):
        print(f"✓ Authenticated as: {auth.user_profile.get('username', 'Unknown')}")
        print(f"✓ Roles: {', '.join(auth.user_roles)}")
        print(f"✓ Token expires: {auth.token_expiry}")
        return auth
    else:
        print("✗ Authentication failed")
        return None

def example_game_development(auth):
    """Example: Create and manage a game project."""
    print("\n=== Game Development Example ===")
    
    if not auth or not auth.is_developer():
        print("✗ Developer role required")
        return
    
    rcore = RaCoreClient("ws://localhost:7077/ws")
    project_manager = GameProjectManager(rcore, auth)
    
    # Create a new project
    project = project_manager.create_project(
        "My Awesome RPG",
        "An epic fantasy adventure game"
    )
    
    if project:
        print(f"✓ Project created: {project.name}")
        print(f"✓ Project ID: {project.project_id}")
        
        # Simulate adding an asset
        print("\n  Adding assets...")
        # In real use, you'd read actual file data
        fake_asset_data = b"fake image data"
        if project_manager.add_asset("hero_sprite.png", "image", fake_asset_data):
            print("  ✓ Asset added: hero_sprite.png")
        
        # Save project
        if project_manager.save_project():
            print("  ✓ Project saved")
        
        # List all projects
        print("\n  Available projects:")
        projects = project_manager.list_projects()
        for p in projects:
            print(f"    - {p.get('name')} (ID: {p.get('project_id')})")
    else:
        print("✗ Failed to create project")

def example_game_player(auth):
    """Example: Launch and play a game."""
    print("\n=== Game Player Example ===")
    
    if not auth or not auth.is_player():
        print("✗ Player role required")
        return
    
    rcore = RaCoreClient("ws://localhost:7077/ws")
    launcher = GameLauncher(rcore, auth)
    
    # Get player profile
    profile = launcher.get_player_profile()
    if profile:
        print(f"✓ Player: {profile.get('username', 'Unknown')}")
        print(f"✓ Level: {profile.get('level', 0)}")
    
    # List available games
    print("\n  Available games:")
    games = launcher.get_available_games()
    for game in games:
        print(f"    - {game.get('name')} ({game.get('genre', 'Unknown')})")
    
    if games:
        # Launch first game
        game_id = games[0].get('game_id')
        print(f"\n  Launching game: {games[0].get('name')}...")
        
        if launcher.launch_game(game_id, mode="stream"):
            print("  ✓ Game launched successfully")
            
            # Get achievements
            achievements = launcher.get_achievements(game_id)
            print(f"\n  Achievements ({len(achievements)}):")
            for ach in achievements[:3]:  # Show first 3
                status = "✓" if ach.get('unlocked') else "✗"
                print(f"    {status} {ach.get('name', 'Unknown')}")
            
            # Get leaderboard
            leaderboard = launcher.get_leaderboard(game_id, "global")
            print(f"\n  Leaderboard (Top 3):")
            for i, entry in enumerate(leaderboard[:3], 1):
                print(f"    {i}. {entry.get('player_name')} - {entry.get('score')}")
            
            # Stop game
            if launcher.stop_game():
                print("\n  ✓ Game stopped")
        else:
            print("  ✗ Failed to launch game")

def example_content_management(auth):
    """Example: Create and manage content."""
    print("\n=== Content Management Example ===")
    
    if not auth or not auth.is_authenticated():
        print("✗ Authentication required")
        return
    
    rcore = RaCoreClient("ws://localhost:7077/ws")
    content_mgr = ContentManager(rcore, auth)
    
    # Create new blog post
    content = content_mgr.create_content(
        "blog",
        "Welcome to RaOS",
        "This is my first blog post on RaOS platform!"
    )
    
    if content:
        print(f"✓ Content created: {content.title}")
        print(f"✓ Content ID: {content.asset_id}")
        
        # Update content
        content.content += "\n\nEdited: Added more information!"
        if content_mgr.update_content(content):
            print("✓ Content updated")
        
        # List all content
        print("\n  Available content:")
        content_list = content_mgr.list_content()
        for item in content_list[:5]:  # Show first 5
            print(f"    - {item.get('title')} [{item.get('asset_type')}]")
        
        # Upload a binary asset
        print("\n  Uploading binary asset...")
        fake_image_data = b"fake binary image data"
        asset_id = content_mgr.upload_binary_asset(
            "image",
            "screenshot.png",
            fake_image_data,
            compress=True,
            convert_format="webp"
        )
        
        if asset_id:
            print(f"  ✓ Binary asset uploaded: {asset_id}")
    else:
        print("✗ Failed to create content")

def main():
    """
    Run all examples.
    
    NOTE: This is a demonstration script. In a real environment:
    1. RaOS server must be running at ws://localhost:7077/ws
    2. Valid credentials must be provided
    3. Server must support the protocol defined in docs/PROTOCOL.md
    """
    print("=" * 60)
    print("RaStudios Services - Usage Examples")
    print("=" * 60)
    print("\nNOTE: These examples require a running RaOS server.")
    print("Server URL: ws://localhost:7077/ws")
    print("\nMake sure RaOS server is running and accessible.")
    print("=" * 60)
    
    # Authenticate
    auth = example_authentication()
    
    if auth:
        # Run examples based on user roles
        if auth.is_developer():
            example_game_development(auth)
        
        if auth.is_player():
            example_game_player(auth)
        
        # Content management available to all authenticated users
        example_content_management(auth)
        
        print("\n" + "=" * 60)
        print("Examples completed!")
        print("=" * 60)
    else:
        print("\n✗ Cannot run examples without authentication")
        print("\nTo use these examples:")
        print("1. Start RaOS server")
        print("2. Update credentials in example_authentication()")
        print("3. Run this script again")

if __name__ == "__main__":
    main()
