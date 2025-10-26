using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Weapon effect types that can be applied to projectiles.
/// </summary>
public enum WeaponEffectType
{
    None = 0,
    Burn = 1,      // Damage over time (fire)
    Poison = 2,    // Damage over time (poison)
    Slow = 3,      // Reduces movement speed
    Stun = 4,      // Disables movement temporarily
    Freeze = 5     // Combination of slow + visual effect
}

/// <summary>
/// Configuration data for weapon effects.
/// </summary>
[System.Serializable]
public struct WeaponEffectData
{
    [Tooltip("Type of effect to apply")]
    public WeaponEffectType effectType;
    [Tooltip("Chance to apply effect (0.0 to 1.0)")]
    public float chance;
    [Tooltip("Duration of the effect in seconds")]
    public float duration;
    [Tooltip("Intensity/strength of the effect")]
    public float intensity;
    
    public WeaponEffectData(WeaponEffectType type, float effectChance, float effectDuration, float effectIntensity)
    {
        effectType = type;
        chance = effectChance;
        duration = effectDuration;
        intensity = effectIntensity;
    }
}

/// <summary>
/// Static utility class for applying weapon effects to entities.
/// </summary>
public static class WeaponEffects
{
    /// <summary>
    /// Apply a burn effect (damage over time) to an entity.
    /// </summary>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="duration">Duration of the burn effect.</param>
    /// <param name="damagePerSecond">Damage dealt per second.</param>
    /// <param name="source">Entity that applied this effect.</param>
    public static void ApplyBurnEffect(Entity target, float duration, float damagePerSecond, Entity source)
    {
        if (!target || !DamageSystem.CanTakeDamage(target))
            return;
            
        // Remove existing burn effect if present (refresh)
        var existingBurn = target.GetComponent<DamageOverTimeComponent>();
        if (existingBurn != null)
        {
            target.RemoveComponent<DamageOverTimeComponent>();
        }
        
        // Add new burn effect
        var burnComponent = target.AddComponent<DamageOverTimeComponent>();
        burnComponent.Initialize(damagePerSecond, duration, source);
    }
    
    /// <summary>
    /// Apply a poison effect (damage over time) to an entity.
    /// </summary>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="duration">Duration of the poison effect.</param>
    /// <param name="damagePerSecond">Damage dealt per second.</param>
    /// <param name="source">Entity that applied this effect.</param>
    public static void ApplyPoisonEffect(Entity target, float duration, float damagePerSecond, Entity source)
    {
        if (!target || !DamageSystem.CanTakeDamage(target))
            return;
            
        // Poison can stack - don't remove existing poison
        var poisonComponent = target.AddComponent<PoisonComponent>();
        poisonComponent.Initialize(damagePerSecond, duration, source);
    }
    
    /// <summary>
    /// Apply a slow effect to an entity.
    /// </summary>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="duration">Duration of the slow effect.</param>
    /// <param name="speedMultiplier">Speed multiplier (0.5 = half speed).</param>
    public static void ApplySlowEffect(Entity target, float duration, float speedMultiplier)
    {
        if (!target)
            return;
            
        var enemyComponent = target.GetComponent<Enemy>();
        if (enemyComponent == null)
            return;
            
        // Don't stack slow effects - refresh existing one
        var existingSlow = target.GetComponent<SlowComponent>();
        if (existingSlow != null)
        {
            // Refresh duration and apply stronger slow if applicable
            existingSlow.timeRemaining = Mathf.Max(existingSlow.timeRemaining, duration);
            if (speedMultiplier < existingSlow.speedMultiplier)
            {
                // Apply stronger slow
                if (existingSlow.isApplied)
                {
                    enemyComponent.maxSpeed = existingSlow.originalSpeed * speedMultiplier;
                }
                existingSlow.speedMultiplier = speedMultiplier;
            }
        }
        else
        {
            // Add new slow effect
            var slowComponent = target.AddComponent<SlowComponent>();
            slowComponent.Initialize(speedMultiplier, duration, enemyComponent.maxSpeed);
        }
    }
    
