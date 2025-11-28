using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Static system that handles all ongoing effects like damage over time and stuns.
/// Iterates over all entities with EffectOverTime components each tick.
/// </summary>
[ScriptSourceFile]
public static class EffectsSystem
{
    /// <summary>
    /// Tick the effects system. Should be called every frame from an EffectsSystemUpdater component.
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds.</param>
    public static void Tick(float deltaTime)
    {
        // Find all entities with DamageOverTimeComponent (includes derived types)
        var entitiesWithDoT = Scene.FindEntitiesWithComponent<EffectOverTime>();
        
        // Process each entity with DoT effects
        foreach (var entity in entitiesWithDoT)
        {
            if (!entity)
                continue;
            
            // Get all OverTimeEffect components on this entity
            var effects = entity.GetComponents<EffectOverTime>();
            if (effects == null || effects.Length == 0)
                continue;

            // Process each effect on this entity (backwards to allow removal)
            for (int i = effects.Length - 1; i >= 0; i--)
            {
                var effect = effects[i];
                if (effect == null)
                    continue;

                // Update the effect and check if it should tick
                effect.Update(deltaTime);


                bool isStun = effect is StunComponent;

                // Check if effect is expired and remove it
                if (effect.IsExpired())
                {
                    
                    if (isStun)
                    {
                        var enemy = entity.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.ResumeChasing();
                        }
                    }
                    // Call expiration callback
                    effect.OnExpired();

                    // Remove the component from the entity
                    entity.RemoveComponent(effect);

                    continue;
                }
                
                
                if (isStun)
                {
                    var enemy = entity.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.StopChasing();
                    }
                }
            }
        }
    
    }
    
    /// <summary>
    /// Check if an entity has a specific type of damage over time effect.
    /// </summary>
    /// <typeparam name="T">Type of effect to check for.</typeparam>
    /// <param name="entity">Entity to check.</param>
    /// <returns>True if the entity has the effect.</returns>
    public static bool HasEffect<T>(Entity entity) where T : EffectOverTime
    {
        if (!entity)
            return false;
            
        return entity.HasComponent<T>();
    }
    
    /// <summary>
    /// Get all effects of a specific type on an entity.
    /// </summary>
    /// <typeparam name="T">Type of effect to get.</typeparam>
    /// <param name="entity">Entity to check.</param>
    /// <returns>Array of effects of the specified type.</returns>
    public static T[] GetEffects<T>(Entity entity) where T : EffectOverTime
    {
        if (!entity)
            return new T[0];
            
        return entity.GetComponents<T>();
    }
    
    /// <summary>
    /// Add or refresh a damage over time effect on an entity.
    /// If the effect already exists, it will be refreshed (duration reset and stacks increased).
    /// </summary>
    /// <typeparam name="T">Type of effect to add.</typeparam>
    /// <param name="entity">Entity to add the effect to.</param>
    /// <param name="source">Entity that caused the effect.</param>
    /// <param name="damagePerSecond">Damage per second.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="maxStacks">Maximum number of stacks.</param>
    /// <param name="stacksToApply">Number of stacks to apply at once (default: 1).</param>
    /// <returns>The added or refreshed effect component.</returns>
    public static T AddOrRefreshEffect<T>(Entity entity, Entity source, float damagePerSecond, float duration, int stacksToApply = 1, int maxStacks = 0) where T : DamageOverTimeComponent, new()
    {
        if (!entity)
            return null;
            
        // Try to find existing effect of this type
        T effect = entity.GetComponent<T>();

        if (effect != null)
        {
            // Refresh existing effect
            effect.Refresh(duration);
            
            // Add multiple stacks at once
            effect.AddStack(stacksToApply);
        }
        else
        {
            // Add new effect
            effect = entity.AddComponent<T>();
            if (effect != null)
            {
                effect.Initialize(source, damagePerSecond, duration, maxStacks);
                
                // Add additional stacks if needed (Initialize already sets currentStacks to 1)
                effect.AddStack(stacksToApply);
            }
        }
        
        return effect;
    }
    
    /// <summary>
    /// Add or refresh a stun effect on an entity.
    /// If the entity already has a StunComponent, refreshes its duration.
    /// Otherwise, adds a new StunComponent.
    /// </summary>
    /// <param name="entity">Entity to apply the stun to.</param>
    /// <param name="source">Entity that caused the stun.</param>
    /// <param name="duration">Duration of the stun in seconds.</param>
    /// <returns>The StunComponent that was added or refreshed, or null if failed.</returns>
    public static StunComponent AddOrRefreshStun(Entity entity, Entity source, float duration)
    {
        if (!entity)
            return null;
            
        // Try to find existing stun effect
        StunComponent stun = entity.GetComponent<StunComponent>();

        if (stun != null)
        {
            // Refresh existing stun (use longer duration)
            if (duration > stun.GetRemainingDuration())
            {
                stun.Refresh(duration);
            }
        }
        else
        {
            // Add new stun effect
            stun = entity.AddComponent<StunComponent>();
            if (stun != null)
            {
                stun.Initialize(source, duration);
            }
        }
        
        return stun;
    }
    
    /// <summary>
    /// Check if an entity has a stun effect.
    /// </summary>
    /// <param name="entity">Entity to check.</param>
    /// <returns>True if entity has a StunComponent.</returns>
    public static bool HasStun(Entity entity)
    {
        if (!entity)
            return false;
        return entity.GetComponent<StunComponent>() != null;
    }
}

