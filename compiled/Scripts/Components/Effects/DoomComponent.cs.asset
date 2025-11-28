using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Doom damage over time effect.
/// Doom deals high damage over a short duration and cannot be stacked.
/// Represents a curse or death mark effect.
/// </summary>
[ScriptSourceFile]
public class DoomComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 1)
    {
        // Doom doesn't stack
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = true;
        stacksDuration = false;
    }
    
    public override string GetEffectName()
    {
        return "Doom";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(139, 0, 139, 255)"; // Dark magenta/purple
    }

    public override Color GetDamageColor()
    {
        return new Color(139, 0, 139, 255);
    }
    
    
    /// <summary>
    /// Doom executes targets when remaining damage exceeds their health.
    /// </summary>
    /// <returns>True if the target should be executed.</returns>
    protected bool ShouldExecute()
    {
        if (!owner)
            return false;
        
        // Get the entity's health component
        var health = owner.GetComponent<Health>();
        if (health == null)
            return false;
        
        int currentHealth = health.GetCurrentHealth();
        
        // Calculate total remaining damage from this Doom
        int totalRemainingDamage = GetTotalRemainingDamage();
        
        // Execute if remaining damage exceeds current health
        return totalRemainingDamage >= currentHealth;
    }
    
    /// <summary>
    /// Override damage application to include execute mechanic.
    /// </summary>
    protected override void ApplyDamage()
    {
        if (!owner)
            return;
        
        // Check if we should execute (apply all remaining damage at once)
        if (ShouldExecute())
        {
            ExecuteTarget();
            return;
        }
        
        // Otherwise, apply normal tick damage
        base.ApplyDamage();
    }
}

