using System.Text.Json;

namespace ASHATGoddessClient.Configuration
{
    /// <summary>
    /// Configuration for the ASHAT Host Service
    /// </summary>
    public class AshatHostConfiguration
    {
        public AshatHostSettings AshatHost { get; set; } = new();
        public AshatosEndpointsSettings AshatosEndpoints { get; set; } = new();
        public SessionSettings Session { get; set; } = new();
        public PersonaSettings Persona { get; set; } = new();
        public SearchEngineSettings SearchEngine { get; set; } = new();
        public VisualSettings Visual { get; set; } = new();

        public static AshatHostConfiguration LoadFromFile(string path = "appsettings.json")
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Configuration file not found: {path}");
            }

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AshatHostConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return config ?? new AshatHostConfiguration();
        }

        /// <summary>
        /// Validate configuration settings
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Validate AshatHost settings
            if (string.IsNullOrWhiteSpace(AshatHost.ServerUrl))
            {
                errors.Add("AshatHost.ServerUrl is required");
            }
            else if (!Uri.TryCreate(AshatHost.ServerUrl, UriKind.Absolute, out var serverUri) ||
                     (serverUri.Scheme != "http" && serverUri.Scheme != "https"))
            {
                errors.Add($"AshatHost.ServerUrl must be a valid HTTP/HTTPS URL: {AshatHost.ServerUrl}");
            }

            // Validate Session settings
            if (Session.SessionTimeout <= 0)
            {
                errors.Add("Session.SessionTimeout must be greater than 0");
            }
            if (Session.MaxHistoryLength <= 0)
            {
                errors.Add("Session.MaxHistoryLength must be greater than 0");
            }

            // Validate SearchEngine settings
            if (SearchEngine.Enabled)
            {
                if (string.IsNullOrWhiteSpace(SearchEngine.Provider))
                {
                    errors.Add("SearchEngine.Provider is required when search is enabled");
                }
                else if (!IsValidSearchProvider(SearchEngine.Provider))
                {
                    errors.Add($"SearchEngine.Provider must be one of: duckduckgo, google, bing, custom. Got: {SearchEngine.Provider}");
                }

                // Check API key requirements
                if (SearchEngine.Provider.ToLowerInvariant() == "google" ||
                    SearchEngine.Provider.ToLowerInvariant() == "bing")
                {
                    if (string.IsNullOrWhiteSpace(SearchEngine.ApiKey))
                    {
                        errors.Add($"SearchEngine.ApiKey is required for provider: {SearchEngine.Provider}");
                    }
                }

                // Validate custom endpoint if provider is custom
                if (SearchEngine.Provider.ToLowerInvariant() == "custom")
                {
                    if (string.IsNullOrWhiteSpace(SearchEngine.CustomEndpoint))
                    {
                        errors.Add("SearchEngine.CustomEndpoint is required when provider is 'custom'");
                    }
                    else if (!Uri.TryCreate(SearchEngine.CustomEndpoint, UriKind.Absolute, out var endpointUri) ||
                             (endpointUri.Scheme != "http" && endpointUri.Scheme != "https"))
                    {
                        errors.Add($"SearchEngine.CustomEndpoint must be a valid HTTP/HTTPS URL: {SearchEngine.CustomEndpoint}");
                    }
                }

                // Validate result limit
                if (SearchEngine.ResultLimit <= 0 || SearchEngine.ResultLimit > 100)
                {
                    errors.Add("SearchEngine.ResultLimit must be between 1 and 100");
                }
            }

            return errors;
        }

        private static bool IsValidSearchProvider(string provider)
        {
            var validProviders = new[] { "duckduckgo", "google", "bing", "custom" };
            return validProviders.Contains(provider.ToLowerInvariant());
        }
    }

    public class AshatHostSettings
    {
        public string ServerUrl { get; set; } = "http://agpstudios.online";
        public string Mode { get; set; } = "headless";
        public bool EnableLogging { get; set; } = true;
    }

    public class AshatosEndpointsSettings
    {
        public string LLM { get; set; } = "/api/llm/chat";
        public string TTS { get; set; } = "/api/tts/speak";
        public string ASR { get; set; } = "/api/asr/transcribe";
        public string Memory { get; set; } = "/api/memory";
        public string Health { get; set; } = "/health";
    }

    public class SessionSettings
    {
        public bool PersistentMemory { get; set; } = false;
        public bool ConsentRequired { get; set; } = true;
        public int SessionTimeout { get; set; } = 3600;
        public int MaxHistoryLength { get; set; } = 100;
    }

    public class PersonaSettings
    {
        public string Name { get; set; } = "ASHAT";
        public string Type { get; set; } = "RomanGoddess";
        public string Personality { get; set; } = "wise, playful, mischievous, respectful";
        public string Description { get; set; } = "A charismatic Roman goddess personality";
        public string SystemPrompt { get; set; } = "You are ASHAT, a Roman goddess AI assistant.";
    }

    public class SearchEngineSettings
    {
        /// <summary>
        /// Enable/disable search engine functionality
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Search engine provider: "duckduckgo", "google", "bing", or "custom"
        /// </summary>
        public string Provider { get; set; } = "duckduckgo";

        /// <summary>
        /// API key or token for the search engine (required for Google and Bing)
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// Custom search engine ID (required for Google Custom Search)
        /// </summary>
        public string SearchEngineId { get; set; } = "";

        /// <summary>
        /// Maximum number of search results to return (1-100)
        /// </summary>
        public int ResultLimit { get; set; } = 10;

        /// <summary>
        /// Custom API endpoint URL (used when Provider is "custom")
        /// </summary>
        public string CustomEndpoint { get; set; } = "";

        /// <summary>
        /// Request timeout in seconds for search API calls
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Region/locale for search results (e.g., "en-US", "wt-wt" for no region)
        /// </summary>
        public string Region { get; set; } = "wt-wt";

        /// <summary>
        /// Safe search setting: "off", "moderate", or "strict"
        /// </summary>
        public string SafeSearch { get; set; } = "moderate";
    }

    public class VisualSettings
    {
        /// <summary>
        /// Enable animated effects (glow, pulse, etc.)
        /// </summary>
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// Animation speed multiplier (0.5 = slower, 2.0 = faster)
        /// </summary>
        public double AnimationSpeed { get; set; } = 1.0;

        /// <summary>
        /// Enable ambient visual effects (particles, sparkles, etc.)
        /// </summary>
        public bool EnableAmbientEffects { get; set; } = true;

        /// <summary>
        /// Primary glow color (hex format: #RRGGBB)
        /// </summary>
        public string GlowColor { get; set; } = "#8A2BE2"; // BlueViolet

        /// <summary>
        /// Secondary accent color (hex format: #RRGGBB)
        /// </summary>
        public string AccentColor { get; set; } = "#FFD700"; // Gold

        /// <summary>
        /// Enable transparency effects
        /// </summary>
        public bool EnableTransparency { get; set; } = true;

        /// <summary>
        /// Window opacity (0.0 = fully transparent, 1.0 = fully opaque)
        /// </summary>
        public double WindowOpacity { get; set; } = 0.95;
    }
}
