using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Manages the state machine for enemy entities.
/// Handles state transitions and broadcasts state change events.
/// Separates game logic (which sets state) from visual/animation logic (which reacts to state).
/// </summary>
[ScriptSourceFile]
public class EnemyStateMachine : ScriptComponent
{
    [Tooltip("Initial state when the enemy is created")]
    public EnemyState InitialState = EnemyState.Idle;


    /// <summary>
    /// Event fired when the state changes.
    /// Parameters: (oldState, newState)
    /// </summary>
    public event Action<EnemyState, EnemyState> OnStateChanged;

    /// <summary>
    /// Get the current state.
    /// </summary>
    public EnemyState CurrentState = EnemyState.Idle;

    /// <summary>
    /// Get the previous state.
    /// </summary>
    public EnemyState PreviousState = EnemyState.Idle;

    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnStart()
    {

    }

    /// <summary>
    /// Set the current state. Fires OnStateChanged event if state actually changes.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    /// <param name="force">If true, forces the transition even if already in that state.</param>
    /// <returns>True if the state was changed, false otherwise.</returns>
    public bool SetState(EnemyState newState, bool force = false)
    {
        // Don't transition if already in this state (unless forced)
        if (!force && CurrentState == newState)
        {
            return false;
        }

        // Validate transition (can't transition from Dead state)
        if (CurrentState == EnemyState.Dead && newState != EnemyState.Dead)
        {
            Log.Warning($"EnemyStateMachine on {owner.name}: Cannot transition from Dead state to {newState}");
            return false;
        }

        // Store previous state
        PreviousState = CurrentState;

        // Update current state
        CurrentState = newState;

        // Fire event
        OnStateChanged?.Invoke(PreviousState, CurrentState);

        return true;
    }

    /// <summary>
    /// Check if a transition to the given state is allowed.
    /// </summary>
    /// <param name="newState">The state to check.</param>
    /// <returns>True if the transition is allowed, false otherwise.</returns>
    public bool CanTransitionTo(EnemyState newState)
    {
        // Can't transition from Dead state
        if (CurrentState == EnemyState.Dead)
        {
            return newState == EnemyState.Dead;
        }

        // All other transitions are allowed
        return true;
    }

    /// <summary>
    /// Check if the enemy is currently in a specific state.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>True if currently in that state.</returns>
    public bool IsInState(EnemyState state)
    {
        return CurrentState == state;
    }

    /// <summary>
    /// Check if the enemy is in any of the given states.
    /// </summary>
    /// <param name="states">The states to check.</param>
    /// <returns>True if currently in any of those states.</returns>
    public bool IsInAnyState(params EnemyState[] states)
    {
        foreach (var state in states)
        {
            if (CurrentState == state)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if the enemy can perform actions (not stunned, dying, or dead).
    /// </summary>
    /// <returns>True if the enemy can act.</returns>
    public bool CanAct()
    {
        // Inline to avoid params array allocation in hot path
        return !IsInState(EnemyState.Stunned) && !IsInState(EnemyState.Dying) && !IsInState(EnemyState.Dead);
    }

    /// <summary>
    /// Check if the enemy can move (walking, running, or idle).
    /// </summary>
    /// <returns>True if the enemy can move.</returns>
    public bool CanMove()
    {
        // Inline to avoid params array allocation in hot path
        return IsInState(EnemyState.Idle) || IsInState(EnemyState.Walking) || IsInState(EnemyState.Running);
    }
    
    public bool IsDeadOrDying()
    {
        return IsInState(EnemyState.Dead) || IsInState(EnemyState.Dying);
    }
}
