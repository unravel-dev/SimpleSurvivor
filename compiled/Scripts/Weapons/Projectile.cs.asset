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
    
    // Effects list
    private List<IWeaponEffect> effects = new List<IWeaponEffect>();
    
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
        
        // Automatically find and add effect components
        CollectEffectComponents();
        
        if (debugProjectile)
        {
            Log.Info($"Projectile initialized on {owner.name} with {effects.Count} effects");
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
        
        // Apply all effects
        ApplyEffects(target, collision);
        
        // Destroy projectile if configured to do so
        if (destroyOnCollision)
        {
            Scene.DestroyEntity(owner);
        }
    }
    
    /// <summary>
    /// Collect all effect components attached to this entity.
    /// </summary>
    private void CollectEffectComponents()
    {
        effects.Clear();
        
        // Get all script components and check if they implement IProjectileEffect
        var scriptComponents = owner.GetComponents<ScriptComponent>();
        foreach (var component in scriptComponents)
        {
            if (component is IWeaponEffect effect)
            {
                effects.Add(effect);
                
                if (debugProjectile)
                {
                    Log.Info($"Projectile: Found effect {effect.GetEffectName()} on {owner.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply all effects to the target.
    /// </summary>
    /// <param name="target">Target entity.</param>
    /// <param name="collision">Collision details.</param>
    private void ApplyEffects(Entity target, Collision collision)
    {
        int effectsApplied = 0;
        
        foreach (var effect in effects)
        {
            try
            {
                if (effect.ApplyEffect(owner, target, collision))
                {
                    effectsApplied++;
                    
                    if (debugProjectile)
                    {
                        Log.Info($"Projectile: Applied effect {effect.GetEffectName()} to {target.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Projectile: Error applying effect {effect.GetEffectName()}: {ex.Message}");
            }
        }
        
        if (debugProjectile)
        {
            Log.Info($"Projectile: Applied {effectsApplied}/{effects.Count} effects to {target.name}");
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
    /// Add an effect to this projectile.
    /// </summary>
    /// <param name="effect">Effect to add.</param>
    public void AddEffect(IWeaponEffect effect)
    {
        if (effect != null && !effects.Contains(effect))
        {
            effects.Add(effect);
            
            if (debugProjectile)
            {
                Log.Info($"Projectile: Added effect {effect.GetEffectName()} to {owner.name}");
            }
        }
    }
    
    /// <summary>
    /// Remove an effect from this projectile.
    /// </summary>
    /// <param name="effect">Effect to remove.</param>
    public void RemoveEffect(IWeaponEffect effect)
    {
        if (effects.Remove(effect))
        {
            if (debugProjectile)
            {
                Log.Info($"Projectile: Removed effect {effect.GetEffectName()} from {owner.name}");
            }
        }
    }
    
    /// <summary>
    /// Get the number of effects on this projectile.
    /// </summary>
    /// <returns>Number of effects.</returns>
    public int GetEffectCount()
    {
        return effects.Count;
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
