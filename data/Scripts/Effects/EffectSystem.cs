using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Centralized system for processing all entity effects efficiently.
/// Handles DOT, Slow, Stun, and other status effects in batches.
/// </summary>
[ScriptSourceFile]
public class EffectSystem : ScriptComponent
{
    [Tooltip("Enable debug logging for effect processing")]
    public bool debugEffects = false;
    
    
    /// <summary>
    /// Called every frame to process all active effects.
    /// </summary>
    public override void OnUpdate()
    {
        ProcessDamageOverTimeEffects();
        ProcessPoisonEffects();
        ProcessSlowEffects();
        ProcessStunEffects();
        
        // Clean up expired effects
        CleanupExpiredEffects();
    }
    
    /// <summary>
    /// Process all damage over time effects.
    /// </summary>
    private void ProcessDamageOverTimeEffects()
    {
        // Find all entities with DOT effects
        var entitiesToProcess = Scene.FindEntitiesWithComponent<DamageOverTimeComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var dotComponents = entity.GetComponents<DamageOverTimeComponent>();
            var healthComponent = entity.GetComponent<Health>();
            
            if(healthComponent == null)
                continue;
            
            foreach (var dotComponent in dotComponents)
            {
                // Update timer
                dotComponent.timeRemaining -= Time.deltaTime;

                // Apply damage on tick intervals
                if (Time.time >= dotComponent.nextTickTime)
                {
                    float damage = dotComponent.damagePerSecond * dotComponent.tickInterval;
                    healthComponent.TakeDamage(damage, dotComponent.source);

                    dotComponent.nextTickTime = Time.time + dotComponent.tickInterval;

                    if (debugEffects)
                    {
                        Log.Info($"DOT: Applied {damage} damage to {entity.name} (remaining: {dotComponent.timeRemaining:F1}s)");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Process all poison effects.
    /// </summary>
    private void ProcessPoisonEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<PoisonComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var poisonComponents = entity.GetComponents<PoisonComponent>();
            var healthComponent = entity.GetComponent<Health>();
            
            if(healthComponent == null)
                continue;
            
            foreach (var poisonComponent in poisonComponents)
            {
                // Update timer
                poisonComponent.timeRemaining -= Time.deltaTime;

                // Apply damage on tick intervals
                if (Time.time >= poisonComponent.nextTickTime)
                {
                    float damage = poisonComponent.damagePerSecond * poisonComponent.tickInterval;
                    healthComponent.TakeDamage(damage, poisonComponent.source);

                    poisonComponent.nextTickTime = Time.time + poisonComponent.tickInterval;

                    if (debugEffects)
                    {
                        Log.Info($"Poison: Applied {damage} damage to {entity.name} (remaining: {poisonComponent.timeRemaining:F1}s)");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Process all slow effects.
    /// </summary>
    private void ProcessSlowEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<SlowComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var slowComponent = entity.GetComponent<SlowComponent>();
            var enemyComponent = entity.GetComponent<Enemy>();
            
            if(enemyComponent == null)
                continue;
                
            // Apply slow effect if not already applied
            if (!slowComponent.isApplied)
            {
                slowComponent.originalSpeed = enemyComponent.maxSpeed;
                enemyComponent.maxSpeed *= slowComponent.speedMultiplier;
                slowComponent.isApplied = true;
                
                if (debugEffects)
                {
                    Log.Info($"Slow: Applied to {entity.name} - Speed: {slowComponent.originalSpeed} -> {enemyComponent.maxSpeed}");
                }
            }
            
            // Update timer
            slowComponent.timeRemaining -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Process all stun effects.
    /// </summary>
    private void ProcessStunEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<StunComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var stunComponent = entity.GetComponent<StunComponent>();
            var enemyComponent = entity.GetComponent<Enemy>();
            
            if(enemyComponent == null)
                continue;
                
            // Apply stun effect if not already applied
            if (!stunComponent.isApplied)
            {
                enemyComponent.StopChasing();
                stunComponent.isApplied = true;
                
                if (debugEffects)
                {
                    Log.Info($"Stun: Applied to {entity.name} for {stunComponent.timeRemaining:F1}s");
                }
            }
            
            // Update timer
            stunComponent.timeRemaining -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Clean up expired effects and restore original values.
    /// </summary>
    private void CleanupExpiredEffects()
    {
        // Clean up expired DOT effects
        CleanupExpiredDOTEffects();
        
        // Clean up expired poison effects
        CleanupExpiredPoisonEffects();
        
        // Clean up expired slow effects
        CleanupExpiredSlowEffects();
        
        // Clean up expired stun effects
        CleanupExpiredStunEffects();
    }
    
    private void CleanupExpiredDOTEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<DamageOverTimeComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var comps = entity.GetComponents<DamageOverTimeComponent>();
            foreach (var comp in comps)
            {
                if (comp.timeRemaining <= 0)
                {
                    entity.RemoveComponent(comp);
                }
            }
           
        }
    }
    private void CleanupExpiredPoisonEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<PoisonComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var comps = entity.GetComponents<PoisonComponent>();
            foreach (var comp in comps)
            {
                if (comp.timeRemaining <= 0)
                {
                    entity.RemoveComponent(comp);
                }
            }
        }
    }
    
    private void CleanupExpiredSlowEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<SlowComponent>();
        
        foreach (var entity in entitiesToProcess)
        {
            var slowComponent = entity.GetComponent<SlowComponent>();
            if (slowComponent != null && slowComponent.timeRemaining <= 0)
            {
                // Restore original speed before removing
                if (slowComponent.isApplied)
                {
                    var enemyComponent = entity.GetComponent<Enemy>();
                    if (enemyComponent != null)
                    {
                        enemyComponent.maxSpeed = slowComponent.originalSpeed;
                        
                        if (debugEffects)
                        {
                            Log.Info($"Slow: Expired on {entity.name} - Speed restored to {slowComponent.originalSpeed}");
                        }
                    }
                }
                
                entity.RemoveComponent(slowComponent);
            }
        }
    }
    
    
    private void CleanupExpiredStunEffects()
    {
        var entitiesToProcess = Scene.FindEntitiesWithComponent<StunComponent>();

        foreach (var entity in entitiesToProcess)
        {
            var stunComponent = entity.GetComponent<StunComponent>();
            if (stunComponent != null && stunComponent.timeRemaining <= 0)
            {
                // Restore enemy behavior before removing
                if (stunComponent.isApplied)
                {
                    var enemyComponent = entity.GetComponent<Enemy>();
                    if (enemyComponent != null)
                    {
                        enemyComponent.ResumeChasing();

                        if (debugEffects)
                        {
                            Log.Info($"Stun: Expired on {entity.name} - Movement restored");
                        }
                    }
                }

                entity.RemoveComponent(stunComponent);
            }
        }
    }
}
