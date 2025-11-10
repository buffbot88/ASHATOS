"""
Game Development Panel for RaOS IDE functionality.
Provides UI for game project management, scene editing, and asset management.
"""
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QPushButton, 
                              QLabel, QLineEdit, QTextEdit, QListWidget, QSplitter,
                              QMessageBox, QFileDialog, QGroupBox)
from PyQt6.QtCore import Qt
from services.game_project_manager import GameProjectManager

class GameDevPanel(QWidget):
    """
    Game Development IDE panel.
    Provides interface for creating/loading games, managing assets, and editing content.
    """
    
    def __init__(self, game_project_manager: GameProjectManager):
        super().__init__()
        self.game_project_manager = game_project_manager
        self._init_ui()
        
    def _init_ui(self):
        """Initialize UI components."""
        layout = QVBoxLayout()
        
        # Project controls
        project_group = QGroupBox("Game Project")
        project_layout = QVBoxLayout()
        
        # Project name and description
        name_layout = QHBoxLayout()
        name_layout.addWidget(QLabel("Project Name:"))
        self.project_name_input = QLineEdit()
        self.project_name_input.setPlaceholderText("Enter project name...")
        name_layout.addWidget(self.project_name_input)
        project_layout.addLayout(name_layout)
        
        desc_layout = QHBoxLayout()
        desc_layout.addWidget(QLabel("Description:"))
        self.project_desc_input = QLineEdit()
        self.project_desc_input.setPlaceholderText("Enter project description...")
        desc_layout.addWidget(self.project_desc_input)
        project_layout.addLayout(desc_layout)
        
        # Project action buttons
        btn_layout = QHBoxLayout()
        self.new_project_btn = QPushButton("New Project")
        self.new_project_btn.clicked.connect(self._on_new_project)
        btn_layout.addWidget(self.new_project_btn)
        
        self.load_project_btn = QPushButton("Load Project")
        self.load_project_btn.clicked.connect(self._on_load_project)
        btn_layout.addWidget(self.load_project_btn)
        
        self.save_project_btn = QPushButton("Save Project")
        self.save_project_btn.clicked.connect(self._on_save_project)
        btn_layout.addWidget(self.save_project_btn)
        
        self.sync_assets_btn = QPushButton("Sync Assets")
        self.sync_assets_btn.clicked.connect(self._on_sync_assets)
        btn_layout.addWidget(self.sync_assets_btn)
        
        project_layout.addLayout(btn_layout)
        project_group.setLayout(project_layout)
        layout.addWidget(project_group)
        
        # Splitter for assets and content
        splitter = QSplitter(Qt.Orientation.Horizontal)
        
        # Assets list
        assets_group = QGroupBox("Assets")
        assets_layout = QVBoxLayout()
        
        self.assets_list = QListWidget()
        assets_layout.addWidget(self.assets_list)
        
        asset_btn_layout = QHBoxLayout()
        self.add_asset_btn = QPushButton("Add Asset")
        self.add_asset_btn.clicked.connect(self._on_add_asset)
        asset_btn_layout.addWidget(self.add_asset_btn)
        
        self.remove_asset_btn = QPushButton("Remove Asset")
        self.remove_asset_btn.clicked.connect(self._on_remove_asset)
        asset_btn_layout.addWidget(self.remove_asset_btn)
        
        assets_layout.addLayout(asset_btn_layout)
        assets_group.setLayout(assets_layout)
        splitter.addWidget(assets_group)
        
        # Content editor
        editor_group = QGroupBox("Scene/Script Editor")
        editor_layout = QVBoxLayout()
        
        self.content_editor = QTextEdit()
        self.content_editor.setPlaceholderText("Scene/script content will appear here...")
        editor_layout.addWidget(self.content_editor)
        
        editor_group.setLayout(editor_layout)
        splitter.addWidget(editor_group)
        
        layout.addWidget(splitter)
        
        # Status bar
        self.status_label = QLabel("Ready - No project loaded")
        layout.addWidget(self.status_label)
        
        self.setLayout(layout)
        
    def _on_new_project(self):
        """Handle new project creation."""
        name = self.project_name_input.text().strip()
        description = self.project_desc_input.text().strip()
        
        if not name:
            QMessageBox.warning(self, "Error", "Please enter a project name")
            return
            
        project = self.game_project_manager.create_project(name, description)
        
        if project:
            self.status_label.setText(f"Project created: {project.name}")
            self._update_ui_for_project(project)
            QMessageBox.information(self, "Success", f"Project '{name}' created successfully!")
        else:
            QMessageBox.critical(self, "Error", "Failed to create project. Check authentication.")
    
    def _on_load_project(self):
        """Handle project loading."""
        # Get list of available projects
        projects = self.game_project_manager.list_projects()
        
        if not projects:
            QMessageBox.information(self, "No Projects", "No projects available to load.")
            return
        
        # For simplicity, show project IDs in a simple dialog
        # In a real implementation, use a proper selection dialog
        project_id = projects[0].get("project_id") if projects else None
        
        if project_id:
            project = self.game_project_manager.load_project(project_id)
            if project:
                self.status_label.setText(f"Project loaded: {project.name}")
                self._update_ui_for_project(project)
                QMessageBox.information(self, "Success", f"Project '{project.name}' loaded!")
            else:
                QMessageBox.critical(self, "Error", "Failed to load project.")
    
    def _on_save_project(self):
        """Handle project saving."""
        if self.game_project_manager.save_project():
            self.status_label.setText("Project saved successfully")
            QMessageBox.information(self, "Success", "Project saved!")
        else:
            QMessageBox.critical(self, "Error", "Failed to save project.")
    
    def _on_sync_assets(self):
        """Handle asset synchronization."""
        if self.game_project_manager.sync_assets():
            self._refresh_assets_list()
            self.status_label.setText("Assets synchronized")
            QMessageBox.information(self, "Success", "Assets synchronized!")
        else:
            QMessageBox.critical(self, "Error", "Failed to sync assets.")
    
    def _on_add_asset(self):
        """Handle adding an asset."""
        file_path, _ = QFileDialog.getOpenFileName(
            self,
            "Select Asset File",
            "",
            "All Files (*);;Images (*.png *.jpg *.jpeg);;Audio (*.wav *.mp3);;Models (*.obj *.fbx)"
        )
        
        if file_path:
            try:
                with open(file_path, 'rb') as f:
                    asset_data = f.read()
                
                import os
                asset_name = os.path.basename(file_path)
                asset_type = self._detect_asset_type(asset_name)
                
                if self.game_project_manager.add_asset(asset_name, asset_type, asset_data):
                    self._refresh_assets_list()
                    self.status_label.setText(f"Asset added: {asset_name}")
                    QMessageBox.information(self, "Success", f"Asset '{asset_name}' added!")
                else:
                    QMessageBox.critical(self, "Error", "Failed to add asset.")
                    
            except Exception as e:
                QMessageBox.critical(self, "Error", f"Failed to read asset file: {e}")
    
    def _on_remove_asset(self):
        """Handle removing an asset."""
        current_item = self.assets_list.currentItem()
        if current_item:
            asset_name = current_item.text()
            # Remove from current project
            if self.game_project_manager.current_project:
                self.game_project_manager.current_project.assets = [
                    a for a in self.game_project_manager.current_project.assets 
                    if a.get('name') != asset_name
                ]
                self._refresh_assets_list()
                self.status_label.setText(f"Asset removed: {asset_name}")
    
    def _update_ui_for_project(self, project):
        """Update UI to reflect loaded project."""
        self.project_name_input.setText(project.name)
        self.project_desc_input.setText(project.description)
        self._refresh_assets_list()
    
    def _refresh_assets_list(self):
        """Refresh the assets list display."""
        self.assets_list.clear()
        if self.game_project_manager.current_project:
            for asset in self.game_project_manager.current_project.assets:
                asset_name = asset.get('name', 'Unknown')
                asset_type = asset.get('type', 'unknown')
                self.assets_list.addItem(f"{asset_name} ({asset_type})")
    
    def _detect_asset_type(self, filename: str) -> str:
        """Detect asset type from filename extension."""
        import os
        _, ext = os.path.splitext(filename.lower())
        
        if ext in ['.png', '.jpg', '.jpeg', '.gif', '.bmp']:
            return 'image'
        elif ext in ['.wav', '.mp3', '.ogg', '.flac']:
            return 'audio'
        elif ext in ['.obj', '.fbx', '.gltf', '.glb']:
            return 'model'
        elif ext in ['.py', '.cs', '.js', '.lua']:
            return 'script'
        else:
            return 'generic'
