namespace LegendaryClientBuilder.Configuration;

/// <summary>
/// Configuration for Legendary Client Builder.
/// </summary>
public interface IClientBuilderConfiguration
{
    string Environment { get; }
    string OutputPath { get; }
    bool EnableCustomTemplates { get; }
    int MaxClientsPerUser { get; }
    T GetValue<T>(string key, T defaultValue);
}

/// <summary>
/// Implementation of client builder configuration.
/// </summary>
public class ClientBuilderConfiguration : IClientBuilderConfiguration
{
    private readonly Dictionary<string, object> _settings;
    private readonly string _configPath;

    public string Environment { get; private set; }
    public string OutputPath { get; private set; }
    public bool EnableCustomTemplates { get; private set; }
    public int MaxClientsPerUser { get; private set; }

    public ClientBuilderConfiguration(string configPath)
    {
        _configPath = configPath;
        _settings = new Dictionary<string, object>();

        // Default values
        Environment = "Production";
        OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "GameClients");
        EnableCustomTemplates = true;
        MaxClientsPerUser = 10;

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                // Simple JSON parsing for basic settings
                // In production, you'd use System.Text.Json or Newtonsoft.Json
            }
            catch
            {
                // Use defaults if config file can't be read
            }
        }
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
}
