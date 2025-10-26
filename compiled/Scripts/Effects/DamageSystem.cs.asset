using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Centralized damage system for applying damage and managing death events.
/// Provides a single point of control for all damage-related operations.
/// </summary>
public static class DamageSystem
{
    // Events for other systems to subscribe to
    public static System.Action<Entity, Entity, float> OnDamageApplied; // (target, source, damage)
    public static System.Action<Entity, Entity> OnEntityDied; // (deadEntity, killer)
    
    /// <summary>
    /// Apply damage to an entity through its Health component.
    /// </summary>
    /// <param name="target">Entity to damage.</param>
    /// <param name="damage">Amount of damage to deal.</param>
    /// <param name="source">Entity that caused the damage.</param>
    /// <returns>True if the target died from this damage.</returns>
    public static bool ApplyDamage(Entity target, float damage, Entity source)
    {
        if (!target || damage <= 0)
            return false;
            
        var healthComponent = target.GetComponent<Health>();
        if (healthComponent == null)
            return false;
            
        // Store health before damage
        float healthBefore = healthComponent.GetCurrentHealth();
        bool wasAlive = !healthComponent.IsDead();
        
        // Apply damage
        bool died = healthComponent.TakeDamage(damage, source);
        
        // Trigger damage event
        OnDamageApplied?.Invoke(target, source, damage);
        
        // Check if entity died from this damage
        if (died && wasAlive)
        {
            OnEntityDied?.Invoke(target, source);
        }
        
        return died;
    }
    
    /// <summary>
    /// Apply healing to an entity through its Health component.
    /// </summary>
    /// <param name="target">Entity to heal.</param>
    /// <param name="healAmount">Amount of healing to apply.</param>
    /// <param name="source">Entity that provided the healing.</param>
    /// <returns>Actual amount healed.</returns>
    public static float ApplyHealing(Entity target, float healAmount, Entity source)
    {
        if (!target || healAmount <= 0)
            return 0;
            
        var healthComponent = target.GetComponent<Health>();
        if (healthComponent == null)
            return 0;
            
        return healthComponent.Heal(healAmount, source);
    }
    
    /// <summary>
    /// Kill an entity instantly.
    /// </summary>
    /// <param name="target">Entity to kill.</param>
    /// <param name="source">Entity responsible for the kill.</param>
    public static void KillEntity(Entity target, Entity source)
    {
        if (!target)
            return;
            
        var healthComponent = target.GetComponent<Health>();
        if (healthComponent == null)
            return;
            
        bool wasAlive = !healthComponent.IsDead();
        healthComponent.Kill();
        
        if (wasAlive)
        {
            OnEntityDied?.Invoke(target, source);
        }
    }
    
    /// <summary>
    /// Check if an entity can take damage.
    /// </summary>
    /// <param name="target">Entity to check.</param>
    /// <returns>True if the entity can take damage.</returns>
    public static bool CanTakeDamage(Entity target)
    {
        if (!target)
            return false;
            
        var healthComponent = target.GetComponent<Health>();
        return healthComponent != null && !healthComponent.IsDead();
    }
    
    /// <summary>
    /// Get the current health of an entity.
    /// </summary>
    /// <param name="target">Entity to check.</param>
    /// <returns>Current health, or 0 if no health component.</returns>
    public static float GetCurrentHealth(Entity target)
    {
        if (!target)
            return 0;
            
        var healthComponent = target.GetComponent<Health>();
        return healthComponent?.GetCurrentHealth() ?? 0;
    }
    
    /// <summary>
    /// Get the maximum health of an entity.
    /// </summary>
    /// <param name="target">Entity to check.</param>
    /// <returns>Maximum health, or 0 if no health component.</returns>
    public static float GetMaxHealth(Entity target)
    {
        if (!target)
            return 0;
            
        var healthComponent = target.GetComponent<Health>();
        return healthComponent?.GetMaxHealth() ?? 0;
    }
    
    /// <summary>
    /// Get the health percentage of an entity.
    /// </summary>
    /// <param name="target">Entity to check.</param>
    /// <returns>Health percentage (0.0 to 1.0).</returns>
    public static float GetHealthPercentage(Entity target)
    {
        if (!target)
            return 0;
            
        var healthComponent = target.GetComponent<Health>();
        return healthComponent?.GetHealthPercentage() ?? 0;
    }
}
