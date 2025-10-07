# ☁️ CloudFlare Proxy & SSL Setup Guide

## Phase 9.3.7: CloudFlare Integration for RaCore

This guide covers setting up CloudFlare as a reverse proxy with SSL/TLS for your RaCore deployment. CloudFlare provides DDoS protection, CDN, and free SSL certificates.

---

## 📋 Prerequisites

Before starting, ensure you have:
- ✅ RaCore server deployed and running (see `LINUX_HOSTING_SETUP.md`)
- ✅ A domain name (pointed to your server's external IP)
- ✅ CloudFlare account (free tier is sufficient)
- ✅ Access to your domain's DNS settings
- ✅ Server with public IP address

---

## Part 1: CloudFlare Account Setup

### Step 1: Create CloudFlare Account

1. Go to [https://dash.cloudflare.com/sign-up](https://dash.cloudflare.com/sign-up)
2. Sign up for a free account
3. Verify your email address

### Step 2: Add Your Domain

1. Click **"Add a Site"** in CloudFlare dashboard
2. Enter your domain name (e.g., `yourdomain.com`)
3. Select the **Free** plan
4. Click **"Continue"**

### Step 3: Review DNS Records

CloudFlare will scan your existing DNS records:
1. Review the imported records
2. Ensure your **A record** points to your server's IP
3. If missing, add:
   ```
   Type: A
   Name: @ (or yourdomain.com)
   IPv4 address: YOUR_SERVER_IP
   Proxy status: Proxied (orange cloud)
   ```
4. Add www subdomain (optional):
   ```
   Type: CNAME
   Name: www
   Target: yourdomain.com
   Proxy status: Proxied (orange cloud)
   ```

### Step 4: Update Nameservers

CloudFlare will provide nameservers (e.g., `bob.ns.cloudflare.com`):
1. Copy the CloudFlare nameservers
2. Log into your domain registrar (where you bought the domain)
3. Replace existing nameservers with CloudFlare's nameservers
4. Save changes (DNS propagation can take 1-24 hours)

### Step 5: Verify Nameserver Change

1. Return to CloudFlare dashboard
2. Click **"Done, check nameservers"**
3. Wait for CloudFlare to verify (you'll receive an email when active)

---

## Part 2: CloudFlare SSL/TLS Configuration

### Step 1: Enable SSL/TLS

1. In CloudFlare dashboard, go to **SSL/TLS** tab
2. Select **SSL/TLS encryption mode**:
   - ✅ **Full (Strict)** - Recommended for production
   - Requires valid SSL certificate on origin server
   - Ensures end-to-end encryption

### Step 2: Configure SSL/TLS Settings

Go to **SSL/TLS** → **Edge Certificates**:

1. **Always Use HTTPS**: ON ✅
   - Automatically redirects HTTP to HTTPS
   
2. **Minimum TLS Version**: TLS 1.2 ✅
   - Ensures modern encryption standards
   
3. **Opportunistic Encryption**: ON ✅
   - Enables TLS for all connections
   
4. **TLS 1.3**: ON ✅
   - Latest and fastest TLS protocol
   
5. **Automatic HTTPS Rewrites**: ON ✅
   - Rewrites HTTP URLs to HTTPS

### Step 3: Enable HSTS (HTTP Strict Transport Security)

1. Go to **SSL/TLS** → **Edge Certificates**
2. Scroll to **HTTP Strict Transport Security (HSTS)**
3. Click **Enable HSTS**
4. Configure settings:
   ```
   Max Age: 12 months (31536000 seconds)
   Include subdomains: YES ✅
   Preload: YES ✅ (optional)
   No-Sniff header: YES ✅
   ```
5. Click **Save**

**⚠️ Warning**: HSTS is permanent! Only enable after verifying HTTPS works.

---

## Part 3: Origin Server SSL Certificate

You have two options for securing traffic between CloudFlare and your server:

### Option A: CloudFlare Origin CA Certificate (Recommended)

CloudFlare provides free SSL certificates for origin servers:

1. In CloudFlare dashboard, go to **SSL/TLS** → **Origin Server**
2. Click **Create Certificate**
3. Select:
   - Key type: **RSA (2048)**
   - Hostnames: `yourdomain.com, *.yourdomain.com`
   - Certificate validity: **15 years**
4. Click **Create**
5. Copy the **Origin Certificate** and **Private Key**

#### Install on Server:

```bash
# Create SSL directory for CloudFlare certificates
sudo mkdir -p /etc/ssl/cloudflare
sudo chmod 700 /etc/ssl/cloudflare

# Save Origin Certificate
sudo nano /etc/ssl/cloudflare/origin-cert.pem
# Paste the Origin Certificate, then save (Ctrl+X, Y, Enter)

# Save Private Key
sudo nano /etc/ssl/cloudflare/origin-key.pem
# Paste the Private Key, then save (Ctrl+X, Y, Enter)

# Set proper permissions
sudo chmod 600 /etc/ssl/cloudflare/origin-cert.pem
sudo chmod 600 /etc/ssl/cloudflare/origin-key.pem
```

### Option B: Let's Encrypt SSL Certificate

If you prefer Let's Encrypt:

```bash
# Temporarily set CloudFlare DNS to "DNS Only" (gray cloud)
# This allows Let's Encrypt to verify your domain

# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Get certificate
sudo certbot certonly --nginx -d yourdomain.com -d www.yourdomain.com

# Certificates will be saved to:
# /etc/letsencrypt/live/yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/yourdomain.com/privkey.pem

# After getting certificate, re-enable CloudFlare proxy (orange cloud)
```

---

## Part 4: Nginx Configuration for CloudFlare

### Step 1: Create CloudFlare-Optimized Nginx Config

Create or update your nginx configuration:

```bash
sudo nano /etc/nginx/sites-available/racore-cloudflare
```

Add the following configuration:

```nginx
# CloudFlare IP ranges for real IP restoration
# Updated list: https://www.cloudflare.com/ips/

set_real_ip_from 173.245.48.0/20;
set_real_ip_from 103.21.244.0/22;
set_real_ip_from 103.22.200.0/22;
set_real_ip_from 103.31.4.0/22;
set_real_ip_from 141.101.64.0/18;
set_real_ip_from 108.162.192.0/18;
set_real_ip_from 190.93.240.0/20;
set_real_ip_from 188.114.96.0/20;
set_real_ip_from 197.234.240.0/22;
set_real_ip_from 198.41.128.0/17;
set_real_ip_from 162.158.0.0/15;
set_real_ip_from 104.16.0.0/13;
set_real_ip_from 104.24.0.0/14;
set_real_ip_from 172.64.0.0/13;
set_real_ip_from 131.0.72.0/22;

# IPv6 ranges
set_real_ip_from 2400:cb00::/32;
set_real_ip_from 2606:4700::/32;
set_real_ip_from 2803:f800::/32;
set_real_ip_from 2405:b500::/32;
set_real_ip_from 2405:8100::/32;
set_real_ip_from 2a06:98c0::/29;
set_real_ip_from 2c0f:f248::/32;

real_ip_header CF-Connecting-IP;
real_ip_recursive on;

# HTTPS Server Block
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    # SSL Configuration (CloudFlare Origin Certificate)
    ssl_certificate /etc/ssl/cloudflare/origin-cert.pem;
    ssl_certificate_key /etc/ssl/cloudflare/origin-key.pem;
    
    # Or use Let's Encrypt:
    # ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    # ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    # SSL Settings
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    
    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # WebSocket timeout settings
    proxy_connect_timeout 7d;
    proxy_send_timeout 7d;
    proxy_read_timeout 7d;

    # Proxy to RaCore backend
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        
        # WebSocket support
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Standard proxy headers
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # CloudFlare headers
        proxy_set_header CF-Ray $http_cf_ray;
        proxy_set_header CF-Connecting-IP $http_cf_connecting_ip;
        proxy_set_header CF-IPCountry $http_cf_ipcountry;
        proxy_set_header CF-Visitor $http_cf_visitor;
    }
}

# HTTP to HTTPS redirect
server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com www.yourdomain.com;
    
    # CloudFlare handles HTTPS redirect, but this adds a safety net
    return 301 https://$server_name$request_uri;
}
```

### Step 2: Enable Configuration

```bash
# Disable old config if exists
sudo rm /etc/nginx/sites-enabled/racore

# Enable CloudFlare config
sudo ln -s /etc/nginx/sites-available/racore-cloudflare /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

---

## Part 5: CloudFlare Page Rules (Optional but Recommended)

Page Rules allow fine-grained control over CloudFlare behavior:

### Step 1: Create Page Rules

Go to **Rules** → **Page Rules** in CloudFlare dashboard:

#### Rule 1: API Endpoint Bypass
```
URL: yourdomain.com/api/*
Settings:
  - Cache Level: Bypass
  - Security Level: High
  - SSL: Full (Strict)
```

#### Rule 2: WebSocket Bypass
```
URL: yourdomain.com/ws
Settings:
  - Cache Level: Bypass
  - Disable Performance: ON
  - SSL: Full (Strict)
```

#### Rule 3: Control Panel Security
```
URL: yourdomain.com/control-panel.html
Settings:
  - Cache Level: Bypass
  - Security Level: High
  - Browser Integrity Check: ON
  - SSL: Full (Strict)
```

#### Rule 4: Static Assets Caching
```
URL: yourdomain.com/*.css
Settings:
  - Cache Level: Standard
  - Browser Cache TTL: 4 hours
```

---

## Part 6: CloudFlare Security Settings

### Step 1: Configure Firewall

Go to **Security** → **WAF** (Web Application Firewall):

1. **Managed Rules**: ON ✅
   - CloudFlare Managed Ruleset: ON
   - CloudFlare OWASP Core Ruleset: ON

2. **Rate Limiting**:
   - Create rule for login protection:
     ```
     If URL path equals /api/auth/login
     Then rate limit: 5 requests per minute
     ```

### Step 2: Enable Bot Fight Mode (Optional)

Go to **Security** → **Bots**:
- **Bot Fight Mode**: OFF ❌
  - RaCore handles bot detection internally (Phase 9.3.7)
  - CloudFlare bot detection may interfere with search engine crawlers

### Step 3: Configure DDoS Protection

Go to **Security** → **DDoS**:
- **HTTP DDoS Attack Protection**: ON ✅ (automatic)
- **Network-layer DDoS Attack Protection**: ON ✅ (automatic)

---

## Part 7: CloudFlare Speed Optimizations

### Step 1: Enable Auto Minify

Go to **Speed** → **Optimization**:

1. **Auto Minify**:
   - JavaScript: ON ✅
   - CSS: ON ✅
   - HTML: ON ✅

2. **Brotli Compression**: ON ✅

### Step 2: Configure Caching

Go to **Caching** → **Configuration**:

1. **Caching Level**: Standard
2. **Browser Cache TTL**: 4 hours
3. **Always Online**: ON ✅

---

## Part 8: Testing Your Setup

### Step 1: Verify SSL/TLS

```bash
# Test SSL certificate
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com

# Should show CloudFlare certificate for edge
# Origin server should use Origin CA or Let's Encrypt
```

### Step 2: Test CloudFlare Headers

```bash
# Check if CloudFlare is active
curl -I https://yourdomain.com

# Look for CloudFlare headers:
# CF-RAY: (CloudFlare request ID)
# CF-Cache-Status: (caching status)
# Server: cloudflare
```

### Step 3: Verify Real IP Detection

```bash
# Check RaCore logs for real IP addresses
sudo journalctl -u racore -n 50 --no-pager

# Should show CF-Connecting-IP instead of CloudFlare proxy IPs
```

### Step 4: Test Bot Detection

```bash
# Test as regular browser (should get access denied)
curl https://yourdomain.com

# Test as Googlebot (should get homepage)
curl -A "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)" https://yourdomain.com
```

---

## Part 9: Monitoring & Maintenance

### CloudFlare Analytics

Monitor your site through CloudFlare dashboard:
- **Analytics** → **Traffic**: View traffic patterns
- **Analytics** → **Security**: Monitor threats blocked
- **Analytics** → **Performance**: Check speed metrics

### Update CloudFlare IP Ranges

CloudFlare IP ranges may change. Update them periodically:

```bash
# Download latest IP ranges
curl https://www.cloudflare.com/ips-v4 -o /tmp/cf-ips-v4.txt
curl https://www.cloudflare.com/ips-v6 -o /tmp/cf-ips-v6.txt

# Update nginx configuration with new ranges
sudo nano /etc/nginx/sites-available/racore-cloudflare
```

---

## 🔧 Troubleshooting

### Issue: "Too Many Redirects"

**Cause**: SSL/TLS mode mismatch between CloudFlare and origin

**Solution**:
1. Go to CloudFlare **SSL/TLS** settings
2. Set to **Full (Strict)**
3. Ensure origin has valid SSL certificate

### Issue: WebSocket Connection Fails

**Cause**: CloudFlare's proxy interfering with WebSocket

**Solution**:
1. Create Page Rule for `/ws` endpoint
2. Set **Cache Level: Bypass**
3. Enable **Disable Performance**

### Issue: API Returns 520 Error

**Cause**: Origin server not responding properly

**Solution**:
```bash
# Check RaCore status
sudo systemctl status racore

# Check nginx status
sudo systemctl status nginx

# View error logs
sudo tail -f /var/log/nginx/error.log
```

### Issue: Real IP Not Detected

**Cause**: Nginx not properly configured for CloudFlare IPs

**Solution**:
1. Verify `set_real_ip_from` directives in nginx config
2. Ensure `real_ip_header CF-Connecting-IP;` is set
3. Update CloudFlare IP ranges

---

## 📚 Additional Resources

- [CloudFlare SSL/TLS Documentation](https://developers.cloudflare.com/ssl/)
- [CloudFlare API Documentation](https://developers.cloudflare.com/api/)
- [CloudFlare IP Ranges](https://www.cloudflare.com/ips/)
- [Search Engine Bot Verification](https://developers.google.com/search/docs/crawling-indexing/robots/verification)
- [Nginx CloudFlare Configuration](https://www.cloudflare.com/integrations/nginx/)

---

## 🎯 Phase 9.3.7 Implementation Notes

This setup completes Phase 9.3.7 requirements:

✅ **Site-wide homepage unified** - Single homepage endpoint with bot filtering
✅ **Search engine bot filtering** - Only crawlers can access homepage
✅ **CloudFlare proxy ready** - Full configuration and documentation
✅ **SSL/TLS support** - Origin certificates and HTTPS configuration
✅ **Internal implementation** - All components configured and ready for deployment

**Next Steps**: Point your domain to the server and follow this guide to activate CloudFlare!
