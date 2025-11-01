using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Centralized loot system that handles all loot drops in the game.
/// Calculates loot based on factors like player level, time passed, enemy type, etc.
/// </summary>
[ScriptSourceFile]
public class LootSystem : ScriptComponent
{
    [System.Serializable]
    public class LootConfiguration
    {
        [Tooltip("Base experience value for this enemy type")]
        public float baseExperienceValue = 10.0f;
        
        [Tooltip("Experience multiplier per player level")]
        public float experiencePerLevel = 0.1f;
        
        [Tooltip("Number of experience orbs to drop")]
        public int orbCount = 1;
        
        [Tooltip("Random variance in experience value (±percentage)")]
        public float experienceVariance = 0.2f;
        
        [Tooltip("Minimum experience value regardless of other factors")]
        public float minimumExperience = 5.0f;
        
        [Tooltip("Maximum experience value cap")]
        public float maximumExperience = 100.0f;
    }
    
    [System.Serializable]
    public class TimeBasedModifiers
    {
        [Tooltip("Experience multiplier based on game time (per minute)")]
        public float experiencePerMinute = 0.05f;
        
        [Tooltip("Maximum time-based multiplier")]
        public float maxTimeMultiplier = 2.0f;
        
        [Tooltip("Bonus experience every X seconds")]
        public float bonusInterval = 60.0f;
        
        [Tooltip("Bonus experience amount")]
        public float bonusAmount = 5.0f;
    }
    
    [Tooltip("Experience orb prefab to spawn")]
    public Prefab experienceOrbPrefab;
    
    [Tooltip("Default loot configuration")]
    public LootConfiguration defaultLootConfig = new LootConfiguration();
    
    [Tooltip("Time-based modifiers")]
    public TimeBasedModifiers timeModifiers = new TimeBasedModifiers();
    
    [Tooltip("Spread radius for dropped loot")]
    public float dropSpreadRadius = 2.0f;
    
    [Tooltip("Height offset for dropped loot")]
    public float dropHeight = 0.5f;
    
    [Tooltip("Enable debug logging")]
    public bool debugLoot = false;
    
    // Cached references
    private static LootSystem instance;
    private Entity playerEntity;
    private Experience playerExperience;
    private float gameStartTime;
    private Entity experienceContainer;
    
    /// <summary>
    /// Get the singleton instance of the loot system.
    /// </summary>
    public static LootSystem Instance => instance;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnStart()
    {
        // Set up singleton
        if (instance == null)
        {
            instance = this;
            gameStartTime = Time.time;
            
            // Find player entity
            FindPlayerEntity();
            
            // Find or create experience container
            FindOrCreateExperienceContainer();
            
            // Subscribe to death events directly
            DamageSystem.OnEntityDied += OnEntityDied;
            
            if (debugLoot)
            {
                Log.Info("LootSystem: Initialized and subscribed to death events");
            }
        }
        else
        {
            Log.Warning("LootSystem: Multiple instances detected, destroying duplicate");
            Scene.DestroyEntity(owner);
        }
    }
    
    /// <summary>
    /// Called when the script is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        // Unsubscribe from death events
        DamageSystem.OnEntityDied -= OnEntityDied;
        
        if (instance == this)
        {
            instance = null;
        }
        
