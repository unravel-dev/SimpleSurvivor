using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Example usage and documentation for the LootSystem.
/// This file shows how to create custom loot configurations for different enemy types.
/// </summary>
[ScriptSourceFile]
public class LootSystemUsage : ScriptComponent
{
    /// <summary>
    /// Example of how to create custom loot configurations for different enemy types.
    /// Call this method to set up different loot configs for various enemies.
    /// </summary>
    public void SetupEnemyLootConfigurations()
    {
        // Example: Basic enemy configuration
        var basicEnemyLoot = new LootSystem.LootConfiguration
        {
            baseExperienceValue = 8.0f,
            experiencePerLevel = 0.1f, // 10% more per level
            orbCount = 1,
            experienceVariance = 0.2f, // ±20% variance
            minimumExperience = 4.0f,
            maximumExperience = 50.0f
        };
        
        // Example: Elite enemy configuration
        var eliteEnemyLoot = new LootSystem.LootConfiguration
        {
            baseExperienceValue = 25.0f,
            experiencePerLevel = 0.2f, // 20% more per level
            orbCount = 3, // Drops 3 orbs
            experienceVariance = 0.15f, // ±15% variance
            minimumExperience = 15.0f,
            maximumExperience = 150.0f
        };
        
        // Example: Boss enemy configuration
        var bossEnemyLoot = new LootSystem.LootConfiguration
        {
            baseExperienceValue = 100.0f,
            experiencePerLevel = 0.5f, // 50% more per level
            orbCount = 5, // Drops 5 orbs
            experienceVariance = 0.1f, // ±10% variance
            minimumExperience = 50.0f,
            maximumExperience = 500.0f
        };
        
        // You would assign these configurations to specific enemy prefabs
        // in the Unity inspector or through code
        
        Log.Info("LootSystemUsage: Example loot configurations created");
    }
    
    /// <summary>
    /// Example of how the loot system calculates experience based on various factors.
    /// </summary>
    public void ExplainLootCalculation()
    {
        /*
        The LootSystem calculates experience drops based on several factors:
        
        1. Base Experience Value: Set per enemy type
        2. Player Level Multiplier: More XP as player levels up
        3. Time Multiplier: XP increases over time (survival bonus)
        4. Bonus Experience: Periodic bonuses based on time intervals
        5. Variance: Random variation to make drops feel more natural
        
        Formula:
        finalXP = (baseXP * levelMultiplier * timeMultiplier * varianceMultiplier) + bonusXP
        
        Example with default settings:
        - Base XP: 10
        - Player Level: 5 (multiplier = 1 + (5-1) * 0.15 = 1.6)
        - Game Time: 5 minutes (multiplier = 1 + 5 * 0.1 = 1.5)
        - Variance: +10% (multiplier = 1.1)
        - Bonus: 2 intervals passed = 20 bonus XP
        
        Final XP = (10 * 1.6 * 1.5 * 1.1) + 20 = 26.4 + 20 = 46.4 XP
        
        This creates a dynamic system where:
        - Early game: Lower XP values
        - Late game: Higher XP values due to level and time
        - Periodic bonuses reward survival
        - Variance keeps drops feeling random
        */
    }
}
