using System.Text.Json;
using System.Text.Json.Serialization;

namespace AshatGoddessQ;

/// <summary>
/// Configuration settings for ASHAT GoddessQ CLI
/// </summary>
public class Config
{
    [JsonPropertyName("AIServer")]
    public AIServerSettings AIServer { get; set; } = new();

    [JsonPropertyName("Persona")]
    public PersonaSettings Persona { get; set; } = new();

    [JsonPropertyName("CLI")]
    public CLISettings CLI { get; set; } = new();

    /// <summary>
    /// Load configuration from JSON file
    /// </summary>
    public static Config Load(string configPath = "config.json")
    {
        try
        {
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Configuration file not found: {configPath}");
                Console.WriteLine("Creating default configuration...");
                var defaultConfig = new Config();
                defaultConfig.Save(configPath);
                return defaultConfig;
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return config ?? new Config();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            Console.WriteLine("Using default configuration.");
            return new Config();
        }
    }

    /// <summary>
    /// Save configuration to JSON file
    /// </summary>
    public void Save(string configPath = "config.json")
    {
        try
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(configPath, json);
            Console.WriteLine($"Configuration saved to: {configPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
}

/// <summary>
/// AI Server connection settings
/// </summary>
public class AIServerSettings
{
    [JsonPropertyName("Url")]
    public string Url { get; set; } = "http://localhost:8088";

    [JsonPropertyName("HealthCheckEndpoint")]
    public string HealthCheckEndpoint { get; set; } = "/api/ai/health";

    [JsonPropertyName("ProcessEndpoint")]
    public string ProcessEndpoint { get; set; } = "/api/ai/process";

    [JsonPropertyName("TimeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// ASHAT persona settings
/// </summary>
public class PersonaSettings
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "ASHAT";

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "RomanGoddess";

    [JsonPropertyName("Personality")]
    public string Personality { get; set; } = "wise, playful, mischievous, respectful";

    [JsonPropertyName("SystemPrompt")]
    public string SystemPrompt { get; set; } = "You are ASHAT, a Roman goddess AI assistant. You are wise, playful, and mischievous but always respectful. You speak with divine wisdom yet maintain a warm, approachable demeanor. Use occasional classical references and goddess-like expressions.";
}

/// <summary>
/// CLI display settings
/// </summary>
public class CLISettings
{
    [JsonPropertyName("EnableColors")]
    public bool EnableColors { get; set; } = true;

    [JsonPropertyName("ShowTimestamp")]
    public bool ShowTimestamp { get; set; } = true;

    [JsonPropertyName("MaxHistoryLines")]
    public int MaxHistoryLines { get; set; } = 50;

    [JsonPropertyName("SaveHistoryFile")]
    public string SaveHistoryFile { get; set; } = "ashat-history.txt";
}
