using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Experience component that handles collecting experience orbs within pickup range.
/// Manages player experience, level progression, and orb attraction.
/// </summary>
[ScriptSourceFile]
public class Experience : ScriptComponent
{
    public Prefab levelUpEffect;
    //[Header("Pickup Settings")]
    [Tooltip("Range within which experience orbs are attracted to the player")]
    public float pickupRange = 5.0f;
    [Tooltip("How often to check for nearby experience orbs (in seconds)")]
    public float detectionInterval = 0.1f;
    
    //[Header("Experience Settings")]
    [Tooltip("Current experience points")]
    public float currentExperience = 0.0f;
    [Tooltip("Current player level")]
    public int currentLevel = 1;
    [Tooltip("Base experience required for level 2")]
    public float baseExperienceRequired = 100.0f;
    [Tooltip("Multiplier for experience required per level")]
    public float experienceMultiplier = 1.5f;
    
    //[Header("Debug")]
    [Tooltip("Enable debug logging for experience pickup events")]
    public bool debugPickup = false;
    [Tooltip("Draw debug sphere for pickup range")]
    public bool debugDrawRange = false;
    
    // Component references
    private TransformComponent transformComponent;
    
    // State
    private float lastDetectionTime = 0.0f;
    private List<ExperienceOrb> attractedOrbs = new List<ExperienceOrb>();
    
    // Events
    public System.Action<float> OnExperienceGained; // (experienceAmount)
    public System.Action<int, int> OnLevelUp; // (newLevel, oldLevel)
    public System.Action<float, float> OnExperienceChanged; // (currentExp, expToNextLevel)
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        
        if (transformComponent == null)
        {
            Log.Error($"Experience on {owner.name}: TransformComponent not found!");
        }
        
        if (debugPickup)
        {
            Log.Info($"Experience initialized on {owner.name}");
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        lastDetectionTime = 0.0f;
        attractedOrbs.Clear();
        
        if (debugPickup)
        {
            Log.Info($"Experience started on {owner.name} - Level: {currentLevel}, XP: {currentExperience}");
        }
    }
    
    /// <summary>
    /// Called every frame to update pickup logic.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null)
            return;
            
