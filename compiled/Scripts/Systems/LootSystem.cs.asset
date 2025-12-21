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
    
    [Tooltip("Magnet loot prefab to spawn")]
    public Prefab magnetLootPrefab;
    
    [Tooltip("Chance to drop a magnet when an enemy dies (0-100, e.g., 3 = 3% chance)")]
    public float magnetDropChance = 3.0f; // 3% chance by default
    
    [Tooltip("Chest loot prefab to spawn")]
    public Prefab chestLootPrefab;
    
    [Tooltip("Chance to drop a chest when an enemy dies (0-100, e.g., 1 = 1% chance)")]
    
    public float chestDropChance = 1.0f; // 1% chance by default
    
    [Tooltip("Default loot configuration")]
    public LootConfiguration defaultLootConfig = new LootConfiguration();
    
    [Tooltip("Time-based modifiers")]
    public TimeBasedModifiers timeModifiers = new TimeBasedModifiers();
    
    [Tooltip("Spread radius for dropped loot")]
    public float dropSpreadRadius = 2.0f;
    
    [Tooltip("Height offset for dropped loot")]
    public float dropHeight = 0.5f;

    
    [Tooltip("Speed multiplier for experience orbs relative to player speed")]
    public float orbSpeedMultiplier = 1.5f;
    
    // Cached references
    private static LootSystem instance;
    private Entity playerEntity;
    private Experience playerExperience;
    private float gameStartTime;
    
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
            
            // Subscribe to death events directly
            DamageSystem.OnEntityDied += OnEntityDied;
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
        
        // Get custom loot configuration based on enemy type
        var enemyComponent = deadEntity.GetComponent<Enemy>();
        LootConfiguration customConfig = null;
        
        if (enemyComponent != null)
        {
            customConfig = GetEnemyLootConfig(enemyComponent);
        }
        
        // Drop the loot
        Vector3 dropPosition = deadEntity.transform.position;
        HandleEnemyDeath(deadEntity, dropPosition, customConfig);
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
            return;
        }
        
        // Ensure we have player reference
        if (playerEntity == null || playerExperience == null)
        {
            FindPlayerEntity();
            if (playerEntity == null || playerExperience == null)
            {
                return;
            }
        }
        
        // Use custom config or default
        LootConfiguration config = customConfig ?? defaultLootConfig;
        
        // Calculate experience value based on various factors
        float experienceValue = CalculateExperienceValue(config, enemyEntity);
        
        // Drop experience orbs
        DropExperienceOrbs(enemyPosition, experienceValue, config.orbCount);
        
        // Randomly drop magnet loot
        if (magnetLootPrefab != null && Random.Range(0.0f, 100.0f) < magnetDropChance)
        {
            DropMagnetLoot(enemyPosition);
        }
        
        // Check if this is an elite enemy (guaranteed chest drop)
        bool isElite = false;
        var enemyComponent = enemyEntity.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            string enemyType = enemyComponent.GetEnemyType().ToLower(); 
            isElite = enemyType == "elite";
        }
        
        // Drop chest loot - guaranteed for elites, random chance for others
        if (chestLootPrefab != null)
        {
            bool shouldDropChest = isElite || Random.Range(0.0f, 100.0f) < chestDropChance;
            if (shouldDropChest)
            {
                DropChestLoot(enemyPosition);
            }
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
            var orbEntity = Scene.Instantiate(experienceOrbPrefab, ContainerCache.LootContainer);
            if (orbEntity)
            {

                orbEntity.transform.position = orbPosition;
                
                // Configure experience value
                var experienceOrb = orbEntity.GetComponent<ExperienceOrb>();
                if (experienceOrb != null)
                {
                    experienceOrb.SetExperienceValue(experiencePerOrb);
                    experienceOrb.SetPlayerSpeedMultiplier(orbSpeedMultiplier);

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
    /// Set the speed multiplier for experience orbs relative to player speed.
    /// </summary>
    /// <param name="multiplier">Speed multiplier (orbs will move at player speed * multiplier).</param>
    public void SetOrbSpeedMultiplier(float multiplier)
    {
        orbSpeedMultiplier = Mathf.Max(1.0f, multiplier); // Ensure orbs are always at least as fast as player

    }
    
    /// <summary>
    /// Get the current orb speed multiplier.
    /// </summary>
    /// <returns>Current speed multiplier.</returns>
    public float GetOrbSpeedMultiplier()
    {
        return orbSpeedMultiplier;
    }
    
    /// <summary>
    /// Drop a magnet loot item at the specified location.
    /// </summary>
    /// <param name="dropPosition">Position to drop magnet at.</param>
    private void DropMagnetLoot(Vector3 dropPosition)
    {
        if (magnetLootPrefab == null)
            return;
        
        // Calculate random position within spread radius
        Vector2 randomCircle = Random.insideUnitCircle * dropSpreadRadius;
        Vector3 magnetPosition = dropPosition + new Vector3(randomCircle.x, dropHeight, randomCircle.y);
        
        // Instantiate magnet loot
        var magnetEntity = Scene.Instantiate(magnetLootPrefab, ContainerCache.LootContainer);
        if (magnetEntity)
        {

            magnetEntity.transform.position = magnetPosition;

        }
    }
    
    /// <summary>
    /// Drop a chest loot item at the specified location.
    /// </summary>
    /// <param name="dropPosition">Position to drop chest at.</param>
    private void DropChestLoot(Vector3 dropPosition)
    {
        if (chestLootPrefab == null)
            return;
        
        // Calculate random position within spread radius
        Vector2 randomCircle = Random.insideUnitCircle * dropSpreadRadius;
        Vector3 chestPosition = dropPosition + new Vector3(randomCircle.x, 0.0f, randomCircle.y);
        
        // Instantiate chest loot
        var chestEntity = Scene.Instantiate(chestLootPrefab, ContainerCache.LootContainer);
        if (chestEntity)
        {

            chestEntity.transform.position = chestPosition;

        }
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
