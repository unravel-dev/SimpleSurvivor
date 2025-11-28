using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base class for all time-based effects (damage over time, stuns, buffs, etc.).
/// Handles duration tracking and expiration.
/// </summary>
[ScriptSourceFile]
public abstract class EffectOverTime : ScriptComponent
{
    [Tooltip("Duration of the effect in seconds")]
    public float duration = 5.0f;
    
    // Internal state
    protected float remainingDuration;
    protected bool isExpired = false;
    protected Entity sourceEntity;
    
    /// <summary>
    /// Initialize the effect with duration.
    /// </summary>
    /// <param name="source">Entity that caused this effect.</param>
    /// <param name="effectDuration">Duration in seconds.</param>
    public virtual void Initialize(Entity source, float effectDuration)
    {
        sourceEntity = source;
        duration = effectDuration;
        remainingDuration = effectDuration;
        isExpired = false;
    }
    
    /// <summary>
    /// Update the effect. Called by EffectsSystem each frame.
    /// Handles duration tracking.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public virtual void Update(float deltaTime)
    {
        if (isExpired)
            return;
        
        // Update duration
        remainingDuration -= deltaTime;
        if (remainingDuration <= 0.0f)
        {
            isExpired = true;
            OnExpired();
            return;
        }
    }
    
    /// <summary>
    /// Refresh the effect duration (reset to full duration).
    /// </summary>
    /// <param name="newDuration">Optional new duration, uses current duration if not specified.</param>
    public void Refresh(float? newDuration = null)
    {
        if (newDuration.HasValue)
        {
            duration = newDuration.Value;
        }
        
        remainingDuration = duration;
        isExpired = false;
    }
    
    /// <summary>
    /// Check if this effect has expired.
    /// </summary>
    /// <returns>True if expired.</returns>
    public bool IsExpired()
    {
        return isExpired;
    }
    
    /// <summary>
    /// Mark this effect as expired (will be removed on next system tick).
    /// </summary>
    public void MarkAsExpired()
    {
        isExpired = true;
    }
    
    /// <summary>
    /// Get the remaining duration.
    /// </summary>
    /// <returns>Remaining duration in seconds.</returns>
    public float GetRemainingDuration()
    {
        return Mathf.Max(0.0f, remainingDuration);
    }
    
    /// <summary>
    /// Get the source entity that caused this effect.
    /// </summary>
    /// <returns>Source entity.</returns>
    public Entity GetSource()
    {
        return sourceEntity;
    }
    
    /// <summary>
    /// Get the effect name (for UI/debugging).
    /// </summary>
    /// <returns>Name of the effect.</returns>
    public abstract string GetEffectName();
    
    /// <summary>
    /// Get the effect color (for UI visualization).
    /// </summary>
    /// <returns>Color as RGBA string.</returns>
    public abstract string GetEffectColor();
    
    /// <summary>
    /// Called when the effect expires.
    /// Override to add custom behavior.
    /// </summary>
    public virtual void OnExpired()
    {
        // Override in derived classes for custom behavior
    }
}

