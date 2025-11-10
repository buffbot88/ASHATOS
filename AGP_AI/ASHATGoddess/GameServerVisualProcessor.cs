using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abstractions;
using LegendaryGameSystem.Core;

namespace ASHATGoddessClient;

/// <summary>
/// GameServer Visual Processor - Integrates GameServer module for visual processing
/// Uses the GameServer's scene and entity system to manage ASHAT's visual representation
/// </summary>
public class GameServerVisualProcessor : IDisposable
{
    private readonly LegendaryGameEngineModule _gameEngine;
    private string? _goddessSceneId;
    private string? _goddessEntityId;
    private bool _isInitialized;

    public GameServerVisualProcessor()
    {
        _gameEngine = new LegendaryGameEngineModule();
        Console.WriteLine("[GameServerVisualProcessor] Initializing GameServer module for visual processing");
    }

    /// <summary>
    /// Initialize the GameServer visual processor
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            // Initialize the game engine module
            _gameEngine.Initialize(null);
            
            // Create a scene for the goddess visual representation
            var sceneResponse = await _gameEngine.CreateSceneAsync("ASHAT Goddess Visual Scene", "ASHATGoddess");
            
            if (sceneResponse.Success && sceneResponse.Data is GameScene scene)
            {
                _goddessSceneId = scene.Id;
                Console.WriteLine($"[GameServerVisualProcessor] Created goddess scene: {_goddessSceneId}");
                
                // Create the goddess entity in the scene
                var goddessEntity = new GameEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "ASHAT Goddess",
                    Type = "Character",
                    Position = new Vector3 { X = 0, Y = 0, Z = 0 },
                    Scale = new Vector3 { X = 1, Y = 1, Z = 1 },
                    Rotation = new Vector3 { X = 0, Y = 0, Z = 0 },
                    Properties = new Dictionary<string, object>
                    {
                        ["tags"] = new List<string> { "goddess", "ashat", "main-character" },
                        ["appearance"] = "Roman Goddess with golden aura",
                        ["animation_state"] = "idle",
                        ["glow_intensity"] = 0.85,
                        ["crown_sparkle"] = true
                    }
                };
                
                var entityResponse = await _gameEngine.CreateEntityAsync(_goddessSceneId, goddessEntity, "ASHATGoddess");
                
                if (entityResponse.Success)
                {
                    _goddessEntityId = goddessEntity.Id;
                    Console.WriteLine($"[GameServerVisualProcessor] Created goddess entity: {_goddessEntityId}");
                    _isInitialized = true;
                    return true;
                }
                else
                {
                    Console.WriteLine($"[GameServerVisualProcessor] Failed to create goddess entity: {entityResponse.Message}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"[GameServerVisualProcessor] Failed to create scene: {sceneResponse.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServerVisualProcessor] Error during initialization: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update the goddess visual state through the GameServer
    /// </summary>
    public async Task<bool> UpdateVisualStateAsync(string animationState, Dictionary<string, object>? properties = null)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_goddessSceneId) || string.IsNullOrEmpty(_goddessEntityId))
        {
            Console.WriteLine("[GameServerVisualProcessor] Processor not initialized");
            return false;
        }

        try
        {
            // Get the current entity
            var entities = await _gameEngine.ListEntitiesAsync(_goddessSceneId);
            var goddessEntity = entities.FirstOrDefault(e => e.Id == _goddessEntityId);
            
            if (goddessEntity == null)
            {
                Console.WriteLine("[GameServerVisualProcessor] Goddess entity not found");
                return false;
            }

            // Update animation state
            goddessEntity.Properties["animation_state"] = animationState;
            
            // Update additional properties if provided
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    goddessEntity.Properties[prop.Key] = prop.Value;
                }
            }

            // Update the entity in the GameServer
            var updateResponse = await _gameEngine.UpdateEntityAsync(_goddessSceneId, _goddessEntityId, goddessEntity, "ASHATGoddess");
            
            if (updateResponse.Success)
            {
                Console.WriteLine($"[GameServerVisualProcessor] Updated visual state to: {animationState}");
                return true;
            }
            else
            {
                Console.WriteLine($"[GameServerVisualProcessor] Failed to update visual state: {updateResponse.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServerVisualProcessor] Error updating visual state: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get current visual state from GameServer
    /// </summary>
    public async Task<Dictionary<string, object>?> GetVisualStateAsync()
    {
        if (!_isInitialized || string.IsNullOrEmpty(_goddessSceneId) || string.IsNullOrEmpty(_goddessEntityId))
        {
            return null;
        }

        try
        {
            var entities = await _gameEngine.ListEntitiesAsync(_goddessSceneId);
            var goddessEntity = entities.FirstOrDefault(e => e.Id == _goddessEntityId);
            
            return goddessEntity?.Properties;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServerVisualProcessor] Error getting visual state: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get GameServer engine statistics
    /// </summary>
    public async Task<EngineStats?> GetEngineStatsAsync()
    {
        try
        {
            return await _gameEngine.GetStatsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServerVisualProcessor] Error getting engine stats: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if the processor is initialized and working
    /// </summary>
    public bool IsWorking()
    {
        return _isInitialized && !string.IsNullOrEmpty(_goddessSceneId) && !string.IsNullOrEmpty(_goddessEntityId);
    }

    public void Dispose()
    {
        try
        {
            // Clean up the scene when disposing
            if (_isInitialized && !string.IsNullOrEmpty(_goddessSceneId))
            {
                _gameEngine.DeleteSceneAsync(_goddessSceneId, "ASHATGoddess").Wait();
                Console.WriteLine("[GameServerVisualProcessor] Cleaned up goddess scene");
            }
            
            _gameEngine?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServerVisualProcessor] Error during disposal: {ex.Message}");
        }
    }
}
