using System.Text;
using System.Text.Json;

namespace ASHATGoddessClient.Host
{
    /// <summary>
    /// Client for interacting with ASHATOS endpoints
    /// Handles all AI processing requests to the ASHATOS server
    /// </summary>
    public class AshatosApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly Configuration.AshatosEndpointsSettings _endpoints;
        private readonly bool _enableLogging;

        public AshatosApiClient(string baseUrl, Configuration.AshatosEndpointsSettings endpoints, bool enableLogging = true)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _endpoints = endpoints;
            _enableLogging = enableLogging;
        }

        /// <summary>
        /// Check if the ASHATOS server is healthy
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_endpoints.Health);
                var isHealthy = response.IsSuccessStatusCode;

                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Health check: {(isHealthy ? "OK" : "FAILED")}");
                }

                return isHealthy;
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Health check error: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Send a message to the LLM endpoint and get a response
        /// </summary>
        public async Task<string?> SendLLMRequestAsync(string message, string systemPrompt, string personality)
        {
            try
            {
                var request = new
                {
                    message,
                    systemPrompt,
                    personality,
                    timestamp = DateTime.UtcNow
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_endpoints.LLM, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                    if (result.TryGetProperty("response", out var responseProperty))
                    {
                        return responseProperty.GetString();
                    }
                }
                else if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] LLM request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] LLM request error: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Request TTS (Text-to-Speech) for the given text
        /// </summary>
        public async Task<byte[]?> RequestTTSAsync(string text, string voice = "female")
        {
            try
            {
                var request = new
                {
                    text,
                    voice,
                    speed = 1.0
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_endpoints.TTS, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] TTS request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] TTS request error: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Request ASR (Automatic Speech Recognition) for audio data
        /// </summary>
        public async Task<string?> RequestASRAsync(byte[] audioData, string format = "wav")
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(audioData), "audio", $"audio.{format}");
                content.Add(new StringContent(format), "format");

                var response = await _httpClient.PostAsync(_endpoints.ASR, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                    if (result.TryGetProperty("transcription", out var transcription))
                    {
                        return transcription.GetString();
                    }
                }
                else if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] ASR request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] ASR request error: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Store a memory in the vector database
        /// </summary>
        public async Task<bool> StoreMemoryAsync(string sessionId, string content, Dictionary<string, string>? metadata = null)
        {
            try
            {
                var request = new
                {
                    sessionId,
                    content,
                    metadata = metadata ?? new Dictionary<string, string>(),
                    timestamp = DateTime.UtcNow
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_endpoints.Memory}/store", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    if (_enableLogging)
                    {
                        Console.WriteLine($"[AshatosApiClient] Memory stored successfully");
                    }
                    return true;
                }
                else if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Store memory failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Store memory error: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieve relevant memories from the vector database
        /// </summary>
        public async Task<List<string>> RetrieveMemoriesAsync(string sessionId, string query, int limit = 5)
        {
            try
            {
                var request = new
                {
                    sessionId,
                    query,
                    limit
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_endpoints.Memory}/retrieve", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                    if (result.TryGetProperty("memories", out var memories))
                    {
                        var memoryList = new List<string>();
                        foreach (var memory in memories.EnumerateArray())
                        {
                            if (memory.TryGetProperty("content", out var contentProp))
                            {
                                var memContent = contentProp.GetString();
                                if (memContent != null)
                                {
                                    memoryList.Add(memContent);
                                }
                            }
                        }
                        return memoryList;
                    }
                }
                else if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Retrieve memories failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                {
                    Console.WriteLine($"[AshatosApiClient] Retrieve memories error: {ex.Message}");
                }
            }

            return new List<string>();
        }
    }
}
