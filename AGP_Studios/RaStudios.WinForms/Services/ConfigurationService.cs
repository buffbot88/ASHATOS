using System;
using System.IO;
using Newtonsoft.Json;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Services
{
    /// <summary>
    /// Service for loading and managing application configuration.
    /// </summary>
    public class ConfigurationService
    {
        private const string ConfigFileName = "config.json";
        private static ConfigurationService? instance;
        private AppConfiguration? configuration;

        public static ConfigurationService Instance => instance ??= new ConfigurationService();

        private ConfigurationService()
        {
        }

        /// <summary>
        /// Loads configuration from config.json file.
        /// </summary>
        public AppConfiguration LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                if (!File.Exists(configPath))
                {
                    // Return default configuration if file doesn't exist
                    configuration = new AppConfiguration();
                    return configuration;
                }

                var jsonContent = File.ReadAllText(configPath);
                configuration = JsonConvert.DeserializeObject<AppConfiguration>(jsonContent) ?? new AppConfiguration();

                return configuration;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves configuration to config.json file.
        /// </summary>
        public void SaveConfiguration(AppConfiguration config)
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
                var jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, jsonContent);
                configuration = config;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public AppConfiguration GetConfiguration()
        {
            if (configuration == null)
            {
                return LoadConfiguration();
            }
            return configuration;
        }
    }
}
