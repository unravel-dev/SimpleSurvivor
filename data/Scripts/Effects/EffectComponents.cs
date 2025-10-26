using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Damage over time effect component - deals continuous damage.
/// </summary>
[ScriptSourceFile]
public class DamageOverTimeComponent : ScriptComponent
{
    [Tooltip("Damage dealt per second")]
    public float damagePerSecond = 10.0f;
    [Tooltip("Total duration of the effect")]
    public float duration = 3.0f;
    [Tooltip("Time remaining for this effect")]
    public float timeRemaining = 3.0f;
    [Tooltip("Entity that applied this effect")]
    public Entity source;
    [Tooltip("Time until next damage tick")]
    public float nextTickTime = 0.0f;
    [Tooltip("How often damage is applied (in seconds)")]
    public float tickInterval = 0.5f;
    
    /// <summary>
    /// Initialize the DOT effect.
    /// </summary>
    /// <param name="dps">Damage per second.</param>
    /// <param name="effectDuration">Duration of the effect.</param>
    /// <param name="sourceEntity">Entity that applied this effect.</param>
    public void Initialize(float dps, float effectDuration, Entity sourceEntity)
    {
        damagePerSecond = dps;
        duration = effectDuration;
        timeRemaining = effectDuration;
        source = sourceEntity;
        nextTickTime = Time.time + tickInterval;
    }
}

/// <summary>
/// Slow effect component - reduces movement speed.
/// </summary>
[ScriptSourceFile]
public class SlowComponent : ScriptComponent
{
    [Tooltip("Speed multiplier (0.5 = half speed)")]
    public float speedMultiplier = 0.5f;
    [Tooltip("Time remaining for this effect")]
    public float timeRemaining = 2.0f;
    [Tooltip("Original speed before slow was applied")]
    public float originalSpeed = 0.0f;
    [Tooltip("Whether the slow has been applied to the entity")]
    public bool isApplied = false;
    
    /// <summary>
    /// Initialize the slow effect.
    /// </summary>
    /// <param name="multiplier">Speed multiplier.</param>
    /// <param name="effectDuration">Duration of the effect.</param>
    /// <param name="currentSpeed">Current speed to store as original.</param>
    public void Initialize(float multiplier, float effectDuration, float currentSpeed)
    {
        speedMultiplier = multiplier;
        timeRemaining = effectDuration;
        originalSpeed = currentSpeed;
        isApplied = false;
    }
}

/// <summary>
/// Stun effect component - disables entity movement and actions.
/// </summary>
[ScriptSourceFile]
public class StunComponent : ScriptComponent
{
    [Tooltip("Time remaining for this effect")]
    public float timeRemaining = 1.0f;
    [Tooltip("Whether the stun has been applied to the entity")]
    public bool isApplied = false;
    
    /// <summary>
    /// Initialize the stun effect.
    /// </summary>
    /// <param name="effectDuration">Duration of the stun.</param>
    public void Initialize(float effectDuration)
    {
        timeRemaining = effectDuration;
        isApplied = false;
    }
}

/// <summary>
/// Poison effect component - deals damage over time with visual effects.
/// </summary>
[ScriptSourceFile]
public class PoisonComponent : ScriptComponent
{
    [Tooltip("Damage dealt per second")]
    public float damagePerSecond = 5.0f;
    [Tooltip("Time remaining for this effect")]
    public float timeRemaining = 5.0f;
    [Tooltip("Entity that applied this effect")]
    public Entity source;
    [Tooltip("Time until next damage tick")]
    public float nextTickTime = 0.0f;
    [Tooltip("How often damage is applied (in seconds)")]
    public float tickInterval = 1.0f;
    
    /// <summary>
    /// Initialize the poison effect.
    /// </summary>
    /// <param name="dps">Damage per second.</param>
    /// <param name="effectDuration">Duration of the effect.</param>
    /// <param name="sourceEntity">Entity that applied this effect.</param>
    public void Initialize(float dps, float effectDuration, Entity sourceEntity)
    {
        damagePerSecond = dps;
        timeRemaining = effectDuration;
        source = sourceEntity;
        nextTickTime = Time.time + tickInterval;
    }
}
