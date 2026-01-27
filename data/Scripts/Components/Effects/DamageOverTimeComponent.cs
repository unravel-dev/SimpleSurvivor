using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base class for all damage over time effects.
/// Handles stacking and damage calculation.
/// The actual damage application is handled by EffectsSystem.
/// Duration tracking is handled by EffectOverTime base class.
/// </summary>
[ScriptSourceFile]
public abstract class DamageOverTimeComponent : EffectOverTime
{
    [Tooltip("Damage per second")]
    public float damagePerSecond = 5.0f;
    
    [Tooltip("Current number of stacks")]
    public int currentStacks = 1;
    
    [Tooltip("Maximum number of stacks")]
    public int maxStacks = 1;
    
    [Tooltip("Whether stacks increase damage")]
    public bool stacksDamage = true;
    
    [Tooltip("Whether stacks increase duration")]
    public bool stacksDuration = false;
    
    // Internal state
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
        base.Initialize(source, effectDuration);
        damagePerSecond = dps;
        maxStacks = maxStackCount;
        currentStacks = 1;
        timeSinceLastTick = 0.0f;
        
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
    public override void Update(float deltaTime)
    {
        // Call base class update for duration tracking
        base.Update(deltaTime);
        
        if (IsExpired())
            return;
        
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
    /// Override in derived classes to customize damage application behavior.
    /// </summary>
    protected virtual void ApplyDamage()
    {
        if (!owner)
            return;
        
        // Calculate damage for this tick
        int tickDamage = GetDamagePerTick(TICK_INTERVAL);
        
        if (tickDamage <= 0)
            return;
        
        // Apply damage through DamageSystem
        DamageBreakdown breakdown = UpgradeSystem.CalculateDamage(baseDamage: tickDamage, canCrit: false);
        breakdown.color = GetDamageColor();
        DamageSystem.ApplyDamage(owner, GetSource(), breakdown);
        
        // Call the effect's OnDamageApplied callback
        OnDamageApplied(tickDamage);
    }
   
    
    /// <summary>
    /// Calculate the total remaining damage this DoT will deal.
    /// </summary>
    /// <returns>Total remaining damage.</returns>
    protected int GetTotalRemainingDamage()
    {
        // Calculate how many ticks are left
        float ticksRemaining = GetRemainingDuration() / TICK_INTERVAL;
        
        // Calculate damage per tick
        int damagePerTick = GetDamagePerTick(TICK_INTERVAL);
        
        // Total remaining damage
        int totalDamage = Mathf.RoundToInt(ticksRemaining * damagePerTick);
        
        return totalDamage;
    }
    
    /// <summary>
    /// Execute the target by applying all remaining DoT damage at once.
    /// </summary>
    protected void ExecuteTarget()
    {
        if (!owner)
            return;
        
        // Calculate total remaining damage
        int executeDamage = GetTotalRemainingDamage();
        
        if (executeDamage <= 0)
            return;
        
        
        // Apply all remaining damage at once
        DamageBreakdown breakdown = UpgradeSystem.CalculateDamage(baseDamage: executeDamage, canCrit: false);
        breakdown.color = GetDamageColor();
        DamageSystem.ApplyDamage(owner, GetSource(), breakdown);
        
        // Mark effect as expired since we dealt all the damage
        MarkAsExpired();
        
        // Call the effect's OnDamageApplied callback with total damage
        OnDamageApplied(executeDamage);
    }
    
    /// <summary>
    /// Refresh the effect duration (reset to full duration).
    /// </summary>
    /// <param name="newDuration">Optional new duration, uses current duration if not specified.</param>
    public new void Refresh(float? newDuration = null)
    {
        base.Refresh(newDuration);
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
    
}