    /// <summary>
    /// Apply a stun effect to an entity.
    /// </summary>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="duration">Duration of the stun effect.</param>
    public static void ApplyStunEffect(Entity target, float duration)
    {
        if (!target)
            return;
            
        var enemyComponent = target.GetComponent<Enemy>();
        if (enemyComponent == null)
            return;
            
        // Don't stack stun effects - refresh existing one
        var existingStun = target.GetComponent<StunComponent>();
        if (existingStun != null)
        {
            existingStun.timeRemaining = Mathf.Max(existingStun.timeRemaining, duration);
        }
        else
        {
            // Add new stun effect
            var stunComponent = target.AddComponent<StunComponent>();
            stunComponent.Initialize(duration);
        }
    }
    
    /// <summary>
    /// Apply a freeze effect (slow + visual) to an entity.
    /// </summary>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="duration">Duration of the freeze effect.</param>
    /// <param name="speedMultiplier">Speed multiplier for the slow component.</param>
    public static void ApplyFreezeEffect(Entity target, float duration, float speedMultiplier)
    {
        // Freeze is just a slow effect with a different name/visual
        ApplySlowEffect(target, duration, speedMultiplier);
        
        // Could add visual effects here (blue tint, ice particles, etc.)
    }
    
    /// <summary>
    /// Apply a weapon effect based on effect data.
    /// </summary>
    /// <param name="effectData">Effect configuration data.</param>
    /// <param name="target">Entity to apply effect to.</param>
    /// <param name="source">Entity that applied this effect.</param>
    /// <returns>True if the effect was applied.</returns>
    public static bool ApplyWeaponEffect(WeaponEffectData effectData, Entity target, Entity source)
    {
        if (!target || effectData.effectType == WeaponEffectType.None)
            return false;
            
        // Check chance
        if (Random.Range(0f, 1f) > effectData.chance)
            return false;
            
        switch (effectData.effectType)
        {
            case WeaponEffectType.Burn:
                ApplyBurnEffect(target, effectData.duration, effectData.intensity, source);
                return true;
                
            case WeaponEffectType.Poison:
                ApplyPoisonEffect(target, effectData.duration, effectData.intensity, source);
                return true;
                
            case WeaponEffectType.Slow:
                ApplySlowEffect(target, effectData.duration, effectData.intensity);
                return true;
                
            case WeaponEffectType.Stun:
                ApplyStunEffect(target, effectData.duration);
                return true;
                
            case WeaponEffectType.Freeze:
                ApplyFreezeEffect(target, effectData.duration, effectData.intensity);
                return true;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Apply multiple weapon effects to a target.
    /// </summary>
    /// <param name="effects">Array of effect data to apply.</param>
    /// <param name="target">Entity to apply effects to.</param>
    /// <param name="source">Entity that applied these effects.</param>
    /// <returns>Number of effects successfully applied.</returns>
    public static int ApplyWeaponEffects(WeaponEffectData[] effects, Entity target, Entity source)
    {
        if (effects == null || effects.Length == 0 || !target)
            return 0;
            
        int appliedCount = 0;
        
        foreach (var effect in effects)
        {
            if (ApplyWeaponEffect(effect, target, source))
            {
                appliedCount++;
            }
        }
        
        return appliedCount;
    }
    
    /// <summary>
    /// Remove all effects from an entity (useful for cleanse abilities).
    /// </summary>
    /// <param name="target">Entity to cleanse.</param>
    public static void RemoveAllEffects(Entity target)
    {
        if (!target)
            return;
            
        // Remove DOT effects
        var dotComponent = target.GetComponent<DamageOverTimeComponent>();
        if (dotComponent != null)
        {
            target.RemoveComponent<DamageOverTimeComponent>();
        }
        
        // Remove poison effects
        var poisonComponent = target.GetComponent<PoisonComponent>();
        if (poisonComponent != null)
        {
            target.RemoveComponent<PoisonComponent>();
        }
        
        // Remove slow effects (restore speed first)
        var slowComponent = target.GetComponent<SlowComponent>();
        if (slowComponent != null && slowComponent.isApplied)
        {
            var enemyComponent = target.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.maxSpeed = slowComponent.originalSpeed;
            }
            target.RemoveComponent<SlowComponent>();
        }
        
        // Remove stun effects (restore movement first)
        var stunComponent = target.GetComponent<StunComponent>();
        if (stunComponent != null && stunComponent.isApplied)
        {
            var enemyComponent = target.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.ResumeChasing();
            }
            target.RemoveComponent<StunComponent>();
        }
    }
}
