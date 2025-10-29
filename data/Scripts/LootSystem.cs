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
            
            if (debugLoot)
            {
                Log.Info("LootSystem: Initialized");
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
