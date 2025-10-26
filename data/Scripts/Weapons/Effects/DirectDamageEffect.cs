using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// DirectDamage effect that deals damage to entities with Health when projectiles collide.
/// </summary>
[ScriptSourceFile]
public class DirectDamageEffect : ScriptComponent, IWeaponEffect
{
    //[Header("Damage Settings")]
    [Tooltip("Amount of damage to deal on collision")]
    public float damageAmount = 25.0f;
    [Tooltip("Whether to destroy the projectile after dealing damage")]
    public bool destroyProjectileOnHit = true;
    [Tooltip("Enable debug logging for damage events")]
    public bool debugDamage = false;
    
    /// <summary>
    /// Apply damage effect when projectile collides with target.
    /// </summary>
    /// <param name="projectile">The projectile entity.</param>
    /// <param name="target">The target entity that was hit.</param>
    /// <param name="collision">Collision details.</param>
    /// <returns>True if damage was applied.</returns>
    public bool ApplyEffect(Entity projectile, Entity target, Collision collision)
    {
        if (!target || damageAmount <= 0)
            return false;
            
        // Try to get the Health from the target
        var Health = target.GetComponent<Health>();
        if (Health == null)
        {
            if (debugDamage)
            {
                Log.Info($"DirectDamageEffect: Target {target.name} has no Health, no damage applied");
            }
            return false;
        }
        
        // Deal damage
        bool targetDied = Health.TakeDamage(damageAmount, projectile);
        
        if (debugDamage)
        {
            Log.Info($"DirectDamageEffect: Dealt {damageAmount} damage to {target.name} (died: {targetDied})");
        }
        
        // Destroy projectile if configured to do so
        if (destroyProjectileOnHit && projectile)
        {
            Scene.DestroyEntity(projectile);
        }
        
        return true;
    }
    
    /// <summary>
    /// Get the effect name for debugging.
    /// </summary>
    /// <returns>Effect name.</returns>
    public string GetEffectName()
    {
        return "DirectDamage";
    }
    
    /// <summary>
    /// Set the damage amount for this effect.
    /// </summary>
    /// <param name="newDamage">New damage amount.</param>
    public void SetDamageAmount(float newDamage)
    {
        damageAmount = Mathf.Max(0, newDamage);
        
        if (debugDamage)
        {
            Log.Info($"DirectDamageEffect: Damage amount set to {damageAmount}");
        }
    }
    
    /// <summary>
    /// Get the current damage amount.
    /// </summary>
    /// <returns>Current damage amount.</returns>
    public float GetDamageAmount()
    {
        return damageAmount;
    }
}
