using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abstractions;

namespace ASHATGoddessClient;

/// <summary>
/// GoddessAvatar - Represents the goddess character entity with animation state management
/// </summary>
public class GoddessAvatar
{
    private readonly RomanGoddessEngine _engine;
    private string? _entityId;
    private AnimationState _currentState;
    private DateTime _lastStateChange;
    private bool _isInitialized;

    public enum AnimationState
    {
        Idle,
        Talk,
        Listening,
        Thinking,
        Greeting
    }

    public string? EntityId => _entityId;
    public AnimationState CurrentState => _currentState;
    public bool IsInitialized => _isInitialized;

    public event EventHandler<AnimationStateChangedEventArgs>? OnAnimationStateChanged;

    public GoddessAvatar(RomanGoddessEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _currentState = AnimationState.Idle;
        _lastStateChange = DateTime.UtcNow;
        _isInitialized = false;
        
        Console.WriteLine("[GoddessAvatar] Avatar created");
    }

    /// <summary>
    /// Initialize the goddess avatar entity in the engine
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        if (_isInitialized)
        {
            Console.WriteLine("[GoddessAvatar] Avatar already initialized");
            return true;
        }

        try
        {
            Console.WriteLine("[GoddessAvatar] Creating goddess entity...");

            var properties = new Dictionary<string, object>
            {
                ["animation_state"] = AnimationState.Idle.ToString().ToLower(),
                ["appearance"] = "Roman Goddess with golden aura",
                ["glow_intensity"] = 0.85,
                ["crown_sparkle"] = true,
                ["is_speaking"] = false,
                ["last_message"] = string.Empty
            };

            _entityId = await _engine.CreateEntityAsync(
                name: "Goddess Avatar",
                type: "Character",
                position: new Vector3 { X = 0, Y = 0, Z = 0 },
                properties: properties
            );

            if (!string.IsNullOrEmpty(_entityId))
            {
                _isInitialized = true;
                Console.WriteLine($"[GoddessAvatar] ✓ Avatar entity created with ID: {_entityId}");
                return true;
            }
            else
            {
                Console.WriteLine("[GoddessAvatar] Failed to create avatar entity");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GoddessAvatar] Error initializing avatar: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update the avatar - called each frame by the engine
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!_isInitialized)
        {
            return;
        }

        // Auto-return to idle after speaking/greeting animations
        var timeSinceStateChange = (DateTime.UtcNow - _lastStateChange).TotalSeconds;

        switch (_currentState)
        {
            case AnimationState.Talk:
                // Return to idle after 3 seconds of talking
                if (timeSinceStateChange > 3.0)
                {
                    _ = TransitionToAsync(AnimationState.Idle);
                }
                break;

            case AnimationState.Greeting:
                // Return to idle after 2 seconds of greeting
                if (timeSinceStateChange > 2.0)
                {
                    _ = TransitionToAsync(AnimationState.Idle);
                }
                break;

            case AnimationState.Thinking:
                // Return to idle after 4 seconds of thinking
                if (timeSinceStateChange > 4.0)
                {
                    _ = TransitionToAsync(AnimationState.Idle);
                }
                break;
        }
    }

    /// <summary>
    /// Transition the avatar to a new animation state
    /// </summary>
    public async Task<bool> TransitionToAsync(AnimationState newState)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_entityId))
        {
            Console.WriteLine("[GoddessAvatar] Cannot transition: Avatar not initialized");
            return false;
        }

        if (_currentState == newState)
        {
            return true; // Already in this state
        }

        try
        {
            var oldState = _currentState;
            _currentState = newState;
            _lastStateChange = DateTime.UtcNow;

            // Update entity properties in the engine
            var properties = new Dictionary<string, object>
            {
                ["animation_state"] = newState.ToString().ToLower(),
                ["state_changed_at"] = DateTime.UtcNow
            };

            // Add state-specific properties
            switch (newState)
            {
                case AnimationState.Talk:
                    properties["is_speaking"] = true;
                    properties["glow_intensity"] = 0.95;
                    properties["eye_sparkle"] = true;
                    break;
                case AnimationState.Listening:
                    properties["is_speaking"] = false;
                    properties["glow_intensity"] = 0.85;
                    properties["focused"] = true;
                    break;
                case AnimationState.Thinking:
                    properties["is_speaking"] = false;
                    properties["glow_intensity"] = 0.80;
                    properties["crown_sparkle"] = true;
                    break;
                case AnimationState.Greeting:
                    properties["is_speaking"] = false;
                    properties["glow_intensity"] = 1.0;
                    properties["crown_sparkle"] = true;
                    properties["eye_sparkle"] = true;
                    break;
                case AnimationState.Idle:
                    properties["is_speaking"] = false;
                    properties["glow_intensity"] = 0.75;
                    break;
            }

            var success = await _engine.UpdateEntityAsync(_entityId, properties);

            if (success)
            {
                Console.WriteLine($"[GoddessAvatar] Animation state: {oldState} → {newState}");
                OnAnimationStateChanged?.Invoke(this, new AnimationStateChangedEventArgs
                {
                    OldState = oldState,
                    NewState = newState
                });
                return true;
            }
            else
            {
                // Revert state if update failed
                _currentState = oldState;
                Console.WriteLine($"[GoddessAvatar] Failed to transition from {oldState} to {newState}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GoddessAvatar] Error transitioning state: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Make the avatar say something (triggers Talk animation)
    /// </summary>
    public async Task SayAsync(string message)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_entityId))
        {
            Console.WriteLine("[GoddessAvatar] Cannot say: Avatar not initialized");
            return;
        }

        try
        {
            Console.WriteLine($"[GoddessAvatar] Say: \"{message}\"");

            // Transition to Talk state
            await TransitionToAsync(AnimationState.Talk);

            // Update the last message property
            await _engine.UpdateEntityAsync(_entityId, new Dictionary<string, object>
            {
                ["last_message"] = message,
                ["message_time"] = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GoddessAvatar] Error in Say: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the current entity data from the engine
    /// </summary>
    public async Task<GameEntity?> GetEntityAsync()
    {
        if (!_isInitialized || string.IsNullOrEmpty(_entityId))
        {
            return null;
        }

        return await _engine.GetEntityAsync(_entityId);
    }
}

/// <summary>
/// Event args for animation state changes
/// </summary>
public class AnimationStateChangedEventArgs : EventArgs
{
    public GoddessAvatar.AnimationState OldState { get; set; }
    public GoddessAvatar.AnimationState NewState { get; set; }
}
