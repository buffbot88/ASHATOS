namespace RaCore.Engine;

/// <summary>
/// CloudFlare proxy and SSL configuration helper.
/// Phase 9.3.7: Preparation for CloudFlare integration.
/// </summary>
public class CloudFlareConfig
{
    /// <summary>
    /// Gets recommended CloudFlare proxy settings for RaCore.
    /// </summary>
    public static CloudFlareSettings GetRecommendedSettings()
    {
        return new CloudFlareSettings
        {
            ProxyEnabled = true,
            SslMode = "Full (Strict)",
            AlwaysUseHttps = true,
            AutomaticHttpsRewrites = true,
            MinimumTlsVersion = "1.2",
            OpportunisticEncryption = true,
            Tls13Enabled = true,
            Hsts = new HstsSettings
            {
                Enabled = true,
                MaxAge = 31536000, // 1 year
                IncludeSubdomains = true,
                Preload = true
            },
            IpGeolocation = true,
            WebApplicationFirewall = true,
            RateLimiting = true,
            DdosProtection = true,
            BotFightMode = false, // Disabled since we handle bots ourselves
            CachingLevel = "Standard",
            BrowserCacheTtl = 14400, // 4 hours
            AlwaysOnline = true,
            DevelopmentMode = false
        };
    }

    /// <summary>
    /// Generates CloudFlare Page Rules for RaCore.
    /// </summary>
    public static List<CloudFlarePageRule> GetRecommendedPageRules(string domain)
    {
        return new List<CloudFlarePageRule>
        {
            new CloudFlarePageRule
            {
                Url = $"{domain}/api/*",
                Settings = new Dictionary<string, object>
                {
                    { "cache_level", "bypass" }, // Don't cache API responses
                    { "security_level", "high" },
                    { "ssl", "strict" }
                }
            },
            new CloudFlarePageRule
            {
                Url = $"{domain}/ws",
                Settings = new Dictionary<string, object>
                {
                    { "cache_level", "bypass" }, // Don't cache WebSocket
                    { "disable_performance", true }, // Disable performance features for WebSocket
                    { "ssl", "strict" }
                }
            },
            new CloudFlarePageRule
            {
                Url = $"{domain}/control-panel.html",
                Settings = new Dictionary<string, object>
                {
                    { "cache_level", "bypass" },
                    { "security_level", "high" },
                    { "ssl", "strict" }
                }
            },
            new CloudFlarePageRule
            {
                Url = $"{domain}/*",
                Settings = new Dictionary<string, object>
                {
                    { "cache_level", "standard" },
                    { "browser_cache_ttl", 14400 },
                    { "security_level", "medium" },
                    { "ssl", "strict" }
                }
            }
        };
    }

    /// <summary>
    /// Detects if the current request is coming through CloudFlare proxy.
    /// </summary>
    public static bool IsCloudFlareRequest(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> headers)
    {
        // CloudFlare adds specific headers to all proxied requests
        return headers.ContainsKey("CF-Ray") || 
               headers.ContainsKey("CF-Connecting-IP") ||
               headers.ContainsKey("CF-IPCountry");
    }

    /// <summary>
    /// Gets the real client IP from CloudFlare headers.
    /// </summary>
    public static string? GetRealClientIp(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> headers)
    {
        // CloudFlare provides the real client IP in this header
        if (headers.TryGetValue("CF-Connecting-IP", out var ip))
        {
            return ip.ToString();
        }
        
        // Fallback to X-Forwarded-For
        if (headers.TryGetValue("X-Forwarded-For", out var forwardedIp))
        {
            var ips = forwardedIp.ToString().Split(',');
            return ips.FirstOrDefault()?.Trim();
        }

        return null;
    }

    /// <summary>
    /// Gets CloudFlare visitor country code if available.
    /// </summary>
    public static string? GetVisitorCountry(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> headers)
    {
        if (headers.TryGetValue("CF-IPCountry", out var country))
        {
            return country.ToString();
        }
        return null;
    }

    /// <summary>
    /// Generates nginx configuration snippet for CloudFlare SSL.
    /// </summary>
    public static string GenerateNginxCloudFlareConfig(string domain)
    {
        return $@"# CloudFlare SSL Configuration for {domain}
# Phase 9.3.7: CloudFlare Proxy Integration

# Restore visitor's real IP from CloudFlare
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
set_real_ip_from 2400:cb00::/32;
set_real_ip_from 2606:4700::/32;
set_real_ip_from 2803:f800::/32;
set_real_ip_from 2405:b500::/32;
set_real_ip_from 2405:8100::/32;
set_real_ip_from 2a06:98c0::/29;
set_real_ip_from 2c0f:f248::/32;

real_ip_header CF-Connecting-IP;
real_ip_recursive on;

server {{
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name {domain};

    # SSL certificates (will be provided by CloudFlare)
    ssl_certificate /etc/ssl/cloudflare/{domain}.pem;
    ssl_certificate_key /etc/ssl/cloudflare/{domain}.key;
    
    # Or use CloudFlare Origin CA Certificate
    # ssl_certificate /etc/ssl/cloudflare/origin-cert.pem;
    # ssl_certificate_key /etc/ssl/cloudflare/origin-key.pem;

    # SSL configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    
    # Security headers
    add_header Strict-Transport-Security ""max-age=31536000; includeSubDomains; preload"" always;
    add_header X-Frame-Options ""SAMEORIGIN"" always;
    add_header X-Content-Type-Options ""nosniff"" always;
    add_header X-XSS-Protection ""1; mode=block"" always;

    # Proxy to RaCore
    location / {{
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection ""upgrade"";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # CloudFlare headers
        proxy_set_header CF-Ray $http_cf_ray;
        proxy_set_header CF-Connecting-IP $http_cf_connecting_ip;
        proxy_set_header CF-IPCountry $http_cf_ipcountry;
    }}
}}

# Redirect HTTP to HTTPS
server {{
    listen 80;
    listen [::]:80;
    server_name {domain};
    return 301 https://$server_name$request_uri;
}}
";
    }
}

/// <summary>
/// CloudFlare configuration settings.
/// </summary>
public class CloudFlareSettings
{
    public bool ProxyEnabled { get; set; }
    public string SslMode { get; set; } = string.Empty;
    public bool AlwaysUseHttps { get; set; }
    public bool AutomaticHttpsRewrites { get; set; }
    public string MinimumTlsVersion { get; set; } = string.Empty;
    public bool OpportunisticEncryption { get; set; }
    public bool Tls13Enabled { get; set; }
    public HstsSettings Hsts { get; set; } = new();
    public bool IpGeolocation { get; set; }
    public bool WebApplicationFirewall { get; set; }
    public bool RateLimiting { get; set; }
    public bool DdosProtection { get; set; }
    public bool BotFightMode { get; set; }
    public string CachingLevel { get; set; } = string.Empty;
    public int BrowserCacheTtl { get; set; }
    public bool AlwaysOnline { get; set; }
    public bool DevelopmentMode { get; set; }
}

/// <summary>
/// HSTS (HTTP Strict Transport Security) settings.
/// </summary>
public class HstsSettings
{
    public bool Enabled { get; set; }
    public int MaxAge { get; set; }
    public bool IncludeSubdomains { get; set; }
    public bool Preload { get; set; }
}

/// <summary>
/// CloudFlare Page Rule configuration.
/// </summary>
public class CloudFlarePageRule
{
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
}
