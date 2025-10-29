using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Example setup script showing how to configure the LootSystem.
/// This script should be attached to a GameObject in your scene.
/// </summary>
[ScriptSourceFile]
public class LootSystemSetup : ScriptComponent
{
    [Tooltip("Experience orb prefab to use for loot drops")]
    public Prefab experienceOrbPrefab;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        // Create a LootSystem entity if one doesn't exist
        SetupLootSystem();
        
        // Create a LootHandler to connect DamageSystem with LootSystem
        SetupLootHandler();
    }
    
    /// <summary>
    /// Set up the loot system with default configuration.
    /// </summary>
    private void SetupLootSystem()
    {
        // Check if LootSystem already exists
        var existingLootSystems = Scene.FindEntitiesWithComponent<LootSystem>();
        if (existingLootSystems != null && existingLootSystems.Length > 0)
        {
            Log.Info("LootSystemSetup: LootSystem already exists");
            return;
        }
        
        // Create new LootSystem entity
        var lootSystemEntity = Scene.CreateEntity("LootSystem");
        var lootSystem = lootSystemEntity.AddComponent<LootSystem>();
        
        // Configure the loot system
        lootSystem.experienceOrbPrefab = experienceOrbPrefab;
        lootSystem.debugLoot = true; // Enable debug for testing
        
        // Configure default loot settings
        lootSystem.defaultLootConfig.baseExperienceValue = 10.0f;
        lootSystem.defaultLootConfig.experiencePerLevel = 0.15f; // 15% more XP per level
        lootSystem.defaultLootConfig.orbCount = 1;
        lootSystem.defaultLootConfig.experienceVariance = 0.3f; // ±30% variance
        lootSystem.defaultLootConfig.minimumExperience = 5.0f;
        lootSystem.defaultLootConfig.maximumExperience = 200.0f;
        
        // Configure time-based modifiers
        lootSystem.timeModifiers.experiencePerMinute = 0.1f; // 10% more XP per minute
        lootSystem.timeModifiers.maxTimeMultiplier = 3.0f; // Max 3x multiplier
        lootSystem.timeModifiers.bonusInterval = 120.0f; // Bonus every 2 minutes
        lootSystem.timeModifiers.bonusAmount = 10.0f; // +10 XP bonus
        
        // Configure drop settings
        lootSystem.dropSpreadRadius = 2.5f;
        lootSystem.dropHeight = 0.5f;
        
        Log.Info("LootSystemSetup: Created and configured LootSystem");
    }
    
    /// <summary>
    /// Set up the loot handler to connect DamageSystem with LootSystem.
    /// </summary>
    private void SetupLootHandler()
    {
        // Check if LootHandler already exists
        var existingLootHandlers = Scene.FindEntitiesWithComponent<LootHandler>();
        if (existingLootHandlers != null && existingLootHandlers.Length > 0)
        {
            Log.Info("LootSystemSetup: LootHandler already exists");
            return;
        }
        
        // Create new LootHandler entity
        var lootHandlerEntity = Scene.CreateEntity("LootHandler");
        var lootHandler = lootHandlerEntity.AddComponent<LootHandler>();
        
        // Configure the loot handler
        lootHandler.debugLootHandler = true; // Enable debug for testing
        
        Log.Info("LootSystemSetup: Created and configured LootHandler");
    }
}
