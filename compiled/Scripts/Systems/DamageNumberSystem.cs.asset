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
    
    // Singleton instance
    private static DamageNumberSystem instance;
    
    // Object pooling with ring buffer
    private Entity[] damageNumberPool;
    private int currentPoolIndex = 0;
    private bool poolInitialized = false;
    
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
            
            // Initialize object pool
            InitializeObjectPool();
            
            // Subscribe to damage events
            SubscribeToDamageEvents();
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
    /// Initialize the object pool with pre-created damage number entities.
    /// </summary>
    private void InitializeObjectPool()
    {
        if (poolInitialized)
            return;
            
        damageNumberPool = new Entity[maxActiveDamageNumbers];
        
        for (int i = 0; i < maxActiveDamageNumbers; i++)
        {
            // Create damage number entity
            Entity pooledEntity = Scene.CreateEntity($"PooledDamageNumber_{i}");
            
            if (pooledEntity)
            {
                // Parent under damage container
                pooledEntity.transform.SetParent(ContainerCache.DamageContainer, true);
                
                // Add TextComponent
                pooledEntity.AddComponent<TextComponent>();
                
                // Add DamageNumber component
                var damageNumberComponent = pooledEntity.AddComponent<DamageNumber>();
                if (damageNumberComponent != null)
                {
                    // Configure default properties
                    damageNumberComponent.lifetime = defaultLifetime;
                    damageNumberComponent.floatSpeed = defaultFloatSpeed;
                    damageNumberComponent.enableBillboard = enableBillboard;
                }
                
                // Disable the entity initially
                pooledEntity.SetActive(false);
                
                damageNumberPool[i] = pooledEntity;
            }
            else
            {
                Log.Error($"DamageNumberSystem: Failed to create pooled entity {i}");
            }
        }
        
        poolInitialized = true;
        currentPoolIndex = 0;
    }
    
    /// <summary>
    /// Subscribe to damage events from the DamageSystem.
    /// </summary>
    private void SubscribeToDamageEvents()
    {
        // Subscribe to the centralized damage and healing event systems
        DamageSystem.OnDamageApplied += OnDamageApplied;
        DamageSystem.OnHealingApplied += OnHealingApplied;
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
    }
    
    /// <summary>
    /// Handle entity damage events and spawn damage numbers.
    /// </summary>
    /// <param name="damagedEntity">The entity that took damage.</param>
    /// <param name="damageSource">Entity that caused the damage.</param>
    /// <param name="damageAmount">Amount of damage taken.</param>
    private void OnDamageApplied(Entity damagedEntity, Entity damageSource, DamageBreakdown breakdown)
    {
        if (!damagedEntity || breakdown.amount <= 0)
            return;
            
        // Spawn damage number above the damaged entity
        SpawnDamageNumber(damagedEntity, breakdown);
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
        // SpawnDamageNumber(healedEntity, healAmount, "heal");
    }
    
    /// <summary>
    /// Spawn a damage number above the specified entity using the object pool.
    /// </summary>
    /// <param name="targetEntity">Entity to spawn damage number above.</param>
    /// <param name="damageAmount">Damage amount to display.</param>
    /// <param name="damageType">Type of damage for color coding.</param>
    /// <returns>The spawned damage number entity.</returns>
    public Entity SpawnDamageNumber(Entity targetEntity, DamageBreakdown breakdown)
    {
        if (!targetEntity)
        {
            Log.Warning("DamageNumberSystem: Cannot spawn damage number - target entity is null");
            return Entity.Invalid;
        }
        
        if (!poolInitialized)
        {
            Log.Error("DamageNumberSystem: Object pool not initialized");
            return Entity.Invalid;
        }
        
        // Get the next entity from the ring buffer
        Entity damageNumberEntity = damageNumberPool[currentPoolIndex];
        
        if (!damageNumberEntity)
        {
            Log.Error($"DamageNumberSystem: Pooled entity at index {currentPoolIndex} is invalid");
            return Entity.Invalid;
        }
        
        // Calculate spawn position
        Vector3 spawnPosition = CalculateSpawnPosition(targetEntity);
        
        // Set position
        damageNumberEntity.transform.position = spawnPosition;
        
        // Get the DamageNumber component
        var damageNumberComponent = damageNumberEntity.GetComponent<DamageNumber>();
        if (damageNumberComponent != null)
        {
            // Reset and configure damage number properties
            damageNumberComponent.lifetime = defaultLifetime;
            damageNumberComponent.floatSpeed = defaultFloatSpeed;
            damageNumberComponent.enableBillboard = enableBillboard;

            // Reset the component state
            damageNumberComponent.ResetDamageNumber();

                        
            // Set damage text
            damageNumberComponent.SetDamageText(breakdown);
            
        }
        
        // Activate the entity
        damageNumberEntity.SetActive(true);
        
        // Move to next index in ring buffer
        currentPoolIndex = (currentPoolIndex + 1) % maxActiveDamageNumbers;

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
    /// Note: With object pooling, this method is no longer needed as entities are reused.
    /// </summary>
    private void CleanupOldDamageNumbers()
    {

    }

    
    /// <summary>
    /// Get the current number of active damage numbers.
    /// </summary>
    /// <returns>Number of active damage numbers.</returns>
    public int GetActiveDamageNumberCount()
    {
        if (!poolInitialized || damageNumberPool == null)
            return 0;
            
        int activeCount = 0;
        for (int i = 0; i < damageNumberPool.Length; i++)
        {
            if (damageNumberPool[i] && damageNumberPool[i].active)
            {
                activeCount++;
            }
        }
        
        return activeCount;
    }
    
    /// <summary>
    /// Set the default lifetime for new damage numbers.
    /// </summary>
    /// <param name="lifetime">New default lifetime in seconds.</param>
    public void SetDefaultLifetime(float lifetime)
    {
        defaultLifetime = Mathf.Max(0.1f, lifetime);
    }
    
    /// <summary>
    /// Clear all active damage numbers by deactivating pooled entities.
    /// </summary>
    public void ClearAllDamageNumbers()
    {
        if (!poolInitialized || damageNumberPool == null)
            return;
            
        int deactivatedCount = 0;
        for (int i = 0; i < damageNumberPool.Length; i++)
        {
            if (damageNumberPool[i] && damageNumberPool[i].active)
            {
                damageNumberPool[i].SetActive(false);
                deactivatedCount++;
            }
        }
        
        // Reset pool index
        currentPoolIndex = 0;
    }
}
