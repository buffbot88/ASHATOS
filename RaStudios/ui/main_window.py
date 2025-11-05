from PyQt6.QtWidgets import QMainWindow, QTabWidget, QApplication, QMessageBox, QInputDialog
from panels.dashboard_panel import DashboardPanel
from panels.logs_panel import LogsPanel
from panels.game_dev_panel import GameDevPanel
from panels.game_player_panel import GamePlayerPanel
from panels.web_browser_panel import WebBrowserPanel
from panels.content_editor_panel import ContentEditorPanel
from viewmodels.dashboard_panel_viewmodel import DashboardPanelViewModel
from viewmodels.logs_panel_viewmodel import LogsPanelViewModel
from viewmodels.modules_page_viewmodel import ModulesPageViewModel
from viewmodels.monitor_page_viewmodel import MonitorPageViewModel
from services.speech_pipeline_service import SpeechPipelineService
from services.auth_service import AuthService
from services.game_project_manager import GameProjectManager
from services.game_launcher import GameLauncher
from services.content_manager import ContentManager
from core.module_manager import ModuleManager

def start_ui(rcore_client):
    app = QApplication([])
    window = QMainWindow()
    tab_widget = QTabWidget()

    # Initialize services
    speech_pipeline = SpeechPipelineService(rcore_client)
    module_manager = ModuleManager(speech_pipeline)
    auth_service = AuthService(rcore_client)
    
    # Initialize RaOS integration services
    game_project_manager = GameProjectManager(rcore_client, auth_service)
    game_launcher = GameLauncher(rcore_client, auth_service)
    content_manager = ContentManager(rcore_client, auth_service)

    # Dashboard tab (existing)
    dashboard_vm = DashboardPanelViewModel(speech_pipeline)
    logs_vm = LogsPanelViewModel()
    dashboard_panel = DashboardPanel(dashboard_vm, logs_vm)
    tab_widget.addTab(dashboard_panel, "Dashboard")

    # Game Development IDE tab
    game_dev_panel = GameDevPanel(game_project_manager)
    tab_widget.addTab(game_dev_panel, "Game Dev (IDE)")

    # Game Player tab
    game_player_panel = GamePlayerPanel(game_launcher)
    tab_widget.addTab(game_player_panel, "Game Player")

    # Web Browser tab
    web_browser_panel = WebBrowserPanel(auth_service)
    tab_widget.addTab(web_browser_panel, "Web Browser")

    # Content Editor tab
    content_editor_panel = ContentEditorPanel(content_manager)
    tab_widget.addTab(content_editor_panel, "Content Editor")

    # Modules tab (existing)
    modules_vm = ModulesPageViewModel(module_manager)
    tab_widget.addTab(LogsPanel(logs_vm), "Modules")

    # Monitor tab (existing)
    monitor_vm = MonitorPageViewModel(module_manager)
    tab_widget.addTab(LogsPanel(logs_vm), "Monitor")

    window.setCentralWidget(tab_widget)
    window.setWindowTitle("RaStudio.py - RaOS Unified Client")
    window.resize(1280, 800)
    window.show()
    
    # Show authentication dialog on startup
    _show_auth_dialog(window, auth_service)
    
    app.exec()

def _show_auth_dialog(parent, auth_service):
    """Show authentication dialog on startup."""
    reply = QMessageBox.question(
        parent,
        "RaOS Authentication",
        "Would you like to authenticate with RaOS server?",
        QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
    )
    
    if reply == QMessageBox.StandardButton.Yes:
        username, ok1 = QInputDialog.getText(parent, "Login", "Username:")
        if ok1 and username:
            password, ok2 = QInputDialog.getText(parent, "Login", "Password:", echo=QInputDialog.EchoMode.Password)
            if ok2 and password:
                if auth_service.authenticate(username, password):
                    QMessageBox.information(parent, "Success", "Authentication successful!")
                else:
                    QMessageBox.warning(parent, "Failed", "Authentication failed. You can still use the app in offline mode.")