using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// DamageNumberSystem manages the spawning and lifecycle of 3D damage number entities.
/// Handles container management and provides a centralized system for damage number creation.
/// </summary>
[ScriptSourceFile]
public class DamageNumberSystem : ScriptComponent
{
    //[Header("Damage Number Settings")]
    [Tooltip("Default lifetime for damage numbers in seconds")]
    public float defaultLifetime = 1.5f;
    [Tooltip("Default floating speed for damage numbers")]
    public float defaultFloatSpeed = 2.0f;
    [Tooltip("Height offset above the damaged entity")]
    public float spawnHeightOffset = 2.0f;
    [Tooltip("Random position spread radius")]
    public float spawnSpreadRadius = 0.5f;
    
    //[Header("Text Settings")]
    [Tooltip("Default text size for damage numbers")]
    public float defaultTextSize = 1.0f;
    [Tooltip("Enable billboard behavior for all damage numbers")]
    public bool enableBillboard = true;
    
    //[Header("Performance")]
    [Tooltip("Maximum number of active damage numbers")]
    public int maxActiveDamageNumbers = 50;
    [Tooltip("Enable automatic cleanup of old damage numbers")]
    public bool enableAutoCleanup = true;
    
    //[Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugDamageSystem = false;
    
    // Singleton instance
    private static DamageNumberSystem instance;
    
    // Container management
    private Entity damageContainer;
    
    // Performance tracking
    private int activeDamageNumberCount = 0;
    