        if (debugLoot)
        {
            Log.Info("LootSystem: Unsubscribed from death events and cleaned up");
        }
    }
    
    /// <summary>
    /// Find and cache the player entity and experience component.
    /// </summary>
    private void FindPlayerEntity()
    {
        // Find player by tag or component
        var playerEntities = Scene.FindEntitiesWithComponent<Player>();
        if (playerEntities != null && playerEntities.Length > 0)
        {
            playerEntity = playerEntities[0];
            playerExperience = playerEntity.GetComponent<Experience>();
            
            if (debugLoot)
            {
                Log.Info($"LootSystem: Found player entity {playerEntity.name}");
            }
        }
        else
        {
            if (debugLoot)
            {
                Log.Warning("LootSystem: No player entity found");
            }
        }
    }
    
    /// <summary>
    /// Find or create the ExperienceContainer entity to parent all experience orbs.
    /// </summary>
    private void FindOrCreateExperienceContainer()
    {
        // First try to find existing container
        experienceContainer = Scene.FindEntityByName("ExperienceContainer");
        
        if (!experienceContainer)
        {
            // Create new container entity
            experienceContainer = Scene.CreateEntity("ExperienceContainer");
            
            if (experienceContainer)
            {
                // Position it at world origin
                experienceContainer.transform.position = Vector3.zero;
                
                if (debugLoot)
                {
                    Log.Info("LootSystem: Created ExperienceContainer entity");
                }
            }
            else
            {
                Log.Error("LootSystem: Failed to create ExperienceContainer entity");
            }
        }
        else
        {
            if (debugLoot)
            {
                Log.Info("LootSystem: Found existing ExperienceContainer entity");
            }
        }
    }
    
    /// <summary>
    /// Handle entity death events from DamageSystem.
    /// </summary>
    /// <param name="deadEntity">The entity that died.</param>
    /// <param name="killer">The entity that killed it.</param>
    private void OnEntityDied(Entity deadEntity, Entity killer)
    {
        if (!deadEntity)
            return;
            
        if (debugLoot)
        {
            string killerName = killer ? killer.name : "Unknown";
            Log.Info($"LootSystem: Entity {deadEntity.name} died, killed by {killerName}");
        }
        
        // Check if this entity should drop loot
        if (ShouldDropLoot(deadEntity, killer))
        {
            ProcessLootDrop(deadEntity, killer);
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
            if (debugLoot)
            {
                Log.Info($"LootSystem: {deadEntity.name} is not an enemy, no loot drop");
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
    /// Process the loot drop for the dead entity.
    /// </summary>
    /// <param name="deadEntity">The entity that died.</param>
    /// <param name="killer">The entity that killed it.</param>
    private void ProcessLootDrop(Entity deadEntity, Entity killer)
    {
        // Get the enemy's position for loot drop
        var transformComponent = deadEntity.GetComponent<TransformComponent>();
        if (transformComponent == null)
        {
            if (debugLoot)
            {
                Log.Warning($"LootSystem: {deadEntity.name} has no TransformComponent, cannot determine drop position");
            }
            return;
        }
        
        // Get custom loot configuration based on enemy type
        var enemyComponent = deadEntity.GetComponent<Enemy>();
        LootConfiguration customConfig = null;
        
        if (enemyComponent != null)
        {
            customConfig = GetEnemyLootConfig(enemyComponent);
        }
        
        // Drop the loot
        Vector3 dropPosition = transformComponent.position;
        HandleEnemyDeath(deadEntity, dropPosition, customConfig);
        
        if (debugLoot)
        {
            Log.Info($"LootSystem: Processed loot drop for {deadEntity.name} at {dropPosition}");
        }
    }
    
    /// <summary>
    /// Get the loot configuration from an enemy component based on its type.
    /// This method creates different loot configs for different enemy types.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <returns>Custom loot configuration, or null to use default.</returns>
    private LootConfiguration GetEnemyLootConfig(Enemy enemy)
    {
        string enemyType = enemy.GetEnemyType().ToLower();
        
        // Create loot configs based on enemy type
        switch (enemyType)
        {
            case "boss":
                return CreateLootConfig(100.0f, 0.5f, 5); // Boss: 100 base XP, 50% per level, 5 orbs
                
            case "elite":
                return CreateLootConfig(25.0f, 0.2f, 3); // Elite: 25 base XP, 20% per level, 3 orbs
                
            case "heavy":
                return CreateLootConfig(18.0f, 0.15f, 2); // Heavy: 18 base XP, 15% per level, 2 orbs
                
            case "fast":
                return CreateLootConfig(12.0f, 0.12f, 1); // Fast: 12 base XP, 12% per level, 1 orb
                
            case "weak":
            case "small":
                return CreateLootConfig(5.0f, 0.05f, 1); // Weak: 5 base XP, 5% per level, 1 orb
                
            case "basic":
            default:
                // Return null to use LootSystem's default configuration
                return null;
        }
    }
    
    /// <summary>
    /// Handle loot drop when an enemy dies.
    /// </summary>
    /// <param name="enemyEntity">The enemy that died.</param>
    /// <param name="enemyPosition">Position where the enemy died.</param>
    /// <param name="customConfig">Optional custom loot configuration for this enemy.</param>
    public void HandleEnemyDeath(Entity enemyEntity, Vector3 enemyPosition, LootConfiguration customConfig = null)
    {
        if (experienceOrbPrefab == null)
        {
            if (debugLoot)
            {
                Log.Warning("LootSystem: No experience orb prefab assigned");
            }
            return;
        }
        
        // Ensure we have player reference
        if (playerEntity == null || playerExperience == null)
        {
            FindPlayerEntity();
            if (playerEntity == null || playerExperience == null)
            {
                if (debugLoot)
                {
                    Log.Warning("LootSystem: Cannot drop loot - no player found");
                }
                return;
            }
        }
        
        // Use custom config or default
        LootConfiguration config = customConfig ?? defaultLootConfig;
        
        // Calculate experience value based on various factors
        float experienceValue = CalculateExperienceValue(config, enemyEntity);
        
        // Drop experience orbs
        DropExperienceOrbs(enemyPosition, experienceValue, config.orbCount);
        
        if (debugLoot)
        {
            Log.Info($"LootSystem: Dropped loot for {enemyEntity.name} - Experience: {experienceValue:F1}");
        }
    }
    
    /// <summary>
    /// Calculate experience value based on player level, time, and other factors.
    /// </summary>
    /// <param name="config">Loot configuration to use.</param>
    /// <param name="enemyEntity">The enemy entity (for future enemy-specific modifiers).</param>
    /// <returns>Calculated experience value.</returns>
    private float CalculateExperienceValue(LootConfiguration config, Entity enemyEntity)
    {
        float baseValue = config.baseExperienceValue;
        
        // Player level modifier
        int playerLevel = playerExperience != null ? playerExperience.GetCurrentLevel() : 1;
        float levelMultiplier = 1.0f + (playerLevel - 1) * config.experiencePerLevel;
        
        // Time-based modifier
        float gameTime = Time.time - gameStartTime;
        float timeMultiplier = 1.0f + (gameTime / 60.0f) * timeModifiers.experiencePerMinute;
        timeMultiplier = Mathf.Min(timeMultiplier, timeModifiers.maxTimeMultiplier);
        
        // Bonus experience based on time intervals
        float bonusExperience = 0.0f;
        if (timeModifiers.bonusInterval > 0)
        {
            int bonusIntervals = Mathf.FloorToInt(gameTime / timeModifiers.bonusInterval);
            bonusExperience = bonusIntervals * timeModifiers.bonusAmount;
        }
        
        // Apply variance
        float variance = Random.Range(-config.experienceVariance, config.experienceVariance);
        float varianceMultiplier = 1.0f + variance;
        
        // Calculate final value
        float finalValue = (baseValue * levelMultiplier * timeMultiplier * varianceMultiplier) + bonusExperience;
        
        // Apply min/max constraints
        finalValue = Mathf.Clamp(finalValue, config.minimumExperience, config.maximumExperience);
        
        if (debugLoot)
        {
            Log.Info($"LootSystem: Experience calculation - Base: {baseValue:F1}, Level: x{levelMultiplier:F2}, Time: x{timeMultiplier:F2}, Bonus: +{bonusExperience:F1}, Final: {finalValue:F1}");
        }
        
        return finalValue;
    }
    
    /// <summary>
    /// Drop experience orbs at the specified location.
    /// </summary>
    /// <param name="dropPosition">Position to drop orbs at.</param>
    /// <param name="totalExperience">Total experience to distribute across orbs.</param>
    /// <param name="orbCount">Number of orbs to create.</param>
    private void DropExperienceOrbs(Vector3 dropPosition, float totalExperience, int orbCount)
    {
        float experiencePerOrb = totalExperience / orbCount;
        
        for (int i = 0; i < orbCount; i++)
        {
            // Calculate random position within spread radius
            Vector2 randomCircle = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 orbPosition = dropPosition + new Vector3(randomCircle.x, dropHeight, randomCircle.y);
            
            // Instantiate experience orb
            var orbEntity = Scene.Instantiate(experienceOrbPrefab);
            if (orbEntity)
            {
                orbEntity.transform.position = orbPosition;
                
                // Parent the orb under the ExperienceContainer
                if (experienceContainer)
                {
                    orbEntity.transform.SetParent(experienceContainer, true);
                }
                else
                {
                    // Try to find/create container if it doesn't exist
                    FindOrCreateExperienceContainer();
                    if (experienceContainer)
                    {
                        orbEntity.transform.SetParent(experienceContainer, true);
                    }
                }
                
                // Configure experience value
                var experienceOrb = orbEntity.GetComponent<ExperienceOrb>();
                if (experienceOrb != null)
                {
                    experienceOrb.SetExperienceValue(experiencePerOrb);
                    
                    if (debugLoot)
                    {
                        Log.Info($"LootSystem: Spawned experience orb with value {experiencePerOrb:F1} at {orbPosition}");
                    }
                }
                else
                {
                    if (debugLoot)
                    {
                        Log.Warning("LootSystem: Experience orb prefab missing ExperienceOrb component");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get current game time in seconds since start.
    /// </summary>
    /// <returns>Game time in seconds.</returns>
    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }
    
    /// <summary>
    /// Get current player level for external use.
    /// </summary>
    /// <returns>Current player level or 1 if no player found.</returns>
    public int GetPlayerLevel()
    {
        if (playerExperience != null)
        {
            return playerExperience.GetCurrentLevel();
        }
        return 1;
    }
    
    /// <summary>
    /// Get the ExperienceContainer entity for external access.
    /// </summary>
    /// <returns>The ExperienceContainer entity, or null if not found/created.</returns>
    public Entity GetExperienceContainer()
    {
        // Ensure container exists
        if (!experienceContainer)
        {
            FindOrCreateExperienceContainer();
        }
        
        return experienceContainer;
    }
    
    /// <summary>
    /// Create a custom loot configuration for specific enemy types.
    /// </summary>
    /// <param name="baseExp">Base experience value.</param>
    /// <param name="levelMultiplier">Experience per level multiplier.</param>
    /// <param name="orbCount">Number of orbs to drop.</param>
    /// <returns>New loot configuration.</returns>
    public static LootConfiguration CreateLootConfig(float baseExp, float levelMultiplier = 0.1f, int orbCount = 1)
    {
        return new LootConfiguration
        {
            baseExperienceValue = baseExp,
            experiencePerLevel = levelMultiplier,
            orbCount = orbCount,
            experienceVariance = 0.2f,
            minimumExperience = baseExp * 0.5f,
            maximumExperience = baseExp * 3.0f
        };
    }
}
