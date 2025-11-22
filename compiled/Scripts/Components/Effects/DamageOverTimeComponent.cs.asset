using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base class for all damage over time effects.
/// Handles duration, stacking, and damage calculation.
/// The actual damage application is handled by EffectsSystem.
/// </summary>
[ScriptSourceFile]
public abstract class DamageOverTimeComponent : ScriptComponent
{
    [Tooltip("Damage per second")]
    public float damagePerSecond = 5.0f;
    
    [Tooltip("Duration of the effect in seconds")]
    public float duration = 5.0f;
    
    [Tooltip("Current number of stacks")]
    public int currentStacks = 1;
    
    [Tooltip("Maximum number of stacks")]
    public int maxStacks = 1;
    
    [Tooltip("Whether stacks increase damage")]
    public bool stacksDamage = true;
    
    [Tooltip("Whether stacks increase duration")]
    public bool stacksDuration = false;
    
    // Internal state
    private float remainingDuration;
    private Entity sourceEntity;
    private bool isExpired = false;
    private float timeSinceLastTick = 0.0f;
    private const float TICK_INTERVAL = 0.5f; // Apply damage every 0.5 seconds
    
    /// <summary>
    /// Initialize the damage over time effect.
    /// </summary>
    /// <param name="source">Entity that caused this effect.</param>
    /// <param name="dps">Damage per second.</param>
    /// <param name="effectDuration">Duration in seconds.</param>
    /// <param name="maxStackCount">Maximum number of stacks.</param>
    public virtual void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 0)
    {
        sourceEntity = source;
        damagePerSecond = dps;
        duration = effectDuration;
        remainingDuration = effectDuration;
        maxStacks = maxStackCount;
        currentStacks = 1;
        isExpired = false;
        
        // Add DamageSourceComponent to track damage statistics
        // Try to find the ability component from the source entity
        if (owner && source)
        {
            var damageSource = owner.GetComponent<DamageSourceComponent>();
            if (damageSource == null)
            {
                damageSource = owner.AddComponent<DamageSourceComponent>();
            }
            
            if (damageSource != null)
            {
                // Try to find ability component from source entity
                var ability = source.GetComponent<Ability>();
                if (ability != null)
                {
                    damageSource.Initialize(source, ability);
                }
                else
                {
                    // If source is a projectile, check its DamageSourceComponent directly
                    var projectileDamageSource = source.GetComponent<DamageSourceComponent>();
                    if (projectileDamageSource != null)
                    {
                        var projectileAbility = projectileDamageSource.GetAbilityComponent();
                        Entity projectileSource = projectileDamageSource.GetAbilityEntity();
                        if (projectileAbility != null && projectileSource)
                        {
                            damageSource.Initialize(projectileSource, projectileAbility);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Update the effect. Called by EffectsSystem each frame.
    /// Handles duration tracking and damage application.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public void Update(float deltaTime)
    {
        if (isExpired)
            return;
        
        // Update duration
        remainingDuration -= deltaTime;
        if (remainingDuration <= 0.0f)
        {
            isExpired = true;
            return;
        }
        
        // Update tick timer
        timeSinceLastTick += deltaTime;
        
        // Check if it's time to tick (apply damage)
        if (timeSinceLastTick >= TICK_INTERVAL)
        {
            timeSinceLastTick -= TICK_INTERVAL;
            ApplyDamage();
        }
    }
    
    /// <summary>
    /// Apply damage from this effect.
    /// </summary>
    private void ApplyDamage()
    {
        if (!owner)
            return;
        
        // Calculate damage for this tick
        int tickDamage = GetDamagePerTick(TICK_INTERVAL);
        
        if (tickDamage <= 0)
            return;
        
        // Apply damage through DamageSystem
        DamageBreakdown breakdown = UpgradeSystem.CalculateDamage(tickDamage);
        breakdown.color = GetDamageColor();
        DamageSystem.ApplyDamage(owner, sourceEntity, breakdown);
        
        // Call the effect's OnDamageApplied callback
        OnDamageApplied(tickDamage);
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
    /// Add a stack to this effect.
    /// </summary>
    public void AddStack(int count = 1)
    {
        if (maxStacks == 0 || currentStacks < maxStacks)
        {
            currentStacks += count;
            
            if (stacksDuration)
            {
                // Refresh duration when stacking
                Refresh();
            }
        }
        else
        {
            // At max stacks, just refresh duration
            Refresh();
        }
    }
    
    /// <summary>
    /// Get the damage to apply for this tick.
    /// </summary>
    /// <param name="tickInterval">Time interval for this tick.</param>
    /// <returns>Damage amount for this tick.</returns>
    public virtual int GetDamagePerTick(float tickInterval)
    {
        float baseDamage = damagePerSecond * tickInterval;
        
        if (stacksDamage)
        {
            baseDamage *= currentStacks;
        }
        
        return Mathf.RoundToInt(baseDamage);
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

    public abstract Color GetDamageColor();
    
    /// <summary>
    /// Called when damage is applied by this effect.
    /// Override to add custom behavior.
    /// </summary>
    /// <param name="damageAmount">Amount of damage that was applied.</param>
    public virtual void OnDamageApplied(int damageAmount)
    {
        // Override in derived classes for custom behavior
    }
    
    /// <summary>
    /// Called when the effect expires.
    /// Override to add custom behavior.
    /// </summary>
    public virtual void OnExpired()
    {
        // Override in derived classes for custom behavior
    }
}

