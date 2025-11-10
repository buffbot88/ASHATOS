using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace ASHATGoddessClient.PluginSystem
{
    /// <summary>
    /// Plugin manager for loading external game logic plugins
    /// </summary>
    public class PluginManager
    {
        private readonly Dictionary<string, IPlugin> _loadedPlugins = new();
        private readonly List<AssemblyLoadContext> _loadContexts = new();
        private readonly string _pluginDirectory;

        public IReadOnlyDictionary<string, IPlugin> LoadedPlugins => _loadedPlugins;

        public event Action<string, IPlugin>? OnPluginLoaded;
        public event Action<string>? OnPluginUnloaded;

        public PluginManager(string pluginDirectory = "Plugins")
        {
            _pluginDirectory = pluginDirectory;

            if (!Directory.Exists(_pluginDirectory))
            {
                Directory.CreateDirectory(_pluginDirectory);
            }
        }

        /// <summary>
        /// Load all plugins from the plugin directory
        /// </summary>
        public void LoadAllPlugins()
        {
            Console.WriteLine($"[PluginManager] Loading plugins from {_pluginDirectory}");

            var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    LoadPlugin(pluginFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginManager] Failed to load plugin {pluginFile}: {ex.Message}");
                }
            }

            Console.WriteLine($"[PluginManager] Loaded {_loadedPlugins.Count} plugins");
        }

        /// <summary>
        /// Load a specific plugin from file
        /// </summary>
        public bool LoadPlugin(string pluginPath)
        {
            try
            {
                var context = new AssemblyLoadContext(pluginPath, isCollectible: true);
                _loadContexts.Add(context);

                var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(pluginPath));
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    var plugin = (IPlugin?)Activator.CreateInstance(type);
                    if (plugin != null)
                    {
                        plugin.OnLoad();
                        _loadedPlugins[plugin.Name] = plugin;
                        OnPluginLoaded?.Invoke(plugin.Name, plugin);
                        Console.WriteLine($"[PluginManager] Loaded plugin: {plugin.Name} v{plugin.Version}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginManager] Error loading plugin {pluginPath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unload a specific plugin
        /// </summary>
        public bool UnloadPlugin(string pluginName)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                try
                {
                    plugin.OnUnload();
                    _loadedPlugins.Remove(pluginName);
                    OnPluginUnloaded?.Invoke(pluginName);
                    Console.WriteLine($"[PluginManager] Unloaded plugin: {pluginName}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginManager] Error unloading plugin {pluginName}: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a loaded plugin by name
        /// </summary>
        public IPlugin? GetPlugin(string name)
        {
            _loadedPlugins.TryGetValue(name, out var plugin);
            return plugin;
        }

        /// <summary>
        /// Get a plugin of a specific type
        /// </summary>
        public T? GetPlugin<T>() where T : class, IPlugin
        {
            return _loadedPlugins.Values.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get all plugins of a specific type
        /// </summary>
        public IEnumerable<T> GetPlugins<T>() where T : class, IPlugin
        {
            return _loadedPlugins.Values.OfType<T>();
        }

        /// <summary>
        /// Update all plugins
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var plugin in _loadedPlugins.Values)
            {
                try
                {
                    plugin.OnUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginManager] Error updating plugin {plugin.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Unload all plugins and cleanup
        /// </summary>
        public void UnloadAll()
        {
            foreach (var plugin in _loadedPlugins.Values.ToList())
            {
                UnloadPlugin(plugin.Name);
            }

            foreach (var context in _loadContexts)
            {
                context.Unload();
            }

            _loadContexts.Clear();
            Console.WriteLine("[PluginManager] All plugins unloaded");
        }
    }

    /// <summary>
    /// Base interface for all plugins
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Plugin name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Plugin author
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Called every frame
        /// </summary>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// Called when the plugin is unloaded
        /// </summary>
        void OnUnload();
    }

    /// <summary>
    /// Base plugin class for easier implementation
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public virtual void OnLoad()
        {
            Console.WriteLine($"[Plugin] {Name} loaded");
        }

        public virtual void OnUpdate(float deltaTime)
        {
            // Override in derived classes if needed
        }

        public virtual void OnUnload()
        {
            Console.WriteLine($"[Plugin] {Name} unloaded");
        }
    }

    /// <summary>
    /// Example game logic plugin interface
    /// </summary>
    public interface IGameLogicPlugin : IPlugin
    {
        void ProcessGameLogic();
    }

    /// <summary>
    /// Example rendering plugin interface
    /// </summary>
    public interface IRenderingPlugin : IPlugin
    {
        void Render();
    }

    /// <summary>
    /// Example AI plugin interface
    /// </summary>
    public interface IAIPlugin : IPlugin
    {
        void ProcessAI();
    }

    /// <summary>
    /// Plugin metadata for marketplace
    /// </summary>
    public class PluginMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public string DownloadUrl { get; set; } = string.Empty;
        public long DownloadCount { get; set; }
        public float Rating { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Plugin marketplace client (placeholder for future implementation)
    /// </summary>
    public class PluginMarketplace
    {
        private readonly string _marketplaceUrl;

        public PluginMarketplace(string marketplaceUrl = "https://plugins.ashat.example.com")
        {
            _marketplaceUrl = marketplaceUrl;
        }

        /// <summary>
        /// Browse available plugins
        /// </summary>
        public List<PluginMetadata> BrowsePlugins(string? searchQuery = null, string? category = null)
        {
            Console.WriteLine($"[Marketplace] Browsing plugins (search: {searchQuery}, category: {category})");
            // Placeholder - would connect to actual marketplace API
            return new List<PluginMetadata>();
        }

        /// <summary>
        /// Download a plugin
        /// </summary>
        public bool DownloadPlugin(string pluginId, string destinationPath)
        {
            Console.WriteLine($"[Marketplace] Downloading plugin {pluginId} to {destinationPath}");
            // Placeholder - would download from marketplace
            return false;
        }

        /// <summary>
        /// Install a downloaded plugin
        /// </summary>
        public bool InstallPlugin(string pluginPath, PluginManager pluginManager)
        {
            Console.WriteLine($"[Marketplace] Installing plugin from {pluginPath}");
            return pluginManager.LoadPlugin(pluginPath);
        }
    }
}
