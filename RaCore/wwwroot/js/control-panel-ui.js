/**
 * RaCore Control Panel UI Manager
 * Handles UI rendering and user interactions
 */

class ControlPanelUI {
    constructor(api) {
        this.api = api;
        this.currentUser = null;
        this.currentSection = 'dashboard';
    }

    /**
     * Initialize the control panel
     */
    async init() {
        try {
            // Check authentication
            const token = this.api.getToken();
            if (!token) {
                this.showLogin();
                return;
            }

            // Get current user
            this.currentUser = await this.api.getCurrentUser();
            this.renderHeader();
            this.renderDashboard();
            this.setupEventListeners();
        } catch (error) {
            console.error('Init error:', error);
            this.showLogin();
        }
    }

    /**
     * Show login form
     */
    showLogin() {
        document.body.innerHTML = `
            <div class="login-container">
                <div class="login-box">
                    <h1>RaCore Control Panel</h1>
                    <h3>Administrator Login</h3>
                    <form id="loginForm">
                        <input type="text" id="username" placeholder="Username" required>
                        <input type="password" id="password" placeholder="Password" required>
                        <button type="submit">Login</button>
                        <div id="loginError" class="error-message"></div>
                    </form>
                </div>
            </div>
        `;

        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const errorDiv = document.getElementById('loginError');

            try {
                await this.api.login(username, password);
                location.reload();
            } catch (error) {
                errorDiv.textContent = error.message;
                errorDiv.style.display = 'block';
            }
        });
    }

    /**
     * Render header with user info
     */
    renderHeader() {
        const headerHtml = `
            <div class="header">
                <h1>RaCore Control Panel</h1>
                <div class="user-info">
                    <span class="role-badge">${this.currentUser.role}</span>
                    <span>${this.currentUser.username}</span>
                    <button id="logoutBtn" class="btn-secondary">Logout</button>
                </div>
            </div>
        `;
        document.querySelector('.container').insertAdjacentHTML('afterbegin', headerHtml);
        
        document.getElementById('logoutBtn').addEventListener('click', async () => {
            await this.api.logout();
            location.reload();
        });
    }

    /**
     * Setup navigation and event listeners
     */
    setupEventListeners() {
        // Sidebar navigation
        document.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', (e) => {
                const section = e.target.dataset.section;
                this.switchSection(section);
            });
        });
    }

    /**
     * Switch between sections
     */
    switchSection(section) {
        this.currentSection = section;
        
        // Update active nav item
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
            if (item.dataset.section === section) {
                item.classList.add('active');
            }
        });

        // Render section content
        switch (section) {
            case 'dashboard':
                this.renderDashboard();
                break;
            case 'users':
                this.renderUserManagement();
                break;
            case 'licenses':
                this.renderLicenseManagement();
                break;
            case 'racoin':
                this.renderRaCoinManagement();
                break;
            case 'game':
                this.renderGameServerConfig();
                break;
            case 'forum':
                this.renderForumModeration();
                break;
            default:
                this.renderDashboard();
        }
    }

    /**
     * Render dashboard
     */
    async renderDashboard() {
        const content = document.getElementById('content');
        content.innerHTML = '<div class="loading">Loading dashboard...</div>';

        try {
            const stats = await this.api.getStats();
            content.innerHTML = `
                <h2>Dashboard</h2>
                <div class="stats-grid">
                    <div class="stat-card">
                        <h3>Total Users</h3>
                        <div class="stat-value">${stats.totalUsers || 0}</div>
                    </div>
                    <div class="stat-card">
                        <h3>Active Licenses</h3>
                        <div class="stat-value">${stats.activeLicenses || 0}</div>
                    </div>
                    <div class="stat-card">
                        <h3>Total RaCoins</h3>
                        <div class="stat-value">${stats.totalRaCoins || 0}</div>
                    </div>
                    <div class="stat-card">
                        <h3>Game Scenes</h3>
                        <div class="stat-value">${stats.gameScenes || 0}</div>
                    </div>
                </div>
                <div class="quick-actions">
                    <h3>Quick Actions</h3>
                    <button class="btn-primary" onclick="ui.switchSection('users')">Manage Users</button>
                    <button class="btn-primary" onclick="ui.switchSection('licenses')">Manage Licenses</button>
                    <button class="btn-primary" onclick="ui.switchSection('racoin')">Manage RaCoins</button>
                </div>
            `;
        } catch (error) {
            this.showError(content, error.message);
        }
    }

    /**
     * Render user management
     */
    async renderUserManagement() {
        const content = document.getElementById('content');
        content.innerHTML = '<div class="loading">Loading users...</div>';

        try {
            const users = await this.api.getUsers();
            let html = `
                <h2>User Management</h2>
                <div class="section-actions">
                    <button class="btn-primary" onclick="ui.showCreateUserForm()">Create New User</button>
                </div>
                <div id="createUserForm" style="display: none;"></div>
                <div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Username</th>
                                <th>Role</th>
                                <th>Created</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            users.forEach(user => {
                html += `
                    <tr>
                        <td>${user.username}</td>
                        <td><span class="role-badge">${user.role}</span></td>
                        <td>${new Date(user.createdAt).toLocaleDateString()}</td>
                        <td>
                            <button class="btn-small" onclick="ui.editUser('${user.id}')">Edit</button>
                        </td>
                    </tr>
                `;
            });

            html += `
                        </tbody>
                    </table>
                </div>
            `;

            content.innerHTML = html;
        } catch (error) {
            this.showError(content, error.message);
        }
    }

    /**
     * Show create user form
     */
    showCreateUserForm() {
        const formDiv = document.getElementById('createUserForm');
        formDiv.style.display = 'block';
        formDiv.innerHTML = `
            <div class="form-card">
                <h3>Create New User</h3>
                <form id="newUserForm">
                    <input type="text" id="newUsername" placeholder="Username" required>
                    <input type="password" id="newPassword" placeholder="Password" required>
                    <select id="newUserRole" required>
                        <option value="User">User</option>
                        <option value="Admin">Admin</option>
                        <option value="SuperAdmin">Super Admin</option>
                        <option value="GameMaster">Game Master</option>
                        <option value="GameMonitor">Game Monitor</option>
                        <option value="ForumModerator">Forum Moderator</option>
                    </select>
                    <div class="form-actions">
                        <button type="submit" class="btn-primary">Create User</button>
                        <button type="button" class="btn-secondary" onclick="document.getElementById('createUserForm').style.display='none'">Cancel</button>
                    </div>
                    <div id="createUserError" class="error-message"></div>
                </form>
            </div>
        `;

        document.getElementById('newUserForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const errorDiv = document.getElementById('createUserError');
            
            try {
                const userData = {
                    username: document.getElementById('newUsername').value,
                    password: document.getElementById('newPassword').value,
                    role: document.getElementById('newUserRole').value
                };

                await this.api.createUser(userData);
                this.showSuccess('User created successfully!');
                this.renderUserManagement();
            } catch (error) {
                errorDiv.textContent = error.message;
                errorDiv.style.display = 'block';
            }
        });
    }

    /**
     * Render license management
     */
    async renderLicenseManagement() {
        const content = document.getElementById('content');
        content.innerHTML = '<div class="loading">Loading licenses...</div>';

        try {
            const licenses = await this.api.getLicenses();
            content.innerHTML = `
                <h2>License Management</h2>
                <div class="section-actions">
                    <button class="btn-primary" onclick="ui.showCreateLicenseForm()">Assign New License</button>
                </div>
                <div id="createLicenseForm" style="display: none;"></div>
                <div class="license-grid">
                    ${licenses.map(license => `
                        <div class="license-card">
                            <h3>${license.type}</h3>
                            <p><strong>Instance:</strong> ${license.instanceName}</p>
                            <p><strong>Key:</strong> ${license.licenseKey}</p>
                            <p><strong>Expires:</strong> ${new Date(license.expiresAt).toLocaleDateString()}</p>
                            <p><strong>Status:</strong> <span class="status-${license.isActive ? 'active' : 'inactive'}">${license.isActive ? 'Active' : 'Inactive'}</span></p>
                        </div>
                    `).join('')}
                </div>
            `;
        } catch (error) {
            this.showError(content, error.message);
        }
    }

    /**
     * Render RaCoin management
     */
    async renderRaCoinManagement() {
        const content = document.getElementById('content');
        content.innerHTML = '<div class="loading">Loading RaCoin data...</div>';

        try {
            const balances = await this.api.getRaCoinBalances();
            content.innerHTML = `
                <h2>RaCoin Management</h2>
                <div class="info-box">
                    <p><strong>Exchange Rate:</strong> 1 USD = 1,000 RaCoins</p>
                </div>
                <div class="section-actions">
                    <button class="btn-primary" onclick="ui.showTopUpForm()">Top Up RaCoins</button>
                </div>
                <div id="topUpForm" style="display: none;"></div>
                <div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>User</th>
                                <th>Balance</th>
                                <th>USD Equivalent</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${balances.map(balance => `
                                <tr>
                                    <td>${balance.username}</td>
                                    <td>${balance.balance} RC</td>
                                    <td>$${(balance.balance / 1000).toFixed(2)}</td>
                                    <td>
                                        <button class="btn-small" onclick="ui.topUpUser('${balance.userId}')">Top Up</button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            `;
        } catch (error) {
            this.showError(content, error.message);
        }
    }

    /**
     * Render game server config
     */
    async renderGameServerConfig() {
        const content = document.getElementById('content');
        content.innerHTML = '<div class="loading">Loading game server data...</div>';

        try {
            const stats = await this.api.getGameStats();
            content.innerHTML = `
                <h2>Game Server Configuration</h2>
                <div class="stats-grid">
                    <div class="stat-card">
                        <h3>Active Scenes</h3>
                        <div class="stat-value">${stats.activeScenes || 0}</div>
                    </div>
                    <div class="stat-card">
                        <h3>Total Entities</h3>
                        <div class="stat-value">${stats.totalEntities || 0}</div>
                    </div>
                    <div class="stat-card">
                        <h3>Uptime</h3>
                        <div class="stat-value">${stats.uptime || '0h'}</div>
                    </div>
                </div>
                <div class="config-section">
                    <h3>Server Settings</h3>
                    <p>Game server configuration coming soon...</p>
                </div>
            `;
        } catch (error) {
            this.showError(content, error.message);
        }
    }

    /**
     * Render forum moderation section
     */
    async renderForumModeration() {
        const content = document.getElementById('content');
        content.innerHTML = `
            <h2>Forum Moderation</h2>
            <div id="forumStats" class="stats-grid"></div>
            <div id="forumPosts"></div>
        `;
        
        try {
            // Load forum stats
            const statsData = await this.api.get('/api/control/forum/stats');
            const statsGrid = document.getElementById('forumStats');
            statsGrid.innerHTML = `
                <div class="stat-card">
                    <h4>Total Posts</h4>
                    <p class="stat-value">${statsData.stats.totalPosts}</p>
                </div>
                <div class="stat-card">
                    <h4>Active Threads</h4>
                    <p class="stat-value">${statsData.stats.activeThreads}</p>
                </div>
                <div class="stat-card">
                    <h4>Locked Threads</h4>
                    <p class="stat-value">${statsData.stats.lockedThreads}</p>
                </div>
                <div class="stat-card">
                    <h4>Banned Users</h4>
                    <p class="stat-value">${statsData.stats.bannedUsers}</p>
                </div>
            `;
            
            // Load forum posts
            const postsData = await this.api.get('/api/control/forum/posts');
            const postsContainer = document.getElementById('forumPosts');
            
            if (postsData.posts && postsData.posts.length > 0) {
                postsContainer.innerHTML = `
                    <table>
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Author</th>
                                <th>Replies</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${postsData.posts.map(post => `
                                <tr>
                                    <td>${post.title}</td>
                                    <td>${post.username}</td>
                                    <td>${post.replyCount}</td>
                                    <td>
                                        ${post.isDeleted ? '<span class="badge badge-error">Deleted</span>' : ''}
                                        ${post.isLocked ? '<span class="badge badge-warning">Locked</span>' : ''}
                                        ${!post.isDeleted && !post.isLocked ? '<span class="badge badge-success">Active</span>' : ''}
                                    </td>
                                    <td>
                                        ${!post.isDeleted ? `
                                            <button onclick="ui.deleteForumPost('${post.id}')">Delete</button>
                                            <button onclick="ui.toggleLockPost('${post.id}', ${!post.isLocked})">${post.isLocked ? 'Unlock' : 'Lock'}</button>
                                        ` : ''}
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                `;
            } else {
                postsContainer.innerHTML = '<p>No posts found.</p>';
            }
        } catch (error) {
            this.showError(content, error.message);
        }
    }
    
    /**
     * Delete a forum post
     */
    async deleteForumPost(postId) {
        const reason = prompt('Enter reason for deletion:');
        if (!reason) return;
        
        try {
            await this.api.delete(`/api/control/forum/posts/${postId}`, { reason });
            this.showSuccess('Post deleted successfully');
            this.renderForumModeration(); // Refresh
        } catch (error) {
            alert('Error: ' + error.message);
        }
    }
    
    /**
     * Toggle post lock status
     */
    async toggleLockPost(postId, locked) {
        try {
            await this.api.put(`/api/control/forum/posts/${postId}/lock`, { locked });
            this.showSuccess(locked ? 'Thread locked' : 'Thread unlocked');
            this.renderForumModeration(); // Refresh
        } catch (error) {
            alert('Error: ' + error.message);
        }
    }

    /**
     * Show error message
     */
    showError(container, message) {
        container.innerHTML = `
            <div class="alert alert-error">
                <strong>Error:</strong> ${message}
            </div>
        `;
    }

    /**
     * Show success message
     */
    showSuccess(message) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-success';
        alert.textContent = message;
        document.body.appendChild(alert);
        
        setTimeout(() => {
            alert.remove();
        }, 3000);
    }
}

// Export as global
window.ControlPanelUI = ControlPanelUI;
