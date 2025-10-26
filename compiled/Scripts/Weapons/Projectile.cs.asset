using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Projectile that handles projectile behavior, collision detection, and effect application.
/// </summary>
[ScriptSourceFile]
public class Projectile : ScriptComponent
{
    //[Header("Projectile Settings")]
    [Tooltip("Lifetime of the projectile in seconds (0 = infinite)")]
    public float lifetime = 5.0f;
    [Tooltip("Whether to destroy on collision with any entity")]
    public bool destroyOnCollision = true;
    [Tooltip("Enable debug logging for projectile events")]
    public bool debugProjectile = false;
    
    // Component references
    private TransformComponent transformComponent;
    private PhysicsComponent physicsComponent;
    
    // Projectile state
    private float timeAlive = 0.0f;
    private bool hasCollided = false;
    private Entity sourceEntity; // Entity that fired this projectile
    
    // Weapon data
    private WeaponEffectData[] weaponEffects = new WeaponEffectData[0];
    private float weaponDamage = 25.0f;

    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        physicsComponent = owner.GetComponent<PhysicsComponent>();

        if (transformComponent == null)
        {
            Log.Error($"Projectile on {owner.name}: TransformComponent not found!");
        }

        if (physicsComponent == null)
        {
            Log.Warning($"Projectile on {owner.name}: PhysicsComponent not found! Collision detection may not work properly.");
        }

        if (debugProjectile)
        {
            Log.Info($"Projectile initialized on {owner.name}");
        }

    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        timeAlive = 0.0f;
        hasCollided = false;
        
        if (debugProjectile)
        {
            Log.Info($"Projectile started on {owner.name}");
        }
    }
    
    /// <summary>
    /// Called every frame to update projectile logic.
    /// </summary>
    public override void OnUpdate()
    {
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if projectile should be destroyed due to lifetime
        if (lifetime > 0 && timeAlive >= lifetime)
        {
            if (debugProjectile)
            {
                Log.Info($"Projectile {owner.name} expired after {timeAlive:F2} seconds");
            }
            
            Scene.DestroyEntity(owner);
            return;
        }
    }
    
    /// <summary>
    /// Called when this projectile collides with another entity.
    /// </summary>
    /// <param name="collision">Collision details.</param>
    public override void OnCollisionEnter(Collision collision)
    {
        if (hasCollided)
            return;
            
        Entity target = collision.entity;
        
        // Skip collision with source entity (the entity that fired this projectile)
        if (target == sourceEntity)
        {
            if (debugProjectile)
            {
                Log.Info($"Projectile {owner.name} ignored collision with source entity {target.name}");
            }
            return;
        }
        
        // Physics layer filtering is handled automatically by the PhysicsComponent
        // If we get here, the collision is valid according to the physics layer settings
        
        hasCollided = true;
        
        if (debugProjectile)
        {
            Log.Info($"Projectile {owner.name} collided with {target.name}");
        }
        
        // Apply damage and effects
        ApplyDamageAndEffects(target);
        
        // Destroy projectile if configured to do so
        if (destroyOnCollision)
        {
            Scene.DestroyEntity(owner);
        }
    }
    
    /// <summary>
    /// Apply damage and weapon effects to the target using the new systems.
    /// </summary>
    /// <param name="target">Target entity to damage.</param>
    private void ApplyDamageAndEffects(Entity target)
    {
        // Apply base damage using DamageSystem
        bool targetDied = DamageSystem.ApplyDamage(target, weaponDamage, sourceEntity);
        
        // Apply weapon effects if target survived and we have effects
        if (!targetDied && weaponEffects != null && weaponEffects.Length > 0)
        {
            int effectsApplied = WeaponEffects.ApplyWeaponEffects(weaponEffects, target, sourceEntity);
            
            if (debugProjectile && effectsApplied > 0)
            {
                Log.Info($"Projectile: Applied {effectsApplied} weapon effects to {target.name}");
            }
        }
        
        if (debugProjectile)
        {
            Log.Info($"Projectile: Dealt {weaponDamage} damage to {target.name} (died: {targetDied})");
        }
    }
    
    
    /// <summary>
    /// Set the source entity that fired this projectile.
    /// </summary>
    /// <param name="source">Source entity.</param>
    public void SetSourceEntity(Entity source)
    {
        sourceEntity = source;
        
        if (debugProjectile)
        {
            Log.Info($"Projectile {owner.name} source set to {(source ? source.name : "null")}");
        }
    }
    
    /// <summary>
    /// Get the source entity that fired this projectile.
    /// </summary>
    /// <returns>Source entity.</returns>
    public Entity GetSourceEntity()
    {
        return sourceEntity;
    }
    
    /// <summary>
    /// Set the weapon effects for this projectile.
    /// </summary>
    /// <param name="effects">Array of weapon effect data.</param>
    public void SetWeaponEffects(WeaponEffectData[] effects)
    {
        weaponEffects = effects ?? new WeaponEffectData[0];
        
        if (debugProjectile)
        {
            Log.Info($"Projectile: Set {weaponEffects.Length} weapon effects on {owner.name}");
        }
    }
    
    /// <summary>
    /// Set the weapon damage for this projectile.
    /// </summary>
    /// <param name="damage">Damage amount.</param>
    public void SetWeaponDamage(float damage)
    {
        weaponDamage = damage;
        
        if (debugProjectile)
        {
            Log.Info($"Projectile: Set weapon damage to {weaponDamage} on {owner.name}");
        }
    }
    
    /// <summary>
    /// Get the number of weapon effects on this projectile.
    /// </summary>
    /// <returns>Number of weapon effects.</returns>
    public int GetEffectCount()
    {
        return weaponEffects?.Length ?? 0;
    }
    
    /// <summary>
    /// Get the time this projectile has been alive.
    /// </summary>
    /// <returns>Time alive in seconds.</returns>
    public float GetTimeAlive()
    {
        return timeAlive;
    }
    
    /// <summary>
    /// Check if this projectile has collided with something.
    /// </summary>
    /// <returns>True if has collided.</returns>
    public bool HasCollided()
    {
        return hasCollided;
    }
    
}
