"""
Content Editor Panel for RaOS content processing.
Provides UI for fetching, editing, and uploading RaOS content assets.
"""
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QPushButton, 
                              QLabel, QLineEdit, QTextEdit, QListWidget, QGroupBox,
                              QMessageBox, QFileDialog, QComboBox, QCheckBox, QSplitter)
from PyQt6.QtCore import Qt
from services.content_manager import ContentManager

class ContentEditorPanel(QWidget):
    """
    Content Editor panel for RaOS content processing.
    Provides interface for content management and asset pipeline.
    """
    
    def __init__(self, content_manager: ContentManager):
        super().__init__()
        self.content_manager = content_manager
        self._init_ui()
        
    def _init_ui(self):
        """Initialize UI components."""
        layout = QVBoxLayout()
        
        # Content type and actions
        controls_group = QGroupBox("Content Management")
        controls_layout = QVBoxLayout()
        
        # Content type selector and filter
        type_layout = QHBoxLayout()
        type_layout.addWidget(QLabel("Content Type:"))
        
        self.content_type_combo = QComboBox()
        self.content_type_combo.addItems([
            "All", "Blog", "Post", "Image", "Video", "Code", "Document"
        ])
        self.content_type_combo.currentTextChanged.connect(self._on_type_changed)
        type_layout.addWidget(self.content_type_combo)
        
        self.refresh_btn = QPushButton("Refresh List")
        self.refresh_btn.clicked.connect(self._on_refresh_content)
        type_layout.addWidget(self.refresh_btn)
        
        controls_layout.addLayout(type_layout)
        
        # Action buttons
        action_layout = QHBoxLayout()
        
        self.new_content_btn = QPushButton("New Content")
        self.new_content_btn.clicked.connect(self._on_new_content)
        action_layout.addWidget(self.new_content_btn)
        
        self.save_content_btn = QPushButton("Save Content")
        self.save_content_btn.clicked.connect(self._on_save_content)
        action_layout.addWidget(self.save_content_btn)
        
        self.delete_content_btn = QPushButton("Delete Content")
        self.delete_content_btn.clicked.connect(self._on_delete_content)
        action_layout.addWidget(self.delete_content_btn)
        
        self.upload_binary_btn = QPushButton("Upload Binary Asset")
        self.upload_binary_btn.clicked.connect(self._on_upload_binary)
        action_layout.addWidget(self.upload_binary_btn)
        
        controls_layout.addLayout(action_layout)
        controls_group.setLayout(controls_layout)
        layout.addWidget(controls_group)
        
        # Main content splitter
        splitter = QSplitter(Qt.Orientation.Horizontal)
        
        # Content list
        list_group = QGroupBox("Content List")
        list_layout = QVBoxLayout()
        
        self.content_list = QListWidget()
        self.content_list.itemClicked.connect(self._on_content_selected)
        list_layout.addWidget(self.content_list)
        
        list_group.setLayout(list_layout)
        splitter.addWidget(list_group)
        
        # Content editor
        editor_group = QGroupBox("Content Editor")
        editor_layout = QVBoxLayout()
        
        # Title input
        title_layout = QHBoxLayout()
        title_layout.addWidget(QLabel("Title:"))
        self.title_input = QLineEdit()
        self.title_input.setPlaceholderText("Enter content title...")
        title_layout.addWidget(self.title_input)
        editor_layout.addLayout(title_layout)
        
        # Content editor
        self.content_editor = QTextEdit()
        self.content_editor.setPlaceholderText("Enter or edit content here...")
        editor_layout.addWidget(self.content_editor)
        
        # Asset pipeline options
        pipeline_layout = QHBoxLayout()
        self.compress_checkbox = QCheckBox("Compress on upload")
        self.compress_checkbox.setChecked(True)
        pipeline_layout.addWidget(self.compress_checkbox)
        
        pipeline_layout.addWidget(QLabel("Convert to:"))
        self.convert_format_combo = QComboBox()
        self.convert_format_combo.addItems(["None", "PNG", "JPEG", "WebP", "MP4", "WebM"])
        pipeline_layout.addWidget(self.convert_format_combo)
        
        editor_layout.addLayout(pipeline_layout)
        
        editor_group.setLayout(editor_layout)
        splitter.addWidget(editor_group)
        
        layout.addWidget(splitter)
        
        # Status bar
        self.status_label = QLabel("Ready - Select or create content")
        layout.addWidget(self.status_label)
        
        self.setLayout(layout)
        
        # Store content data
        self.current_content_id = None
        self.content_data = []
        
    def _on_type_changed(self, content_type: str):
        """Handle content type filter change."""
        self._on_refresh_content()
    
    def _on_refresh_content(self):
        """Refresh content list."""
        content_type = self.content_type_combo.currentText()
        filter_type = None if content_type == "All" else content_type.lower()
        
        content_list = self.content_manager.list_content(filter_type)
        
        self.content_data = content_list
        self.content_list.clear()
        
        for content in content_list:
            title = content.get('title', 'Untitled')
            ctype = content.get('asset_type', 'unknown')
            self.content_list.addItem(f"{title} [{ctype}]")
        
        self.status_label.setText(f"Loaded {len(content_list)} content items")
    
    def _on_content_selected(self):
        """Handle content selection."""
        current_row = self.content_list.currentRow()
        
        if current_row >= 0 and current_row < len(self.content_data):
            content_meta = self.content_data[current_row]
            content_id = content_meta.get('asset_id')
            
            # Fetch full content
            content = self.content_manager.fetch_content(content_id)
            
            if content:
                self.current_content_id = content_id
                self.title_input.setText(content.title)
                self.content_editor.setPlainText(content.content)
                self.status_label.setText(f"Loaded: {content.title}")
            else:
                QMessageBox.warning(self, "Error", "Failed to load content")
    
    def _on_new_content(self):
        """Create new content."""
        self.current_content_id = None
        self.title_input.clear()
        self.content_editor.clear()
        self.status_label.setText("New content - enter title and content")
    
    def _on_save_content(self):
        """Save current content."""
        title = self.title_input.text().strip()
        content = self.content_editor.toPlainText()
        
        if not title:
            QMessageBox.warning(self, "Error", "Please enter a title")
            return
        
        if self.current_content_id:
            # Update existing content
            current_content = self.content_manager.current_asset
            if current_content:
                current_content.title = title
                current_content.content = content
                
                if self.content_manager.update_content(current_content):
                    self.status_label.setText("Content updated successfully")
                    QMessageBox.information(self, "Success", "Content updated!")
                    self._on_refresh_content()
                else:
                    QMessageBox.critical(self, "Error", "Failed to update content")
        else:
            # Create new content
            content_type = self.content_type_combo.currentText().lower()
            if content_type == "all":
                content_type = "post"
            
            new_content = self.content_manager.create_content(content_type, title, content)
            
            if new_content:
                self.current_content_id = new_content.asset_id
                self.status_label.setText("Content created successfully")
                QMessageBox.information(self, "Success", "Content created!")
                self._on_refresh_content()
            else:
                QMessageBox.critical(self, "Error", "Failed to create content")
    
    def _on_delete_content(self):
        """Delete current content."""
        if not self.current_content_id:
            QMessageBox.warning(self, "Error", "No content selected")
            return
        
        reply = QMessageBox.question(
            self,
            "Confirm Delete",
            "Are you sure you want to delete this content?",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
        )
        
        if reply == QMessageBox.StandardButton.Yes:
            if self.content_manager.delete_content(self.current_content_id):
                self.status_label.setText("Content deleted")
                self._on_new_content()
                self._on_refresh_content()
                QMessageBox.information(self, "Success", "Content deleted!")
            else:
                QMessageBox.critical(self, "Error", "Failed to delete content")
    
    def _on_upload_binary(self):
        """Upload binary asset with asset pipeline."""
        file_path, _ = QFileDialog.getOpenFileName(
            self,
            "Select Binary Asset",
            "",
            "All Files (*);;Images (*.png *.jpg *.jpeg);;Videos (*.mp4 *.avi);;Audio (*.mp3 *.wav)"
        )
        
        if not file_path:
            return
        
        try:
            with open(file_path, 'rb') as f:
                file_data = f.read()
            
            import os
            filename = os.path.basename(file_path)
            asset_type = self._detect_asset_type(filename)
            
            # Get pipeline options
            compress = self.compress_checkbox.isChecked()
            convert_format = self.convert_format_combo.currentText()
            if convert_format == "None":
                convert_format = None
            else:
                convert_format = convert_format.lower()
            
            asset_id = self.content_manager.upload_binary_asset(
                asset_type,
                filename,
                file_data,
                compress=compress,
                convert_format=convert_format
            )
            
            if asset_id:
                self.status_label.setText(f"Binary asset uploaded: {filename}")
                QMessageBox.information(
                    self,
                    "Success",
                    f"Asset '{filename}' uploaded successfully!\nAsset ID: {asset_id}"
                )
            else:
                QMessageBox.critical(self, "Error", "Failed to upload binary asset")
                
        except Exception as e:
            QMessageBox.critical(self, "Error", f"Failed to read file: {e}")
    
    def _detect_asset_type(self, filename: str) -> str:
        """Detect asset type from filename."""
        import os
        _, ext = os.path.splitext(filename.lower())
        
        if ext in ['.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp']:
            return 'image'
        elif ext in ['.mp4', '.avi', '.mov', '.webm', '.mkv']:
            return 'video'
        elif ext in ['.mp3', '.wav', '.ogg', '.flac']:
            return 'audio'
        elif ext in ['.pdf', '.doc', '.docx', '.txt']:
            return 'document'
        else:
            return 'generic'
