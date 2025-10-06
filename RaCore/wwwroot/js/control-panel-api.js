/**
 * RaCore Control Panel API Client
 * Handles all API communication with the backend
 */

class ControlPanelAPI {
    constructor() {
        this.baseUrl = window.location.origin;
        this.token = localStorage.getItem('auth_token');
    }

    /**
     * Set authentication token
     */
    setToken(token) {
        this.token = token;
        localStorage.setItem('auth_token', token);
    }

    /**
     * Get authentication token
     */
    getToken() {
        return this.token || localStorage.getItem('auth_token');
    }

    /**
     * Clear authentication token
     */
    clearToken() {
        this.token = null;
        localStorage.removeItem('auth_token');
    }

    /**
     * Make authenticated API request
     */
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(url, {
                ...options,
                headers
            });

            if (response.status === 401) {
                this.clearToken();
                throw new Error('Authentication required. Please login.');
            }

            if (response.status === 403) {
                throw new Error('Insufficient permissions for this action.');
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || `Request failed: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    }

    // Dashboard & Stats
    async getStats() {
        return this.request('/api/control/stats');
    }

    // User Management
    async getUsers() {
        return this.request('/api/control/users');
    }

    async createUser(userData) {
        return this.request('/api/control/users', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    }

    async updateUserRole(userId, role) {
        return this.request(`/api/control/users/${userId}/role`, {
            method: 'PUT',
            body: JSON.stringify({ role })
        });
    }

    // License Management
    async getLicenses() {
        return this.request('/api/control/licenses');
    }

    async createLicense(licenseData) {
        return this.request('/api/control/licenses', {
            method: 'POST',
            body: JSON.stringify(licenseData)
        });
    }

    // RaCoin Management
    async getRaCoinBalances() {
        return this.request('/api/control/racoin/balances');
    }

    async topUpRaCoins(userId, amount) {
        return this.request('/api/control/racoin/topup', {
            method: 'POST',
            body: JSON.stringify({ userId, amount })
        });
    }

    // Game Server
    async getGameStats() {
        return this.request('/api/control/game/stats');
    }

    // Authentication (using existing auth endpoints)
    async login(username, password) {
        const response = await this.request('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        if (response.token) {
            this.setToken(response.token);
        }
        return response;
    }

    async getCurrentUser() {
        return this.request('/api/auth/me');
    }

    async logout() {
        try {
            await this.request('/api/auth/logout', { method: 'POST' });
        } finally {
            this.clearToken();
        }
    }
}

// Export as global for use in control panel
window.ControlPanelAPI = ControlPanelAPI;