        // Check for nearby experience orbs at intervals
        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            DetectNearbyOrbs();
            lastDetectionTime = Time.time;
        }
        
        // Clean up invalid attracted orbs
        CleanupAttractedOrbs();
        
        // Debug drawing
        if (debugDrawRange)
        {
            DrawDebugRange();
        }
    }
    
    /// <summary>
    /// Detect and attract nearby experience orbs.
    /// </summary>
    private void DetectNearbyOrbs()
    {
        Vector3 playerPosition = transformComponent.position;
        
        // Find all entities with ExperienceOrb components within range
        var nearbyEntities = Physics.SphereOverlap(playerPosition, pickupRange, LayerMask.GetMask("Experience"), true);
        
        if (nearbyEntities == null || nearbyEntities.Length == 0)
            return;
            
        int orbsFound = 0;
        int orbsAttracted = 0;
        
        foreach (var entity in nearbyEntities)
        {
            if (!entity) continue;
            
            var experienceOrb = entity.GetComponent<ExperienceOrb>();
            if (experienceOrb == null) continue;
            
            orbsFound++;
            
            // Skip if already being attracted
            if (experienceOrb.IsBeingAttracted()) continue;
            
            // Start attracting this orb
            experienceOrb.StartAttraction(owner);
            attractedOrbs.Add(experienceOrb);
            orbsAttracted++;
            
            if (debugPickup)
            {
                Log.Info($"Experience: Started attracting orb {entity.name} (value: {experienceOrb.GetExperienceValue()})");
            }
        }
        
        if (debugPickup && orbsFound > 0)
        {
            Log.Info($"Experience: Found {orbsFound} orbs, attracted {orbsAttracted} new orbs");
        }
    }
    
    /// <summary>
    /// Clean up references to destroyed or invalid orbs.
    /// </summary>
    private void CleanupAttractedOrbs()
    {
        for (int i = attractedOrbs.Count - 1; i >= 0; i--)
        {
            if (attractedOrbs[i] == null || !attractedOrbs[i].owner)
            {
                attractedOrbs.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Collect experience from an orb.
    /// </summary>
    /// <param name="experienceAmount">Amount of experience to add.</param>
    /// <param name="orbEntity">The orb entity that was collected.</param>
    public void CollectExperience(float experienceAmount, Entity orbEntity)
    {
        if (experienceAmount <= 0)
            return;
            
        float previousExperience = currentExperience;
        int previousLevel = currentLevel;
        
        // Add experience
        currentExperience += experienceAmount;
        
        if (debugPickup)
        {
            string orbInfo = orbEntity ? $" from {orbEntity.name}" : "";
            Log.Info($"Experience: Gained {experienceAmount} XP{orbInfo} - Total: {currentExperience}");
        }
        
        // Trigger experience gained event
        OnExperienceGained?.Invoke(experienceAmount);
        
        // Check for level up
        CheckLevelUp(previousLevel);
        
        // Trigger experience changed event
        float expToNextLevel = GetExperienceToNextLevel();
        OnExperienceChanged?.Invoke(currentExperience, expToNextLevel);
        
        // Remove orb from attracted list if it was tracked
        if (orbEntity)
        {
            var orbComponent = orbEntity.GetComponent<ExperienceOrb>();
            if (orbComponent != null)
            {
                attractedOrbs.Remove(orbComponent);
            }
        }
    }
    
    /// <summary>
    /// Check if the player should level up and handle level progression.
    /// </summary>
    /// <param name="previousLevel">The player's previous level.</param>
    private void CheckLevelUp(int previousLevel)
    {
        float expRequired = GetExperienceRequiredForLevel(currentLevel + 1);
        
        while (currentExperience >= expRequired)
        {
            // Level up!
            currentLevel++;
            var effect = Scene.Instantiate(levelUpEffect);
            effect.transform.position = transformComponent.position;
            effect.transform.SetParent(owner, true);
            Scene.DestroyEntity(effect, 2.0f);

            if (debugPickup)
            {
                Log.Info($"Experience: LEVEL UP! {previousLevel} -> {currentLevel}");
            }
            
            // Trigger level up event
            OnLevelUp?.Invoke(currentLevel, previousLevel);
            
            previousLevel = currentLevel;
            expRequired = GetExperienceRequiredForLevel(currentLevel + 1);
        }
    }
    
    /// <summary>
    /// Calculate experience required to reach a specific level.
    /// </summary>
    /// <param name="level">Target level.</param>
    /// <returns>Total experience required to reach that level.</returns>
    public float GetExperienceRequiredForLevel(int level)
    {
        if (level <= 1)
            return 0;
            
        float totalExp = 0;
        for (int i = 2; i <= level; i++)
        {
            float expForThisLevel = baseExperienceRequired * Mathf.Pow(experienceMultiplier, i - 2);
            totalExp += expForThisLevel;
        }
        
        return totalExp;
    }
    
    /// <summary>
    /// Get experience required to reach the next level.
    /// </summary>
    /// <returns>Experience needed for next level.</returns>
    public float GetExperienceToNextLevel()
    {
        float expForNextLevel = GetExperienceRequiredForLevel(currentLevel + 1);
        return expForNextLevel - currentExperience;
    }
    
    /// <summary>
    /// Get experience progress towards next level as a percentage.
    /// </summary>
    /// <returns>Progress percentage (0.0 to 1.0).</returns>
    public float GetLevelProgress()
    {
        float expForCurrentLevel = GetExperienceRequiredForLevel(currentLevel);
        float expForNextLevel = GetExperienceRequiredForLevel(currentLevel + 1);
        
        if (expForNextLevel <= expForCurrentLevel)
            return 1.0f;
            
        float expInCurrentLevel = currentExperience - expForCurrentLevel;
        float expNeededForLevel = expForNextLevel - expForCurrentLevel;
        
        return Mathf.Clamp01(expInCurrentLevel / expNeededForLevel);
    }
    
    /// <summary>
    /// Draw debug visualization for pickup range.
    /// </summary>
    private void DrawDebugRange()
    {
        // This would typically use a debug drawing system
        // For now, we'll just log the range periodically
        if (Time.time % 2.0f < Time.deltaTime) // Every 2 seconds
        {
            if (debugPickup)
            {
                Vector3 playerPosition = transformComponent.position;
                Log.Info($"Experience Debug - Range: {pickupRange}, Position: {playerPosition}, Attracted Orbs: {attractedOrbs.Count}");
            }
        }
    }
    
    /// <summary>
    /// Get the current experience amount.
    /// </summary>
    /// <returns>Current experience.</returns>
    public float GetCurrentExperience()
    {
        return currentExperience;
    }
    
    /// <summary>
    /// Get the current level.
    /// </summary>
    /// <returns>Current level.</returns>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// Set the pickup range.
    /// </summary>
    /// <param name="newRange">New pickup range.</param>
    public void SetPickupRange(float newRange)
    {
        pickupRange = Mathf.Max(0.1f, newRange);
        
        if (debugPickup)
        {
            Log.Info($"Experience: Pickup range set to {pickupRange}");
        }
    }
    
    /// <summary>
    /// Add experience directly (for testing or special cases).
    /// </summary>
    /// <param name="amount">Amount of experience to add.</param>
    public void AddExperience(float amount)
    {
        CollectExperience(amount, owner);
    }
    
    /// <summary>
    /// Set the player's level directly (for testing or loading save data).
    /// </summary>
    /// <param name="level">New level.</param>
    /// <param name="adjustExperience">Whether to adjust experience to match the level.</param>
    public void SetLevel(int level, bool adjustExperience = false)
    {
        int previousLevel = currentLevel;
        currentLevel = Mathf.Max(1, level);
        
        if (adjustExperience)
        {
            currentExperience = GetExperienceRequiredForLevel(currentLevel);
        }
        
        if (debugPickup)
        {
            Log.Info($"Experience: Level set to {currentLevel} (XP: {currentExperience})");
        }
        
        OnLevelUp?.Invoke(currentLevel, previousLevel);
        OnExperienceChanged?.Invoke(currentExperience, GetExperienceToNextLevel());
    }
    
    /// <summary>
    /// Get the number of orbs currently being attracted.
    /// </summary>
    /// <returns>Number of attracted orbs.</returns>
    public int GetAttractedOrbCount()
    {
        CleanupAttractedOrbs();
        return attractedOrbs.Count;
    }
}
