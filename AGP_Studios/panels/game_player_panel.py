"""
Game Player Panel for RaOS game playing functionality.
Provides UI for browsing games, launching, and viewing player profile/achievements.
"""
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QPushButton, 
                              QLabel, QListWidget, QGroupBox, QMessageBox,
                              QTextEdit, QSplitter, QComboBox)
from PyQt6.QtCore import Qt
from services.game_launcher import GameLauncher

class GamePlayerPanel(QWidget):
    """
    Game Player panel.
    Provides interface for discovering, launching, and playing RaOS games.
    """
    
    def __init__(self, game_launcher: GameLauncher):
        super().__init__()
        self.game_launcher = game_launcher
        self._init_ui()
        
    def _init_ui(self):
        """Initialize UI components."""
        layout = QVBoxLayout()
        
        # Player profile section
        profile_group = QGroupBox("Player Profile")
        profile_layout = QHBoxLayout()
        
        self.profile_label = QLabel("Not logged in")
        profile_layout.addWidget(self.profile_label)
        
        self.refresh_profile_btn = QPushButton("Refresh Profile")
        self.refresh_profile_btn.clicked.connect(self._on_refresh_profile)
        profile_layout.addWidget(self.refresh_profile_btn)
        
        profile_group.setLayout(profile_layout)
        layout.addWidget(profile_group)
        
        # Main content splitter
        splitter = QSplitter(Qt.Orientation.Horizontal)
        
        # Games list section
        games_group = QGroupBox("Available Games")
        games_layout = QVBoxLayout()
        
        self.games_list = QListWidget()
        self.games_list.itemSelectionChanged.connect(self._on_game_selected)
        games_layout.addWidget(self.games_list)
        
        game_btn_layout = QHBoxLayout()
        self.refresh_games_btn = QPushButton("Refresh Games")
        self.refresh_games_btn.clicked.connect(self._on_refresh_games)
        game_btn_layout.addWidget(self.refresh_games_btn)
        
        self.launch_stream_btn = QPushButton("Launch (Stream)")
        self.launch_stream_btn.clicked.connect(lambda: self._on_launch_game("stream"))
        game_btn_layout.addWidget(self.launch_stream_btn)
        
        self.launch_download_btn = QPushButton("Launch (Download)")
        self.launch_download_btn.clicked.connect(lambda: self._on_launch_game("download"))
        game_btn_layout.addWidget(self.launch_download_btn)
        
        games_layout.addLayout(game_btn_layout)
        
        self.stop_game_btn = QPushButton("Stop Game")
        self.stop_game_btn.clicked.connect(self._on_stop_game)
        self.stop_game_btn.setEnabled(False)
        games_layout.addWidget(self.stop_game_btn)
        
        games_group.setLayout(games_layout)
        splitter.addWidget(games_group)
        
        # Game details and achievements section
        details_group = QGroupBox("Game Details & Achievements")
        details_layout = QVBoxLayout()
        
        self.game_details_text = QTextEdit()
        self.game_details_text.setReadOnly(True)
        self.game_details_text.setPlaceholderText("Select a game to view details...")
        details_layout.addWidget(self.game_details_text)
        
        # Achievements section
        achievements_layout = QHBoxLayout()
        achievements_layout.addWidget(QLabel("View:"))
        
        self.view_selector = QComboBox()
        self.view_selector.addItems(["Achievements", "Leaderboard"])
        self.view_selector.currentTextChanged.connect(self._on_view_changed)
        achievements_layout.addWidget(self.view_selector)
        
        self.leaderboard_category = QComboBox()
        self.leaderboard_category.addItems(["Global", "Friends", "Regional"])
        self.leaderboard_category.setVisible(False)
        achievements_layout.addWidget(self.leaderboard_category)
        
        details_layout.addLayout(achievements_layout)
        
        self.achievements_list = QListWidget()
        details_layout.addWidget(self.achievements_list)
        
        details_group.setLayout(details_layout)
        splitter.addWidget(details_group)
        
        layout.addWidget(splitter)
        
        # Status bar
        self.status_label = QLabel("Ready - Select a game to play")
        layout.addWidget(self.status_label)
        
        self.setLayout(layout)
        
        # Store game data
        self.current_game_id = None
        self.games_data = []
        
    def _on_refresh_profile(self):
        """Refresh player profile."""
        profile = self.game_launcher.get_player_profile()
        
        if profile:
            username = profile.get('username', 'Unknown')
            level = profile.get('level', 0)
            self.profile_label.setText(f"Player: {username} (Level {level})")
            self.status_label.setText("Profile refreshed")
        else:
            QMessageBox.warning(self, "Error", "Failed to fetch player profile")
    
    def _on_refresh_games(self):
        """Refresh available games list."""
        games = self.game_launcher.get_available_games()
        
        if games:
            self.games_data = games
            self.games_list.clear()
            
            for game in games:
                game_name = game.get('name', 'Unknown Game')
                self.games_list.addItem(game_name)
            
            self.status_label.setText(f"Found {len(games)} games")
        else:
            QMessageBox.information(self, "No Games", "No games available")
            self.status_label.setText("No games found")
    
    def _on_game_selected(self):
        """Handle game selection."""
        current_row = self.games_list.currentRow()
        
        if current_row >= 0 and current_row < len(self.games_data):
            game = self.games_data[current_row]
            self.current_game_id = game.get('game_id')
            
            # Display game details
            details = f"Name: {game.get('name', 'Unknown')}\n"
            details += f"Description: {game.get('description', 'No description')}\n"
            details += f"Genre: {game.get('genre', 'N/A')}\n"
            details += f"Players: {game.get('players', 'N/A')}\n"
            details += f"Rating: {game.get('rating', 'N/A')}\n"
            
            self.game_details_text.setPlainText(details)
            
            # Load achievements for this game
            self._load_achievements()
    
    def _on_launch_game(self, mode: str):
        """Launch selected game."""
        if not self.current_game_id:
            QMessageBox.warning(self, "Error", "Please select a game first")
            return
        
        if self.game_launcher.launch_game(self.current_game_id, mode):
            self.status_label.setText(f"Game launched in {mode} mode")
            self.stop_game_btn.setEnabled(True)
            self.launch_stream_btn.setEnabled(False)
            self.launch_download_btn.setEnabled(False)
            QMessageBox.information(self, "Success", f"Game launched in {mode} mode!")
        else:
            QMessageBox.critical(self, "Error", "Failed to launch game")
    
    def _on_stop_game(self):
        """Stop currently running game."""
        if self.game_launcher.stop_game():
            self.status_label.setText("Game stopped")
            self.stop_game_btn.setEnabled(False)
            self.launch_stream_btn.setEnabled(True)
            self.launch_download_btn.setEnabled(True)
            QMessageBox.information(self, "Success", "Game stopped")
        else:
            QMessageBox.critical(self, "Error", "Failed to stop game")
    
    def _on_view_changed(self, view_type: str):
        """Handle view change between achievements and leaderboard."""
        if view_type == "Leaderboard":
            self.leaderboard_category.setVisible(True)
            self._load_leaderboard()
        else:
            self.leaderboard_category.setVisible(False)
            self._load_achievements()
    
    def _load_achievements(self):
        """Load achievements for current game."""
        if not self.current_game_id:
            return
        
        achievements = self.game_launcher.get_achievements(self.current_game_id)
        
        self.achievements_list.clear()
        for achievement in achievements:
            name = achievement.get('name', 'Unknown')
            unlocked = achievement.get('unlocked', False)
            status = "✓" if unlocked else "✗"
            self.achievements_list.addItem(f"{status} {name}")
    
    def _load_leaderboard(self):
        """Load leaderboard for current game."""
        if not self.current_game_id:
            return
        
        category = self.leaderboard_category.currentText().lower()
        leaderboard = self.game_launcher.get_leaderboard(self.current_game_id, category)
        
        self.achievements_list.clear()
        for i, entry in enumerate(leaderboard, 1):
            player = entry.get('player_name', 'Unknown')
            score = entry.get('score', 0)
            self.achievements_list.addItem(f"{i}. {player} - {score}")
