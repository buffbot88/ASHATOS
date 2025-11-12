using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AshatGoddessQ;

/// <summary>
/// AI Client for communicating with ASHAT AI Server
/// </summary>
public class AIClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly AIServerSettings _settings;
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public AIClient(AIServerSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.Url),
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };
    }

    /// <summary>
    /// Check if the AI server is available
    /// </summary>
    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_settings.HealthCheckEndpoint);
            _isConnected = response.IsSuccessStatusCode;
            return _isConnected;
        }
        catch (Exception)
        {
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Process a message through the AI server
    /// </summary>
    public async Task<string?> ProcessMessageAsync(string message, string? modelName = null)
    {
        try
        {
            var request = new
            {
                prompt = message,
                modelName = modelName
            };

            var response = await _httpClient.PostAsJsonAsync(_settings.ProcessEndpoint, request);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (result.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                if (result.TryGetProperty("response", out var responseText))
                {
                    return responseText.GetString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get server status information
    /// </summary>
    public async Task<string> GetServerStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/ai/status");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return "Unable to retrieve server status";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
