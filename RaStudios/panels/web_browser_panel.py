"""
Web Browser Panel for RaOS web content browsing.
Provides embedded browser for navigating RaOS-powered sites with secure authentication.
Includes homepage with news, updates, login panel, and update notifications.
"""
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QPushButton, 
                              QLabel, QLineEdit, QMessageBox, QProgressBar, QGroupBox)
from PyQt6.QtCore import QUrl, Qt
from PyQt6.QtGui import QFont
import json
from datetime import datetime

try:
    from PyQt6.QtWebEngineWidgets import QWebEngineView
    from PyQt6.QtWebEngineCore import QWebEnginePage
    WEBENGINE_AVAILABLE = True
except ImportError:
    WEBENGINE_AVAILABLE = False
    QWebEngineView = None
    QWebEnginePage = None

class WebBrowserPanel(QWidget):
    """
    Web Browser panel for RaOS content.
    Provides embedded browser with authentication integration.
    Note: Requires PyQt6-WebEngine package to be installed.
    """
    
    def __init__(self, auth_service=None):
        super().__init__()
        self.auth_service = auth_service
        self.is_authenticated = False
        self.current_user = ""
        self._init_ui()
        
    def _init_ui(self):
        """Initialize UI components."""
        layout = QVBoxLayout()
        
        # Top control panel with login and updates
        top_panel = self._create_top_panel()
        layout.addWidget(top_panel)
        
        # Navigation bar
        nav_layout = QHBoxLayout()
        
        self.back_btn = QPushButton("◀")
        self.back_btn.setMaximumWidth(40)
        self.back_btn.clicked.connect(self._on_back)
        nav_layout.addWidget(self.back_btn)
        
        self.forward_btn = QPushButton("▶")
        self.forward_btn.setMaximumWidth(40)
        self.forward_btn.clicked.connect(self._on_forward)
        nav_layout.addWidget(self.forward_btn)
        
        self.refresh_btn = QPushButton("⟳")
        self.refresh_btn.setMaximumWidth(40)
        self.refresh_btn.clicked.connect(self._on_refresh)
        nav_layout.addWidget(self.refresh_btn)
        
        self.url_input = QLineEdit()
        self.url_input.setPlaceholderText("Enter RaOS URL...")
        self.url_input.returnPressed.connect(self._on_navigate)
        nav_layout.addWidget(self.url_input)
        
        self.go_btn = QPushButton("Go")
        self.go_btn.clicked.connect(self._on_navigate)
        nav_layout.addWidget(self.go_btn)
        
        layout.addLayout(nav_layout)
        
        # Web view
        if WEBENGINE_AVAILABLE:
            self.web_view = QWebEngineView()
            self.web_view.urlChanged.connect(self._on_url_changed)
            self.web_view.loadFinished.connect(self._on_load_finished)
            
            # Set default page
            self.web_view.setHtml(self._get_welcome_html())
            
            layout.addWidget(self.web_view)
        else:
            # Fallback if WebEngine not available
            warning_label = QLabel(
                "⚠️ WebEngine not available\n\n"
                "To use the browser feature, install PyQt6-WebEngine:\n"
                "pip install PyQt6-WebEngine"
            )
            warning_label.setStyleSheet("padding: 20px; background-color: #fff3cd; color: #856404;")
            layout.addWidget(warning_label)
        
        # Status bar with progress
        status_layout = QHBoxLayout()
        self.status_label = QLabel("Ready")
        status_layout.addWidget(self.status_label)
        
        self.progress_bar = QProgressBar()
        self.progress_bar.setMaximumWidth(300)
        self.progress_bar.setVisible(False)
        status_layout.addWidget(self.progress_bar)
        
        layout.addLayout(status_layout)
        
        self.setLayout(layout)
        
    def _create_top_panel(self):
        """Create top panel with login and update controls."""
        group_box = QGroupBox("Controls")
        layout = QHBoxLayout()
        
        # Login button
        self.login_btn = QPushButton("🔒 Login to ASHATOS")
        self.login_btn.clicked.connect(self._on_login_clicked)
        self.login_btn.setStyleSheet("""
            QPushButton {
                background-color: #3498db;
                color: white;
                border: none;
                padding: 8px 16px;
                border-radius: 4px;
                font-weight: bold;
            }
            QPushButton:hover {
                background-color: #2980b9;
            }
        """)
        layout.addWidget(self.login_btn)
        
        # User label
        self.user_label = QLabel("Not logged in")
        self.user_label.setStyleSheet("color: gray; padding: 8px;")
        layout.addWidget(self.user_label)
        
        layout.addStretch()
        
        # Check updates button
        self.update_btn = QPushButton("⚡ Check for Updates")
        self.update_btn.clicked.connect(self._on_check_updates)
        self.update_btn.setStyleSheet("""
            QPushButton {
                background-color: #2ecc71;
                color: white;
                border: none;
                padding: 8px 16px;
                border-radius: 4px;
                font-weight: bold;
            }
            QPushButton:hover {
                background-color: #27ae60;
            }
        """)
        layout.addWidget(self.update_btn)
        
        # Connection status label
        self.connection_label = QLabel("ASHATOS: Disconnected")
        self.connection_label.setStyleSheet("color: red; padding: 8px;")
        layout.addWidget(self.connection_label)
        
        group_box.setLayout(layout)
        return group_box
        
    def _on_back(self):
        """Navigate back."""
        if WEBENGINE_AVAILABLE and self.web_view:
            self.web_view.back()
    
    def _on_forward(self):
        """Navigate forward."""
        if WEBENGINE_AVAILABLE and self.web_view:
            self.web_view.forward()
    
    def _on_refresh(self):
        """Refresh current page."""
        if WEBENGINE_AVAILABLE and self.web_view:
            self.web_view.reload()
    
    def _on_navigate(self):
        """Navigate to URL."""
        if not WEBENGINE_AVAILABLE:
            return
            
        url_text = self.url_input.text().strip()
        
        if not url_text:
            return
        
        # Add https:// if no protocol specified
        if not url_text.startswith(('http://', 'https://')):
            url_text = 'https://' + url_text
        
        # Check if authenticated for RaOS URLs
        if self.auth_service and 'raos' in url_text.lower():
            if not self.auth_service.is_authenticated():
                QMessageBox.warning(
                    self,
                    "Authentication Required",
                    "Please authenticate before accessing RaOS content"
                )
                return
            
            # Inject authentication token if available
            auth_header = self.auth_service.get_auth_header()
            if auth_header:
                # Set custom header for authentication
                # Note: This is a simplified approach. Real implementation would use interceptors
                pass
        
        url = QUrl(url_text)
        self.web_view.setUrl(url)
        self.status_label.setText(f"Loading {url_text}...")
    
    def _on_url_changed(self, url: QUrl):
        """Handle URL change."""
        self.url_input.setText(url.toString())
    
    def _on_load_finished(self, success: bool):
        """Handle page load completion."""
        if success:
            self.status_label.setText("Page loaded")
        else:
            self.status_label.setText("Failed to load page")
            QMessageBox.warning(self, "Error", "Failed to load the page")
    
    def navigate_to(self, url: str):
        """
        Programmatically navigate to URL.
        
        Args:
            url: URL to navigate to
        """
        if WEBENGINE_AVAILABLE:
            self.url_input.setText(url)
            self._on_navigate()
    
    def _on_login_clicked(self):
        """Handle login button click."""
        if self.is_authenticated:
            # Logout
            self.is_authenticated = False
            self.current_user = ""
            self.login_btn.setText("🔒 Login to ASHATOS")
            self.user_label.setText("Not logged in")
            self.user_label.setStyleSheet("color: gray; padding: 8px;")
            self.connection_label.setText("ASHATOS: Disconnected")
            self.connection_label.setStyleSheet("color: red; padding: 8px;")
            if self.auth_service:
                self.auth_service.logout()
            QMessageBox.information(self, "Logged Out", "You have been logged out successfully.")
        else:
            # Show login dialog
            from PyQt6.QtWidgets import QDialog, QFormLayout, QLineEdit, QDialogButtonBox
            
            dialog = QDialog(self)
            dialog.setWindowTitle("ASHATOS Login")
            dialog.setMinimumWidth(350)
            
            layout = QFormLayout()
            
            username_input = QLineEdit()
            password_input = QLineEdit()
            password_input.setEchoMode(QLineEdit.EchoMode.Password)
            
            layout.addRow("Username:", username_input)
            layout.addRow("Password:", password_input)
            
            buttons = QDialogButtonBox(
                QDialogButtonBox.StandardButton.Ok | QDialogButtonBox.StandardButton.Cancel
            )
            buttons.accepted.connect(dialog.accept)
            buttons.rejected.connect(dialog.reject)
            layout.addWidget(buttons)
            
            dialog.setLayout(layout)
            
            if dialog.exec() == QDialog.DialogCode.Accepted:
                username = username_input.text()
                password = password_input.text()
                
                if self.auth_service:
                    if self.auth_service.authenticate(username, password):
                        self.is_authenticated = True
                        self.current_user = username
                        self.login_btn.setText("🔓 Logout")
                        self.user_label.setText(f"Logged in as: {username}")
                        self.user_label.setStyleSheet("color: green; padding: 8px; font-weight: bold;")
                        self.connection_label.setText("ASHATOS: Connected")
                        self.connection_label.setStyleSheet("color: green; padding: 8px;")
                        QMessageBox.information(self, "Login Successful", f"Welcome, {username}!")
                    else:
                        QMessageBox.warning(self, "Login Failed", "Invalid credentials. Please try again.")
                else:
                    QMessageBox.warning(self, "No Auth Service", "Authentication service not available.")
    
    def _on_check_updates(self):
        """Handle check for updates button click."""
        self.status_label.setText("Checking for updates...")
        self.progress_bar.setVisible(True)
        self.progress_bar.setMaximum(0)  # Indeterminate progress
        
        # Simulate update check (in real implementation, this would query a server)
        from PyQt6.QtCore import QTimer
        
        def finish_check():
            self.progress_bar.setVisible(False)
            self.status_label.setText("You are running the latest version!")
            QMessageBox.information(
                self,
                "Update Check",
                "Your RaStudios client is up to date!\n\n"
                "Version: 1.0.0\n"
                f"Build: {datetime.now().strftime('%Y.%m.%d')}"
            )
        
        QTimer.singleShot(2000, finish_check)  # Simulate 2 second delay
    
    def update_connection_status(self, connected: bool, message: str = ""):
        """Update connection status display."""
        if connected:
            self.connection_label.setText(f"ASHATOS: Connected")
            self.connection_label.setStyleSheet("color: green; padding: 8px;")
        else:
            self.connection_label.setText(f"ASHATOS: Disconnected")
            self.connection_label.setStyleSheet("color: red; padding: 8px;")
        
        if message:
            self.status_label.setText(message)
    
    def _get_welcome_html(self) -> str:
        """Get welcome page HTML."""
        return """
        <!DOCTYPE html>
        <html>
        <head>
            <title>RaStudios - Homepage</title>
            <style>
                body {
                    font-family: 'Segoe UI', Arial, sans-serif;
                    margin: 0;
                    padding: 0;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                }
                .container {
                    max-width: 1200px;
                    margin: 0 auto;
                    padding: 40px 20px;
                }
                h1 {
                    font-size: 3em;
                    margin-bottom: 10px;
                    text-align: center;
                    text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
                }
                .subtitle {
                    text-align: center;
                    font-size: 1.2em;
                    margin-bottom: 40px;
                    opacity: 0.9;
                }
                .grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
                    gap: 20px;
                    margin-top: 40px;
                }
                .card {
                    background: rgba(255, 255, 255, 0.1);
                    padding: 30px;
                    border-radius: 15px;
                    backdrop-filter: blur(10px);
                    box-shadow: 0 8px 32px rgba(0,0,0,0.1);
                    border: 1px solid rgba(255, 255, 255, 0.2);
                    transition: transform 0.3s ease;
                }
                .card:hover {
                    transform: translateY(-5px);
                    background: rgba(255, 255, 255, 0.15);
                }
                .card h2 {
                    margin-top: 0;
                    font-size: 1.5em;
                    margin-bottom: 15px;
                }
                .card p {
                    line-height: 1.6;
                    opacity: 0.9;
                }
                .icon {
                    font-size: 2.5em;
                    margin-bottom: 15px;
                    display: block;
                }
                .news-section {
                    margin-top: 40px;
                    background: rgba(255, 255, 255, 0.1);
                    padding: 30px;
                    border-radius: 15px;
                    backdrop-filter: blur(10px);
                }
                .news-item {
                    margin-bottom: 20px;
                    padding-bottom: 20px;
                    border-bottom: 1px solid rgba(255, 255, 255, 0.2);
                }
                .news-item:last-child {
                    border-bottom: none;
                    margin-bottom: 0;
                    padding-bottom: 0;
                }
                .news-date {
                    font-size: 0.9em;
                    opacity: 0.7;
                    margin-bottom: 5px;
                }
                .quick-links {
                    display: flex;
                    flex-wrap: wrap;
                    gap: 10px;
                    margin-top: 30px;
                    justify-content: center;
                }
                .quick-link {
                    background: rgba(255, 255, 255, 0.2);
                    padding: 10px 20px;
                    border-radius: 25px;
                    text-decoration: none;
                    color: white;
                    transition: background 0.3s ease;
                }
                .quick-link:hover {
                    background: rgba(255, 255, 255, 0.3);
                }
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>🎮 Welcome to RaStudios</h1>
                <p class='subtitle'>Your Unified Game Development & Play Client</p>
                
                <div class='grid'>
                    <div class='card'>
                        <span class='icon'>🎨</span>
                        <h2>Game Development</h2>
                        <p>Create amazing games with our integrated IDE. Build, test, and deploy your games seamlessly with ASHATOS integration.</p>
                    </div>
                    
                    <div class='card'>
                        <span class='icon'>🕹️</span>
                        <h2>Game Player</h2>
                        <p>Play games built with RaStudios. Access your library, track achievements, and connect with friends.</p>
                    </div>
                    
                    <div class='card'>
                        <span class='icon'>🌐</span>
                        <h2>Content Browser</h2>
                        <p>Explore RaOS-powered websites and content. Discover new games, mods, and community creations.</p>
                    </div>
                    
                    <div class='card'>
                        <span class='icon'>🔒</span>
                        <h2>Secure Authentication</h2>
                        <p>Connect securely to ASHATOS servers with token-based authentication and encrypted communication.</p>
                    </div>
                    
                    <div class='card'>
                        <span class='icon'>⚡</span>
                        <h2>Real-time Updates</h2>
                        <p>Stay up to date with automatic client updates and instant notifications about new features.</p>
                    </div>
                    
                    <div class='card'>
                        <span class='icon'>🐍</span>
                        <h2>Python Power</h2>
                        <p>Built with PyQt6 for cross-platform compatibility and powerful desktop integration.</p>
                    </div>
                </div>
                
                <div class='news-section'>
                    <h2>📰 Latest News & Updates</h2>
                    
                    <div class='news-item'>
                        <div class='news-date'>2025-10-30</div>
                        <h3>Enhanced Homepage with Login & Updates!</h3>
                        <p>We've improved the homepage with integrated authentication, update notifications, and better ASHATOS connectivity.</p>
                    </div>
                    
                    <div class='news-item'>
                        <div class='news-date'>2025-10-28</div>
                        <h3>Game Player Panel Added</h3>
                        <p>New game player panel allows you to launch and play games directly from RaStudios!</p>
                    </div>
                    
                    <div class='news-item'>
                        <div class='news-date'>2025-10-25</div>
                        <h3>Web Browser Integration</h3>
                        <p>Browse RaOS-powered content with our integrated web browser featuring secure authentication.</p>
                    </div>
                </div>
                
                <div class='quick-links'>
                    <a href='#' class='quick-link'>📚 Documentation</a>
                    <a href='#' class='quick-link'>💬 Community</a>
                    <a href='#' class='quick-link'>🐛 Report Bug</a>
                    <a href='#' class='quick-link'>💡 Feature Request</a>
                    <a href='#' class='quick-link'>🎯 Roadmap</a>
                </div>
            </div>
        </body>
        </html>
        """
