using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Handles loot drops by listening to the DamageSystem's OnEntityDied event.
/// This provides centralized loot management without requiring individual entities to handle their own drops.
/// </summary>
[ScriptSourceFile]
public class LootHandler : ScriptComponent
{
    [Tooltip("Enable debug logging for loot handling")]
    public bool debugLootHandler = false;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        // Subscribe to the DamageSystem's death event
        DamageSystem.OnEntityDied += HandleEntityDeath;
        
        if (debugLootHandler)
        {
            Log.Info("LootHandler: Subscribed to DamageSystem OnEntityDied event");
        }
    }
    
    /// <summary>
    /// Called when the script is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        DamageSystem.OnEntityDied -= HandleEntityDeath;
        
        if (debugLootHandler)
        {
            Log.Info("LootHandler: Unsubscribed from DamageSystem OnEntityDied event");
        }
    }
    
    /// <summary>
    /// Handle entity death and determine if loot should be dropped.
    /// </summary>
    /// <param name="deadEntity">The entity that died.</param>
    /// <param name="killer">The entity that killed it (could be player, weapon, etc.).</param>
    private void HandleEntityDeath(Entity deadEntity, Entity killer)
    {
        if (!deadEntity)
            return;
            
        if (debugLootHandler)
        {
            string killerName = killer ? killer.name : "Unknown";
            Log.Info($"LootHandler: Entity {deadEntity.name} died, killed by {killerName}");
        }
        
        // Check if this entity should drop loot
        if (ShouldDropLoot(deadEntity, killer))
        {
            HandleLootDrop(deadEntity, killer);
        }
    }
    
    /// <summary>
    /// Determine if the dead entity should drop loot.
    /// </summary>
    /// <param name="deadEntity">The entity that died.</param>
    /// <param name="killer">The entity that killed it.</param>
    /// <returns>True if loot should be dropped.</returns>
    private bool ShouldDropLoot(Entity deadEntity, Entity killer)
    {
        // Only drop loot for enemies (entities with Enemy component)
        var enemyComponent = deadEntity.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            if (debugLootHandler)
            {
                Log.Info($"LootHandler: {deadEntity.name} is not an enemy, no loot drop");
            }
            return false;
        }
        
        // Could add additional conditions here, such as:
        // - Only drop loot if killed by player
        // - Check if entity is in a "no loot" zone
        // - Check if loot drops are enabled globally
        // - Check entity-specific loot flags
        
        // For now, all enemies drop loot when they die
        return true;
    }
    
    /// <summary>
    /// Handle the actual loot drop for the dead entity.
    /// </summary>
    /// <param name="deadEntity">The entity that died.</param>
    /// <param name="killer">The entity that killed it.</param>
    private void HandleLootDrop(Entity deadEntity, Entity killer)
    {
        // Get the loot system
        if (LootSystem.Instance == null)
        {
            if (debugLootHandler)
            {
                Log.Warning($"LootHandler: No LootSystem found, cannot drop loot for {deadEntity.name}");
            }
            return;
        }
        
        // Get the enemy's position for loot drop
        var transformComponent = deadEntity.GetComponent<TransformComponent>();
        if (transformComponent == null)
        {
            if (debugLootHandler)
            {
                Log.Warning($"LootHandler: {deadEntity.name} has no TransformComponent, cannot determine drop position");
            }
            return;
        }
        
        // Get custom loot configuration based on enemy type
        var enemyComponent = deadEntity.GetComponent<Enemy>();
        LootSystem.LootConfiguration customConfig = null;
        
        if (enemyComponent != null)
        {
            customConfig = GetEnemyLootConfig(enemyComponent);
        }
        
        // Drop the loot
        Vector3 dropPosition = transformComponent.position;
        LootSystem.Instance.HandleEnemyDeath(deadEntity, dropPosition, customConfig);
        
        if (debugLootHandler)
        {
            Log.Info($"LootHandler: Processed loot drop for {deadEntity.name} at {dropPosition}");
        }
    }
    
    /// <summary>
    /// Get the loot configuration from an enemy component based on its type.
    /// This method creates different loot configs for different enemy types.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <returns>Custom loot configuration, or null to use default.</returns>
    private LootSystem.LootConfiguration GetEnemyLootConfig(Enemy enemy)
    {
        string enemyType = enemy.GetEnemyType().ToLower();
        
        // Create loot configs based on enemy type
        switch (enemyType)
        {
            case "boss":
                return LootSystem.CreateLootConfig(100.0f, 0.5f, 5); // Boss: 100 base XP, 50% per level, 5 orbs
                
            case "elite":
                return LootSystem.CreateLootConfig(25.0f, 0.2f, 3); // Elite: 25 base XP, 20% per level, 3 orbs
                
            case "heavy":
                return LootSystem.CreateLootConfig(18.0f, 0.15f, 2); // Heavy: 18 base XP, 15% per level, 2 orbs
                
            case "fast":
                return LootSystem.CreateLootConfig(12.0f, 0.12f, 1); // Fast: 12 base XP, 12% per level, 1 orb
                
            case "weak":
            case "small":
                return LootSystem.CreateLootConfig(5.0f, 0.05f, 1); // Weak: 5 base XP, 5% per level, 1 orb
                
            case "basic":
            default:
                // Return null to use LootSystem's default configuration
                return null;
        }
    }
}