    /// <summary>
    /// Get the singleton instance of the damage number system.
    /// </summary>
    public static DamageNumberSystem Instance => instance;
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Set up singleton
        if (instance == null)
        {
            instance = this;
            
            // Find or create damage container
            FindOrCreateDamageContainer();
            
            // Subscribe to damage events
            SubscribeToDamageEvents();
            
            if (debugDamageSystem)
            {
                Log.Info("DamageNumberSystem: Initialized and ready");
            }
        }
        else
        {
            // Destroy duplicate instances
            Scene.DestroyEntity(owner);
            Log.Warning("DamageNumberSystem: Duplicate instance destroyed");
        }
    }
    
    /// <summary>
    /// Called when the component is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        if (instance == this)
        {
            UnsubscribeFromDamageEvents();
            instance = null;
        }
    }
    
    /// <summary>
    /// Find or create the DamageContainer entity to parent all damage numbers.
    /// </summary>
    private void FindOrCreateDamageContainer()
    {
        // First try to find existing container
        damageContainer = Scene.FindEntityByName("DamageContainer");
        
        if (!damageContainer)
        {
            // Create new container entity
            damageContainer = Scene.CreateEntity("DamageContainer");
            
            if (damageContainer)
            {
                // Position it at world origin
                damageContainer.transform.position = Vector3.zero;
                
                if (debugDamageSystem)
                {
                    Log.Info("DamageNumberSystem: Created DamageContainer entity");
                }
            }
            else
            {
                Log.Error("DamageNumberSystem: Failed to create DamageContainer entity");
            }
        }
        else
        {
            if (debugDamageSystem)
            {
                Log.Info("DamageNumberSystem: Found existing DamageContainer entity");
            }
        }
    }
    
    /// <summary>
    /// Subscribe to damage events from the DamageSystem.
    /// </summary>
    private void SubscribeToDamageEvents()
    {
        // Subscribe to the centralized damage and healing event systems
        DamageSystem.OnDamageApplied += OnDamageApplied;
        DamageSystem.OnHealingApplied += OnHealingApplied;
        
        if (debugDamageSystem)
        {
            Log.Info("DamageNumberSystem: Subscribed to damage and healing events");
        }
    }
    
    /// <summary>
    /// Unsubscribe from damage events.
    /// </summary>
    private void UnsubscribeFromDamageEvents()
    {
        if (DamageSystem.OnDamageApplied != null)
        {
            DamageSystem.OnDamageApplied -= OnDamageApplied;
        }
        
        if (DamageSystem.OnHealingApplied != null)
        {
            DamageSystem.OnHealingApplied -= OnHealingApplied;
        }
        
        if (debugDamageSystem)
        {
            Log.Info("DamageNumberSystem: Unsubscribed from damage and healing events");
        }
    }
    
    /// <summary>
    /// Handle entity damage events and spawn damage numbers.
    /// </summary>
    /// <param name="damagedEntity">The entity that took damage.</param>
    /// <param name="damageSource">Entity that caused the damage.</param>
    /// <param name="damageAmount">Amount of damage taken.</param>
    private void OnDamageApplied(Entity damagedEntity, Entity damageSource, float damageAmount)
    {
        if (!damagedEntity || damageAmount <= 0)
            return;
            
        // Spawn damage number above the damaged entity
        SpawnDamageNumber(damagedEntity, damageAmount, "normal");
    }
    
    /// <summary>
    /// Handle entity healing events and spawn healing numbers.
    /// </summary>
    /// <param name="healedEntity">The entity that was healed.</param>
    /// <param name="healSource">Entity that provided the healing.</param>
    /// <param name="healAmount">Amount of healing applied.</param>
    private void OnHealingApplied(Entity healedEntity, Entity healSource, float healAmount)
    {
        if (!healedEntity || healAmount <= 0)
            return;
            
        // Spawn healing number above the healed entity
        SpawnDamageNumber(healedEntity, healAmount, "heal");
    }
    
    /// <summary>
    /// Spawn a damage number above the specified entity.
    /// </summary>
    /// <param name="targetEntity">Entity to spawn damage number above.</param>
    /// <param name="damageAmount">Damage amount to display.</param>
    /// <param name="damageType">Type of damage for color coding.</param>
    /// <returns>The spawned damage number entity.</returns>
    public Entity SpawnDamageNumber(Entity targetEntity, float damageAmount, string damageType = "normal")
    {
        if (!targetEntity)
        {
            Log.Warning("DamageNumberSystem: Cannot spawn damage number - target entity is null");
            return Entity.Invalid;
        }
        
        // Check performance limits
        if (activeDamageNumberCount >= maxActiveDamageNumbers)
        {
            if (enableAutoCleanup)
            {
                CleanupOldDamageNumbers();
            }
            else
            {
                if (debugDamageSystem)
                {
                    Log.Warning($"DamageNumberSystem: Maximum damage numbers reached ({maxActiveDamageNumbers})");
                }
                return Entity.Invalid;
            }
        }
        
        // Calculate spawn position
        Vector3 spawnPosition = CalculateSpawnPosition(targetEntity);
        
        // Create damage number entity
        Entity damageNumberEntity = Scene.CreateEntity($"DamageNumber_{damageAmount:F0}");
        
        if (!damageNumberEntity)
        {
            Log.Error("DamageNumberSystem: Failed to create damage number entity");
            return Entity.Invalid;
        }
        
        // Set position
        damageNumberEntity.transform.position = spawnPosition;
        
        // Parent under damage container
        if (damageContainer)
        {
            damageNumberEntity.transform.SetParent(damageContainer, true);
        }
        
        // Add TextComponent
        var textComponent = damageNumberEntity.AddComponent<TextComponent>();
        
        // Add DamageNumber component
        var damageNumberComponent = damageNumberEntity.AddComponent<DamageNumber>();
        if (damageNumberComponent != null)
        {
            // Configure damage number properties
            damageNumberComponent.lifetime = defaultLifetime;
            damageNumberComponent.floatSpeed = defaultFloatSpeed;
            damageNumberComponent.enableBillboard = enableBillboard;
            damageNumberComponent.debugDamageNumber = debugDamageSystem;
            
            // Set damage text
            damageNumberComponent.SetDamageText(damageAmount, damageType);
        }
        
        // Track active damage numbers
        activeDamageNumberCount++;
        
        if (debugDamageSystem)
        {
            Log.Info($"DamageNumberSystem: Spawned damage number '{damageAmount:F0}' at {spawnPosition} (Active: {activeDamageNumberCount})");
        }
        
        return damageNumberEntity;
    }
    
    /// <summary>
    /// Calculate the spawn position for a damage number above the target entity.
    /// </summary>
    /// <param name="targetEntity">Target entity.</param>
    /// <returns>Calculated spawn position.</returns>
    private Vector3 CalculateSpawnPosition(Entity targetEntity)
    {
        Vector3 basePosition = targetEntity.transform.position;
        
        // Add height offset
        Vector3 spawnPosition = basePosition + Vector3.up * spawnHeightOffset;
        
        // Add random spread
        if (spawnSpreadRadius > 0)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnSpreadRadius;
            spawnPosition += new Vector3(randomCircle.x, 0, randomCircle.y);
        }
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Clean up old damage numbers to maintain performance.
    /// </summary>
    private void CleanupOldDamageNumbers()
    {
        if (!damageContainer)
            return;
            
        // Find all damage number entities
        var damageNumbers = Scene.FindEntitiesWithComponent<DamageNumber>();
        if (damageNumbers == null || damageNumbers.Length == 0)
        {
            activeDamageNumberCount = 0;
            return;
        }
        
        int cleanedUp = 0;
        
        // Remove damage numbers that are about to expire
        foreach (var entity in damageNumbers)
        {
            if (!entity) continue;
            
            var damageNumber = entity.GetComponent<DamageNumber>();
            if (damageNumber != null && damageNumber.IsAboutToExpire())
            {
                Scene.DestroyEntity(entity);
                cleanedUp++;
                
                if (cleanedUp >= 5) // Limit cleanup per frame
                    break;
            }
        }
        
        // Update count
        activeDamageNumberCount = Mathf.Max(0, activeDamageNumberCount - cleanedUp);
        
        if (debugDamageSystem && cleanedUp > 0)
        {
            Log.Info($"DamageNumberSystem: Cleaned up {cleanedUp} old damage numbers (Active: {activeDamageNumberCount})");
        }
    }
    
    /// <summary>
    /// Spawn a healing number (green text).
    /// </summary>
    /// <param name="targetEntity">Entity that was healed.</param>
    /// <param name="healAmount">Amount of healing.</param>
    /// <returns>The spawned healing number entity.</returns>
    public Entity SpawnHealingNumber(Entity targetEntity, float healAmount)
    {
        return SpawnDamageNumber(targetEntity, healAmount, "heal");
    }
    
    /// <summary>
    /// Spawn a critical damage number (red text).
    /// </summary>
    /// <param name="targetEntity">Entity that took critical damage.</param>
    /// <param name="damageAmount">Amount of critical damage.</param>
    /// <returns>The spawned critical damage number entity.</returns>
    public Entity SpawnCriticalDamageNumber(Entity targetEntity, float damageAmount)
    {
        return SpawnDamageNumber(targetEntity, damageAmount, "critical");
    }
    
    /// <summary>
    /// Get the DamageContainer entity for external access.
    /// </summary>
    /// <returns>The DamageContainer entity.</returns>
    public Entity GetDamageContainer()
    {
        if (!damageContainer)
        {
            FindOrCreateDamageContainer();
        }
        
        return damageContainer;
    }
    
    /// <summary>
    /// Get the current number of active damage numbers.
    /// </summary>
    /// <returns>Number of active damage numbers.</returns>
    public int GetActiveDamageNumberCount()
    {
        return activeDamageNumberCount;
    }
    
    /// <summary>
    /// Set the default lifetime for new damage numbers.
    /// </summary>
    /// <param name="lifetime">New default lifetime in seconds.</param>
    public void SetDefaultLifetime(float lifetime)
    {
        defaultLifetime = Mathf.Max(0.1f, lifetime);
        
        if (debugDamageSystem)
        {
            Log.Info($"DamageNumberSystem: Default lifetime set to {defaultLifetime:F2} seconds");
        }
    }
    
    /// <summary>
    /// Clear all active damage numbers.
    /// </summary>
    public void ClearAllDamageNumbers()
    {
        var damageNumbers = Scene.FindEntitiesWithComponent<DamageNumber>();
        if (damageNumbers != null)
        {
            foreach (var entity in damageNumbers)
            {
                if (entity)
                {
                    Scene.DestroyEntity(entity);
                }
            }
        }
        
        activeDamageNumberCount = 0;
        
        if (debugDamageSystem)
        {
            Log.Info("DamageNumberSystem: Cleared all damage numbers");
        }
    }
}
