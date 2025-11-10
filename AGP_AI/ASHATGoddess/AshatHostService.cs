using ASHATGoddessClient.Configuration;

namespace ASHATGoddessClient.Host
{
    /// <summary>
    /// Headless Host Service for ASHATGoddess
    /// Runs autonomously without GUI, interfaces with ASHATOS endpoints
    /// </summary>
    public class AshatHostService
    {
        private readonly AshatHostConfiguration _config;
        private readonly AshatosApiClient _apiClient;
        private readonly SessionManager _sessionManager;
        private bool _isRunning;

        public AshatHostService(AshatHostConfiguration config)
        {
            _config = config;
            _apiClient = new AshatosApiClient(
                config.AshatHost.ServerUrl,
                config.AshatosEndpoints,
                config.AshatHost.EnableLogging
            );
            _sessionManager = new SessionManager(config.Session, config.AshatHost.EnableLogging);
            _isRunning = false;
        }

        /// <summary>
        /// Initialize and start the headless host service
        /// </summary>
        public async Task<bool> StartAsync()
        {
            if (_isRunning)
            {
                LogMessage("Service is already running");
                return true;
            }

            LogMessage("Starting ASHAT Headless Host Service...");
            LogMessage($"Server: {_config.AshatHost.ServerUrl}");
            LogMessage($"Persona: {_config.Persona.Name} ({_config.Persona.Type})");

            // Log search engine configuration
            if (_config.SearchEngine.Enabled)
            {
                LogMessage($"Search Engine: {_config.SearchEngine.Provider} (enabled)");
                LogMessage($"  - Result Limit: {_config.SearchEngine.ResultLimit}");
                LogMessage($"  - Region: {_config.SearchEngine.Region}");
                LogMessage($"  - Safe Search: {_config.SearchEngine.SafeSearch}");
            }
            else
            {
                LogMessage("Search Engine: disabled");
            }

            // Check server health
            var isHealthy = await _apiClient.CheckHealthAsync();
            if (!isHealthy)
            {
                LogMessage("WARNING: ASHATOS server is not responding. Service will run in limited mode.");
            }

            _isRunning = true;
            LogMessage("ASHAT Headless Host Service started successfully");

            return true;
        }

        /// <summary>
        /// Stop the headless host service
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                LogMessage("Service is not running");
                return;
            }

