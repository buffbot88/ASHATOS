// ASHAT Os Session Manager - Client-side session management
(function() {
    // Check if user is logged in and update UI accordingly
    function checkSession() {
        const token = localStorage.getItem('ASHATCore_token') || localStorage.getItem('authToken');
        const username = localStorage.getItem('username');
        const userRole = localStorage.getItem('userRole');
        
        // Update navigation links based on login status
        updateNavigationUI(token, username, userRole);
        
        return { token, username, userRole, isLoggedIn: !!token };
    }
    
    // Update navigation UI to show user info and appropriate links
    function updateNavigationUI(token, username, userRole) {
        // Find all navigation containers
        const navElements = document.querySelectorAll('.nav');
        
        navElements.forEach(nav => {
            // Remove existing session-added elements
            const existingUserInfo = nav.querySelector('.user-info');
            if (existingUserInfo) {
                existingUserInfo.remove();
            }
            
            if (token && username) {
                // User is logged in - show user info and logout
                const loginLink = nav.querySelector('a[href="/login"]');
                if (loginLink) {
                    loginLink.style.display = 'none';
                }
                
                const registerLink = nav.querySelector('a[href="/register"]');
                if (registerLink) {
                    registerLink.style.display = 'none';
                }
                
                // Add user info element
                const userInfo = document.createElement('div');
                userInfo.className = 'user-info';
                userInfo.style.cssText = 'display: flex; align-items: center; gap: 10px;';
                
                const userNameSpan = document.createElement('span');
                userNameSpan.textContent = '👤 ' + username;
                userNameSpan.style.cssText = 'color: #c084fc; padding: 10px; font-weight: 600;';
                
                const logoutLink = document.createElement('a');
                logoutLink.href = '#';
                logoutLink.textContent = 'Logout';
                logoutLink.onclick = function(e) {
                    e.preventDefault();
                    logout();
                };
                logoutLink.style.cssText = 'color: #c084fc; text-decoration: none; padding: 10px 20px; background: rgba(138, 43, 226, 0.2); border: 1px solid rgba(138, 43, 226, 0.4); border-radius: 8px; transition: all 0.3s; font-weight: 600;';
                logoutLink.onmouseover = function() {
                    this.style.background = 'rgba(138, 43, 226, 0.4)';
                    this.style.borderColor = 'rgba(138, 43, 226, 0.6)';
                    this.style.transform = 'translateY(-2px)';
                };
                logoutLink.onmouseout = function() {
                    this.style.background = 'rgba(138, 43, 226, 0.2)';
                    this.style.borderColor = 'rgba(138, 43, 226, 0.4)';
                    this.style.transform = 'translateY(0)';
                };
                
                userInfo.appendChild(userNameSpan);
                userInfo.appendChild(logoutLink);
                
                nav.appendChild(userInfo);
            } else {
                // User is not logged in - show login and register links
                const loginLink = nav.querySelector('a[href="/login"]');
                if (loginLink) {
                    loginLink.style.display = '';
                }
                
                const registerLink = nav.querySelector('a[href="/register"]');
                if (registerLink) {
                    registerLink.style.display = '';
                }
            }
        });
    }
    
    // Logout function
    function logout() {
        // Clear all auth data from localStorage
        localStorage.removeItem('ASHATCore_token');
        localStorage.removeItem('authToken');
        localStorage.removeItem('username');
        localStorage.removeItem('userRole');
        
        // Redirect to home page
        window.location.href = '/';
    }
    
    // Validate token with server
    async function validateToken() {
        const token = localStorage.getItem('ASHATCore_token') || localStorage.getItem('authToken');
        if (!token) {
            return false;
        }
        
        try {
            const response = await fetch('/api/control/stats', {
                headers: { 'Authorization': 'Bearer ' + token }
            });
            
            if (response.status === 401 || response.status === 403) {
                // Token is invalid or expired
                logout();
                return false;
            }
            
            return response.ok;
        } catch (error) {
            console.error('Error validating token:', error);
            return false;
        }
    }
    
    // Make functions globally available
    window.ASHATSession = {
        check: checkSession,
        logout: logout,
        validate: validateToken,
        isLoggedIn: function() {
            return !!localStorage.getItem('ASHATCore_token') || !!localStorage.getItem('authToken');
        },
        getUsername: function() {
            return localStorage.getItem('username');
        },
        getUserRole: function() {
            return localStorage.getItem('userRole');
        },
        getToken: function() {
            return localStorage.getItem('ASHATCore_token') || localStorage.getItem('authToken');
        }
    };
    
    // Auto-check session on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', checkSession);
    } else {
        checkSession();
    }
})();
