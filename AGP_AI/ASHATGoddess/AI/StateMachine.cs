using System;
using System.Collections.Generic;

namespace ASHATGoddessClient.AI;

/// <summary>
/// State machine for AI behavior management
/// </summary>
public class StateMachine<TState> where TState : notnull
{
    private readonly Dictionary<TState, State<TState>> _states = new();
    private State<TState>? _currentState;
    private TState? _currentStateId;

    /// <summary>
    /// Add a state to the state machine
    /// </summary>
    public void AddState(TState stateId, State<TState> state)
    {
        _states[stateId] = state;
        state.StateMachine = this;
        state.StateId = stateId;
    }

    /// <summary>
    /// Set the initial state
    /// </summary>
    public void SetInitialState(TState stateId)
    {
        if (_states.TryGetValue(stateId, out var state))
        {
            _currentState = state;
            _currentStateId = stateId;
            _currentState.OnEnter();
        }
    }

    /// <summary>
    /// Transition to a new state
    /// </summary>
    public void TransitionTo(TState newStateId)
    {
        if (_states.TryGetValue(newStateId, out var newState))
        {
            _currentState?.OnExit();
            _currentState = newState;
            _currentStateId = newStateId;
            _currentState.OnEnter();
        }
    }

    /// <summary>
    /// Update the current state
    /// </summary>
    public void Update(float deltaTime)
    {
        _currentState?.OnUpdate(deltaTime);
    }

    /// <summary>
    /// Get the current state ID
    /// </summary>
    public TState? GetCurrentState()
    {
        return _currentStateId;
    }
}

/// <summary>
/// Base class for states
/// </summary>
public abstract class State<TState> where TState : notnull
{
    public StateMachine<TState>? StateMachine { get; set; }
    public TState? StateId { get; set; }

    /// <summary>
    /// Called when entering this state
    /// </summary>
    public virtual void OnEnter() { }

    /// <summary>
    /// Called every frame while in this state
    /// </summary>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// Called when exiting this state
    /// </summary>
    public virtual void OnExit() { }

    /// <summary>
    /// Transition to another state
    /// </summary>
    protected void TransitionTo(TState newState)
    {
        StateMachine?.TransitionTo(newState);
    }
}

/// <summary>
/// Example AI states for common behaviors
/// </summary>
public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Flee,
    Investigate
}

/// <summary>
/// Example idle state implementation
/// </summary>
public class IdleState : State<AIState>
{
    private readonly Func<bool> _shouldPatrol;
    private readonly Func<bool> _detectEnemy;
    private float _idleTime;
    private readonly float _maxIdleTime;

    public IdleState(Func<bool> shouldPatrol, Func<bool> detectEnemy, float maxIdleTime = 3.0f)
    {
        _shouldPatrol = shouldPatrol;
        _detectEnemy = detectEnemy;
        _maxIdleTime = maxIdleTime;
    }

    public override void OnEnter()
    {
        _idleTime = 0;
        Console.WriteLine("[AI] Entering Idle State");
    }

    public override void OnUpdate(float deltaTime)
    {
        _idleTime += deltaTime;

        if (_detectEnemy())
        {
            TransitionTo(AIState.Chase);
            return;
        }

        if (_idleTime >= _maxIdleTime && _shouldPatrol())
        {
            TransitionTo(AIState.Patrol);
        }
    }

    public override void OnExit()
    {
        Console.WriteLine("[AI] Exiting Idle State");
    }
}

/// <summary>
/// Example patrol state implementation
/// </summary>
public class PatrolState : State<AIState>
{
    private readonly Func<bool> _detectEnemy;
    private readonly Func<bool> _reachedWaypoint;
    private readonly Action _moveToNextWaypoint;

    public PatrolState(Func<bool> detectEnemy, Func<bool> reachedWaypoint, Action moveToNextWaypoint)
    {
        _detectEnemy = detectEnemy;
        _reachedWaypoint = reachedWaypoint;
        _moveToNextWaypoint = moveToNextWaypoint;
    }

    public override void OnEnter()
    {
        Console.WriteLine("[AI] Entering Patrol State");
        _moveToNextWaypoint();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_detectEnemy())
        {
            TransitionTo(AIState.Chase);
            return;
        }

        if (_reachedWaypoint())
        {
            _moveToNextWaypoint();
        }
    }

    public override void OnExit()
    {
        Console.WriteLine("[AI] Exiting Patrol State");
    }
}

/// <summary>
/// Example chase state implementation
/// </summary>
public class ChaseState : State<AIState>
{
    private readonly Func<bool> _isEnemyInRange;
    private readonly Func<bool> _lostEnemy;
    private readonly Action _chaseEnemy;

    public ChaseState(Func<bool> isEnemyInRange, Func<bool> lostEnemy, Action chaseEnemy)
    {
        _isEnemyInRange = isEnemyInRange;
        _lostEnemy = lostEnemy;
        _chaseEnemy = chaseEnemy;
    }

    public override void OnEnter()
    {
        Console.WriteLine("[AI] Entering Chase State");
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_isEnemyInRange())
        {
            TransitionTo(AIState.Attack);
            return;
        }

        if (_lostEnemy())
        {
            TransitionTo(AIState.Idle);
            return;
        }

        _chaseEnemy();
    }

    public override void OnExit()
    {
        Console.WriteLine("[AI] Exiting Chase State");
    }
}