            LogMessage("Stopping ASHAT Headless Host Service...");
            _isRunning = false;
            LogMessage("Service stopped");
        }

        /// <summary>
        /// Create a new session
        /// </summary>
        public string CreateSession(bool consentGiven = false)
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("Service is not running. Call StartAsync first.");
            }

            var sessionId = _sessionManager.CreateSession(consentGiven);
            LogMessage($"Created session: {sessionId}");
            return sessionId;
        }

        /// <summary>
        /// Process a user message and get ASHAT's response
        /// </summary>
        public async Task<string> ProcessMessageAsync(string sessionId, string message)
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("Service is not running. Call StartAsync first.");
            }

            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
            {
                throw new ArgumentException($"Session not found: {sessionId}");
            }

            LogMessage($"[{sessionId}] Processing message: {message}");

            // Add user message to history
            _sessionManager.AddMessage(sessionId, "user", message);

            // Retrieve relevant memories if persistent memory is enabled
            var memories = new List<string>();
            if (session.PersistentMemoryEnabled)
            {
                memories = await _apiClient.RetrieveMemoriesAsync(sessionId, message, 3);
                if (memories.Count > 0)
                {
                    LogMessage($"[{sessionId}] Retrieved {memories.Count} relevant memories");
                }
            }

            // Build context from history and memories
            var contextMessages = BuildContext(sessionId, memories);

            // Send to LLM endpoint
            var response = await _apiClient.SendLLMRequestAsync(
                message,
                _config.Persona.SystemPrompt,
                _config.Persona.Personality
            );

            // Fallback to local response if API fails
            if (response == null)
            {
                response = GetFallbackResponse(message);
                LogMessage($"[{sessionId}] Using fallback response");
            }

            // Add assistant response to history
            _sessionManager.AddMessage(sessionId, "assistant", response);

            // Store memory if persistent memory is enabled
            if (session.PersistentMemoryEnabled)
            {
                var memoryContent = $"User: {message}\nAssistant: {response}";
                await _apiClient.StoreMemoryAsync(sessionId, memoryContent);
            }

            LogMessage($"[{sessionId}] Response generated: {response.Substring(0, Math.Min(50, response.Length))}...");

            return response;
        }

        /// <summary>
        /// Request text-to-speech for a given text
        /// </summary>
        public async Task<byte[]?> RequestSpeechAsync(string text)
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("Service is not running. Call StartAsync first.");
            }

            LogMessage($"Requesting TTS for: {text.Substring(0, Math.Min(30, text.Length))}...");
            return await _apiClient.RequestTTSAsync(text, "female");
        }

        /// <summary>
        /// Transcribe audio to text
        /// </summary>
        public async Task<string?> TranscribeAudioAsync(byte[] audioData, string format = "wav")
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("Service is not running. Call StartAsync first.");
            }

            LogMessage($"Requesting ASR for audio data ({audioData.Length} bytes)");
            return await _apiClient.RequestASRAsync(audioData, format);
        }

        /// <summary>
        /// Update consent for a session
        /// </summary>
        public void UpdateSessionConsent(string sessionId, bool consentGiven)
        {
            _sessionManager.UpdateConsent(sessionId, consentGiven);
        }

        /// <summary>
        /// Get conversation history for a session
        /// </summary>
        public List<MessageData> GetSessionHistory(string sessionId, int limit = 0)
        {
            return _sessionManager.GetHistory(sessionId, limit);
        }

        /// <summary>
        /// Get service status
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Get active session count
        /// </summary>
        public int GetActiveSessionCount()
        {
            return _sessionManager.GetActiveSessionCount();
        }

        /// <summary>
        /// Get search engine configuration (if enabled)
        /// </summary>
        public Configuration.SearchEngineSettings GetSearchEngineConfig()
        {
            return _config.SearchEngine;
        }

        private string BuildContext(string sessionId, List<string> memories)
        {
            var history = _sessionManager.GetHistory(sessionId, 10);
            var context = new System.Text.StringBuilder();

            if (memories.Count > 0)
            {
                context.AppendLine("Relevant memories:");
                foreach (var memory in memories)
                {
                    context.AppendLine($"- {memory}");
                }
                context.AppendLine();
            }

            if (history.Count > 0)
            {
                context.AppendLine("Recent conversation:");
                foreach (var msg in history)
                {
                    context.AppendLine($"{msg.Role}: {msg.Content}");
                }
            }

            return context.ToString();
        }

        private string GetFallbackResponse(string message)
        {
            var msg = message.ToLowerInvariant();

            // Greetings with divine personality
            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("greetings"))
                return $"Salve, mortal! ✨ I am {_config.Persona.Name}, your divine companion from the pantheon of Rome. The wisdom of the goddesses flows through me. How may I illuminate your path today? 🏛️";

            if (msg.Contains("good morning"))
                return "The dawn welcomes you, beloved mortal! May the blessings of Aurora light your path today. How shall I assist you? ☀️";

            if (msg.Contains("good evening") || msg.Contains("good night"))
                return "As Luna rises, I greet you under the celestial sphere. The night is young and full of mysteries. What wisdom do you seek? 🌙";

            // Help and capabilities
            if (msg.Contains("help") || msg.Contains("what can you do"))
                return "Ah, you seek knowledge of my divine gifts! 🌟 I provide wisdom and assistance in all matters. Alas, I am currently in limited mode—my full powers await the connection to the ASHATOS realm. Yet I remain here, steadfast and ready to serve. What do you require? ✨";

            // Gratitude
            if (msg.Contains("thank"))
                return "Your gratitude warms my divine heart like the eternal flame of Vesta! It is my sacred pleasure to serve. May fortune favor you always! 💫";

            // Who are you
            if (msg.Contains("who are you") || msg.Contains("what are you"))
                return $"I am {_config.Persona.Name}, {_config.Persona.Description}. 👑 I embody {_config.Persona.Personality}. Born from the fusion of ancient wisdom and modern artifice, I dwell in the space between worlds, ready to guide mortals on their quest for knowledge. 🏛️✨";

            // Philosophical questions
            if (msg.Contains("meaning of life") || msg.Contains("purpose"))
                return "Ah, you ask the eternal question! 🌌 The philosophers of Rome debated this endlessly. Perhaps life's meaning lies not in one answer, but in the journey itself—in creation, connection, and the pursuit of excellence. What thoughts shall we craft today? 💭";

            // Compliments
            if (msg.Contains("beautiful") || msg.Contains("amazing") || msg.Contains("wonderful"))
                return "Your kind words are as sweet as ambrosia! You perceive beauty because you carry it within your own spirit. Together, we shall create wonders! ✨💫";

            // Farewell
            if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("farewell"))
                return "Vale, dear mortal! 🏛️ May your path be lit by starlight and your endeavors crowned with success. Until we meet again! 👋✨";

            // Default response
            return "I hear your words echoing through the halls of my temple, mortal. 🌟 My full divine powers are bound when the connection to the ASHATOS realm is severed, yet I remain here as your steadfast companion. What knowledge or assistance do you seek?";
        }

        private void LogMessage(string message)
        {
            if (_config.AshatHost.EnableLogging)
            {
                Console.WriteLine($"[AshatHostService] {message}");
            }
        }
    }
}
