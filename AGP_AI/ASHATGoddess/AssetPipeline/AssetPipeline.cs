using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASHATGoddessClient.AssetPipeline;

/// <summary>
/// Asset pipeline for advanced asset loading and optimization
/// </summary>
public class AssetPipeline
{
    private readonly Dictionary<string, IAsset> _loadedAssets = new();
    private readonly Dictionary<Type, IAssetLoader> _loaders = new();
    private readonly string _assetRoot;
    private readonly AssetCache _cache;

    public AssetPipeline(string assetRoot = "Assets")
    {
        _assetRoot = assetRoot;
        _cache = new AssetCache();
        
        if (!Directory.Exists(_assetRoot))
        {
            Directory.CreateDirectory(_assetRoot);
        }

        RegisterDefaultLoaders();
    }

    /// <summary>
    /// Register default asset loaders
    /// </summary>
    private void RegisterDefaultLoaders()
    {
        RegisterLoader<TextAsset>(new TextAssetLoader());
        RegisterLoader<JsonAsset>(new JsonAssetLoader());
        RegisterLoader<BinaryAsset>(new BinaryAssetLoader());
    }

    /// <summary>
    /// Register an asset loader for a specific type
    /// </summary>
    public void RegisterLoader<T>(IAssetLoader loader) where T : IAsset
    {
        _loaders[typeof(T)] = loader;
    }

    /// <summary>
    /// Load an asset asynchronously
    /// </summary>
    public async Task<T?> LoadAsync<T>(string path) where T : class, IAsset
    {
        var fullPath = Path.Combine(_assetRoot, path);

        // Check cache first
        if (_cache.TryGet<T>(fullPath, out var cachedAsset))
        {
            return cachedAsset;
        }

        // Load from disk
        if (_loaders.TryGetValue(typeof(T), out var loader))
        {
            try
            {
                var asset = await loader.LoadAsync(fullPath) as T;
                
                if (asset != null)
                {
                    _loadedAssets[fullPath] = asset;
                    _cache.Add(fullPath, asset);
                    Console.WriteLine($"[AssetPipeline] Loaded asset: {path}");
                    return asset;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssetPipeline] Failed to load asset {path}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[AssetPipeline] No loader registered for type {typeof(T).Name}");
        }

        return null;
    }

    /// <summary>
    /// Load an asset synchronously
    /// </summary>
    public T? Load<T>(string path) where T : class, IAsset
    {
        return LoadAsync<T>(path).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Unload an asset
    /// </summary>
    public void Unload(string path)
    {
        var fullPath = Path.Combine(_assetRoot, path);
        
        if (_loadedAssets.Remove(fullPath))
        {
            _cache.Remove(fullPath);
            Console.WriteLine($"[AssetPipeline] Unloaded asset: {path}");
        }
    }

    /// <summary>
    /// Unload all assets
    /// </summary>
    public void UnloadAll()
    {
        _loadedAssets.Clear();
        _cache.Clear();
        Console.WriteLine("[AssetPipeline] Unloaded all assets");
    }

    /// <summary>
    /// Get all loaded assets
    /// </summary>
    public IEnumerable<IAsset> GetLoadedAssets()
    {
        return _loadedAssets.Values;
    }

    /// <summary>
    /// Get asset count
    /// </summary>
    public int GetAssetCount()
    {
        return _loadedAssets.Count;
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStats GetCacheStats()
    {
        return _cache.GetStats();
    }
}

/// <summary>
/// Base interface for all assets
/// </summary>
public interface IAsset
{
    string Path { get; set; }
    long Size { get; set; }
    DateTime LoadedAt { get; set; }
}

/// <summary>
/// Interface for asset loaders
/// </summary>
public interface IAssetLoader
{
    Task<IAsset> LoadAsync(string path);
}

/// <summary>
/// Text asset
/// </summary>
public class TextAsset : IAsset
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LoadedAt { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// JSON asset
/// </summary>
public class JsonAsset : IAsset
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LoadedAt { get; set; }
    public JsonElement Data { get; set; }
}

/// <summary>
/// Binary asset
/// </summary>
public class BinaryAsset : IAsset
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LoadedAt { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Text asset loader
/// </summary>
public class TextAssetLoader : IAssetLoader
{
    public async Task<IAsset> LoadAsync(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        var fileInfo = new FileInfo(path);

        return new TextAsset
        {
            Path = path,
            Size = fileInfo.Length,
            LoadedAt = DateTime.UtcNow,
            Content = content
        };
    }
}

/// <summary>
/// JSON asset loader
/// </summary>
public class JsonAssetLoader : IAssetLoader
{
    public async Task<IAsset> LoadAsync(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<JsonElement>(content);
        var fileInfo = new FileInfo(path);

        return new JsonAsset
        {
            Path = path,
            Size = fileInfo.Length,
            LoadedAt = DateTime.UtcNow,
            Data = data
        };
    }
}

/// <summary>
/// Binary asset loader
/// </summary>
public class BinaryAssetLoader : IAssetLoader
{
    public async Task<IAsset> LoadAsync(string path)
    {
        var data = await File.ReadAllBytesAsync(path);

        return new BinaryAsset
        {
            Path = path,
            Size = data.Length,
            LoadedAt = DateTime.UtcNow,
            Data = data
        };
    }
}

/// <summary>
/// Asset cache for performance optimization
/// </summary>
public class AssetCache
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private long _totalHits;
    private long _totalMisses;

    public void Add<T>(string path, T asset) where T : IAsset
    {
        _cache[path] = new CacheEntry
        {
            Asset = asset,
            LastAccessed = DateTime.UtcNow
        };
    }

    public bool TryGet<T>(string path, out T? asset) where T : class, IAsset
    {
        if (_cache.TryGetValue(path, out var entry))
        {
            entry.LastAccessed = DateTime.UtcNow;
            entry.HitCount++;
            asset = entry.Asset as T;
            _totalHits++;
            return asset != null;
        }

        asset = null;
        _totalMisses++;
        return false;
    }

    public void Remove(string path)
    {
        _cache.Remove(path);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public CacheStats GetStats()
    {
        return new CacheStats
        {
            EntryCount = _cache.Count,
            TotalHits = _totalHits,
            TotalMisses = _totalMisses,
            HitRate = _totalHits + _totalMisses > 0 
                ? (float)_totalHits / (_totalHits + _totalMisses) 
                : 0
        };
    }

    private class CacheEntry
    {
        public IAsset Asset { get; set; } = null!;
        public DateTime LastAccessed { get; set; }
        public int HitCount { get; set; }
    }
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStats
{
    public int EntryCount { get; set; }
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public float HitRate { get; set; }
}
